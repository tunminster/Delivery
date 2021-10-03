using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddDriverPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DriverPaymentDate",
                table: "DriverOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverPaymentId",
                table: "DriverOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverPaymentStatus",
                table: "DriverOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DriverPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalPaymentAmount = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    InsertedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPayments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "DriverPayments",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverPayments");

            migrationBuilder.DropColumn(
                name: "DriverPaymentDate",
                table: "DriverOrders");

            migrationBuilder.DropColumn(
                name: "DriverPaymentId",
                table: "DriverOrders");

            migrationBuilder.DropColumn(
                name: "DriverPaymentStatus",
                table: "DriverOrders");
        }
    }
}
