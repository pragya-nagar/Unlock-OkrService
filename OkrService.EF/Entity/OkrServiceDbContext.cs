using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class OkrServiceDbContext : DataContext
    {
        public OkrServiceDbContext()
        {
        }

        public OkrServiceDbContext(DbContextOptions<OkrServiceDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Constant> Constant { get; set; }
        public virtual DbSet<ErrorLog> ErrorLog { get; set; }
        public virtual DbSet<GoalKey> GoalKey { get; set; }
        public virtual DbSet<GoalKeyAudit> GoalKeyAudit { get; set; }
        public virtual DbSet<GoalObjective> GoalObjective { get; set; }
        public virtual DbSet<TypeOfGoalCreation> TypeOfGoalCreation { get; set; }
        public virtual DbSet<UnLockLog> UnLockLog { get; set; }
        public virtual DbSet<MessageMaster> MessageMaster { get; set; }
        public virtual DbSet<UnlockSupportTeam> UnlockSupportTeam { get; set; }
        public virtual DbSet<GoalSequence> GoalSequence { get; set; }
        public virtual DbSet<GoalKeyHistory> GoalKeyHistory { get; set; }
        public virtual DbSet<TeamSequence> TeamSequence { get; set; }        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=52.21.77.184;Database=Okr_Service_QA;User Id=okr-admin;Password=abcd@1234;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Constant>(entity =>
            {
                entity.Property(e => e.ConstantName).HasMaxLength(500);

                entity.Property(e => e.ConstantValue).HasMaxLength(500);

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("PK__tbErrorL__5E5486487F68FA59");

                entity.Property(e => e.ApplicationName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ErrorDetail).IsRequired();

                entity.Property(e => e.FunctionName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PageName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<GoalKey>(entity =>
            {
                entity.HasKey(e => e.GoalKeyId)
                    .HasName("goalKey_pk")
                    .IsClustered(false);

                entity.HasIndex(e => e.GoalKeyId)
                    .HasName("myIndex")
                    .IsClustered();

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.EmployeeId).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Score).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.KeyNotes).HasColumnType("text");



                entity.HasOne(d => d.GoalObjective)
                    .WithMany(p => p.GoalKey)
                    .HasForeignKey(d => d.GoalObjectiveId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GoalKey__GoalObj__67DE6983");
            });

            modelBuilder.Entity<GoalKeyAudit>(entity =>
            {
                entity.Property(e => e.UpdatedColumn).HasMaxLength(250);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<GoalObjective>(entity =>
            {
                entity.HasKey(e => e.GoalObjectiveId)
                    .HasName("goalObjective_pk")
                    .IsClustered(false);

                entity.HasIndex(e => e.GoalObjectiveId)
                    .HasName("myIndex")
                    .IsClustered();

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ObjectiveDescription).HasColumnType("text");

                entity.Property(e => e.ObjectiveName)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Score).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Sequence).HasColumnType("int").HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<TypeOfGoalCreation>(entity =>
            {
                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.PrimaryText)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.SecondaryText)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<UnLockLog>(entity =>
            {
                entity.HasKey(e => e.UnLockLogId)
                    .HasName("unLockLog_pk")
                    .IsClustered(false);

                entity.HasIndex(e => e.UnLockLogId)
                    .HasName("myIndex")
                    .IsClustered();

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LockedOn).HasColumnType("datetime");

                entity.Property(e => e.LockedTill).HasColumnType("datetime");
            });

            modelBuilder.Entity<MessageMaster>(entity =>
            {
                entity.HasKey(e => e.MessageMasterId)
                    .HasName("PK__MessageM__4A0CDE96FAB419DE");

                entity.Property(e => e.MessageDesc).HasColumnType("NVARCHAR(1000)");

                entity.Property(e => e.IsActive).HasColumnType("bit");
            });

            modelBuilder.Entity<UnlockSupportTeam>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__UnlockSu__3214EC076063E603");

                entity.Property(e => e.EmailId).HasColumnType("NVARCHAR(80)");

                entity.Property(e => e.FullName).HasColumnType("NVARCHAR(100)");
            });

            modelBuilder.Entity<KrStatusMessage>(entity =>
            {
                entity.HasKey(e => e.KrStatusMessageId)
                    .HasName("PK__KrStatus__0F073272896360AC");

                entity.Property(e => e.CreatedOnAssignee).HasColumnType("datetime");
                entity.Property(e => e.CreatedOnAssigner).HasColumnType("datetime");
                entity.Property(e => e.IsActive).HasColumnType("bit");
            });
            modelBuilder.Entity<GoalSequence>(entity =>
            {
                entity.HasKey(e => e.SequenceId).HasName("PK_GoalSequence");
                entity.Property(e => e.GoalId).HasColumnType("bigint");
                entity.Property(e => e.EmployeeId).HasColumnType("bigint");
                entity.Property(e => e.GoalType).HasColumnType("int");
                entity.Property(e => e.GoalCycleId).HasColumnType("int");
                entity.Property(e => e.IsActive).HasColumnType("bit");
                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });
            modelBuilder.Entity<GoalKeyHistory>(entity =>
            {
                entity.HasKey(e => e.HistoryId).HasName("PK_GoalKeyHistory");
                entity.Property(e => e.GoalKeyId).HasColumnType("bigint");
                entity.Property(e => e.CurrentValue).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ContributorValue).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Score).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");
                entity.Property(e => e.CreatedBy).HasColumnType("bigint");
            });
            modelBuilder.Entity<TeamSequence>(entity =>
            {
                entity.HasKey(e => e.TeamSequenceId).HasName("PK__TeamSequ__4C042603CE20BC0E");
                entity.Property(e => e.TeamId).HasColumnType("bigint");
                entity.Property(e => e.EmployeeId).HasColumnType("bigint");
                entity.Property(e => e.CycleId).HasColumnType("int");
                entity.Property(e => e.Sequence).HasColumnType("int");
                entity.Property(e => e.IsActive).HasColumnType("bit");
                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });
           
        }
    }
}
