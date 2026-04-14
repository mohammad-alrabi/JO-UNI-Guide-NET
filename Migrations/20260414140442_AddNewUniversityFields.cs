using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JO_UNI_Guide.Migrations
{
    /// <inheritdoc />
    public partial class AddNewUniversityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Universities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Universities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Universities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Universities",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentDate",
                table: "ContactMessages",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Universities");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentDate",
                table: "ContactMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
