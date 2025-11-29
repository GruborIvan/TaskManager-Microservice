using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Migrations
{
    public partial class RenameTaskRelationDboToTaskRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskRelationDbo_Tasks_TaskId",
                table: "TaskRelationDbo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskRelationDbo",
                table: "TaskRelationDbo");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TaskRelationDbo");

            migrationBuilder.RenameTable(
                name: "TaskRelationDbo",
                newName: "TaskRelations");

            migrationBuilder.RenameIndex(
                name: "IX_TaskRelationDbo_TaskId",
                table: "TaskRelations",
                newName: "IX_TaskRelations_TaskId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Tasks",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ChangedDate",
                table: "Tasks",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Comments",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<Guid>(
                name: "RelationId",
                table: "TaskRelations",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskRelations",
                table: "TaskRelations",
                column: "RelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRelations_Tasks_TaskId",
                table: "TaskRelations",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskRelations_Tasks_TaskId",
                table: "TaskRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskRelations",
                table: "TaskRelations");

            migrationBuilder.DropColumn(
                name: "RelationId",
                table: "TaskRelations");

            migrationBuilder.RenameTable(
                name: "TaskRelations",
                newName: "TaskRelationDbo");

            migrationBuilder.RenameIndex(
                name: "IX_TaskRelations_TaskId",
                table: "TaskRelationDbo",
                newName: "IX_TaskRelationDbo_TaskId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ChangedDate",
                table: "Tasks",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Comments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TaskRelationDbo",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskRelationDbo",
                table: "TaskRelationDbo",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRelationDbo_Tasks_TaskId",
                table: "TaskRelationDbo",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
