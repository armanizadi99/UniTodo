using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class TodoListRunChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "description",
                table: "todoItems",
                newName: "Description");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosedAt",
                table: "todoListRuns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "todoListRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Members",
                table: "todoListRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RunId",
                table: "todoListRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "todoItems",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<Guid>(
                name: "AsignedTo",
                table: "todoItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "todoItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "todoListRuns");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "todoListRuns");

            migrationBuilder.DropColumn(
                name: "Members",
                table: "todoListRuns");

            migrationBuilder.DropColumn(
                name: "RunId",
                table: "todoListRuns");

            migrationBuilder.DropColumn(
                name: "AsignedTo",
                table: "todoItems");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "todoItems");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "todoItems",
                newName: "description");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "todoItems",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
