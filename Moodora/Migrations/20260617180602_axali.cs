using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moodora.Migrations
{
    /// <inheritdoc />
    public partial class axali : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoodCategoryId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_MoodCategoryId",
                table: "Products",
                column: "MoodCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_MoodCategories_MoodCategoryId",
                table: "Products",
                column: "MoodCategoryId",
                principalTable: "MoodCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_MoodCategories_MoodCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_MoodCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MoodCategoryId",
                table: "Products");
        }
    }
}
