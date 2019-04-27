using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreApiSeed.Data;
using CoreApiSeed.Models;
using Microsoft.AspNetCore.Identity;

namespace CodeApiSeed.Services
{
    public class UserProfileDto : LookUpDto
    {
        //public string DefaultView { get; set; }
        [Required]
        public List<string> Privileges { get; set; }
    }

    public interface IUserProfileService : IModelService<UserProfileDto> { }

    public class UserProfileService:IUserProfileService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserProfileService(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<UserProfileDto> Find(long id)
        {
            var record = _context.UserProfiles.Find(id);
            return await Task.FromResult(record != null ? new UserProfileDto
            {
                Id = record.Id,
                Name=record.Name,
                Notes = record.Description,
                Privileges = record.Privileges.Split(',').ToList()
            } : null);
        }

        public async Task<List<UserProfileDto>> FetchAll()
        {
            return await Task.FromResult(_context.UserProfiles.ToList().Select(q => new UserProfileDto
            {
                Id = q.Id,
                Name = q.Name,
                Notes = q.Description,
                Privileges = q.Privileges.Split(',').ToList()
            }).ToList());
        }

        public async Task<long> Save(UserProfileDto record)
        {
            var profile = new UserProfile
            {
                Name = record.Name,
                Description = record.Notes,
                Privileges = record.Privileges.Aggregate((a, b) => $"{a},{b}")
            };

            await _context.UserProfiles.AddAsync(profile);
            _context.SaveChanges();

            return profile.Id;
        }

        public async Task<long> Update(UserProfileDto record)
        {
            var profile = await _context.UserProfiles.FindAsync(record.Id);
            profile.Name = record.Name;
            profile.Description = record.Notes;
            profile.Privileges = record.Privileges.Aggregate((a, b) => $"{a},{b}");

            //Update user privileges with profile new privileges
            var users = _context.Users.Where(q => q.ProfileId == record.Id).ToList();
            foreach (var user in users)
            {
                var oldRoles = await _userManager.GetRolesAsync(user);
                var clearRoles = await _userManager.RemoveFromRolesAsync(user, oldRoles);
                if (clearRoles.Succeeded)
                {
                    await _userManager.AddToRolesAsync(user, record.Privileges);
                }
            }

            _context.UserProfiles.Update(profile);
            _context.SaveChanges();
            return record.Id;
        }

        public async Task<bool> Delete(long id)
        {
            var record = await _context.UserProfiles.FindAsync(id);
            _context.UserProfiles.Remove(record);
            _context.SaveChanges();
            return true;
        }
    }
}
