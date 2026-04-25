using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JO_UNI_Guide.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMinGPA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinGPA",
                table: "Departments");

            migrationBuilder.AlterColumn<double>(
                name: "AcceptanceRate",
                table: "Departments",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "AcceptanceRate",
                table: "Departments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<double>(
                name: "MinGPA",
                table: "Departments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
