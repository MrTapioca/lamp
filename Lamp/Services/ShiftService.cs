using Microsoft.EntityFrameworkCore;
using Lamp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Services
{
    public class ShiftService
    {
        private readonly ApplicationDbContext _db;

        public ShiftService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Exists(int shiftId)
        {
            return await _db.Shifts
                .AnyAsync(s => s.Id == shiftId);
        }

        public async Task CreateShift(Shift newShift)
        {
            _db.Shifts.Add(newShift);
            await _db.SaveChangesAsync();
        }

        public async Task CreateShifts(IEnumerable<Shift> newShifts)
        {
            _db.Shifts.AddRange(newShifts);
            await _db.SaveChangesAsync();
        }

        public async Task EditShift(Shift modifiedShift)
        {
            _db.Update(modifiedShift);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteShift(Shift shift)
        {
            _db.Shifts.Remove(shift);
            await _db.SaveChangesAsync();
        }

        public async Task<Shift> GetShift(int shiftId)
        {
            return await _db.Shifts
                .Include(s => s.Location)
                    .ThenInclude(l => l.Group)
                .Include(s => s.Participants)
                    .ThenInclude((Participant p) => p.Member)
                        .ThenInclude(m => m.User)
                .SingleOrDefaultAsync(s => s.Id == shiftId);
        }

        public async Task<List<Shift>> GetShifts(int locationId, DateTime date, bool includeDisabled)
        {
            return await _db.Shifts
                .Where(s => s.LocationId == locationId)
                .Where(s => s.Start.Date == date.Date)
                .Where(s => includeDisabled ? true : s.Enabled)
                .Include(s => s.Location)
                    .ThenInclude(l => l.Group)
                .Include(s => s.Participants)
                    .ThenInclude((Participant p) => p.Member)
                        .ThenInclude(m => m.User)
                .OrderBy(s => s.Start)
                .ToListAsync();
        }

        public async Task<List<Participant>> GetParticipants(int shiftId)
        {
            return await _db.Participants
                .Where(p => p.ShiftId == shiftId)
                .ToListAsync();
        }

        public async Task<Participant> GetParticipant(int participantId)
        {
            return await _db.Participants
                .Where(p => p.Id == participantId)
                .Include(p => p.Shift)
                .Include(p => p.Member)
                    .ThenInclude(m => m.Group)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> HasParticipant(int shiftId, string memberId)
        {
            return await _db.Participants
                .AnyAsync(p => p.ShiftId == shiftId && p.MemberId == memberId);
        }

        public async Task AddParticipant(int shiftId, string memberId)
        {
            _db.Participants.Add(new Participant
            {
                ShiftId = shiftId,
                MemberId = memberId
            });
            await _db.SaveChangesAsync();
        }

        public async Task RemoveParticipant(int participantId)
        {
            Participant participant = await _db.Participants
                .FirstAsync(p => p.Id == participantId);
            _db.Participants.Remove(participant);
            await _db.SaveChangesAsync();
        }
    }
}
