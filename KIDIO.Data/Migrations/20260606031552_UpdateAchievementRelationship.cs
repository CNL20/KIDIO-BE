using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KIDIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAchievementRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AchievementType",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "BadgeUrl",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "Threshold",
                table: "Achievements");

            migrationBuilder.AddColumn<Guid>(
                name: "AchievementDefinitionId",
                table: "Achievements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AchievementDefinitionId",
                table: "Achievements",
                column: "AchievementDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_AchievementDefinitions_AchievementDefinitionId",
                table: "Achievements",
                column: "AchievementDefinitionId",
                principalTable: "AchievementDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_AchievementDefinitions_AchievementDefinitionId",
                table: "Achievements");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_AchievementDefinitionId",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "AchievementDefinitionId",
                table: "Achievements");

            migrationBuilder.AddColumn<string>(
                name: "AchievementType",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BadgeUrl",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Threshold",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
