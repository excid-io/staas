﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Excid.Staas.Data;

#nullable disable

namespace Staas.Migrations
{
    [DbContext(typeof(StassDbContext))]
    partial class StassDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("Excid.Staas.Models.SignedItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CAKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Certificate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RekorLogEntry")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RekorLogEntryUUID")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Signature")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("SignedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Signer")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SignedItems");
                });
#pragma warning restore 612, 618
        }
    }
}
