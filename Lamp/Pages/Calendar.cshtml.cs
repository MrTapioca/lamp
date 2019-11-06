using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [Authorize]
    public class CalendarModel : PageModel
    {
        // Services
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Constructor
        public CalendarModel(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public class CreateShiftInputModel
        {
            //public string GroupId { get; set; }
            [Required]
            public int LocationId { get; set; }
            [Required]
            public DateTime Start { get; set; }
            [Required]
            public DateTime End { get; set; }
            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Number must be greater than 0.")]
            [Display(Description = "Required Slots")]
            public int MinSlots { get; set; }
            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Number must be greater than 0.")]
            [Display(Description = "Max Slots")]
            public int MaxSlots { get; set; }
            [DataType(DataType.MultilineText)]
            public string Instructions { get; set; }
        }

        [BindProperty]
        public CreateShiftInputModel CreateShiftInput { get; set; }

        public List<SelectListItem> GroupList { get; set; }
        public List<SelectListItem> LocationList { get; set; }
        public string SelectedGroupId { get; set; }
        public bool SelectedGroupManager { get; set; }
        public int SelectedLcationId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            GroupList = new List<SelectListItem>();
            LocationList = new List<SelectListItem>();

            var user = await _userManager.GetUserAsync(User);

            // Verify the user exists
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return NotFound($"The user was not found. Please reload the page.");
            }

            // Load the user's joined groups
            var groups = await _db.Groups
                .Where(g => g.Members.Any(m => m.User == user && m.Approved))
                .OrderBy(g => g.Name)
                .ToArrayAsync();

            if (groups.Length == 0)
                return Page();

            foreach (var group in groups)
            {
                GroupList.Add(new SelectListItem(group.Name, group.Id));
            }

            // Load the selected group's locations
            SelectedGroupId = Request.Cookies["selectedGroup"];

            if (string.IsNullOrEmpty(SelectedGroupId) ||
                !GroupList.Any(i => i.Value == SelectedGroupId))
            {
                SelectedGroupId = GroupList[0].Value;
            }

            Member member = await _db.Members
                .SingleAsync(m => m.UserId == user.Id && m.GroupId == SelectedGroupId);
            SelectedGroupManager = member.CanManageShifts;

            var locations = await _db.Locations
                .Where(l => l.Group.Id == SelectedGroupId)
                .OrderBy(l => l.Name)
                .ToArrayAsync();

            if (locations.Length == 0)
                return Page();

            foreach (var location in locations)
            {
                LocationList.Add(new SelectListItem(location.Name, location.Id.ToString()));
            }

            int.TryParse(Request.Cookies["selectedLocation"], out int selectedId);
            SelectedLcationId = selectedId;

            if (!locations.Any(l => l.Id == SelectedLcationId))
            {
                SelectedLcationId = int.Parse(LocationList[0].Value);
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Validate input
            if (!ModelState.IsValid)
                return NotFound("Invalid input.");

            // Load user and its groups and locations
            var user = await _userManager.GetUserAsync(User);

            await _db.Members.Where(m => m.User == user)
                .Include(m => m.Group)
                    .ThenInclude(group => group.Locations)
                .LoadAsync();

            // Load the member data of the group with that location
            Member member = user.Members.FirstOrDefault(m =>
                m.Group.Locations.Any(l =>
                    l.Id == CreateShiftInput.LocationId));

            // Load the location
            Location location = member.Group.Locations.First(l => l.Id == CreateShiftInput.LocationId);

            // Confirm the location exists
            if (member == null)
                return NotFound($"The specified location id {CreateShiftInput.LocationId} was not found.");

            // Confirm the user is approved and can create shifts
            if (!member.Approved || !GroupRole.CanCreateShifts(member.Role))
                return NotFound("You do not have access to create shifts at this location.");

            // Create the shift
            Shift shift = new Shift()
            {
                Location = location,
                Start = CreateShiftInput.Start,
                End = CreateShiftInput.End,
                MinParticipants = CreateShiftInput.MinSlots,
                MaxParticipants = CreateShiftInput.MaxSlots,
                Instructions = CreateShiftInput.Instructions,
                Enabled = true
            };
            _db.Shifts.Add(shift);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}