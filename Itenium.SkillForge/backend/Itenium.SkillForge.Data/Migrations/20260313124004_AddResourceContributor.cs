using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itenium.SkillForge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceContributor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContributedAt",
                table: "Resources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContributedBy",
                table: "Resources",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContributedAt",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ContributedBy",
                table: "Resources");
        }
    }
}
