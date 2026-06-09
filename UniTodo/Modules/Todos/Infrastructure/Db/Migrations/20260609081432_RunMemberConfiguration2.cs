using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class RunMemberConfiguration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RunMember");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RunMember",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunMember",
                table: "RunMember",
                column: "Id");
        }
    }
}
