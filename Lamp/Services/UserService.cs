using Microsoft.EntityFrameworkCore;
using Lamp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _db;

        public UserService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> HasShiftAccess(string userId, int shiftId)
        {
            return await _db.Shifts
                .Where(s => s.Id == shiftId)
                .Where(s => s.Location.Group.Members
                    .Any(m => m.UserId == userId && m.Approved))
                .AnyAsync();
        }

        public async Task<bool> HasLocationAccess(string userId, int locationId)
        {
            return await _db.Locations
                .Where(l => l.Id == locationId)
                .Where(l => l.Group.Members
                    .Any(m => m.UserId == userId && m.Approved))
                .AnyAsync();
        }
    }
}
