using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Migrations
{
    public partial class add_reporting_views : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Reporting");

            migrationBuilder.Sql(@"
                    EXEC('CREATE VIEW Reporting.Tasks_V
                    AS
                    SELECT TaskId AS TaskId
                          ,CreatedDate AS CreatedDate
                          ,SourceId AS SourceId
                          ,TaskType AS TaskType
                          ,Data AS Data
                          ,Status AS Status
                          ,Callback AS Callback
                          ,FinalState AS FinalState
                          ,ChangedBy AS ChangedBy
                          ,ChangedDate AS ChangedDate
                          ,AssignedToEntityId AS AssignedToEntityId
                          ,AssignmentType AS AssignmentType
                          ,CreatedById AS CreatedById
                          ,FourEyeSubjectId AS FourEyeSubjectId
                          ,Change AS Change
                          ,SourceName AS SourceName
                          ,Subject AS Subject
                          ,DATEADD(DAY, 0, SysStartTime) AS SysStartTime
                          ,DATEADD(DAY, 0, SysEndTime) AS SysEndTime
                    FROM dbo.Tasks')");

            migrationBuilder.Sql(@"
                    EXEC('CREATE VIEW Reporting.TaskHistory_V
                    AS
                    SELECT TaskId AS TaskId
                          ,CreatedDate AS CreatedDate
                          ,SourceId AS SourceId
                          ,TaskType AS TaskType
                          ,Data AS Data
                          ,Status AS Status
                          ,Callback AS Callback
                          ,FinalState AS FinalState
                          ,ChangedBy AS ChangedBy
                          ,ChangedDate AS ChangedDate
                          ,AssignedToEntityId AS AssignedToEntityId
                          ,AssignmentType AS AssignmentType
                          ,CreatedById AS CreatedById
                          ,FourEyeSubjectId AS FourEyeSubjectId
                          ,Change AS Change
                          ,SourceName AS SourceName
                          ,Subject AS Subject
                          ,SysStartTime AS SysStartTime
                          ,SysEndTime AS SysEndTime
                    FROM dbo.TaskHistory')");


            migrationBuilder.Sql(@"
                    EXEC('CREATE VIEW Reporting.Comments_V
                    AS
                    SELECT TaskId AS TaskId
                          ,Text AS Text
                          ,CreatedDate AS CreatedDate
                          ,CreatedById AS CreatedById
                          ,CommentId AS CommentId
                    FROM dbo.Comments')");

            migrationBuilder.Sql(@"
                    EXEC('CREATE VIEW Reporting.TaskRelations_V
                    AS
                    SELECT EntityId AS EntityId
                          ,TaskId AS TaskId
                          ,EntityType AS EntityType
                          ,RelationId AS RelationId
                          ,IsMain AS IsMain
                    FROM dbo.TaskRelations')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('DROP VIEW Reporting.Comments_V;')");
            migrationBuilder.Sql(@"EXEC('DROP VIEW Reporting.Tasks_V;')");
            migrationBuilder.Sql(@"EXEC('DROP VIEW Reporting.TaskHistory_V;')");
            migrationBuilder.Sql(@"EXEC('DROP VIEW Reporting.TaskRelations_V;')");
        }
    }
}
