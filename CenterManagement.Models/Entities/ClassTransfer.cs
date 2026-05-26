using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class ClassTransfer : BaseEntity
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int FromClassId { get; set; }
        public int ToClassId { get; set; }
        public DateOnly TransferDate { get; set; }
        public string? Reason { get; set; }

        public Student Student { get; set; } = null!;
        public Class FromClass { get; set; } = null!;
        public Class ToClass { get; set; } = null!;

    }
}
