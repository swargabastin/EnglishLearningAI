using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveLearningEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "learner_assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CefrLevel = table.Column<int>(type: "integer", nullable: false),
                    GrammarScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    VocabularyScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SentenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ReadingScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SpeakingScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OverallScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Confidence = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResponsesJson = table.Column<string>(type: "text", nullable: false),
                    WeaknessesJson = table.Column<string>(type: "text", nullable: false),
                    ReassessmentRecommended = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learner_assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learner_assessments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learner_skill_masteries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MasteryScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    IsMastered = table.Column<bool>(type: "boolean", nullable: false),
                    LastPracticedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextRevisionDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learner_skill_masteries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learner_skill_masteries_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learning_activity_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskIdentifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    AnswerPayloadJson = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_activity_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learning_activity_histories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learning_objectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SkillCategory = table.Column<int>(type: "integer", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    CefrLevel = table.Column<int>(type: "integer", nullable: false),
                    PrerequisiteCodesJson = table.Column<string>(type: "text", nullable: false),
                    MasteryScoreThreshold = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SuggestedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_objectives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "study_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanDate = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ObjectiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    ActualMinutes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_plans_learning_objectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "learning_objectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_study_plans_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_plan_phases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseOrder = table.Column<int>(type: "integer", nullable: false),
                    PhaseType = table.Column<int>(type: "integer", nullable: false),
                    ObjectiveTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LessonContentJson = table.Column<string>(type: "text", nullable: false),
                    TasksJson = table.Column<string>(type: "text", nullable: false),
                    PlannedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ActualDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plan_phases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_plan_phases_study_plans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalTable: "study_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_learner_assessments_UserId",
                table: "learner_assessments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_learner_skill_masteries_UserId_ObjectiveCode",
                table: "learner_skill_masteries",
                columns: new[] { "UserId", "ObjectiveCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_learning_activity_histories_UserId",
                table: "learning_activity_histories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_learning_objectives_Code",
                table: "learning_objectives",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_plan_phases_StudyPlanId_PhaseOrder",
                table: "study_plan_phases",
                columns: new[] { "StudyPlanId", "PhaseOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_ObjectiveId",
                table: "study_plans",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_UserId_PlanDate",
                table: "study_plans",
                columns: new[] { "UserId", "PlanDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "learner_assessments");

            migrationBuilder.DropTable(
                name: "learner_skill_masteries");

            migrationBuilder.DropTable(
                name: "learning_activity_histories");

            migrationBuilder.DropTable(
                name: "study_plan_phases");

            migrationBuilder.DropTable(
                name: "study_plans");

            migrationBuilder.DropTable(
                name: "learning_objectives");
        }
    }
}
