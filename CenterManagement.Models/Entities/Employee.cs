using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Employee : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BranchId { get; set; }                 // Chi nhánh làm việc

        // Công việc
        public string? Position { get; set; }              // Chức vụ
        public string? Department { get; set; }            // Phòng ban
        public string? ContractType { get; set; }          // Loại hợp đồng (thử việc, chính thức, thời vụ)
        public DateOnly? HireDate { get; set; }            // Ngày vào làm
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }
        public decimal? Salary { get; set; }               // Lương cơ bản

        // Thông tin cá nhân
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }            // CCCD/CMND
        public string? Address { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }

        // Học vấn
        public string? Education { get; set; }             // Trình độ học vấn
        public string? Major { get; set; }                 // Chuyên ngành

        // Liên hệ khẩn cấp
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? EmergencyRelationship { get; set; } // Mối quan hệ với người liên hệ

        // Tài chính
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }

        public User User { get; set; } = null!;
        public Branch? Branch { get; set; }
    }
}
