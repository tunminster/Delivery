using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class UpdateStripePayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripeClientSecret",
                table: "StripePayments",
                newName: "StripePaymentMethodId");

            migrationBuilder.AddColumn<long>(
                name: "AmountCaptured",
                table: "StripePayments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Captured",
                table: "StripePayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CapturedDateTime",
                table: "StripePayments",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "FailureCode",
                table: "StripePayments",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureMessage",
                table: "StripePayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "StripePayments",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptUrl",
                table: "StripePayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountCaptured",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "Captured",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "CapturedDateTime",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "FailureCode",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "FailureMessage",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "ReceiptUrl",
                table: "StripePayments");

            migrationBuilder.RenameColumn(
                name: "StripePaymentMethodId",
                table: "StripePayments",
                newName: "StripeClientSecret");
        }
    }
}
