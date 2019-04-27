using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CoreApiSeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeApiSeed.Models
{
    public class AppSetting:HasId
    {
        [MaxLength(512),Required]
        public string Name { get; set; }
        public string Value { get; set; }
    }

    internal class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
    {
        public void Configure(EntityTypeBuilder<AppSetting> builder)
        {
            builder.HasIndex(q => q.Name).IsUnique();
        }
    }

}
