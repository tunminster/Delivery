using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddOrderItemOptionMeats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItemMeatOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    MeatOptionId = table.Column<int>(type: "int", nullable: false),
                    MeatOptionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemMeatOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemMeatOptions_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemMeatOptionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemMeatOptionId = table.Column<int>(type: "int", nullable: false),
                    MeatOptionValueId = table.Column<int>(type: "int", nullable: false),
                    MeatOptionValueText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemMeatOptionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemMeatOptionValues_OrderItemMeatOptions_OrderItemMeatOptionId",
                        column: x => x.OrderItemMeatOptionId,
                        principalTable: "OrderItemMeatOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemMeatOptions_OrderItemId",
                table: "OrderItemMeatOptions",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "OrderItemMeatOptions",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemMeatOptionValues_OrderItemMeatOptionId",
                table: "OrderItemMeatOptionValues",
                column: "OrderItemMeatOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "OrderItemMeatOptionValues",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemMeatOptionValues");

            migrationBuilder.DropTable(
                name: "OrderItemMeatOptions");
        }
    }
}
