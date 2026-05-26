namespace CenterManagement.Models.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int TimeSlotId { get; set; }
        public string TimeSlotName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public DateOnly ScheduleDate { get; set; }
    }

    public class CreateScheduleDto
    {
        public int ClassId { get; set; }
        public int RoomId { get; set; }
        public int TimeSlotId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateOnly ScheduleDate { get; set; }
    }

    public class TimeSlotDto
    {
        public int Id { get; set; }
        public string SlotName { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
