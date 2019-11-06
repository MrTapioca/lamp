using Microsoft.EntityFrameworkCore;
using Lamp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Services
{
    public class LocationService
    {
        private readonly ApplicationDbContext _db;

        public LocationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Exists(int locationId)
        {
            return await _db.Locations
                .AnyAsync(l => l.Id == locationId);
        }

        public async Task<Location> GetLocation(int locationId)
        {
            return await _db.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId);
        }
    }
}
