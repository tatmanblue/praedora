using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Praedora.Infrastructure.Data.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: false),
                    RoleTitle = table.Column<string>(type: "TEXT", nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    JobDescriptionRaw = table.Column<string>(type: "TEXT", nullable: false),
                    JobDescriptionHtml = table.Column<string>(type: "TEXT", nullable: true),
                    JdHash = table.Column<string>(type: "TEXT", nullable: false),
                    JdExtract = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ResumeVersionUsed = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    HasFlaggedDuplicate = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CandidateEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MatchedApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceEmailId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProposedStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    Reasoning = table.Column<string>(type: "TEXT", nullable: true),
                    RawSnippet = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyConflictFlagged = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DetectedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalMessageId = table.Column<string>(type: "TEXT", nullable: false),
                    FromAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    BodyText = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ClassificationResultJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false),
                    Component = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    ContextJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceEmailId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Confidence = table.Column<double>(type: "REAL", nullable: true),
                    RawSnippet = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusEvents_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CompanyName",
                table: "Applications",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ApplicationId",
                table: "Contacts",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_ExternalMessageId",
                table: "EmailMessages",
                column: "ExternalMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusEvents_ApplicationId",
                table: "StatusEvents",
                column: "ApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "CandidateEvents");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "StatusEvents");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
