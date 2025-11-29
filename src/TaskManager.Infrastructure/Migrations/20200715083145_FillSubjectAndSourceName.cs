using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Migrations
{
    public partial class FillSubjectAndSourceName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE dbo.Tasks SET Subject = 'Subject' WHERE ISNULL(Subject, '') = ''
UPDATE dbo.Tasks SET SourceName = 'WorkflowName' WHERE ISNULL(SourceName, '') = ''
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE dbo.Tasks SET Subject = NULL WHERE Subject = 'Subject'
UPDATE dbo.Tasks SET SourceName = NULL WHERE SourceName = 'WorkflowName'
");
        }
    }
}
