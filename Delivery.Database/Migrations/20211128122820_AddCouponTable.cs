using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddCouponTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CouponDiscountPaid",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CouponCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CouponId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CouponCodeType = table.Column<int>(type: "int", maxLength: 256, nullable: false),
                    RedeemBy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NumberOfTimes = table.Column<int>(type: "int", nullable: false),
                    MinimumOrderValue = table.Column<int>(type: "int", nullable: false),
                    PromotionCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DiscountAmount = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "CouponCodes",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponCodes");

            migrationBuilder.DropColumn(
                name: "CouponDiscountPaid",
                table: "Orders");
        }
    }
}
