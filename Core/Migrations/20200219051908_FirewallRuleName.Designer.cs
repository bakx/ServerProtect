﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SP.Core;

namespace SP.Core.Migrations
{
    [DbContext(typeof(Db))]
    [Migration("20200219051908_FirewallRuleName")]
    partial class FirewallRuleName
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1");

            modelBuilder.Entity("SP.Models.Blocks", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Details")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirewallRuleName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Hostname")
                        .HasColumnType("TEXT");

                    b.Property<string>("ISP")
                        .HasColumnType("TEXT");

                    b.Property<string>("IpAddress")
                        .HasColumnType("TEXT");

                    b.Property<byte>("IpAddress1")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("IpAddress2")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("IpAddress3")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("IpAddress4")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("IsBlocked")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Blocks");
                });

            modelBuilder.Entity("SP.Models.LoginAttempts", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Details")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("IpAddress")
                        .HasColumnType("TEXT");

                    b.Property<byte>("IpAddress1")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("IpAddress2")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("IpAddress3")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("IpAddress4")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Login.Attempts");
                });

            modelBuilder.Entity("SP.Models.StatisticsBlocks", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("Attempts")
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<string>("ISP")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Statistics.Blocks");
                });
#pragma warning restore 612, 618
        }
    }
}
