# Database Backup Utility

A clean, developer-friendly CLI for PostgreSQL backup and restore workflows with Docker-powered execution.

## Why this project

This project demonstrates practical backend engineering skills:
- Designing an extensible provider architecture
- Building a real CLI UX with `System.CommandLine`
- Orchestrating external tools (`pg_dump`, `pg_restore`) through Docker
- Adding logging and operational observability

## Current capabilities

- ✅ Test PostgreSQL connectivity
- ✅ Create PostgreSQL backups from a running container
- ✅ Restore backups into PostgreSQL
- ✅ Friendly CLI commands and `--help` output
- ✅ Structured logging to console and file (`Logs/dbbackup.log`)

## Tech stack

- .NET 10 / C#
- System.CommandLine
- Npgsql
- Serilog (Console + File sinks)
- Docker + PostgreSQL

## Project structure

```text
DbBackupUtility/
├── Commands/
│   ├── BackupCommand.cs
│   ├── RestoreCommand.cs
│   └── TestCommand.cs
├── Models/
│   └── DatabaseConnectionInfo.cs
├── Providers/
│   ├── IDatabaseProvider.cs
│   ├── DatabaseProviderFactory.cs
│   └── PostgreSqlProvider.cs
├── Services/
│   └── LoggingService.cs
└── Program.cs
```

## Quick start

### 1) Prerequisites

- .NET 10 SDK
- Docker

### 2) Clone and restore

```bash
git clone https://github.com/omar344/database-backup-utility.git
cd database-backup-utility
dotnet restore DbBackupUtility/DbBackupUtility.csproj
```

### 3) Start PostgreSQL container

```bash
docker compose up -d
```

## Usage

### See CLI help

```bash
dotnet run --project DbBackupUtility -- --help
```

### Test connection

```bash
dotnet run --project DbBackupUtility -- test \
  --provider postgres \
  --host localhost \
  --port 5433 \
  --user admin \
  --password admin \
  --database testdb
```

### Create backup

```bash
dotnet run --project DbBackupUtility -- backup \
  --provider postgres \
  --host localhost \
  --port 5433 \
  --user admin \
  --password admin \
  --database testdb
```

### Restore backup

```bash
dotnet run --project DbBackupUtility -- restore \
  --provider postgres \
  --host localhost \
  --port 5433 \
  --user admin \
  --password admin \
  --database testdb \
  --file Backups/testdb_20260623_130000.backup
```

## LinkedIn-ready project summary

If you want to share this project on LinkedIn, you can describe it as:

> Built a cross-platform .NET CLI utility for PostgreSQL backup/restore automation. Implemented provider abstraction, command-based UX with System.CommandLine, Docker-based pg_dump/pg_restore execution, and structured logging with Serilog.

## Next roadmap items

- Add MySQL, MongoDB, and SQLite providers
- Add compression and cloud storage backends
- Add CI and automated test coverage
- Harden security (argument sanitization and secret-safe logging)

## License

MIT License. See [LICENSE](LICENSE).
