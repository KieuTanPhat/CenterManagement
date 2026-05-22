using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class ClassStudent
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int StudentId { get; set; }

    public int? RegistrationId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Registration? Registration { get; set; }

    public virtual Student Student { get; set; } = null!;
}
