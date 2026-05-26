using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Teacher : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // Chuyên môn & kinh nghiệm
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }         // Bằng cấp (Cử nhân, Thạc sĩ, ...)
        public int? YearsOfExperience { get; set; }        // Số năm kinh nghiệm
        public string? Certificates { get; set; }          // Chứng chỉ (IELTS, TOEFL, CELTA...)
        public string? Biography { get; set; }             // Tiểu sử ngắn

        // Thông tin cá nhân
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }                // Nam / Nữ / Khác
        public string? NationalId { get; set; }            // CCCD/CMND
        public string? Address { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }

        // Tài chính
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }                 // Mã số thuế

        // Hợp đồng
        public string? ContractType { get; set; }          // Cộng tác viên / Hợp đồng lao động
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }

        public User User { get; set; } = null!;
        public ICollection<TeacherClassRegistration> TeacherClassRegistrations { get; set; } = new List<TeacherClassRegistration>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    }
}
