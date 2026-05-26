using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class TimeSlot : BaseEntity
    {
        public int Id { get; set; }
        public string SlotName { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
