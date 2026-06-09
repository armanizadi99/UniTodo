using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class RunMemberConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Members",
                table: "todoListRuns");

            migrationBuilder.RenameColumn(
                name: "AsignedTo",
                table: "todoItems",
                newName: "AssignedTo");

            migrationBuilder.CreateTable(
                name: "RunMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RunId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RunMember_todoListRuns_RunId",
                        column: x => x.RunId,
                        principalTable: "todoListRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunMember_RunId",
                table: "RunMember",
                column: "RunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunMember");

            migrationBuilder.RenameColumn(
                name: "AssignedTo",
                table: "todoItems",
                newName: "AsignedTo");

            migrationBuilder.AddColumn<string>(
                name: "Members",
                table: "todoListRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
