using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class InitialTodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResetPolicy = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ownerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResetsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsShared = table.Column<bool>(type: "bit", nullable: false),
                    Permissions_MemberAllowedToAddItems = table.Column<bool>(type: "bit", nullable: false),
                    Permissions_MemberAllowedToChangeDescriptions = table.Column<bool>(type: "bit", nullable: false),
                    Permissions_MemberAllowedToCompleteUnassignedItems = table.Column<bool>(type: "bit", nullable: false),
                    Permissions_MemberAllowedToMarkIncompleteUnassignedItems = table.Column<bool>(type: "bit", nullable: false),
                    Permissions_MemberAllowedToModifyNotesForUnassignedItems = table.Column<bool>(type: "bit", nullable: false),
                    Permissions_MemberAllowedToRemoveItems = table.Column<bool>(type: "bit", nullable: false),
                    Settings_EndOfWeekDay = table.Column<int>(type: "int", nullable: false),
                    Settings_PreserveHistory = table.Column<bool>(type: "bit", nullable: false),
                    Settings_TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "todoLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    ResetPolicy = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todoLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "runIterations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RunId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "runMembers",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runMembers", x => new { x.UserId, x.RunId });
                    table.ForeignKey(
                        name: "FK_runMembers_runs_RunId",
                        column: x => x.RunId,
                        principalTable: "runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "todoItemTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TodoListId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todoItemTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_todoItemTemplates_todoLists_TodoListId",
                        column: x => x.TodoListId,
                        principalTable: "todoLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "runItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunIterationId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssignedTo = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_runMembers_RunId",
                table: "runMembers",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_todoItemTemplates_Description_TodoListId",
                table: "todoItemTemplates",
                columns: new[] { "Description", "TodoListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_todoItemTemplates_TodoListId",
                table: "todoItemTemplates",
                column: "TodoListId");

            migrationBuilder.CreateIndex(
                name: "IX_todoLists_Name",
                table: "todoLists",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "runItems");

            migrationBuilder.DropTable(
                name: "runMembers");

            migrationBuilder.DropTable(
                name: "todoItemTemplates");

            migrationBuilder.DropTable(
                name: "runIterations");

            migrationBuilder.DropTable(
                name: "todoLists");

            migrationBuilder.DropTable(
                name: "runs");
        }
    }
}
