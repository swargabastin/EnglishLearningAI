using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnglishLearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RecommendedDifficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IconKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EnglishLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LearningGoal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "conversation_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnglishLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OverallScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversation_sessions_topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conversation_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrammarScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    VocabularyScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FluencyScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    PronunciationScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    OverallScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    StrengthsJson = table.Column<string>(type: "jsonb", nullable: false),
                    AreasForImprovementJson = table.Column<string>(type: "jsonb", nullable: false),
                    GrammarCorrectionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    VocabularySuggestionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assessments_conversation_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "conversation_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MessageText = table.Column<string>(type: "text", nullable: false),
                    AudioStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversation_messages_conversation_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "conversation_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Meaning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Example = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MasteryLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ContextSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vocabulary_conversation_sessions_ContextSessionId",
                        column: x => x.ContextSessionId,
                        principalTable: "conversation_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_vocabulary_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "topics",
                columns: new[] { "Id", "CreatedAt", "Description", "IconKey", "IsActive", "Name", "RecommendedDifficulty", "SortOrder", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Casual everyday discussions about your routines, hobbies, weather, and weekend plans.", "MessageCircle", true, "Daily Conversation", "A1", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Practice answering behavioral interview questions, describing your experience and career strengths.", "Briefcase", true, "Job Interview", "B2", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Simulate airport check-ins, asking for directions, hotel bookings, and navigating foreign cities.", "Compass", true, "Travel", "B1", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Inquiring about sizes, prices, returns, bargaining politely, and making purchases in retail stores.", "ShoppingBag", true, "Shopping", "A2", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ordering meals, asking about ingredients, dietary requirements, requesting the bill, and complimenting the chef.", "UtensilsCrossed", true, "Restaurant", "A2", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Participating in team meetings, giving status updates, negotiating deadlines, and collaborating with colleagues.", "Building2", true, "Workplace", "B1", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Build confidence introducing your background, interests, profession, and family in personal or networking setups.", "UserCheck", true, "Introducing Yourself", "A1", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_assessments_SessionId",
                table: "assessments",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_SessionId",
                table: "conversation_messages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_SessionId_SequenceNumber",
                table: "conversation_messages",
                columns: new[] { "SessionId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_conversation_sessions_StartedAt",
                table: "conversation_sessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_sessions_TopicId",
                table: "conversation_sessions",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_sessions_UserId",
                table: "conversation_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_topics_Name",
                table: "topics",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_ContextSessionId",
                table: "vocabulary",
                column: "ContextSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_UserId",
                table: "vocabulary",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_UserId_Word",
                table: "vocabulary",
                columns: new[] { "UserId", "Word" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessments");

            migrationBuilder.DropTable(
                name: "conversation_messages");

            migrationBuilder.DropTable(
                name: "vocabulary");

            migrationBuilder.DropTable(
                name: "conversation_sessions");

            migrationBuilder.DropTable(
                name: "topics");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
