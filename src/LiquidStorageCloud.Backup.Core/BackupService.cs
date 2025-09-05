using LiquidStorageCloud.DataManagement.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;

namespace LiquidStorageCloud.Backup.Core;

public class BackupService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISurrealDbClient _surrealDb;
    private readonly S3Service _s3Service;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        ApplicationDbContext dbContext,
        ISurrealDbClient surrealDb,
        S3Service s3Service,
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _dbContext = dbContext;
        _surrealDb = surrealDb;
        _s3Service = s3Service;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task BackupSqlDatabaseAsync()
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var backupPath = Path.Combine(Path.GetTempPath(), $"sqlbackup_{timestamp}.bak");

            // Generate SQL backup script
            var connection = _dbContext.Database.GetDbConnection();
            var backupCommand = $"BACKUP DATABASE [{connection.Database}] TO DISK = '{backupPath}'";
            
            await _dbContext.Database.ExecuteSqlRawAsync(backupCommand);
            _logger.LogInformation("SQL backup created at {Path}", backupPath);

            // Upload to S3
            var s3Key = $"backups/sql/{Path.GetFileName(backupPath)}";
            await _s3Service.UploadFileAsync(backupPath, s3Key);

            // Delete local backup
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                _logger.LogInformation("Deleted local SQL backup file");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SQL database backup");
            throw;
        }
    }

    public async Task BackupSurrealDbAsync()
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var backupPath = Path.Combine(Path.GetTempPath(), $"surrealdb_{timestamp}.dump");

            // Export SurrealDB data
            var result = await _surrealDb.RawQuery("EXPORT");
            var exportData = result[0].ToString();
            await File.WriteAllTextAsync(backupPath, exportData);
            _logger.LogInformation("SurrealDB backup created at {Path}", backupPath);

            // Upload to S3
            var s3Key = $"backups/surrealdb/{Path.GetFileName(backupPath)}";
            await _s3Service.UploadFileAsync(backupPath, s3Key);

            // Delete local backup
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                _logger.LogInformation("Deleted local SurrealDB backup file");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SurrealDB backup");
            throw;
        }
    }
}
