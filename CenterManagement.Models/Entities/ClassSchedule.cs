using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class ClassSchedule
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }

    public virtual Class Class { get; set; } = null!;
}
