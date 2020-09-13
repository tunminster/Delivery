using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Api.Migrations
{
    public partial class AddedPaymentResponse_entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentResponses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderCode = table.Column<string>(maxLength: 250, nullable: true),
                    Token = table.Column<string>(maxLength: 250, nullable: true),
                    OrderDescription = table.Column<string>(maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(maxLength: 20, nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 10, nullable: true),
                    PaymentStatus = table.Column<string>(maxLength: 10, nullable: true),
                    MaskedCardNumber = table.Column<string>(maxLength: 30, nullable: true),
                    CvcResultCode = table.Column<string>(maxLength: 10, nullable: true),
                    Environment = table.Column<string>(maxLength: 10, nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentResponses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentResponses");
        }
    }
}
