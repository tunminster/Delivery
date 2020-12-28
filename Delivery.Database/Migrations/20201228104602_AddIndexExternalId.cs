using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddIndexExternalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "Reports",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "Products",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "PaymentResponses",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "PaymentCards",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "Orders",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "OrderItems",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "Customers",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "Categories",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "Addresses",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "PaymentResponses");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "PaymentCards");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_UniqueExternalId",
                table: "Addresses");
        }
    }
}
