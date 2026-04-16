using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JO_UNI_Guide.Migrations
{
    /// <inheritdoc />
    public partial class FixIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredTrack",
                table: "Departments",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TawjihiTrack",
                table: "AspNetUsers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredTrack",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "TawjihiTrack",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
