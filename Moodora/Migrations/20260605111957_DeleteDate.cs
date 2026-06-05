using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moodora.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "MoodCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "MoodCategories");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "AspNetUsers");
        }
    }
}
