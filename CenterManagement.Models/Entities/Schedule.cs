using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Schedule : BaseEntity
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int RoomId { get; set; }
        public int TimeSlotId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateOnly ScheduleDate { get; set; }

        public Class Class { get; set; } = null!;
        public Room Room { get; set; } = null!;
        public TimeSlot TimeSlot { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
