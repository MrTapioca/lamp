using System.Collections.Generic;

namespace Lamp.Data
{
    public class Location
    {
        public int Id { get; set; }
        public string GroupId { get; set; }
        public Group Group { get; set; }
        public string Name { get; set; }

        public ICollection<Shift> Shifts { get; set; }
    }
}