using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CodeApiSeed.Models;
using CoreApiSeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApiSeed.Models
{
    public class UserProfile:HasId
    {
        [Required,MaxLength(512)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public string Privileges { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
        public virtual List<User> Users { get; set; }
    }

    internal class UserProfileConfiguration:IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasIndex(q => q.Name).IsUnique();
        }
    }
}
