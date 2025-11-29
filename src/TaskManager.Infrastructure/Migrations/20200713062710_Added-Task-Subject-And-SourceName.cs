using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Migrations
{
    public partial class AddedTaskSubjectAndSourceName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceName",
                table: "Tasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceName",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Tasks");
        }
    }
}
