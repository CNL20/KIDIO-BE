using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KIDIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Achievements_ChildId",
                table: "Achievements");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_ChildId_AchievementDefinitionId",
                table: "Achievements",
                columns: new[] { "ChildId", "AchievementDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Achievements_ChildId_AchievementDefinitionId",
                table: "Achievements");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_ChildId",
                table: "Achievements",
                column: "ChildId");
        }
    }
}
