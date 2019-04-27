using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CoreApiSeed.Models;
using Microsoft.AspNetCore.Identity;

namespace CoreApiSeed.Models
{
    public class User:IdentityUser
    {
        [MaxLength(128),Required]
        public string Name { get; set; }
        public string Picture { get; set; }
        [MaxLength(40)]
        public override string PhoneNumber { get; set; }
        public long ProfileId { get; set; }
        public virtual UserProfile Profile { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
    }
}
