﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RocketChatToDoServer.Database;

namespace RocketChatToDoServer.Migrations
{
    [DbContext(typeof(TaskContext))]
    partial class TaskContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.8-servicing-32085");

            modelBuilder.Entity("RocketChatToDoServer.Database.Models.Task", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("Description");

                    b.Property<bool>("Done");

                    b.Property<DateTime>("DueDate");

                    b.Property<int>("InitiatorID");

                    b.Property<string>("Title");

                    b.HasKey("ID");

                    b.HasIndex("InitiatorID");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("RocketChatToDoServer.Database.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("RocketChatToDoServer.Database.Models.UserTaskMap", b =>
                {
                    b.Property<int>("TaskID");

                    b.Property<int>("UserID");

                    b.HasKey("TaskID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("UserTaskMaps");
                });

            modelBuilder.Entity("RocketChatToDoServer.Database.Models.Task", b =>
                {
                    b.HasOne("RocketChatToDoServer.Database.Models.User", "Initiator")
                        .WithMany()
                        .HasForeignKey("InitiatorID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RocketChatToDoServer.Database.Models.UserTaskMap", b =>
                {
                    b.HasOne("RocketChatToDoServer.Database.Models.Task", "Task")
                        .WithMany("Assignees")
                        .HasForeignKey("TaskID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RocketChatToDoServer.Database.Models.User", "User")
                        .WithMany("Tasks")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
