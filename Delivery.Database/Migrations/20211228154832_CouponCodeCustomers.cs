using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class CouponCodeCustomers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CouponCodeCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExternalId = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCodeCustomers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UniqueExternalId",
                table: "CouponCodeCustomers",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponCodeCustomers");
        }
    }
}
