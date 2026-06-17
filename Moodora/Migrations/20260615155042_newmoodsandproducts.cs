using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moodora.Migrations
{
    /// <inheritdoc />
    public partial class newmoodsandproducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Orders",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ProductMoodCategories",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MoodCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMoodCategories", x => new { x.ProductId, x.MoodCategoryId });
                    table.ForeignKey(
                        name: "FK_ProductMoodCategories_MoodCategories_MoodCategoryId",
                        column: x => x.MoodCategoryId,
                        principalTable: "MoodCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMoodCategories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMoodCategories_MoodCategoryId",
                table: "ProductMoodCategories",
                column: "MoodCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductMoodCategories");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "MoodCategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_MoodCategoryId",
                table: "Products",
                column: "MoodCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_MoodCategories_MoodCategoryId",
                table: "Products",
                column: "MoodCategoryId",
                principalTable: "MoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
