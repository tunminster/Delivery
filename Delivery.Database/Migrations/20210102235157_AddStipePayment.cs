using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddStipePayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StripePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StripeClientSecret = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InsertedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InsertionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StripePayments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StripePayments_OrderId",
                table: "StripePayments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "StripePayments",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StripePayments");
        }
    }
}
