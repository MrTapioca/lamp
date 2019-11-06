using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Data
{
    public class Member
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public Group Group { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Role { get; set; }
        public bool Approved { get; set; }

        [NotMapped]
        public bool CanManageShifts => Role == GroupRole.Administrator || Role == GroupRole.Assistant;
    }
}

public static class GroupRole
{
    public const string Publisher = "Publisher";
    public const string Assistant = "Assistant";
    public const string Administrator = "Administrator";

    public static bool Exists(string role)
    {
        if (role == Publisher ||
            role == Assistant ||
            role == Administrator)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool CanCreateShifts(string role)
    {
        if (role == Administrator || role == Assistant)
            return true;
        else
            return false;
    }
}

// TODO: Rename class to Member