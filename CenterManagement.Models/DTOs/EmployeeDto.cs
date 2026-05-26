namespace CenterManagement.Models.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? ContractType { get; set; }
        public DateOnly? HireDate { get; set; }
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }
        public decimal? Salary { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Education { get; set; }
        public string? Major { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? EmergencyRelationship { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int RoleId { get; set; } = 3;       // Mặc định Staff
        public int? BranchId { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? ContractType { get; set; }
        public DateOnly? HireDate { get; set; }
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }
        public decimal? Salary { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Education { get; set; }
        public string? Major { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? EmergencyRelationship { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int? BranchId { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? ContractType { get; set; }
        public DateOnly? HireDate { get; set; }
        public DateOnly? ContractStartDate { get; set; }
        public DateOnly? ContractEndDate { get; set; }
        public decimal? Salary { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Education { get; set; }
        public string? Major { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? EmergencyRelationship { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? TaxId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
