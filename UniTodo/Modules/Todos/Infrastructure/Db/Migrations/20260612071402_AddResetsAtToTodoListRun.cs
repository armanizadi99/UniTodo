using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddResetsAtToTodoListRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResetsAt",
                table: "todoListRuns",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetsAt",
                table: "todoListRuns");
        }
    }
}
