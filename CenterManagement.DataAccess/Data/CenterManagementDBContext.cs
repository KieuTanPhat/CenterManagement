using Microsoft.EntityFrameworkCore;
using CenterManagement.Models.Entities;
using CenterManagement.Models.Enums;

namespace CenterManagement.DataAccess.Data
{
    public class CenterManagementDBContext : DbContext
    {
        public CenterManagementDBContext(DbContextOptions<CenterManagementDBContext> options)
            : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<TeacherClassRegistration> TeacherClassRegistrations { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ClassTransfer> ClassTransfers { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(e =>
            {
                e.HasKey(r => r.Id);
                e.HasIndex(r => r.RoleName).IsUnique();
                e.Property(r => r.RoleName).HasMaxLength(50).IsRequired();
                e.Property(r => r.Description).HasMaxLength(200);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.UserName).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.PhoneNumber).IsUnique();
                e.Property(u => u.UserName).HasMaxLength(100).IsRequired();
                e.Property(u => u.Email).HasMaxLength(200).IsRequired();
                e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
                e.Property(u => u.PhoneNumber).HasMaxLength(20);
                e.HasOne(u => u.Role)
                 .WithMany(r => r.Users)
                 .HasForeignKey(u => u.RoleId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.HasKey(rt => rt.Id);
                e.HasIndex(rt => rt.JwtId).IsUnique();
                e.HasIndex(rt => rt.Token).IsUnique();
                e.HasOne(rt => rt.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(rt => rt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Employee>(e =>
            {
                e.HasKey(em => em.Id);
                e.HasIndex(em => em.UserId).IsUnique();
                e.Property(em => em.Position).HasMaxLength(100);
                e.HasOne(em => em.User)
                 .WithOne(u => u.Employee)
                 .HasForeignKey<Employee>(em => em.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Teacher>(e =>
            {
                e.HasKey(t => t.Id);
                e.HasIndex(t => t.UserId).IsUnique();
                e.Property(t => t.Specialization).HasMaxLength(200);
                e.HasOne(t => t.User)
                 .WithOne(u => u.Teacher)
                 .HasForeignKey<Teacher>(t => t.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Student>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasIndex(s => s.StudentCode).IsUnique();
                e.Property(s => s.StudentCode).HasMaxLength(20);
                e.Property(s => s.FullName).HasMaxLength(100).IsRequired();
                e.Property(s => s.Phone).HasMaxLength(20);
                e.Property(s => s.Email).HasMaxLength(100);
                e.Property(s => s.ParentName).HasMaxLength(200);
                e.Property(s => s.ParentPhone).HasMaxLength(20);
            });

            modelBuilder.Entity<Branch>(e =>
            {
                e.HasKey(b => b.Id);
                e.Property(b => b.BranchName).HasMaxLength(200).IsRequired();
                e.Property(b => b.City).HasMaxLength(100).IsRequired();
                e.Property(b => b.Address).HasMaxLength(500).IsRequired();
                e.Property(b => b.Phone).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Room>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.RoomName).HasMaxLength(100).IsRequired();
                e.Property(r => r.RoomType).IsRequired();
                e.HasOne(r => r.Branch)
                 .WithMany(b => b.Rooms)
                 .HasForeignKey(r => r.BranchId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Course>(e =>
            {
                e.HasKey(c => c.Id);
                e.HasIndex(c => c.CourseCode).IsUnique();
                e.Property(c => c.CourseName).HasMaxLength(200).IsRequired();
                e.Property(c => c.CourseCode).HasMaxLength(20);
                e.Property(c => c.Description).HasMaxLength(1000);
                e.Property(c => c.TuitionFee).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Class>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.ClassName).HasMaxLength(200).IsRequired();
                e.Property(c => c.Status).IsRequired();
                e.HasOne(c => c.Course)
                 .WithMany(co => co.Classes)
                 .HasForeignKey(c => c.CourseId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TeacherClassRegistration>(e =>
            {
                e.HasKey(tcr => tcr.Id);
                e.HasIndex(tcr => new { tcr.TeacherId, tcr.ClassId }).IsUnique();
                e.HasOne(tcr => tcr.Teacher)
                 .WithMany(t => t.TeacherClassRegistrations)
                 .HasForeignKey(tcr => tcr.TeacherId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(tcr => tcr.Class)
                 .WithMany(c => c.TeacherClassRegistrations)
                 .HasForeignKey(tcr => tcr.ClassId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TimeSlot>(e =>
            {
                e.HasKey(ts => ts.Id);
                e.Property(ts => ts.SlotName).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Schedule>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasOne(s => s.Class)
                 .WithMany(c => c.Schedules)
                 .HasForeignKey(s => s.ClassId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(s => s.Room)
                 .WithMany(r => r.Schedules)
                 .HasForeignKey(s => s.RoomId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(s => s.TimeSlot)
                 .WithMany(ts => ts.Schedules)
                 .HasForeignKey(s => s.TimeSlotId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Enrollment>(e =>
            {
                e.HasKey(en => en.Id);
                e.HasIndex(en => new { en.ClassId, en.StudentId }).IsUnique();
                e.Property(en => en.Status).IsRequired();
                e.HasOne(en => en.Class)
                 .WithMany(c => c.Enrollments)
                 .HasForeignKey(en => en.ClassId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(en => en.Student)
                 .WithMany(s => s.Enrollments)
                 .HasForeignKey(en => en.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payment>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
                e.Property(p => p.PaymentMethod).IsRequired();
                e.Property(p => p.Status).IsRequired();
                e.Property(p => p.Note).HasMaxLength(500);
                e.HasOne(p => p.Enrollment)
                 .WithMany(en => en.Payments)
                 .HasForeignKey(p => p.EnrollmentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClassTransfer>(e =>
            {
                e.HasKey(ct => ct.Id);
                e.Property(ct => ct.Reason).HasMaxLength(500);
                e.HasOne(ct => ct.Student)
                 .WithMany(s => s.ClassTransfers)
                 .HasForeignKey(ct => ct.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(ct => ct.FromClass)
                 .WithMany()
                 .HasForeignKey(ct => ct.FromClassId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(ct => ct.ToClass)
                 .WithMany(c => c.ClassTransfers)
                 .HasForeignKey(ct => ct.ToClassId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Attendance>(e =>
            {
                e.HasKey(a => a.Id);
                e.HasIndex(a => new { a.ScheduleId, a.StudentId }).IsUnique();
                e.Property(a => a.Status).IsRequired();
                e.Property(a => a.Note).HasMaxLength(500);
                e.HasOne(a => a.Schedule)
                 .WithMany(s => s.Attendances)
                 .HasForeignKey(a => a.ScheduleId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(a => a.Student)
                 .WithMany(s => s.Attendances)
                 .HasForeignKey(a => a.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Exam>(e =>
            {
                e.HasKey(ex => ex.Id);
                e.Property(ex => ex.ExamName).HasMaxLength(200).IsRequired();
                e.Property(ex => ex.MaxScore).HasColumnType("decimal(5,2)").IsRequired();
                e.Property(ex => ex.Description).HasMaxLength(500);
                e.HasOne(ex => ex.Class)
                 .WithMany(c => c.Exams)
                 .HasForeignKey(ex => ex.ClassId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ExamResult>(e =>
            {
                e.HasKey(er => er.Id);
                e.HasIndex(er => new { er.ExamId, er.StudentId }).IsUnique();
                e.Property(er => er.Score).HasColumnType("decimal(5,2)").IsRequired();
                e.Property(er => er.Note).HasMaxLength(500);
                e.HasOne(er => er.Exam)
                 .WithMany(ex => ex.ExamResults)
                 .HasForeignKey(er => er.ExamId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(er => er.Student)
                 .WithMany(s => s.ExamResults)
                 .HasForeignKey(er => er.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "SystemAdmin", Description = "Quản lý chuỗi" },
                new Role { Id = 2, RoleName = "BranchManager", Description = "Quản lý chi nhánh" },
                new Role { Id = 3, RoleName = "Staff", Description = "Nhân viên" },
                new Role { Id = 4, RoleName = "Teacher", Description = "Giáo viên" }
            );
        }
    }
}