using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KIDIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMinDifficultyWithLevelNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinDifficulty",
                table: "Topics",
                newName: "LevelNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LevelNumber",
                table: "Topics",
                newName: "MinDifficulty");
        }
    }
}
