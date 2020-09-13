using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Api.Migrations
{
    public partial class AddedOrdertables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 15, nullable: true),
                    PaymentType = table.Column<string>(maxLength: 15, nullable: true),
                    PaymentCard = table.Column<string>(maxLength: 16, nullable: true),
                    PaymentStatus = table.Column<string>(maxLength: 15, nullable: true),
                    CustomerId = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentCards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(maxLength: 1000, nullable: true),
                    Name = table.Column<string>(maxLength: 150, nullable: true),
                    CardType = table.Column<string>(maxLength: 30, nullable: true),
                    MaskedCardNumber = table.Column<string>(maxLength: 16, nullable: true),
                    ExpiryMonth = table.Column<string>(maxLength: 10, nullable: true),
                    ExpiryYear = table.Column<string>(maxLength: 10, nullable: true),
                    CustomerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentCards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCards_CustomerId",
                table: "PaymentCards",
                column: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PaymentCards");
        }
    }
}
