using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTodo.Modules.Todos.Infrastructure.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddRunSettingsAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Permissions_MemberAllowdToRemoveItems",
                table: "runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Permissions_MemberAllowedToAddItems",
                table: "runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Permissions_MemberAllowedToChangeDescriptions",
                table: "runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Permissions_MemberAllowedToCompleteUnassignedItems",
                table: "runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Settings_EndOfWeekDay",
                table: "runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Settings_PreserveHystory",
                table: "runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Settings_TimeZone",
                table: "runs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Permissions_MemberAllowdToRemoveItems",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "Permissions_MemberAllowedToAddItems",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "Permissions_MemberAllowedToChangeDescriptions",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "Permissions_MemberAllowedToCompleteUnassignedItems",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "Settings_EndOfWeekDay",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "Settings_PreserveHystory",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "Settings_TimeZone",
                table: "runs");
        }
    }
}
