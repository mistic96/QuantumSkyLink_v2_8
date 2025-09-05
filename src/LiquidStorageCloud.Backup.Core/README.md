# LiquidStorageCloud Backup Core

This package provides backup functionality for LiquidStorageCloud, including:

- SQL Database backup capabilities
- SurrealDB backup support
- S3 integration for cloud storage
- Automated backup scheduling
- Secure backup handling

## Features

- Automated database backups
- S3 storage integration
- Multiple database support (SQL + SurrealDB)
- Secure file handling
- Configurable backup schedules

## Usage

Configure the backup service in your application:

```csharp
services.AddScoped<BackupService>();
services.AddScoped<S3Service>();
```

For more information, see the [documentation](../docs/).
