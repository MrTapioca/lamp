using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Data
{
    public class Shift
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
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

        public ICollection<Participant> Participants { get; set; }

        public bool AcceptingParticipants => (Participants?.Count ?? 0) < MaxParticipants;

        public ShiftStatus GetShiftStatus()
        {
            if (!Enabled) return ShiftStatus.Disabled;

            switch (Participants.Count)
            {
                case 0:
                    return ShiftStatus.Empty;
                case var count when (count < MinParticipants):
                    return ShiftStatus.Incomplete;
                case var count when (count < MaxParticipants):
                    return ShiftStatus.Complete;
            }

            return ShiftStatus.Full;
        }
    }
}

public enum ShiftStatus
{
    Disabled,
    Empty,
    Incomplete,
    Complete,
    Full
}
// Status: Disabled (black), Empty (white), Incomplete (red), Complete (yellow), Full (green)