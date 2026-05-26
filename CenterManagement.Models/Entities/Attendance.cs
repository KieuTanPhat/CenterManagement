using CenterManagement.Models.Enums;
using System;

namespace CenterManagement.Models.Entities
{
    public class Attendance : BaseEntity
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Note { get; set; }

        public Schedule Schedule { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
