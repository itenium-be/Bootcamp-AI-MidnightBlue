using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itenium.SkillForge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileToConsultant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "ConsultantProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantProfiles_ProfileId",
                table: "ConsultantProfiles",
                column: "ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantProfiles_CompetenceCentreProfiles_ProfileId",
                table: "ConsultantProfiles",
                column: "ProfileId",
                principalTable: "CompetenceCentreProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantProfiles_CompetenceCentreProfiles_ProfileId",
                table: "ConsultantProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ConsultantProfiles_ProfileId",
                table: "ConsultantProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "ConsultantProfiles");
        }
    }
}
