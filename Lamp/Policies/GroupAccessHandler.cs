using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Lamp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lamp.Policies
{
    public class GroupAccessHandler : AuthorizationHandler<GroupAccessRequirement>
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupAccessHandler(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupAccessRequirement requirement)
        {
            string groupId = "";

            // Get group id from the route data or from supplied resource
            if (context.Resource is AuthorizationFilterContext authContext)
            {
                groupId = authContext.RouteData.Values[requirement.ParameterName] as string;
            }
            else if (context.Resource is string id)
            {
                groupId = id;
            }

            if (groupId == "")
                return;

            // Load user details
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
                return;

            await _db.Entry(user).Collection(x => x.Members).LoadAsync();

            // Confirm that the user belongs to the group and has required role
            var member = user.Members.SingleOrDefault(m => m.GroupId == groupId);

            if (member != null)
            {
                if (requirement.GroupRoles.Count() == 0)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    if (requirement.GroupRoles.Any(x => member.Role == x))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}