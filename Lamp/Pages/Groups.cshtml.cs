using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lamp.Data;
using Lamp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Lamp.ViewModels;
using Lamp.Interfaces;

namespace Lamp.Pages
{
    [Authorize]
    public class GroupsModel : PageModel
    {
        // Services
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger _logger;

        // Constructor
        public GroupsModel(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IKeyGenerator keyGenerator,
            ILogger<GroupsModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _keyGenerator = keyGenerator;
            _logger = logger;
        }

        // Data display
        public class JoinedGroup
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool CanManage { get; set; }
        }

        public List<JoinedGroup> JoinedGroups { get; set; }

        [TempData]
        public string Message { get; set; }
        [TempData]
        public string MessageType { get; set; }

        // Input data
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Group code")]
            public string JoinGroupCode { get; set; }

            [Required]
            [Display(Name = "Group name")]
            public string CreateGroupName { get; set; }

            [Required]
            public string LeaveGroupCode { get; set; }
        }

        // GET
        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // Load list of joined groups
            JoinedGroups = new List<JoinedGroup>();

            await _db.Entry(user).Collection(x => x.Members)
                .Query()
                .Include(g => g.Group)
                .OrderBy(g => g.Group.Name)
                .LoadAsync();

            foreach (Member member in user.Members)
            {
                var group = new JoinedGroup
                {
                    Id = member.GroupId,
                    Name = member.Group.Name,
                    CanManage = false
                };

                if (member.Role == GroupRole.Administrator ||
                    member.Role == GroupRole.Assistant)
                {
                    group.CanManage = true;
                }

                JoinedGroups.Add(group);
            }

            return Page();
        }

        // POST Leave
        public async Task<IActionResult> OnPostLeave()
        {
            if (!ModelState.IsValid)
                return RedirectToPage();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // Confirm that the group exists
            var group = await _db.Groups
                .Include(g => g.Members)
                .SingleOrDefaultAsync(g => g.Id == Input.LeaveGroupCode);

            if (group == null)
            {
                Message = string.Format("The group code <b>{0}</b> is invalid. " +
                    "The group may have been deleted.", Input.LeaveGroupCode);
                MessageType = StatusMessageType.Error;
                return RedirectToPage();
            }

            // Confirm that the user belongs to the group
            if (!group.Members.Any(m => m.UserId == user.Id))
            {   
                Message = string.Format("You are not a member of the <b>{0}</b> group. " +
                    "An administrator may have already removed you.", group.Name);
                MessageType = StatusMessageType.Error;
                return RedirectToPage();
            }

            var userLink = group.Members.Single(m => m.UserId == user.Id);

            // Confirm that the user is not the only admin on the group
            if (userLink.Role == GroupRole.Administrator &&
                !group.Members.Any(m => m.UserId != user.Id && m.Role == GroupRole.Administrator))
            {
                Message = string.Format("You cannot leave the <b>{0}</b> group because you are the only administrator. " +
                    "You must assign another administrator before leaving the group.", group.Name);
                MessageType = StatusMessageType.Error;
                return RedirectToPage();
            }

            // Remove the user from the group
            //group.Members.Remove(userLink);
            _db.Members.Remove(userLink);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Error removing user from group. " + ex.ToString());

                Message = "<b>Error!</b> Could not remove this user from the group. Please contact Support for help.";
                MessageType = StatusMessageType.Error;
                return RedirectToPage();
            }

            // If we made it this far, the operation was successful
            Message = string.Format("You have successfully left the <b>{0}</b> group. ", group.Name);
            MessageType = StatusMessageType.Success;
            return RedirectToPage();
        }

        // POST Join
        public async Task<IActionResult> OnPostJoin()
        {
            if (!ModelState.IsValid)
                return RedirectToPage();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // Confirm that the group exists
            var group = await _db.Groups
                .Include(g => g.Members)
                .SingleOrDefaultAsync(g => g.Id == Input.JoinGroupCode);

            if (group == null)
            {
                Message = string.Format("The group code <b>{0}</b> is invalid.", Input.JoinGroupCode);
                MessageType = StatusMessageType.Error;
                return RedirectToPage();
            }

            // Check if the user already belongs to the group
            if (group.Members.Any(m => m.UserId == user.Id))
            {
                Message = string.Format("You have already joined the <b>{0}</b> group. " +
                    "If you are waiting to be approved, please check with a group administrator.", group.Name);
                MessageType = StatusMessageType.Warning;
                return RedirectToPage();
            }

            // Add the user to the group
            group.Members.Add(new Member
            {
                Group = group,
                User = user,
                Role = GroupRole.Publisher,
                Approved = false
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Error adding user to group. " + ex.ToString());

                Message = "<b>Error!</b> Could not add this user to the group. Please contact Support for help.";
                MessageType = StatusMessageType.Error;
                return RedirectToPage();
            }

            // If we made it this far, the operation was successful
            Message = string.Format("<b>Success!</b> You have joined the <b>{0}</b> group. " +
                "Once you are approved by an administrator, you will be able to view available shifts and sign up.", group.Name);
            MessageType = StatusMessageType.Success;
            return RedirectToPage();
        }

        // POST Create
        public async Task<IActionResult> OnPostCreate()
        {
            if (!ModelState.IsValid)
                return RedirectToPage();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // Attempt to create a new, unique group Id
            string newGroupId = "";

            for (int i = 1; i <= 10; i++)
            {
                newGroupId = _keyGenerator.GenerateKey();
                _logger.LogInformation("New group key generated: " + newGroupId);

                if (!_db.Groups.Any(x => x.Id == newGroupId))
                {
                    break;
                }
                else
                {
                    _logger.LogWarning("Duplicate group key found: " + newGroupId);

                    if (i == 10)
                    {
                        throw new ApplicationException("Error while attempting to create a new group.");
                    }
                }
            }

            // Create the new group
            var group = new Group {
                Id = newGroupId,
                Name = Input.CreateGroupName.Trim(),
                UsersCanScheduleOthers = true
            };
            group.Members = new Member[] {
                new Member {
                    User = user,
                    Group = group,
                    Role = GroupRole.Administrator,
                    Approved = true
                }
            };

            _db.Groups.Add(group);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Error creating new group. " + ex.ToString());

                Message = "<b>Error!</b> Could not create the new group. Please contact Support for help.";
                MessageType = StatusMessageType.Error;
                return RedirectToPage();
            }

            // If we made it this far, the operation was successful
            return RedirectToPage("/Group", new { groupId = newGroupId });
        }
    }
}