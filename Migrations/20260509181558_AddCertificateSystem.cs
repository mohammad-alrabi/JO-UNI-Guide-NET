using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JO_UNI_Guide.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AcceptanceRate",
                table: "Departments",
                newName: "MinEquivalentGrade");

            migrationBuilder.RenameColumn(
                name: "GPA",
                table: "AspNetUsers",
                newName: "OriginalGrade");

            migrationBuilder.AddColumn<int>(
                name: "CertificateType",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "EquivalentGrade",
                table: "AspNetUsers",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EquivalentGrade",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "MinEquivalentGrade",
                table: "Departments",
                newName: "AcceptanceRate");

            migrationBuilder.RenameColumn(
                name: "OriginalGrade",
                table: "AspNetUsers",
                newName: "GPA");
        }
    }
}
