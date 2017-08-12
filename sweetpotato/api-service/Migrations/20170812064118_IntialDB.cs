using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace apiservice.Migrations
{
    public partial class IntialDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(nullable: false),
                    ExpiredTime = table.Column<DateTime>(nullable: false),
                    IsSettled = table.Column<bool>(nullable: false),
                    PayerCashTag = table.Column<string>(nullable: true),
                    PayerUserId = table.Column<int>(nullable: false),
                    ReferenceNo = table.Column<string>(nullable: true),
                    RequesterCashTag = table.Column<string>(nullable: true),
                    RequesterUserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRequests");
        }
    }
}
