using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class Todo4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "todoLists",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "todoItemTemplates",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_todoLists_Name",
                table: "todoLists",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_todoItemTemplates_Description",
                table: "todoItemTemplates",
                column: "Description",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_todoLists_Name",
                table: "todoLists");

            migrationBuilder.DropIndex(
                name: "IX_todoItemTemplates_Description",
                table: "todoItemTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "todoLists",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "todoItemTemplates",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldCollation: "NOCASE");
        }
    }
}
