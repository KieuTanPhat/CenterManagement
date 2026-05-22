using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Registration
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int BranchId { get; set; }

    public int StaffId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual Course Course { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
