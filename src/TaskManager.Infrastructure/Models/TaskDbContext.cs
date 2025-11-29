using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace TaskManager.Infrastructure.Models
{
    public partial class TasksDbContext : DbContext
    {
        private const string _newId = "NEWID()";

        public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
        {
            var sqlServerOptionsExtension = options.FindExtension<SqlServerOptionsExtension>();
            if (sqlServerOptionsExtension != null)
            {
                var connection = (SqlConnection)Database.GetDbConnection();
                connection.AccessToken =
                    (new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider())
                    .GetAccessTokenAsync("https://database.windows.net/").Result;
            }
        }

        public virtual DbSet<TaskDbo> Tasks { get; set; }
        public virtual DbSet<CommentDbo> Comments { get; set; }
        public virtual DbSet<TaskRelationDbo> TaskRelations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskDbo>(entity =>
            {
                entity.HasKey(t => t.TaskId);
                entity.Property(t => t.TaskId).ValueGeneratedOnAdd();
                entity.Ignore(t => t.DomainEvents);
            });

            modelBuilder.Entity<CommentDbo>(entity =>
            {
                entity.HasKey(t => t.CommentId);
                entity.Property(t => t.CommentId).ValueGeneratedOnAdd();
                entity.Ignore(t => t.DomainEvents);
            });

            modelBuilder.Entity<TaskRelationDbo>(entity =>
            {
                entity.HasKey(t => t.RelationId);
                entity.Property(t => t.RelationId)
                    .HasDefaultValueSql(_newId)
                    .ValueGeneratedOnAdd();
                entity.Ignore(t => t.DomainEvents);
            });
        }
    }
}
