using Microsoft.EntityFrameworkCore;
using Lamp.Data;
using Lamp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Services
{
    public class MemberService
    {
        private readonly ApplicationDbContext _db;

        public MemberService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Exists(string memberId)
        {
            return await _db.Members
                .AnyAsync(m => m.Id == memberId);
        }

        public async Task<bool> IsMember(string groupId, string userId, bool onlyApproved)
        {
            return await _db.Members
                .Where(m => m.UserId == userId && m.GroupId == groupId)
                .AnyAsync(m => onlyApproved ? m.Approved : true);
        }

        public async Task<Member> GetMember(string groupId, string userId)
        {
            return await _db.Members
                .Where(m => m.GroupId == groupId)
                .Where(m => m.UserId == userId)
                .Include(m => m.Group)
                .Include(m => m.User)
                .SingleOrDefaultAsync();
        }

        public async Task<Member> GetMember(string memberId)
        {
            return await _db.Members
                .Include(m => m.Group)
                .Include(m => m.User)
                .SingleOrDefaultAsync(m => m.Id == memberId);
        }

        public async Task<List<Member>> GetMembers(string groupId, bool onlyApproved, int maxCount)
        {
            return await _db.Members
                .Where(m => m.GroupId == groupId)
                .Where(m => onlyApproved ? m.Approved : true)
                .Include(m => m.Group)
                .Include(m => m.User)
                .OrderBy(m => m.User.FullName)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<List<Member>> FindMembersByName(string groupId, string query, bool onlyApproved, int maxCount)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Value must not be null nor empty.", nameof(query));

            return await _db.Members
                .Where(m => m.GroupId == groupId)
                .Where(m => m.User.FirstName.Contains(query) || m.User.LastName.Contains(query))
                .Where(m => onlyApproved ? m.Approved : true)
                .Include(m => m.Group)
                .Include(m => m.User)
                .OrderBy(m => m.User.FullName)
                .Take(maxCount)
                .ToListAsync();
        }
    }
}