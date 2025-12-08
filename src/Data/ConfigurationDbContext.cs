using Microsoft.EntityFrameworkCore;
using AzureNamingTool.Models;

namespace AzureNamingTool.Data
{
    /// <summary>
    /// Entity Framework DbContext for SQLite configuration storage
    /// </summary>
    public class ConfigurationDbContext : DbContext
    {
        private readonly string _databasePath;

        /// <summary>
        /// Initializes a new instance of ConfigurationDbContext with a database path
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file</param>
        public ConfigurationDbContext(string databasePath)
        {
            _databasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
        }

        /// <summary>
        /// Initializes a new instance of ConfigurationDbContext with DbContext options
        /// </summary>
        /// <param name="options">Database context options</param>
        public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : base(options)
        {
            _databasePath = string.Empty; // Will use options for connection
        }

        /// <summary>
        /// Gets or sets the ResourceTypes entity set
        /// </summary>
        public DbSet<ResourceType> ResourceTypes { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceLocations entity set
        /// </summary>
        public DbSet<ResourceLocation> ResourceLocations { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceEnvironments entity set
        /// </summary>
        public DbSet<ResourceEnvironment> ResourceEnvironments { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceOrgs entity set
        /// </summary>
        public DbSet<ResourceOrg> ResourceOrgs { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceProjAppSvcs entity set
        /// </summary>
        public DbSet<ResourceProjAppSvc> ResourceProjAppSvcs { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceUnitDepts entity set
        /// </summary>
        public DbSet<ResourceUnitDept> ResourceUnitDepts { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceFunctions entity set
        /// </summary>
        public DbSet<ResourceFunction> ResourceFunctions { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceDelimiters entity set
        /// </summary>
        public DbSet<ResourceDelimiter> ResourceDelimiters { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the ResourceComponents entity set
        /// </summary>
        public DbSet<ResourceComponent> ResourceComponents { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the CustomComponents entity set
        /// </summary>
        public DbSet<CustomComponent> CustomComponents { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the AdminUsers entity set
        /// </summary>
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the AdminLogMessages entity set
        /// </summary>
        public DbSet<AdminLogMessage> AdminLogMessages { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the GeneratedNames entity set
        /// </summary>
        public DbSet<GeneratedName> GeneratedNames { get; set; } = null!;

        /// <summary>
        /// Gets or sets the AzureValidationSettings entity set
        /// Singleton settings - only one record with Id=1
        /// </summary>
        public DbSet<AzureValidationSettings> AzureValidationSettings { get; set; } = null!;

        /// <summary>
        /// Configures the database connection and options
        /// </summary>
        /// <param name="optionsBuilder">Options builder for database configuration</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_databasePath))
            {
                optionsBuilder.UseSqlite($"Data Source={_databasePath}");
            }
        }

        /// <summary>
        /// Configures entity models and relationships
        /// </summary>
        /// <param name="modelBuilder">Model builder for entity configuration</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ResourceType
            modelBuilder.Entity<ResourceType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // We manage IDs
                entity.Property(e => e.Resource).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500); // Nullable
                entity.Property(e => e.Enabled).IsRequired();
                entity.Property(e => e.ApplyDelimiter).IsRequired();
                // All other string properties with defaults
                entity.Property(e => e.Optional).HasMaxLength(50);
                entity.Property(e => e.Exclude).HasMaxLength(50);
                entity.Property(e => e.Property).HasMaxLength(200);
                entity.Property(e => e.Scope).HasMaxLength(100);
                entity.Property(e => e.LengthMin).HasMaxLength(10);
                entity.Property(e => e.LengthMax).HasMaxLength(10);
                entity.Property(e => e.ValidText).HasMaxLength(500);
                entity.Property(e => e.InvalidText).HasMaxLength(500);
                entity.Property(e => e.InvalidCharacters).HasMaxLength(200);
                entity.Property(e => e.InvalidCharactersStart).HasMaxLength(200);
                entity.Property(e => e.InvalidCharactersEnd).HasMaxLength(200);
                entity.Property(e => e.InvalidCharactersConsecutive).HasMaxLength(200);
                entity.Property(e => e.Regx).HasMaxLength(500);
                entity.Property(e => e.StaticValues).HasMaxLength(1000);
            });

            // Configure ResourceLocation
            modelBuilder.Entity<ResourceLocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Enabled).IsRequired();
            });

            // Configure ResourceEnvironment
            modelBuilder.Entity<ResourceEnvironment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SortOrder).IsRequired();
            });

            // Configure ResourceOrg
            modelBuilder.Entity<ResourceOrg>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SortOrder).IsRequired();
            });

            // Configure ResourceProjAppSvc
            modelBuilder.Entity<ResourceProjAppSvc>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SortOrder).IsRequired();
            });

            // Configure ResourceUnitDept
            modelBuilder.Entity<ResourceUnitDept>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SortOrder).IsRequired();
            });

            // Configure ResourceFunction
            modelBuilder.Entity<ResourceFunction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SortOrder).IsRequired();
            });

            // Configure ResourceDelimiter
            modelBuilder.Entity<ResourceDelimiter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Delimiter).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Enabled).IsRequired();
            });

            // Configure ResourceComponent
            modelBuilder.Entity<ResourceComponent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Enabled).IsRequired();
                entity.Property(e => e.SortOrder).IsRequired();
                entity.Property(e => e.MinLength).IsRequired();
                entity.Property(e => e.MaxLength).IsRequired();
            });

            // Configure CustomComponent
            modelBuilder.Entity<CustomComponent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ParentComponent).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SortOrder).IsRequired();
                entity.Property(e => e.MinLength).HasMaxLength(10);
                entity.Property(e => e.MaxLength).HasMaxLength(10);
            });

            // Configure AdminUser
            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Configure AdminLogMessage
            modelBuilder.Entity<AdminLogMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedOn).IsRequired();
            });

            // Configure GeneratedName
            modelBuilder.Entity<GeneratedName>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ResourceName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ResourceTypeName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.User).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.CreatedOn).IsRequired();
                // Components is a complex type (List<string[]>) - will be serialized as JSON
                entity.Property(e => e.Components)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string[]>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string[]>()
                    )
                    .HasColumnType("TEXT");
            });

            // Configure AzureValidationSettings (singleton - always Id=1)
            modelBuilder.Entity<AzureValidationSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Enabled).IsRequired();
                entity.Property(e => e.AuthMode).IsRequired();
                entity.Property(e => e.TenantId).HasMaxLength(100);
                entity.Property(e => e.ManagementGroupId).HasMaxLength(100);
                
                // SubscriptionIds is a List<string> - serialize as JSON
                entity.Property(e => e.SubscriptionIds)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                    )
                    .HasColumnType("TEXT");

                // ServicePrincipal is a complex type - serialize as JSON
                entity.Property(e => e.ServicePrincipal)
                    .HasConversion(
                        v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<ServicePrincipalSettings>(v, (System.Text.Json.JsonSerializerOptions?)null)
                    )
                    .HasColumnType("TEXT");

                // KeyVault is a complex type - serialize as JSON
                entity.Property(e => e.KeyVault)
                    .HasConversion(
                        v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<KeyVaultSettings>(v, (System.Text.Json.JsonSerializerOptions?)null)
                    )
                    .HasColumnType("TEXT");

                // ConflictResolution is a complex type - serialize as JSON
                entity.Property(e => e.ConflictResolution)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<ConflictResolutionSettings>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new ConflictResolutionSettings()
                    )
                    .HasColumnType("TEXT");

                // Cache is a complex type - serialize as JSON
                entity.Property(e => e.Cache)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<CacheSettings>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new CacheSettings()
                    )
                    .HasColumnType("TEXT");
            });
        }
    }
}
