using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceRunAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RunMember_todoListRuns_RunId",
                table: "RunMember");

            migrationBuilder.DropTable(
                name: "todoItems");

            migrationBuilder.DropTable(
                name: "todoListRuns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember");

            migrationBuilder.RenameTable(
                name: "RunMember",
                newName: "runMembers");

            migrationBuilder.RenameIndex(
                name: "IX_RunMember_RunId",
                table: "runMembers",
                newName: "IX_runMembers_RunId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_runMembers",
                table: "runMembers",
                columns: new[] { "UserId", "RunId" });

            migrationBuilder.CreateTable(
                name: "runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResetPolicy = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ownerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ResetsAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    IsShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "runIterations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runIterations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runIterations_runs_RunId",
                        column: x => x.RunId,
                        principalTable: "runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "runItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunIterationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CompletedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AssignedTo = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runItems_runIterations_RunIterationId",
                        column: x => x.RunIterationId,
                        principalTable: "runIterations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_runItems_RunIterationId",
                table: "runItems",
                column: "RunIterationId");

            migrationBuilder.CreateIndex(
                name: "IX_runIterations_RunId",
                table: "runIterations",
                column: "RunId");

            migrationBuilder.AddForeignKey(
                name: "FK_runMembers_runs_RunId",
                table: "runMembers",
                column: "RunId",
                principalTable: "runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_runMembers_runs_RunId",
                table: "runMembers");

            migrationBuilder.DropTable(
                name: "runItems");

            migrationBuilder.DropTable(
                name: "runIterations");

            migrationBuilder.DropTable(
                name: "runs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_runMembers",
                table: "runMembers");

            migrationBuilder.RenameTable(
                name: "runMembers",
                newName: "RunMember");

            migrationBuilder.RenameIndex(
                name: "IX_runMembers_RunId",
                table: "RunMember",
                newName: "IX_RunMember_RunId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "todoListRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IsShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ResetPolicy = table.Column<int>(type: "INTEGER", nullable: false),
                    ResetsAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ownerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todoListRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "todoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedTo = table.Column<Guid>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CompletedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_todoItems_todoListRuns_RunId",
                        column: x => x.RunId,
                        principalTable: "todoListRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_todoItems_RunId",
                table: "todoItems",
                column: "RunId");

            migrationBuilder.AddForeignKey(
                name: "FK_RunMember_todoListRuns_RunId",
                table: "RunMember",
                column: "RunId",
                principalTable: "todoListRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
