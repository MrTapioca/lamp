using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lamp.Data;

namespace Lamp.Pages
{
    [Authorize("HasGroupManagementAccess")]
    public class GroupModel : PageModel
    {
        readonly ApplicationDbContext _db;
        readonly UserManager<ApplicationUser> _userManager;

        public class MemberModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public bool Approved { get; set; }
        }

        public class LocationModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class EmailLinkModel
        {
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        // Model properties to be consumed by View
        public IEnumerable<SelectListItem> Roles { get; set; }
        public IEnumerable<LocationModel> Locations { get; set; }
        public IEnumerable<MemberModel> Members { get; set; }
        public EmailLinkModel EmailLink { get; set; }

        [TempData]
        public string StatusMessage { get; set; }
        [TempData]
        public string StatusMessageType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string GroupId { get; set; }

        [BindProperty]
        public bool UsersCanScheduleOthers { get; set; }

        public string GroupName { get; set; }

        // Constructor
        public GroupModel(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;

            Roles = new SelectListItem[]
            {
                new SelectListItem(GroupRole.Administrator, GroupRole.Administrator),
                new SelectListItem(GroupRole.Assistant, GroupRole.Assistant),
                new SelectListItem(GroupRole.Publisher, GroupRole.Publisher)
            };
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // The group is guaranteed to exist due to the authorization policy
            var group = _db.Groups
                .Include(g => g.Members)
                    .ThenInclude(gu => gu.User)
                .Include(g => g.Locations)
                .SingleOrDefault(g => g.Id == GroupId);

            // Create the invitation email link
            EmailLink = new EmailLinkModel
            {
                Subject = "You are invited to join the " + group.Name + " group",
                Body = $"Hi!{Environment.NewLine}{Environment.NewLine}" +
                    $"You are invited to join the {group.Name} group in the Public Witnessing Calendar app. Once you join the group, you will be able to view available shifts and sign up to participate.{Environment.NewLine}{Environment.NewLine}" +
                    $"To join the group, you can use the group code {GroupId} or click the following link:{Environment.NewLine}" +
                    Url.Page("/Group", "", new { groupId = GroupId }, Request.Scheme) + Environment.NewLine + Environment.NewLine +
                    $"Sincerely,{Environment.NewLine}{user.FullName}"
            };
            EmailLink.Subject = Uri.EscapeDataString(EmailLink.Subject);
            EmailLink.Body = Uri.EscapeDataString(EmailLink.Body);

            // Load group settings
            GroupName = group.Name;
            UsersCanScheduleOthers = group.UsersCanScheduleOthers;

            // Get locations
            var locations = new List<LocationModel>();
            foreach (var location in group.Locations)
            {
                locations.Add(new LocationModel
                {
                    Id = location.Id,
                    Name = location.Name
                });
            }

            Locations = locations.OrderBy(l => l.Name).ToArray();

            // Get a list of members
            var members = new List<MemberModel>();
            foreach (var groupUser in group.Members)
            {
                members.Add(new MemberModel
                {
                    Id = groupUser.Id,
                    Name = groupUser.User.FullName,
                    Role = groupUser.Role,
                    Approved = groupUser.Approved
                });
            };

            Members = members.OrderBy(m => m.Name).ToArray();

            return Page();
        }

        public async Task<IActionResult> OnPostSettings()
        {
            var group = _db.Groups.SingleOrDefault(g => g.Id == GroupId);

            group.UsersCanScheduleOthers = UsersCanScheduleOthers;
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLocation(string newLocation)
        {
            if (string.IsNullOrEmpty(newLocation))
                return RedirectToPage();

            newLocation = newLocation.Trim();

            // Check that the location does not already exist
            if (await _db.Locations.AnyAsync(l => l.Name == newLocation))
            {
                StatusMessage = "A location with this name already exists.";
                StatusMessageType = ViewModels.StatusMessageType.Warning;
                return RedirectToPage();
            }

            var location = new Location()
            {
                Group = _db.Groups.First(g => g.Id == GroupId),
                Name = newLocation
            };
            _db.Locations.Add(location);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveLocation(int locationId)
        {
            // Check that the location exists
            var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == locationId);
            if (location == null)
                return NotFound($"Location with Id {locationId} not found.");

            _db.Locations.Remove(location);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApprove(string memberId)
        {
            // Check that the member exists
            var member = await _db.Members
                .Include(gu => gu.User)
                .SingleOrDefaultAsync(gu => gu.Id == memberId);
            if (member == null)
                return NotFound($"Member with Id {memberId} not found.");

            var name = member.User.FullName;

            // Approve the member
            member.Approved = true;
            await _db.SaveChangesAsync();

            StatusMessage = $"The member <b>{name}</b> has been approved.";
            StatusMessageType = ViewModels.StatusMessageType.Success;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReject(string memberId)
        {
            // Check that the member exists
            var member = await _db.Members
                .Include(gu => gu.User)
                .SingleOrDefaultAsync(gu => gu.Id == memberId);
            if (member == null)
                return NotFound($"Member with Id {memberId} not found.");

            var name = member.User.FullName;

            // Reject the member by deleting it
            _db.Members.Remove(member);
            await _db.SaveChangesAsync();

            StatusMessage = $"The member <b>{name}</b> has been rejected from joining this group.";
            StatusMessageType = ViewModels.StatusMessageType.Warning;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemove(string memberId)
        {
            // Check that the member exists
            var member = await _db.Members
                .Include(gu => gu.User)
                .SingleOrDefaultAsync(gu => gu.Id == memberId);
            if (member == null)
                return NotFound($"Member with Id {memberId} not found.");

            var name = member.User.FullName;

            // Verify that the member is not the last Administrator
            if (!await _db.Members.AnyAsync(m =>
                m.Role == GroupRole.Administrator && m.Id != member.Id))
            {
                StatusMessage = $"<b>{name}</b> is the only administrator on this group. " +
                    $"Please promote another administrator before removing this member.";
                StatusMessageType = ViewModels.StatusMessageType.Error;
                return RedirectToPage();
            }

            // Remove the member
            _db.Members.Remove(member);
            await _db.SaveChangesAsync();

            StatusMessage = $"The member <b>{name}</b> has been removed from the group.";
            StatusMessageType = ViewModels.StatusMessageType.Warning;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRole(string memberId, string memberName, string newRole)
        {
            // Confirm that the requested role exists
            if (!GroupRole.Exists(newRole))
                return new JsonResult(new
                {
                    success = false,
                    message = "The requested role does not exist.",
                    type = ViewModels.StatusMessageType.Error,
                });

            // Confirm that the user exists
            var member = await _db.Members
                .SingleOrDefaultAsync(gu => gu.Id == memberId);
            if (member == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Member <b>{memberName}</b> not found.",
                    type = ViewModels.StatusMessageType.Error
                });
            }

            // Verify that the member is not the last Administrator and is being demoted
            if (member.Role == GroupRole.Administrator && newRole != GroupRole.Administrator &&
                !await _db.Members.AnyAsync(m =>
                    m.Role == GroupRole.Administrator && m.Id != member.Id))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"<b>{memberName}</b> is the only administrator on this group. " +
                        $"Please promote another administrator before demoting this member.",
                    type = ViewModels.StatusMessageType.Error
                });
            }

            // Change the member role
            member.Role = newRole;
            await _db.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}