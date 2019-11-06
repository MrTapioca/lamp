using Microsoft.AspNetCore.Identity;
using Lamp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Data
{
    public class ApplicationUser : IdentityUser
    {
        [NotMapped]
        public string FullName => FirstName + " " + LastName;

        [NotMapped]
        public string Initials => FirstName.Substring(0, 1) + LastName.Substring(0, 1);

        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string LastName { get; set; }

        public ICollection<Member> Members { get; set; }
    }
}
