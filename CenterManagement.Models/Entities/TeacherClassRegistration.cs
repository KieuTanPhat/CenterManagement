using System;

namespace CenterManagement.Models.Entities
{
    public class TeacherClassRegistration : BaseEntity
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int ClassId { get; set; }
        public bool IsMainTeacher { get; set; } = true;

        public Teacher Teacher { get; set; } = null!;
        public Class Class { get; set; } = null!;
    }
}
