﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NewSprt.Data.App;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NewSprt.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20201030200121_UpdateUserModelAndWorkTaskModel")]
    partial class UpdateUserModelAndWorkTaskModel
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("NewSprt.Data.App.Models.Department", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("HeadUserId");

                    b.Property<string>("Name");

                    b.Property<string>("ShortName");

                    b.HasKey("Id");

                    b.HasIndex("HeadUserId");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Discription");

                    b.Property<string>("Name");

                    b.Property<string>("ShortName");

                    b.HasKey("Id");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthorizationToken");

                    b.Property<int>("DepartmentId");

                    b.Property<string>("FullName");

                    b.Property<string>("Login");

                    b.Property<string>("Password");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.UserPermission", b =>
                {
                    b.Property<int>("PermissionId");

                    b.Property<int>("UserId");

                    b.HasKey("PermissionId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("UserPermissions");
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.WorkTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CompletionDate");

                    b.Property<DateTime>("CreateDate");

                    b.Property<int>("DepartmentId");

                    b.Property<string>("Discription");

                    b.Property<string>("DocumentNumber");

                    b.Property<string>("FilePath");

                    b.Property<bool>("IsRepeat");

                    b.Property<bool>("IsUrgent");

                    b.Property<string>("Name");

                    b.Property<int>("StatusId");

                    b.Property<int>("TaskManagerId");

                    b.Property<int>("TaskResponsibleId");

                    b.Property<string>("TimelineForCompliance");

                    b.Property<DateTime>("UpdateDate");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("StatusId");

                    b.HasIndex("TaskManagerId");

                    b.HasIndex("TaskResponsibleId");

                    b.ToTable("WorkTasks");
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.WorkTaskStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("WorkTaskStatuses");
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.Department", b =>
                {
                    b.HasOne("NewSprt.Data.App.Models.User", "HeadUser")
                        .WithMany()
                        .HasForeignKey("HeadUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.User", b =>
                {
                    b.HasOne("NewSprt.Data.App.Models.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.UserPermission", b =>
                {
                    b.HasOne("NewSprt.Data.App.Models.Permission", "Permission")
                        .WithMany("UserPermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NewSprt.Data.App.Models.User", "User")
                        .WithMany("UserPermissions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NewSprt.Data.App.Models.WorkTask", b =>
                {
                    b.HasOne("NewSprt.Data.App.Models.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NewSprt.Data.App.Models.WorkTaskStatus", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NewSprt.Data.App.Models.User", "TaskManagerUser")
                        .WithMany()
                        .HasForeignKey("TaskManagerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NewSprt.Data.App.Models.User", "TaskResponsibleUser")
                        .WithMany()
                        .HasForeignKey("TaskResponsibleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
