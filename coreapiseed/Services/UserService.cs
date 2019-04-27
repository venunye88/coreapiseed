using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CodeApiSeed.Helpers;
using CodeApiSeed.Services;
using CoreApiSeed.Configurations;
using CoreApiSeed.Data;
using CoreApiSeed.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CodeApiSeed.Services
{
    public class RegisterUserModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password),
         StringLength(100, ErrorMessage = "The {0} must be at least {6} characters long", MinimumLength = 6)]
        public string Password { get; set; }
        [DataType(DataType.Password),Compare("Password",ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public long ProfileId { get; set; }
        //public string Picture { get; set; }
    }

    public class UpdateUserModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password),
         StringLength(100, ErrorMessage = "The {0} must be at least {6} characters long", MinimumLength = 6)]
        public string Password { get; set; }
        [DataType(DataType.Password),Compare("Password",ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public long ProfileId { get; set; }
        public string Picture { get; set; }
    }

    public class LoginParams
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
public class UserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public long ProfileId { get; set; }
    public string ProfileName { get; set; }
    public string Picture { get; set; }
}

public interface IUserService
{
    Task<LoginResponse> Authenticate(LoginParams loginParams);
    Task<bool> CreateUser(RegisterUserModel model);
    Task<bool> UpdateUser(UpdateUserModel model);
    Task<bool> DeleteUser(string username);
    Task<List<UserDto>> GetAllUsers();
    Task<List<string>> GetPrivileges();
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context,
        IOptions<JwtSettings> jwt)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _jwtSettings = jwt.Value;
    }


    public async Task<LoginResponse> Authenticate(LoginParams loginParams)
    {
        //var user = await _userManager.FindByNameAsync(loginParams.Username);
        var user = _context.Users
            .Include(x => x.Profile)
            .FirstOrDefault(q => q.UserName == loginParams.Username);

        if (user != null && await _userManager.CheckPasswordAsync(user, loginParams.Password))
        {
            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var roles = _userManager.GetRolesAsync(user).Result;
            var claims = roles.Select(x => new Claim("roles", x)).ToList();
            claims.Add(new Claim("username",user.UserName));
            claims.Add(new Claim("profile",user.Profile?.Name));
            claims.Add(new Claim("email",user.Email??""));
            claims.Add(new Claim("phoneNumber",user.PhoneNumber));
            claims.Add(new Claim("fullName",user.Name));

            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256),
                claims: claims);

            return new LoginResponse
            {
                Username = user.UserName,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        throw new Exception("Invalid username or password");
    }

    public async Task<bool> CreateUser(RegisterUserModel model)
    {
        var user = new User
        {
            UserName = model.Username,
            PhoneNumber = model.PhoneNumber,
            Name = model.Name,
            Email = model.Email,
            ProfileId = model.ProfileId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) throw new Exception(ExceptionHelper.ProcessException(result));

        //Add roles in profile to user
        var profile = await _context.UserProfiles.FindAsync(model.ProfileId);
        if (profile != null)
        {
            var theUser = await _userManager.FindByNameAsync(model.Username);
            var privileges = profile.Privileges?.Split(',').ToList().Select(q => q.Trim());
            await _userManager.AddToRolesAsync(user, privileges);
        }

        return result.Succeeded;
    }

    public async Task<bool> UpdateUser(UpdateUserModel model)
    {
       //var user = _context.Users.Include(q=>q.)
        var user = _context.Users.FirstOrDefault(q=>q.UserName==model.Username);
        if (user == null) throw new Exception("User not found.");
        user.Name = model.Name;
        user.ProfileId = model.ProfileId;
        user.UpdatedAt = DateTime.UtcNow;
        user.PhoneNumber = model.PhoneNumber;
        user.Email = model.Email;

        var res = await _userManager.UpdateAsync(user);

        if(!res.Succeeded) throw new Exception(ExceptionHelper.ProcessException(res));

        //Update user roles
        var profile = await _context.UserProfiles.FindAsync(model.ProfileId);
        if (profile != null)
        {
            var oldRoles = await _userManager.GetRolesAsync(user);
            var clearRoles = await _userManager.RemoveFromRolesAsync(user, oldRoles);

            if (clearRoles.Succeeded)
            {
                var privileges = profile.Privileges?.Split(',').ToList().Select(q => q.Trim());
                await _userManager.AddToRolesAsync(user, privileges);
            }

        }

        //Change Password
        if (!string.IsNullOrEmpty(model.Password))
        {
            var clearPassword = await _userManager.RemovePasswordAsync(user);
            if (clearPassword.Succeeded) await _userManager.AddPasswordAsync(user, model.Password);
        }

        return true;
    }

    public async Task<bool> DeleteUser(string username)
    {
        var theUser = await _userManager.FindByNameAsync(username);
        await _userManager.DeleteAsync(theUser);
        return true;
    }

    public async Task<List<UserDto>> GetAllUsers()
    {
        return await Task.FromResult(_context.Users.Select(q => new UserDto
        {
            Id = q.Id,
            Username = q.UserName,
            Name = q.Name,
            Email = q.Email,
            Picture = q.Picture,
            ProfileId = q.ProfileId,
            ProfileName = q.Profile.Name
        }).ToList());

    }

    public async Task<List<string>> GetPrivileges()
    {
        return await Task.FromResult(_roleManager.Roles.Select(q => q.Name).ToList());
    }
}
