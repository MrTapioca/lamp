using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Data
{
    public class Participant
    {
        public int Id { get; set; }
        public int ShiftId { get; set; }
        public string MemberId { get; set; }

        public Shift Shift { get; set; }
        public Member Member { get; set; }
    }
}