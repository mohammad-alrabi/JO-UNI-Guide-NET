using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JO_UNI_Guide.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepartmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Faculties_Faculty_ID",
                table: "Departments");

            migrationBuilder.AlterColumn<int>(
                name: "Faculty_ID",
                table: "Departments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Departments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "AcceptanceRate",
                table: "Departments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HourPrice",
                table: "Departments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalCreditHours",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Faculties_Faculty_ID",
                table: "Departments",
                column: "Faculty_ID",
                principalTable: "Faculties",
                principalColumn: "Faculty_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Faculties_Faculty_ID",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "AcceptanceRate",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HourPrice",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "TotalCreditHours",
                table: "Departments");

            migrationBuilder.AlterColumn<int>(
                name: "Faculty_ID",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Departments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Faculties_Faculty_ID",
                table: "Departments",
                column: "Faculty_ID",
                principalTable: "Faculties",
                principalColumn: "Faculty_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
