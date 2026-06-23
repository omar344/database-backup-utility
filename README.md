# Database Backup Utility

A cross-platform command-line utility for backing up and restoring any type of database. Built with .NET, the tool supports multiple database management systems (DBMS) through a provider-based architecture, with backup compression, local and cloud storage options, activity logging, and optional Slack notifications.

## Project Overview

Database Backup Utility is a CLI application that simplifies database backup and restore operations across different database systems. It acts as a unified interface by leveraging native database tools:

| DBMS | Backup Tool | Restore Tool |
|---|---|---|
| PostgreSQL | `pg_dump` | `pg_restore` |
| MySQL | `mysqldump` | `mysql` |
| MongoDB | `mongodump` | `mongorestore` |
| SQLite | File copy | `.restore` |

The architecture is extensible — additional database providers and storage backends can be added with minimal changes to the core application.

---

## Features

### Database Connectivity

* Support for multiple DBMS: PostgreSQL, MySQL, MongoDB, SQLite
* Configurable connection parameters (host, port, username, password, database name)
* Connection validation before backup/restore operations
* Detailed error handling for connection failures

### Backup Operations

* **Full backups** — complete database snapshots
* **Incremental backups** — capture only changes since last backup (where supported)
* **Differential backups** — capture changes since last full backup (where supported)
* Secure and reliable backup generation
* Designed to handle large databases efficiently

### Compression

* Compress backup files to reduce storage space
* ZIP archive support via `System.IO.Compression`
* Stream-based compression for large files

### Storage Options

* **Local Storage** — store backups directly on the filesystem
* **AWS S3** — upload/download backups to S3 buckets
* **Google Cloud Storage** — store backups in GCS
* **Azure Blob Storage** — store backups in Azure containers

### Logging

Track all backup activities including:

* Start time and end time
* Execution duration
* Backup status (success/failure)
* Error details

Logs are written to:

```text
Logs/
└── dbbackup.log
```

### Notifications

Optional Slack notifications (via webhook) for:

* Backup completion
* Backup failures
* Restore completion

### Restore Operations

* Restore full database backups
* **Selective restore** — restore specific tables or collections (where supported by the DBMS)
* Backup integrity validation before restore

---

## Architecture

```text
DbBackupUtility/
│
├── Commands/
│   ├── BackupCommand.cs          # CLI backup subcommand
│   ├── RestoreCommand.cs         # CLI restore subcommand
│   └── TestCommand.cs            # CLI connection test subcommand
│
├── Providers/
│   ├── IDatabaseProvider.cs      # Provider interface
│   ├── PostgreSqlProvider.cs     # PostgreSQL implementation
│   ├── MySqlProvider.cs          # MySQL implementation (planned)
│   ├── MongoDbProvider.cs        # MongoDB implementation (planned)
│   ├── SqliteProvider.cs         # SQLite implementation (planned)
│   └── DatabaseProviderFactory.cs # Provider factory (planned)
│
├── Services/
│   ├── LoggingService.cs         # Serilog-based logging
│   ├── CompressionService.cs     # ZIP compression (planned)
│   └── SlackNotificationService.cs # Slack webhooks (planned)
│
├── Storage/
│   ├── IStorageProvider.cs       # Storage interface (planned)
│   ├── LocalStorageProvider.cs   # Local filesystem (planned)
│   ├── S3StorageProvider.cs      # AWS S3 (planned)
│   ├── GoogleCloudStorageProvider.cs # GCS (planned)
│   └── AzureBlobStorageProvider.cs   # Azure Blob (planned)
│
├── Models/
│   └── DatabaseConnectionInfo.cs # Connection parameters model
│
├── Backups/                      # Default backup output directory
├── Logs/                         # Log file output directory
└── Program.cs                    # Application entry point
```

---

## Technology Stack

* **.NET 10** / **C#**
* **System.CommandLine** — CLI argument parsing and `--help` generation
* **Serilog** — structured logging (Console + File sinks)
* **Npgsql** — PostgreSQL connectivity
* **Docker** — containerized database environments for backup/restore
* **System.IO.Compression** — ZIP file compression

---

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker](https://www.docker.com/) (for containerized database operations)

### Installation

```bash
git clone https://github.com/omar344/database-backup-utility.git
cd database-backup-utility
dotnet restore DbBackupUtility/DbBackupUtility.csproj
```

### Start the Test Database

```bash
docker compose up -d
```

---

## Usage

### Test Connection

```bash
dotnet run --project DbBackupUtility -- test \
  --provider postgres \
  --host localhost \
  --port 5433 \
  --user admin \
  --password admin \
  --database testdb
```

### Create Backup

```bash
dotnet run --project DbBackupUtility -- backup \
  --provider postgres \
  --host localhost \
  --port 5433 \
  --user admin \
  --password admin \
  --database testdb
```

### Restore Backup

```bash
dotnet run --project DbBackupUtility -- restore \
  --provider postgres \
  --host localhost \
  --port 5433 \
  --user admin \
  --password admin \
  --database testdb \
  --file Backups/testdb_2026-06-23.backup
```

### Help

```bash
dotnet run --project DbBackupUtility -- --help
```

---

## Roadmap

### Phase 1 — CLI Foundation & PostgreSQL MVP

* [x] .NET project structure and provider abstraction
* [x] PostgreSQL connection testing, backup, and restore
* [x] Docker-based backup execution
* [x] Serilog logging (Console + File)
* [ ] System.CommandLine CLI with `test`, `backup`, `restore` subcommands

### Phase 2 — Multi-DBMS Support

* [ ] MySQL provider
* [ ] MongoDB provider
* [ ] SQLite provider
* [ ] Provider factory with `--provider` flag

### Phase 3 — Backup Types & Compression

* [ ] Full, incremental, and differential backup support
* [ ] ZIP compression with `--compress` flag
* [ ] Stream-based compression for large databases

### Phase 4 — Storage Options

* [ ] Local storage abstraction
* [ ] AWS S3 integration
* [ ] Google Cloud Storage integration
* [ ] Azure Blob Storage integration

### Phase 5 — Notifications, Selective Restore & Polish

* [ ] Slack webhook notifications
* [ ] Selective table/collection restore with `--tables` flag
* [ ] Cross-platform validation (Windows, Linux, macOS)
* [ ] Security hardening (input sanitization, password masking in logs)
* [ ] Comprehensive `--help` documentation

---

## Constraints & Design Principles

* **Cross-platform** — works on Windows, Linux, and macOS
* **Secure** — passwords are never logged in plaintext; shell inputs are sanitized
* **Efficient** — stream-based processing for large databases
* **Extensible** — new providers and storage backends via interfaces
* **User-friendly** — clear CLI help, descriptive error messages

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
