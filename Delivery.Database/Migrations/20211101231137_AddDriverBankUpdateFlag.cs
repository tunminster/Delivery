using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Database.Migrations
{
    public partial class AddDriverBankUpdateFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBankDetailsUpdated",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBankDetailsUpdated",
                table: "Drivers");
        }
    }
}
