using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KIDIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessTypeAndStartingLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "Topics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinDifficulty",
                table: "Topics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartingLevel",
                table: "Children",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Access",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "MinDifficulty",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "StartingLevel",
                table: "Children");
        }
    }
}
