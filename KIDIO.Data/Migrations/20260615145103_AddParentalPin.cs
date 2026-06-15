using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KIDIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentalPin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentalPin",
                table: "Users",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentalPin",
                table: "Users");
        }
    }
}
