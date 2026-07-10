using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KIDIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTopicOrderIndexConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_OrderIndex",
                table: "Topics");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_LevelNumber_OrderIndex",
                table: "Topics",
                columns: new[] { "LevelNumber", "OrderIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_LevelNumber_OrderIndex",
                table: "Topics");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_OrderIndex",
                table: "Topics",
                column: "OrderIndex",
                unique: true);
        }
    }
}
