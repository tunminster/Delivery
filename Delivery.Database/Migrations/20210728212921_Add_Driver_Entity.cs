using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class Add_Driver_Entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    DrivingLicenseNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DrivingLicenseExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoutingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImageUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DrivingLicenseFrontUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DrivingLicenseBackUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ServiceArea = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InsertedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InsertionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "Drivers",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drivers");
        }
    }
}
