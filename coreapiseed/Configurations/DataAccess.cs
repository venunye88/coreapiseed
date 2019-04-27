using CodeApiSeed.Services;
using CoreApiSeed.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeApiSeed.Configurations
{
    public static class DataAccess
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("AppDbContext")));
            return services;
        }

        public static IServiceCollection InjectServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserProfileService, UserProfileService>();

            return services;
        }

        public class DataMappingProfile : AutoMapper.Profile
        {
            public DataMappingProfile()
            {
                //CreateMap<Title, TitleDto>().ReverseMap();
                //CreateMap<Bed, BedDto>()
                //    .ForMember(q => q.WardName, m => m.MapFrom(s => s.Ward.Name))
                //    .ReverseMap();
            }
        }
    }
}
