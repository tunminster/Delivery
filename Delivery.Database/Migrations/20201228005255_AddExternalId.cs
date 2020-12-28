using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddExternalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentCard",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "PaymentOrderCodeRef",
                table: "Orders",
                newName: "PaymentIntentId");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Reports",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Products",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "PaymentResponses",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "PaymentCards",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Orders",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "OrderItems",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Customers",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Categories",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Addresses",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "PaymentResponses");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "PaymentCards");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "PaymentIntentId",
                table: "Orders",
                newName: "PaymentOrderCodeRef");

            migrationBuilder.AddColumn<string>(
                name: "PaymentCard",
                table: "Orders",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);
        }
    }
}
