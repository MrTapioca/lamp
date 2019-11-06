using System.Collections.Generic;

namespace Lamp.Data
{
    public class Group
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool UsersCanScheduleOthers { get; set; }

        public ICollection<Member> Members { get; set; }
        public ICollection<Location> Locations { get; set; }

        //public ICollection<Shift> Shifts { get; set; }
    }
}