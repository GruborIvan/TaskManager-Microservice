using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Migrations
{
    public partial class ExtendTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Tasks",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "ChangedBy",
                table: "Tasks",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedDate",
                table: "Tasks",
                nullable: true,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.Sql(
                @"exec ('CREATE TRIGGER [dbo].[Tasks_UPDATE] ON [dbo].[Tasks]
                    AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF ((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                    DECLARE @Id UNIQUEIDENTIFIER

                    SELECT @Id = INSERTED.TaskId
                    FROM INSERTED

                    UPDATE dbo.Tasks
                    SET ChangedDate = GETUTCDATE()
                    WHERE TaskId = @Id
                END')");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Tasks",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ChangedDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tasks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[Tasks_UPDATE];");
        }
    }
}
