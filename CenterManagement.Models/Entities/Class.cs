using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Class
{
    public int Id { get; set; }

    public string ClassName { get; set; } = null!;

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public int BranchId { get; set; }

    public int MaxStudents { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual Course Course { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
