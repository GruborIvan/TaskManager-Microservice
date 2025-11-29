using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Migrations
{
    public partial class AddTemporalTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
ALTER TABLE dbo.Tasks
    ADD
        SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN
            CONSTRAINT DF_SysStart DEFAULT SYSUTCDATETIME()
      , SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN
            CONSTRAINT DF_SysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
        PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
");

            migrationBuilder.Sql(@"
ALTER TABLE dbo.Tasks
    SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.TaskHistory));
");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE dbo.Tasks SET (SYSTEM_VERSIONING = OFF);");

            migrationBuilder.Sql(@"ALTER TABLE dbo.Tasks DROP PERIOD FOR SYSTEM_TIME;");
            migrationBuilder.Sql(@"ALTER TABLE dbo.Tasks DROP CONSTRAINT DF_SysStart");
            migrationBuilder.Sql(@"ALTER TABLE dbo.Tasks DROP  CONSTRAINT DF_SysEnd");
            migrationBuilder.Sql(@"ALTER TABLE dbo.Tasks DROP COLUMN SysEndTime");
            migrationBuilder.Sql(@"ALTER TABLE dbo.Tasks DROP COLUMN SysStartTime");

            migrationBuilder.Sql(@"DROP TABLE dbo.TaskHistory");
        }
    }
}
