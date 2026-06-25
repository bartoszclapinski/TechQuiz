using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechQuiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPooledQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pooled_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    topic = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    explanation = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pooled_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pooled_question_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    pooled_question_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pooled_question_options", x => x.id);
                    table.ForeignKey(
                        name: "fk_pooled_question_options_pooled_questions_pooled_question_id",
                        column: x => x.pooled_question_id,
                        principalTable: "pooled_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pooled_question_options_pooled_question_id",
                table: "pooled_question_options",
                column: "pooled_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_pooled_questions_created_by_user_id",
                table: "pooled_questions",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_pooled_questions_status",
                table: "pooled_questions",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pooled_question_options");

            migrationBuilder.DropTable(
                name: "pooled_questions");
        }
    }
}
