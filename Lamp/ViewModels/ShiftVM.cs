using Lamp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.ViewModels
{
    public class ShiftVM
    {
        public int ShiftId { get; set; }
        public int LocationId { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number must be greater than 0.")]
        [Display(Name = "Participants needed")]
        public int MinParticipants { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number must be greater than 0.")]
        [Display(Name = "Participant limit")]
        public int MaxParticipants { get; set; }
        [DataType(DataType.MultilineText)]
        public string Instructions { get; set; }
        public bool Enabled { get; set; }

        public bool CreateCopies { get; set; }
        public bool CopyMonday { get; set; }
        public bool CopyTuesday { get; set; }
        public bool CopyWednesday { get; set; }
        public bool CopyThursday { get; set; }
        public bool CopyFriday { get; set; }
        public bool CopySaturday { get; set; }
        public bool CopySunday { get; set; }
        [Required]
        public DateTime CopyUntil { get; set; }
    }
}