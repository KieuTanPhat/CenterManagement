namespace CenterManagement.Models.DTOs
{
    public class TeacherDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Certificates { get; set; }
        public string? Biography { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }
        public string? ContractType { get; set; }
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }
        public bool IsActive { get; set; }
        public int ActiveClassCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTeacherDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Certificates { get; set; }
        public string? Biography { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }
        public string? ContractType { get; set; }
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }
    }

    public class UpdateTeacherDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Certificates { get; set; }
        public string? Biography { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }
        public string? ContractType { get; set; }
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class TeacherScheduleDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string TimeSlotName { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public DateOnly ScheduleDate { get; set; }
    }
}
