﻿// <auto-generated />
using System;
using Core.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20200205134155_JsonColumn")]
    partial class JsonColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Core.DataBaseEntities.AuthenticationAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsActive");

                    b.Property<string>("ServiceId")
                        .HasMaxLength(100);

                    b.Property<string>("ServiceToken");

                    b.Property<int>("Type");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("Type", "ServiceId")
                        .IsUnique();

                    b.ToTable("AuthenticationAccounts");
                });

            modelBuilder.Entity("Core.DataBaseEntities.Contest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuditoriumsJson");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("Options");

                    b.Property<Guid>("OwnerId");

                    b.Property<string>("ResultsTableLink");

                    b.Property<string>("TasksDescriptionJson");

                    b.Property<string>("Title");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("Title")
                        .IsUnique();

                    b.ToTable("Contests");
                });

            modelBuilder.Entity("Core.DataBaseEntities.Invite", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("AccountId");

                    b.Property<string>("ConfirmationCode")
                        .HasMaxLength(30);

                    b.Property<string>("Email")
                        .HasMaxLength(100);

                    b.Property<bool>("IsUsed");

                    b.Property<bool>("PasswordRestore");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("Type", "Email", "ConfirmationCode")
                        .IsUnique();

                    b.ToTable("EmailConfirmationRequests");
                });

            modelBuilder.Entity("Core.DataBaseEntities.News", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<Guid>("ContestId");

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("ContestId");

                    b.ToTable("News");
                });

            modelBuilder.Entity("Core.DataBaseEntities.Participant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ContestId");

                    b.Property<string>("Login");

                    b.Property<string>("Pass");

                    b.Property<string>("SerializedResults");

                    b.Property<string>("SerializedUserSnapshot")
                        .HasColumnType("json");

                    b.Property<Guid>("UserId");

                    b.Property<string>("Verification");

                    b.Property<bool>("Verified");

                    b.HasKey("Id");

                    b.ToTable("Participants");
                });

            modelBuilder.Entity("Core.DataBaseEntities.QualificationParticipation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ContestId");

                    b.Property<DateTimeOffset>("EndTime");

                    b.Property<Guid>("ParticipantId");

                    b.Property<string>("SerializedAnswers");

                    b.HasKey("Id");

                    b.ToTable("QualificationParticipations");
                });

            modelBuilder.Entity("Core.DataBaseEntities.QualificationTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Answer");

                    b.Property<Guid>("ContestId");

                    b.Property<int[]>("ForClasses");

                    b.Property<byte[]>("Image");

                    b.Property<int>("Number");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.ToTable("QualificationTasks");
                });

            modelBuilder.Entity("Core.DataBaseEntities.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("LastUse");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("Core.DataBaseEntities.StoredConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("JsonValue");

                    b.Property<string>("TypeName");

                    b.HasKey("Id");

                    b.ToTable("StoredConfigs");
                });

            modelBuilder.Entity("Core.DataBaseEntities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City");

                    b.Property<int?>("Class");

                    b.Property<string>("Coach");

                    b.Property<string>("Name")
                        .HasMaxLength(100);

                    b.Property<int>("Role");

                    b.Property<string>("School");

                    b.Property<int>("Sex");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Core.DataBaseEntities.News", b =>
                {
                    b.HasOne("Core.DataBaseEntities.Contest", "Contest")
                        .WithMany()
                        .HasForeignKey("ContestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
