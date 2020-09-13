using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Api.Migrations
{
    public partial class AddReportOrderEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(nullable: false),
                    Subject = table.Column<string>(maxLength: 250, nullable: true),
                    ContactNumber = table.Column<string>(maxLength: 20, nullable: true),
                    ReportCategory = table.Column<string>(maxLength: 20, nullable: true),
                    Message = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
