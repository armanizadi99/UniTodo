using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class TodoModule5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_todoItemTemplates_Description",
                table: "todoItemTemplates");

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedBy",
                table: "todoItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_todoItemTemplates_Description_TodoListId",
                table: "todoItemTemplates",
                columns: new[] { "Description", "TodoListId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_todoItemTemplates_Description_TodoListId",
                table: "todoItemTemplates");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "todoItems");

            migrationBuilder.CreateIndex(
                name: "IX_todoItemTemplates_Description",
                table: "todoItemTemplates",
                column: "Description",
                unique: true);
        }
    }
}
