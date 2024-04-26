﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.API.Data;

#nullable disable

namespace Server.API.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240426092306_SeedingData")]
    partial class SeedingData
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Server.API.Models.Friendship", b =>
                {
                    b.Property<string>("User1Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("User2Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("date")
                        .HasColumnType("datetime2");

                    b.HasKey("User1Id", "User2Id");

                    b.HasIndex("User2Id");

                    b.ToTable("Friendships");

                    b.HasData(
                        new
                        {
                            User1Id = "59fbd0c8-0e0b-4cba-980e-f196b905a249",
                            User2Id = "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447",
                            Status = "Accepted",
                            date = new DateTime(2024, 4, 26, 9, 23, 6, 606, DateTimeKind.Utc).AddTicks(8467)
                        });
                });

            modelBuilder.Entity("Server.API.Models.Game", b =>
                {
                    b.Property<int>("GameId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("GameId"));

                    b.Property<int>("MaxPlayers")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("GameId");

                    b.ToTable("Games");

                    b.HasData(
                        new
                        {
                            GameId = 1,
                            MaxPlayers = 10,
                            Name = "Hangman"
                        },
                        new
                        {
                            GameId = 2,
                            MaxPlayers = 2,
                            Name = "TicTacToe"
                        },
                        new
                        {
                            GameId = 3,
                            MaxPlayers = 2,
                            Name = "Rock, Paper, Scissors"
                        });
                });

            modelBuilder.Entity("Server.API.Models.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("datetime2");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshToken");
                });

            modelBuilder.Entity("Server.API.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("coins")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "59fbd0c8-0e0b-4cba-980e-f196b905a249",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "6642bff1-f83f-443f-8ce0-53b1a3cf8052",
                            Email = "frank@gmail.com",
                            EmailConfirmed = true,
                            LockoutEnabled = false,
                            NormalizedEmail = "FRANK@GMAIL.COM",
                            NormalizedUserName = "FRANK",
                            PasswordHash = "AQAAAAIAAYagAAAAEJG0S05v4sdXYbPjLVpDkW0OtqES0cN89zOB8JWmMiZCWAvw2Hm2pvwEemNu47dMDA==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "94c3cf4c-d506-4bc3-b93c-9eab6eb6b08f",
                            TwoFactorEnabled = false,
                            UserName = "Frank",
                            coins = 0
                        },
                        new
                        {
                            Id = "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "075bfb6d-56bf-42ae-9503-2b496cfc16e5",
                            Email = "Peter@gmail.com",
                            EmailConfirmed = true,
                            LockoutEnabled = false,
                            NormalizedEmail = "PETER@GMAIL.COM",
                            NormalizedUserName = "PETER",
                            PasswordHash = "AQAAAAIAAYagAAAAENtuLVFoeOOgQM37uwRcHtija0smC74T2ejZ/+vIzJtzaHcx0lYCwdYgPQ+k856pzQ==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "6fdc264b-f195-48a0-8286-bac37ec14a23",
                            TwoFactorEnabled = false,
                            UserName = "Peter",
                            coins = 0
                        },
                        new
                        {
                            Id = "1c7e97d3-a982-4a1b-8d8e-b6b9d7e32c0f",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "a649e50c-ebeb-4168-9ed7-0708050753a3",
                            Email = "Hans@gmail.com",
                            EmailConfirmed = true,
                            LockoutEnabled = false,
                            NormalizedEmail = "HANS@GMAIL.COM",
                            NormalizedUserName = "HANS",
                            PasswordHash = "AQAAAAIAAYagAAAAEHDNaGCMw+3rzd4s9zw1K8989QQGvCEHWvi/wb54Ahih2+x4D6hRKr4MsV7W9msizw==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "843e9e1b-06f5-4f97-81c7-1e5026cbebc6",
                            TwoFactorEnabled = false,
                            UserName = "Hans",
                            coins = 0
                        });
                });

            modelBuilder.Entity("Server.API.Models.UserGame", b =>
                {
                    b.Property<int>("UserGameId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserGameId"));

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserGameId");

                    b.HasIndex("GameId");

                    b.HasIndex("UserId");

                    b.ToTable("UserGames");

                    b.HasData(
                        new
                        {
                            UserGameId = 1,
                            GameId = 1,
                            UserId = "59fbd0c8-0e0b-4cba-980e-f196b905a249"
                        },
                        new
                        {
                            UserGameId = 2,
                            GameId = 2,
                            UserId = "59fbd0c8-0e0b-4cba-980e-f196b905a249"
                        },
                        new
                        {
                            UserGameId = 3,
                            GameId = 3,
                            UserId = "59fbd0c8-0e0b-4cba-980e-f196b905a249"
                        },
                        new
                        {
                            UserGameId = 4,
                            GameId = 1,
                            UserId = "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447"
                        },
                        new
                        {
                            UserGameId = 5,
                            GameId = 1,
                            UserId = "1c7e97d3-a982-4a1b-8d8e-b6b9d7e32c0f"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Server.API.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Server.API.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Server.API.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Server.API.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Server.API.Models.Friendship", b =>
                {
                    b.HasOne("Server.API.Models.User", "User1")
                        .WithMany("Inviters")
                        .HasForeignKey("User1Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Server.API.Models.User", "User2")
                        .WithMany("Invitees")
                        .HasForeignKey("User2Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User1");

                    b.Navigation("User2");
                });

            modelBuilder.Entity("Server.API.Models.RefreshToken", b =>
                {
                    b.HasOne("Server.API.Models.User", null)
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Server.API.Models.UserGame", b =>
                {
                    b.HasOne("Server.API.Models.Game", "Game")
                        .WithMany("UserGames")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Server.API.Models.User", "User")
                        .WithMany("UserGames")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Server.API.Models.Game", b =>
                {
                    b.Navigation("UserGames");
                });

            modelBuilder.Entity("Server.API.Models.User", b =>
                {
                    b.Navigation("Invitees");

                    b.Navigation("Inviters");

                    b.Navigation("RefreshTokens");

                    b.Navigation("UserGames");
                });
#pragma warning restore 612, 618
        }
    }
}
