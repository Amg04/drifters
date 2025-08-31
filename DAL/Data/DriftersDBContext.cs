using DAL.Models;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;

namespace DAL.Data
{
    public class DriftersDBContext : IdentityDbContext<AppUser>
    {
        public DriftersDBContext(DbContextOptions<DriftersDBContext> options) : base(options) { }
        public DriftersDBContext() { }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) { }

        public DbSet<Detection> Detections { get; set; }
        public DbSet<Camera> Cameras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region rename identity Table (AspNetUsers)

            modelBuilder.Entity<AppUser>().ToTable("Users", "Security");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "Security");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "Security");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "Security");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "Security");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "Security");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "Security");

            #endregion

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            ModelBuilder modelBuilder1 = modelBuilder.Entity<Detection>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(d => d.CameraId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(d => d.ObjectType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.Confidence)
                    .IsRequired()
                    .HasPrecision(5, 4); // e.g., 0.9856

                entity.Property(d => d.Timestamp)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(d => d.ImagePath)
                    .HasMaxLength(500);

                // Configure BoundingBox as owned entity (value object)
                entity.OwnsOne(d => d.BoundingBox, bb =>
                {
                    bb.Property(b => b.X)
                        .HasColumnName("BoundingBox_X")
                        .HasPrecision(8, 4);

                    bb.Property(b => b.Y)
                        .HasColumnName("BoundingBox_Y")
                        .HasPrecision(8, 4);

                    bb.Property(b => b.Width)
                        .HasColumnName("BoundingBox_Width")
                        .HasPrecision(8, 4);

                    bb.Property(b => b.Height)
                        .HasColumnName("BoundingBox_Height")
                        .HasPrecision(8, 4);
                });

                // Indexes for performance
                entity.HasIndex(d => d.CameraId)
                    .HasDatabaseName("IX_Detection_CameraId");

                entity.HasIndex(d => d.Timestamp)
                    .HasDatabaseName("IX_Detection_Timestamp");

                entity.HasIndex(d => d.ObjectType)
                    .HasDatabaseName("IX_Detection_ObjectType");

                entity.HasIndex(d => d.Confidence)
                    .HasDatabaseName("IX_Detection_Confidence");

                // Composite index for common queries
                entity.HasIndex(d => new { d.CameraId, d.Timestamp })
                    .HasDatabaseName("IX_Detection_CameraId_Timestamp");

                // Foreign key relationship
                entity.HasOne(d => d.Camera)
                    .WithMany(c => c.Detections)
                    .HasForeignKey(d => d.CameraId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Table name
                entity.ToTable("Detections");
                // Configure Camera entity
                modelBuilder.Entity<Camera>(entity =>
                {
                    entity.HasKey(c => c.Id);

                    entity.Property(c => c.Id)
                        .HasMaxLength(50);

                    entity.Property(c => c.Name)
                        .IsRequired()
                        .HasMaxLength(200);

                    entity.Property(c => c.StreamUrl)
                        .IsRequired()
                        .HasMaxLength(500);

                    entity.Property(c => c.IsActive)
                        .IsRequired()
                        .HasDefaultValue(true);

                    // Configure CameraSettings as owned entity (value object)
                    entity.OwnsOne(c => c.Settings, settings =>
                    {
                        settings.Property(s => s.Location)
                            .HasColumnName("Settings_Location")
                            .HasMaxLength(200);

                        settings.Property(s => s.Resolution)
                            .HasColumnName("Settings_Resolution")
                            .HasMaxLength(50)
                            .HasDefaultValue("1920x1080");

                        settings.Property(s => s.Fps)
                            .HasColumnName("Settings_Fps")
                            .HasDefaultValue(30);

                        settings.Property(s => s.DetectionThreshold)
                            .HasColumnName("Settings_DetectionThreshold")
                            .HasPrecision(3, 2)
                            .HasDefaultValue(0.5f);

                        settings.Property(s => s.SaveImages)
                            .HasColumnName("Settings_SaveImages")
                            .HasDefaultValue(true);

                        settings.Property(s => s.EnableRecording)
                            .HasColumnName("Settings_EnableRecording")
                            .HasDefaultValue(false);

                        settings.Property(s => s.RecordingPath)
                            .HasColumnName("Settings_RecordingPath")
                            .HasMaxLength(500);

                        settings.Property(s => s.MaxStorageDays)
                            .HasColumnName("Settings_MaxStorageDays")
                            .HasDefaultValue(30);

                        // JSON conversion for complex properties
                        settings.Property(s => s.DetectionZones)
                            .HasColumnName("Settings_DetectionZones")
                            .HasConversion(
                                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                                v => JsonSerializer.Deserialize<List<DetectionZone>>(v, (JsonSerializerOptions)null));

                        settings.Property(s => s.AlertSettings)
                            .HasColumnName("Settings_AlertSettings")
                            .HasConversion(
                                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                                v => JsonSerializer.Deserialize<AlertSettings>(v, (JsonSerializerOptions)null));

                        settings.Property(s => s.ScheduleSettings)
                            .HasColumnName("Settings_ScheduleSettings")
                            .HasConversion(
                                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                                v => JsonSerializer.Deserialize<ScheduleSettings>(v, (JsonSerializerOptions)null));
                    });

                    // Indexes
                    entity.HasIndex(c => c.Name)
                        .IsUnique()
                        .HasDatabaseName("IX_Camera_Name_Unique");

                    entity.HasIndex(c => c.IsActive)
                        .HasDatabaseName("IX_Camera_IsActive");

                    // Table name
                    entity.ToTable("Cameras");
                });

                SeedData(modelBuilder);
            };
            
        }
            




        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default cameras
            var defaultCameras = new[]
            {
                new Camera
                {
                    Id = 1,
                    Name = "Front Door",
                    StreamUrl = "rtsp://192.168.1.100:554/stream1",
                    IsActive = true,
                    Settings = new CameraSettings
                    {
                        Location = "Main Entrance",
                        Resolution = "1920x1080",
                        Fps = 30,
                        DetectionThreshold = 0.7f,
                        SaveImages = true,
                        EnableRecording = false,
                        MaxStorageDays = 30,
                        DetectionZones = new List<DetectionZone>(),
                        AlertSettings = new AlertSettings
                        {
                            EmailEnabled = false,
                            SmsEnabled = false,
                            WebhookEnabled = false
                        },
                        ScheduleSettings = new ScheduleSettings
                        {
                            IsEnabled = true,
                            WorkingHours = new List<TimeRange>()
                        }
                    }
                },
                new Camera
                {
                    Id = 2,
                    Name = "Parking Lot",
                    StreamUrl = "rtsp://192.168.1.101:554/stream1",
                    IsActive = true,
                    Settings = new CameraSettings
                    {
                        Location = "Parking Area",
                        Resolution = "1920x1080",
                        Fps = 25,
                        DetectionThreshold = 0.6f,
                        SaveImages = true,
                        EnableRecording = true,
                        RecordingPath = "/recordings/parking",
                        MaxStorageDays = 14,
                        DetectionZones = new List<DetectionZone>(),
                        AlertSettings = new AlertSettings
                        {
                            EmailEnabled = true,
                            SmsEnabled = false,
                            WebhookEnabled = false,
                            EmailRecipients = new List<string> { "admin@company.com" }
                        },
                        ScheduleSettings = new ScheduleSettings
                        {
                            IsEnabled = true,
                            WorkingHours = new List<TimeRange>
                            {
                                new TimeRange { Start = new TimeSpan(6, 0, 0), End = new TimeSpan(22, 0, 0) }
                            }
                        }
                    }
                }
            };
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-set timestamps for new detections
            var detectionEntries = ChangeTracker.Entries<Detection>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in detectionEntries)
            {
                if (entry.Entity.Timestamp == default)
                {
                    entry.Entity.Timestamp = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        // Method to ensure database is created and seeded
        public async Task EnsureDatabaseCreatedAsync()
        {
            await Database.EnsureCreatedAsync();
        }

        // Method to apply pending migrations
        public async Task MigrateDatabaseAsync()
        {
            await Database.MigrateAsync();
        }
    }
}
