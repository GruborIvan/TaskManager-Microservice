using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Migrations
{
    public partial class AddAssignmentToTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToEntityId",
                table: "Tasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentType",
                table: "Tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToEntityId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AssignmentType",
                table: "Tasks");
        }
    }
}
