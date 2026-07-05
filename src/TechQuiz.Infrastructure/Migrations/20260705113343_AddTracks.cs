using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechQuiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "position",
                table: "categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "track_id",
                table: "categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "tracks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    icon_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tracks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_track_id",
                table: "categories",
                column: "track_id");

            migrationBuilder.CreateIndex(
                name: "ix_tracks_name",
                table: "tracks",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_categories_tracks_track_id",
                table: "categories",
                column: "track_id",
                principalTable: "tracks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_categories_tracks_track_id",
                table: "categories");

            migrationBuilder.DropTable(
                name: "tracks");

            migrationBuilder.DropIndex(
                name: "ix_categories_track_id",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "position",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "track_id",
                table: "categories");
        }
    }
}
