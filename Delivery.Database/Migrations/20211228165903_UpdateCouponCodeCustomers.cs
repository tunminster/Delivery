using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class UpdateCouponCodeCustomers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CouponCodeId",
                table: "CouponCodeCustomers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InsertedBy",
                table: "CouponCodeCustomers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InsertionDateTime",
                table: "CouponCodeCustomers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CouponCodeCustomers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CouponCodeCustomers_CouponCodeId",
                table: "CouponCodeCustomers",
                column: "CouponCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponCodeCustomers_CouponCodes_CouponCodeId",
                table: "CouponCodeCustomers",
                column: "CouponCodeId",
                principalTable: "CouponCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouponCodeCustomers_CouponCodes_CouponCodeId",
                table: "CouponCodeCustomers");

            migrationBuilder.DropIndex(
                name: "IX_CouponCodeCustomers_CouponCodeId",
                table: "CouponCodeCustomers");

            migrationBuilder.DropColumn(
                name: "CouponCodeId",
                table: "CouponCodeCustomers");

            migrationBuilder.DropColumn(
                name: "InsertedBy",
                table: "CouponCodeCustomers");

            migrationBuilder.DropColumn(
                name: "InsertionDateTime",
                table: "CouponCodeCustomers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CouponCodeCustomers");
        }
    }
}
