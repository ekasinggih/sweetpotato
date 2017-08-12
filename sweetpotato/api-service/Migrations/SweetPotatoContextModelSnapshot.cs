using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using api_service.Models;

namespace apiservice.Migrations
{
    [DbContext(typeof(SweetPotatoContext))]
    partial class SweetPotatoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("api_service.Models.PaymentRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<DateTime>("ExpiredTime");

                    b.Property<bool>("IsSettled");

                    b.Property<string>("PayerCashTag");

                    b.Property<int>("PayerUserId");

                    b.Property<string>("ReferenceNo");

                    b.Property<string>("RequesterCashTag");

                    b.Property<int>("RequesterUserId");

                    b.Property<DateTime?>("SettledTime");

                    b.HasKey("Id");

                    b.ToTable("PaymentRequests");
                });
        }
    }
}
