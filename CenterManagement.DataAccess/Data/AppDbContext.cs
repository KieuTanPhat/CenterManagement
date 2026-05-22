using System;
using System.Collections.Generic;
using CenterManagement.DataAccess;
using Microsoft.EntityFrameworkCore;
namespace CenterManagement.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassSchedule> ClassSchedules { get; set; }

    public virtual DbSet<ClassStudent> ClassStudents { get; set; }

    public virtual DbSet<Consultation> Consultations { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Lead> Leads { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=KIEUPHAT;Database=CenterManagementDB;User Id=sa;Password=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Branches__3214EC07A50DD30F");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.BranchName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Classes__3214EC07DC1AA385");

            entity.HasIndex(e => e.BranchId, "IX_Classes_BranchId");

            entity.HasIndex(e => e.CourseId, "IX_Classes_CourseId");

            entity.HasIndex(e => e.TeacherId, "IX_Classes_TeacherId");

            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxStudents).HasDefaultValue(20);

            entity.HasOne(d => d.Branch).WithMany(p => p.Classes)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Branches");

            entity.HasOne(d => d.Course).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Courses");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classes)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Teachers");
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClassSch__3214EC07D7622A62");

            entity.HasIndex(e => e.ClassId, "IX_Schedule_ClassId");

            entity.Property(e => e.DayOfWeek).HasMaxLength(10);
            entity.Property(e => e.Room).HasMaxLength(50);

            entity.HasOne(d => d.Class).WithMany(p => p.ClassSchedules)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Classes");
        });

        modelBuilder.Entity<ClassStudent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClassStu__3214EC07E14B54FB");

            entity.HasIndex(e => e.ClassId, "IX_CS_ClassId");

            entity.HasIndex(e => e.StudentId, "IX_CS_StudentId");

            entity.HasIndex(e => new { e.ClassId, e.StudentId }, "UQ_CS_ClassStudent").IsUnique();

            entity.Property(e => e.EnrolledAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CS_Classes");

            entity.HasOne(d => d.Registration).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.RegistrationId)
                .HasConstraintName("FK_CS_Registrations");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CS_Students");
        });

        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Consulta__3214EC07395E28D5");

            entity.HasIndex(e => e.LeadId, "IX_Consult_LeadId");

            entity.HasIndex(e => e.StaffId, "IX_Consult_StaffId");

            entity.Property(e => e.ConsultDate).HasColumnType("datetime");
            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FollowUpDate).HasColumnType("datetime");
            entity.Property(e => e.Result).HasMaxLength(20);

            entity.HasOne(d => d.Lead).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.LeadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Consult_Leads");

            entity.HasOne(d => d.RecommendedCourse).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.RecommendedCourseId)
                .HasConstraintName("FK_Consult_Courses");

            entity.HasOne(d => d.Staff).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Consult_Staff");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Courses__3214EC077F1A4464");

            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Fee).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Level).HasMaxLength(50);
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Leads__3214EC0776978568");

            entity.HasIndex(e => e.BranchId, "IX_Leads_BranchId");

            entity.HasIndex(e => e.CreatedAt, "IX_Leads_CreatedAt");

            entity.HasIndex(e => e.StaffId, "IX_Leads_StaffId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Source).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.Leads)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leads_Branches");

            entity.HasOne(d => d.Staff).WithMany(p => p.Leads)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leads_Staff");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC074B8DCE9D");

            entity.HasIndex(e => e.BranchId, "IX_Reg_BranchId");

            entity.HasIndex(e => e.StudentId, "IX_Reg_StudentId");

            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Branch).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reg_Branches");

            entity.HasOne(d => d.Course).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reg_Courses");

            entity.HasOne(d => d.Staff).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reg_Staff");

            entity.HasOne(d => d.Student).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reg_Students");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC073C238837");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616030CE5901").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Students__3214EC07AE4E69D5");

            entity.HasIndex(e => e.BranchId, "IX_Students_BranchId");

            entity.HasIndex(e => e.LeadId, "IX_Students_LeadId");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Branch).WithMany(p => p.Students)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Branches");

            entity.HasOne(d => d.Lead).WithMany(p => p.Students)
                .HasForeignKey(d => d.LeadId)
                .HasConstraintName("FK_Students_Leads");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Teachers__3214EC07F08264EB");

            entity.HasIndex(e => e.BranchId, "IX_Teachers_BranchId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Specialty).HasMaxLength(100);

            entity.HasOne(d => d.Branch).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teachers_Branches");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07E1DD22AF");

            entity.HasIndex(e => e.BranchId, "IX_Users_BranchId");

            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E44D6C8184").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_Users_Branches");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
