using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lamp.Data;
using Lamp.Interfaces;
using Lamp.Services;
using Lamp.ViewModels;

namespace Lamp.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class ShiftsController : Controller
    {
        private readonly ShiftService _shiftService;
        private readonly MemberService _memberService;
        private readonly UserService _userService;
        private readonly LocationService _locationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public ShiftsController(MemberService memberService,
                                ShiftService shiftService,
                                UserService userService,
                                LocationService locationService,
                                UserManager<ApplicationUser> userManager,
                                ApplicationDbContext db)
        {
            _shiftService = shiftService;
            _userService = userService;
            _memberService = memberService;
            _userManager = userManager;
            _locationService = locationService;
            _db = db;
        }

        [HttpGet("{locationId}")]
        public async Task<JsonResult> Events(int locationId, DateTime start, DateTime end)
        {
            // Set timezone
            //var timezone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);

            // Check that user has access to location
            var user = await _userManager.GetUserAsync(User);
            var approvedLocation = await _db.Locations
                .Where(l => l.Id == locationId)
                .AnyAsync(l => _db.Members
                    .Where(m => m.UserId == user.Id && m.Approved)
                    .Any(m => m.GroupId == l.GroupId));

            if (!approvedLocation)
                return Json(new { });

            // Load member information
            var member = await _db.Members
                .Where(m => m.UserId == user.Id && m.Approved)
                .Where(m => m.Group.Locations.Any(l => l.Id == locationId))
                .SingleAsync();

            // Load shift data
            var shifts = await _db.Shifts
                .Where(s => s.LocationId == locationId)
                .Where(s => s.Start >= start && s.End <= end && (member.CanManageShifts ? true : s.Enabled))
                .Include(s => s.Participants)
                .Select(s => new
                {
                    s.Start,
                    s.End,
                    Status = s.GetShiftStatus()
                    //ClassName = "event-incomplete"
                })
                .ToArrayAsync();

            return Json(shifts);
        }

        [HttpPost()]
        public async Task<IActionResult> Create(ShiftVM shiftModel)
        {
            // Check that the data is present
            if (!ModelState.IsValid || shiftModel.LocationId == 0)
                return BadRequest();

            // Check that the location exists
            Location location = await _locationService.GetLocation(shiftModel.LocationId);
            if (location == null)
                return BadRequest();

            // Check that the user has management access
            string userId = _userManager.GetUserId(User);
            Member member = await _memberService.GetMember(location.GroupId, userId);
            if (member == null || !member.Approved || !member.CanManageShifts)
                return BadRequest();

            // Make sure the system is not overloaded with shifts
            if (shiftModel.CreateCopies &&
                shiftModel.CopyUntil.Date >= shiftModel.Start.Date.AddYears(1))
                return BadRequest();

            var newShifts = new List<Shift>();

            newShifts.Add(new Shift()
            {
                LocationId = shiftModel.LocationId,
                Start = shiftModel.Start,
                End = shiftModel.End,
                MinParticipants = shiftModel.MinParticipants,
                MaxParticipants = shiftModel.MaxParticipants,
                Instructions = shiftModel.Instructions,
                Enabled = shiftModel.Enabled
            });

            if (shiftModel.CreateCopies)
            {
                var totalDays = shiftModel.CopyUntil.Date - shiftModel.Start.Date;

                for (int day = 1; day <= totalDays.Days; day++)
                {
                    DayOfWeek newDayOfWeek = shiftModel.Start.AddDays(day).DayOfWeek;

                    if ((newDayOfWeek == DayOfWeek.Monday && shiftModel.CopyMonday) ||
                        (newDayOfWeek == DayOfWeek.Tuesday && shiftModel.CopyTuesday) ||
                        (newDayOfWeek == DayOfWeek.Wednesday && shiftModel.CopyWednesday) ||
                        (newDayOfWeek == DayOfWeek.Thursday && shiftModel.CopyThursday) ||
                        (newDayOfWeek == DayOfWeek.Friday && shiftModel.CopyFriday) ||
                        (newDayOfWeek == DayOfWeek.Saturday && shiftModel.CopySaturday) ||
                        (newDayOfWeek == DayOfWeek.Sunday && shiftModel.CopySunday))
                    {
                        var newStartDate = shiftModel.Start.AddDays(day);
                        var newEndDate = shiftModel.End.AddDays(day);

                        newShifts.Add(new Shift()
                        {
                            LocationId = shiftModel.LocationId,
                            Start = newStartDate,
                            End = newEndDate,
                            MinParticipants = shiftModel.MinParticipants,
                            MaxParticipants = shiftModel.MaxParticipants,
                            Instructions = shiftModel.Instructions,
                            Enabled = shiftModel.Enabled
                        });
                    }
                }
            }

            await _shiftService.CreateShifts(newShifts);

            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> Edit(EditShiftVM shiftModel)
        {
            // Check that the data is present
            if (!ModelState.IsValid || shiftModel.ShiftId == 0)
                return BadRequest();

            // Check that the shift exists
            Shift shift = await _shiftService.GetShift(shiftModel.ShiftId);
            if (shift == null)
                return BadRequest();

            // Check that the user has management access
            string userId = _userManager.GetUserId(User);
            Member member = await _memberService.GetMember(shift.Location.GroupId, userId);
            if (member == null || !member.Approved || !member.CanManageShifts)
                return BadRequest();

            shift.Start = shiftModel.Start;
            shift.End = shiftModel.End;
            shift.MinParticipants = shiftModel.MinParticipants;
            shift.MaxParticipants = shiftModel.MaxParticipants;
            shift.Instructions = shiftModel.Instructions;

            await _shiftService.EditShift(shift);

            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> Enable(int shiftId, bool enable)
        {
            // Get the shift
            Shift shift = await _shiftService.GetShift(shiftId);
            if (shift == null)
                return BadRequest();

            // Check that the user has management access
            string userId = _userManager.GetUserId(User);
            Member member = await _memberService.GetMember(shift.Location.GroupId, userId);
            if (member == null || !member.Approved || !member.CanManageShifts)
                return BadRequest();

            shift.Enabled = enable;
            await _shiftService.EditShift(shift);

            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> Delete(int shiftId)
        {
            // Get the shift
            Shift shift = await _shiftService.GetShift(shiftId);
            if (shift == null)
                return BadRequest();

            // Check that the user has management access
            string userId = _userManager.GetUserId(User);
            Member member = await _memberService.GetMember(shift.Location.GroupId, userId);
            if (member == null || !member.Approved || !member.CanManageShifts)
                return BadRequest();

            await _shiftService.DeleteShift(shift);

            return Ok();
        }

        [HttpGet("{locationId}/{selectedDate}")]
        [HttpGet("{shiftId}")]
        public async Task<IActionResult> View(int? locationId, DateTime selectedDate, int? shiftId)
        {
            string userId = _userManager.GetUserId(User);
            Member member = null;
            List<Shift> shifts = new List<Shift>();

            // Load shifts in requested location if there is access
            if (locationId.HasValue)
            {
                if (await _userService.HasLocationAccess(userId, locationId.Value))
                {
                    Location location = await _locationService.GetLocation(locationId.Value);
                    member = await _memberService.GetMember(location.GroupId, userId);
                    shifts = await _shiftService.GetShifts(location.Id, selectedDate, member.CanManageShifts);
                }
                else
                {
                    return BadRequest();
                }
            }

            // Load requested shift if there is access
            Shift singleShift;
            if (shiftId.HasValue)
            {
                if (await _userService.HasShiftAccess(userId, shiftId.Value))
                {
                    singleShift = await _shiftService.GetShift(shiftId.Value);
                    member = await _memberService.GetMember(singleShift.Location.GroupId, userId);
                    if (!singleShift.Enabled && !member.CanManageShifts)
                    {
                        return BadRequest();
                    }
                    else
                    {
                        shifts.Add(singleShift);
                    }
                }
                else
                {
                    return BadRequest();
                }
            }

            // Sort participants
            foreach (Shift shift in shifts)
            {
                shift.Participants = shift.Participants.OrderBy(p => p.Member.User.FullName).ToList();
            }

            ViewBag.ShowManageButton = member.CanManageShifts;
            return PartialView("_ShiftsPartial", shifts);
        }

        [HttpPost()]
        public async Task<IActionResult> AddParticipant(int shiftId, string memberId)
        {
            // Shift exists?
            // Member exists?
            if (!(await _shiftService.Exists(shiftId)) ||
                !(await _memberService.Exists(memberId)))
                return BadRequest();

            Shift shift = await _shiftService.GetShift(shiftId);
            Member member = await _memberService.GetMember(memberId);

            // User has access to shift?
            string userId = _userManager.GetUserId(User);
            if (!(await _userService.HasShiftAccess(userId, shiftId)))
                return BadRequest();

            // Can member participate in shift?
            if (!(await _userService.HasShiftAccess(member.UserId, shiftId)))
                return BadRequest();

            // If member is not self, is there permission to add?
            if (member.UserId != userId && !member.Group.UsersCanScheduleOthers)
                return BadRequest();

            // Can shift accept another participant?
            if (!shift.AcceptingParticipants)
                return BadRequest();

            // Check if member is already a participant
            if (await _shiftService.HasParticipant(shiftId, memberId))
                return BadRequest();

            await _shiftService.AddParticipant(shiftId, memberId);

            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> RemoveParticipant(int participantId)
        {
            // Get participant
            Participant participant = await _shiftService.GetParticipant(participantId);
            if (participant == null)
                return BadRequest();

            // User has access to shift?
            string userId = _userManager.GetUserId(User);
            if (!(await _userService.HasShiftAccess(userId, participant.ShiftId)))
                return BadRequest();

            // If member is not self, is there permission to remove?
            if (participant.Member.UserId != userId && !participant.Member.Group.UsersCanScheduleOthers)
                return BadRequest();

            await _shiftService.RemoveParticipant(participantId);

            return Ok();
        }
    }
}