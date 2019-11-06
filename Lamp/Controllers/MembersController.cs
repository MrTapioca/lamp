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

namespace Lamp.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class MembersController : Controller
    {
        private readonly MemberService _memberService;
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly ApplicationDbContext _db;

        public MembersController(MemberService memberService,
                                UserManager<ApplicationUser> userManager)
        {
            _memberService = memberService;
            _userManager = userManager;
            //_db = db;
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> Query(string groupId, string query)
        {
            // Confirm the user belongs to the group and is approved
            ApplicationUser user = await _userManager.GetUserAsync(User);

            // Check for proper access and that query is at least 3 characters
            if (!(await _memberService.IsMember(groupId, user.Id, true)) ||
                (!string.IsNullOrWhiteSpace(query) && query.Length < 3))
            {
                return BadRequest();
            }

            // Create list of all users on the group, with this user on top
            List<Member> members = await _memberService.FindMembersByName(groupId, query, true, 20);

            return Json(members.Select(m => new
            {
                id = m.Id,
                name = m.User.FullName,
                initials = m.User.FirstName.Substring(0, 1) + m.User.LastName.Substring(0, 1)
            }));
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> CurrentUser(string groupId)
        {
            // Confirm the user belongs to the group and is approved
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if (!(await _memberService.IsMember(groupId, user.Id, true)))
            {
                return BadRequest();
            }

            Member member = await _memberService.GetMember(groupId, user.Id);

            return Json(new
            {
                id = member.Id,
                name = member.User.FullName,
                initials = member.User.FirstName.Substring(0, 1) + member.User.LastName.Substring(0, 1)
            });
        }
    }
}