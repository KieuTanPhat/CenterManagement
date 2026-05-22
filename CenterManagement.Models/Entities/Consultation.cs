using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Consultation
{
    public int Id { get; set; }

    public int LeadId { get; set; }

    public int StaffId { get; set; }

    public DateTime ConsultDate { get; set; }

    public string? Content { get; set; }

    public int? RecommendedCourseId { get; set; }

    public string Result { get; set; } = null!;

    public DateTime? FollowUpDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Lead Lead { get; set; } = null!;

    public virtual Course? RecommendedCourse { get; set; }

    public virtual User Staff { get; set; } = null!;
}
