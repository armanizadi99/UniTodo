using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRunIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RunMember_todoListRuns_RunId",
                table: "RunMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember");

            migrationBuilder.DropIndex(
                name: "IX_RunMember_RunId",
                table: "RunMember");

            migrationBuilder.DropColumn(
                name: "RunId",
                table: "RunMember");

            migrationBuilder.RenameTable(
                name: "RunMember",
                newName: "runMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "TodoListRunId",
                table: "runMembers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_todoListRuns_RunId",
                table: "todoListRuns",
                column: "RunId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_runMembers",
                table: "runMembers",
                columns: new[] { "UserId", "TodoListRunId" });

            migrationBuilder.CreateIndex(
                name: "IX_todoListRuns_RunId",
                table: "todoListRuns",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_runMembers_TodoListRunId",
                table: "runMembers",
                column: "TodoListRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_runMembers_todoListRuns_TodoListRunId",
                table: "runMembers",
                column: "TodoListRunId",
                principalTable: "todoListRuns",
                principalColumn: "RunId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_runMembers_todoListRuns_TodoListRunId",
                table: "runMembers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_todoListRuns_RunId",
                table: "todoListRuns");

            migrationBuilder.DropIndex(
                name: "IX_todoListRuns_RunId",
                table: "todoListRuns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_runMembers",
                table: "runMembers");

            migrationBuilder.DropIndex(
                name: "IX_runMembers_TodoListRunId",
                table: "runMembers");

            migrationBuilder.DropColumn(
                name: "TodoListRunId",
                table: "runMembers");

            migrationBuilder.RenameTable(
                name: "runMembers",
                newName: "RunMember");

            migrationBuilder.AddColumn<int>(
                name: "RunId",
                table: "RunMember",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RunMember_RunId",
                table: "RunMember",
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
