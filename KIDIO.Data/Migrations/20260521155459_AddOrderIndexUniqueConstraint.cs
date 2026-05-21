using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KIDIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIndexUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Topics_OrderIndex",
                table: "Topics",
                column: "OrderIndex",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_OrderIndex",
                table: "Topics");
        }
    }
}
