using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class FixStorePaymentAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorePaymentAccounts_Stores_StoreId",
                table: "StorePaymentAccounts");

            migrationBuilder.DropIndex(
                name: "IX_StorePaymentAccounts_StoreId",
                table: "StorePaymentAccounts");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "StorePaymentAccounts");

            migrationBuilder.AddColumn<int>(
                name: "StorePaymentAccountId",
                table: "Stores",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StorePaymentAccountId",
                table: "Stores",
                column: "StorePaymentAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_StorePaymentAccounts_StorePaymentAccountId",
                table: "Stores",
                column: "StorePaymentAccountId",
                principalTable: "StorePaymentAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_StorePaymentAccounts_StorePaymentAccountId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_StorePaymentAccountId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "StorePaymentAccountId",
                table: "Stores");

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "StorePaymentAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StorePaymentAccounts_StoreId",
                table: "StorePaymentAccounts",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_StorePaymentAccounts_Stores_StoreId",
                table: "StorePaymentAccounts",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
