using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Policies
{
    public class GroupAccessRequirement : IAuthorizationRequirement
    {
        public string ParameterName { get; set; }
        public IEnumerable<string> GroupRoles { get; set; }

        public GroupAccessRequirement(string parameterName, params string[] roles)
        {
            ParameterName = parameterName;
            GroupRoles = roles.ToArray();
        }
    }
}