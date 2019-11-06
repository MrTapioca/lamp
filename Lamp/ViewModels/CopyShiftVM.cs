using Lamp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.ViewModels
{
    public class CopyShiftVM
    {
        public int ShiftId { get; set; }
        public DateTime End { get; set; }
        public bool Enabled { get; set; }

        public bool CopyMonday { get; set; }
        public bool CopyTuesday { get; set; }
        public bool CopyWednesday { get; set; }
        public bool CopyThursday { get; set; }
        public bool CopyFriday { get; set; }
        public bool CopySaturday { get; set; }
        public bool CopySunday { get; set; }
        [Required]
        public DateTime CopyStart { get; set; }
        [Required]
        public DateTime CopyThrough { get; set; }
    }
}