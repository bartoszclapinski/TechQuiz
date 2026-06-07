using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechQuiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizAttemptScorePercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "score_percentage",
                table: "quiz_attempts",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "score_percentage",
                table: "quiz_attempts");
        }
    }
}
