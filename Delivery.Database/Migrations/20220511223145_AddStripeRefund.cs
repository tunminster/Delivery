using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddStripeRefund : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StripePaymentRefunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TotalAmount = table.Column<int>(type: "int", nullable: false),
                    InsertedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InsertionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripePaymentRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StripePaymentRefunds_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StripePaymentRefunds_OrderId",
                table: "StripePaymentRefunds",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "StripePaymentRefunds",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StripePaymentRefunds");
        }
    }
}
