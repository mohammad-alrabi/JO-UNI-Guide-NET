using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JO_UNI_Guide.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Departments_Department_ID1",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Department_ID1",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Department_ID1",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Department_ID",
                table: "Courses",
                column: "Department_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Departments_Department_ID",
                table: "Courses",
                column: "Department_ID",
                principalTable: "Departments",
                principalColumn: "Department_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Departments_Department_ID",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Department_ID",
                table: "Courses");

            migrationBuilder.AddColumn<int>(
                name: "Department_ID1",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Department_ID1",
                table: "Courses",
                column: "Department_ID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Departments_Department_ID1",
                table: "Courses",
                column: "Department_ID1",
                principalTable: "Departments",
                principalColumn: "Department_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
