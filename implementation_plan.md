# Development Plan — Database Backup Utility

Aligned with the [official project specification](#project-requirements-source) and the existing [README roadmap](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/README.md).

---

## Current State Assessment

| Area | Status |
|---|---|
| [PostgreSqlProvider.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/PostgreSqlProvider.cs) | ✅ Working — [TestConnectionAsync()](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/PostgreSqlProvider.cs#13-32) + [BackupDatabaseAsync()](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/PostgreSqlProvider.cs#33-84) via Docker |
| [IDatabaseProvider.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/IDatabaseProvider.cs) | ✅ Interface defined (Test + Backup only) |
| [DatabaseConnectionInfo.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Models/DatabaseConnectionInfo.cs) | ✅ Model with Host, Port, DB, User, Password |
| [Program.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Program.cs) | ⚠️ Hardcoded test script — no CLI framework |
| [BackupCommand.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Commands/BackupCommand.cs) / [RestoreCommand.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Commands/RestoreCommand.cs) / [TestCommand.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Commands/TestCommand.cs) | ❌ Empty stubs |
| [LoggingService.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Services/LoggingService.cs) | ❌ Empty stub |
| NuGet packages | ⚠️ Only `Npgsql` installed — missing `System.CommandLine`, `Serilog` |

### Critical Bugs
1. `PostgreSqlProvider.TestConnectionAsync()` **ignores** injected `_connectionInfo` — hardcodes credentials
2. [IDatabaseProvider](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/IDatabaseProvider.cs#3-9) is missing `RestoreDatabaseAsync()`
3. No CLI argument parsing — [Program.cs](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Program.cs) is a throwaway script

---

## Phase 1 — CLI Foundation & PostgreSQL MVP

> **Goal**: A working CLI with `test`, `backup`, `restore` commands, logging, and proper `--help` output.

### Dependencies

#### [MODIFY] [DbBackupUtility.csproj](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/DbBackupUtility.csproj)
- Add `System.CommandLine` — CLI parsing with built-in `--help`
- Add `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File` — structured logging

### Provider Fixes

#### [MODIFY] [IDatabaseProvider.cs](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/IDatabaseProvider.cs)
- Add `Task RestoreDatabaseAsync(string backupFilePath)`
- Add `string ProviderName { get; }` property for identification

#### [MODIFY] [PostgreSqlProvider.cs](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/PostgreSqlProvider.cs)
- Fix [TestConnectionAsync()](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/PostgreSqlProvider.cs#13-32) — use `_connectionInfo` instead of hardcoded values
- Implement `RestoreDatabaseAsync()` via `docker exec pg_restore`
- Add error handling with descriptive messages

### Logging

#### [MODIFY] [LoggingService.cs](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Services/LoggingService.cs)
- Serilog config: Console sink + rolling file sink → `Logs/dbbackup.log`
- Log: start time, end time, duration, status, errors

### CLI Commands

#### [MODIFY] [TestCommand.cs](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Commands/TestCommand.cs)
- `test` subcommand — options: `--host`, `--port`, `--user`, `--password`, `--database`, `--provider`
- Validates credentials by calling [TestConnectionAsync()](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/PostgreSqlProvider.cs#13-32), prints success/failure

#### [MODIFY] [BackupCommand.cs](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Commands/BackupCommand.cs)
- `backup` subcommand — connection options + `--output` path
- Timestamped backup filename in `Backups/` by default

#### [MODIFY] [RestoreCommand.cs](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Commands/RestoreCommand.cs)
- `restore` subcommand — connection options + `--file` (required)
- Validates file existence, calls `RestoreDatabaseAsync()`

#### [MODIFY] [Program.cs](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Program.cs)
- Replace hardcoded script with `System.CommandLine` root command
- Register all subcommands, initialize Serilog

### Phase 1 Deliverables
- [ ] `dotnet run -- test --provider postgres --host localhost --port 5433 ...` → success/failure
- [ ] `dotnet run -- backup --provider postgres ...` → creates file in `Backups/`
- [ ] `dotnet run -- restore --provider postgres --file Backups/<file> ...` → restores DB
- [ ] `dotnet run -- --help` → shows all available commands and options
- [ ] All operations logged to `Logs/dbbackup.log` with timestamps and durations

---

## Phase 2 — Multi-DBMS Support

> **Goal**: Support MySQL, MongoDB, and SQLite alongside PostgreSQL.

### Provider Implementations

#### [NEW] Providers/MySqlProvider.cs
- `docker exec mysqldump` / `mysql` for backup/restore via Docker
- Implement full [IDatabaseProvider](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/IDatabaseProvider.cs#3-9) interface

#### [NEW] Providers/MongoDbProvider.cs
- `docker exec mongodump` / `mongorestore` for backup/restore
- Implement full [IDatabaseProvider](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/IDatabaseProvider.cs#3-9) interface

#### [NEW] Providers/SqliteProvider.cs
- Direct file-copy for backup, `sqlite3 .restore` for restore
- No Docker dependency — SQLite is file-based

### Provider Factory

#### [NEW] Providers/DatabaseProviderFactory.cs
- Factory method: `--provider` option → correct [IDatabaseProvider](file:///home/omar/CS%20&%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/DbBackupUtility/Providers/IDatabaseProvider.cs#3-9) instance
- Supported values: `postgres`, `mysql`, `mongo`, `sqlite`

### Test Infrastructure

#### [MODIFY] [docker-compose.yml](file:///home/omar/CS%20%26%20SE/computing%20foundations/database_systems/ArmyDays_Projects/database-backup-utility/docker-compose.yml)
- Add MySQL and MongoDB services for local testing

### Phase 2 Deliverables
- [ ] All four providers pass `test` command against their containers
- [ ] Backup + restore cycle works for each DBMS
- [ ] `--provider` flag works across all commands

---

## Phase 3 — Backup Types & Compression

> **Goal**: Full/incremental/differential backups and ZIP compression.

### Backup Type Support

#### [MODIFY] IDatabaseProvider.cs
- Add `Task BackupDatabaseAsync(string backupFilePath, BackupType type)` overload
- Add `BackupType` enum: `Full`, `Incremental`, `Differential`

Per-provider support:
| DBMS | Full | Incremental | Differential |
|---|---|---|---|
| PostgreSQL | ✅ `pg_dump` | ✅ WAL archiving | ⚠️ via WAL delta |
| MySQL | ✅ `mysqldump` | ✅ `--incremental-basedir` (Percona) | ⚠️ via binary log |
| MongoDB | ✅ `mongodump` | ✅ oplog-based | ❌ Not natively supported |
| SQLite | ✅ file copy | ⚠️ limited (manual diff) | ❌ Not natively supported |

- Add `--type` flag to `backup` command: `full` (default), `incremental`, `differential`

### Compression

#### [NEW] Services/CompressionService.cs
- `ICompressionService` with `CompressAsync(string sourcePath) → string archivePath`
- Implementation using `System.IO.Compression.ZipFile`
- Stream-based for large databases — avoid loading entire file into memory

#### [MODIFY] BackupCommand.cs
- Add `--compress` flag (default: `false`)
- After backup, optionally compress to `.zip`

### Phase 3 Deliverables
- [ ] `dotnet run -- backup --type full --compress ...` → compressed backup
- [ ] Incremental backup works for PostgreSQL and MySQL
- [ ] Large database backups don't cause memory issues

---

## Phase 4 — Storage Options (Local & Cloud)

> **Goal**: Store backups locally or upload to AWS S3 / Google Cloud / Azure Blob.

### Storage Abstraction

#### [NEW] Storage/IStorageProvider.cs
- `UploadAsync(string localPath, string remotePath)`
- `DownloadAsync(string remotePath, string localPath)`
- `ListBackupsAsync(string prefix) → List<string>`

#### [NEW] Storage/LocalStorageProvider.cs
- Default — file copy/move to `Backups/` directory

#### [NEW] Storage/S3StorageProvider.cs
- AWS SDK (`AWSSDK.S3`) — upload/download to S3 bucket
- Config: `--s3-bucket`, `--s3-region`, `--s3-key`, `--s3-secret`

#### [NEW] Storage/GoogleCloudStorageProvider.cs
- Google Cloud SDK (`Google.Cloud.Storage.V1`)
- Config: `--gcs-bucket`, `--gcs-key-file`

#### [NEW] Storage/AzureBlobStorageProvider.cs
- Azure SDK (`Azure.Storage.Blobs`)
- Config: `--azure-container`, `--azure-connection-string`

#### [MODIFY] BackupCommand.cs / RestoreCommand.cs
- Add `--storage` option: `local` (default), `s3`, `gcs`, `azure`
- Backup → upload; Restore → download then restore

### Phase 4 Deliverables
- [ ] `dotnet run -- backup --storage s3 --s3-bucket my-backups ...` → uploads to S3
- [ ] `dotnet run -- restore --storage s3 --file backups/testdb.zip ...` → downloads & restores
- [ ] Same flow works for GCS and Azure

---

## Phase 5 — Notifications, Selective Restore & Polish

> **Goal**: Slack notifications, advanced restore, cross-platform hardening.

### 📝 Recommended Commits
* `feat(notification): implement SlackNotificationService via webhooks`
* `feat(cli): implement --tables flag for selective restore`
* `fix(security): sanitize shell inputs and prevent plaintext logging of passwords`
* `docs: update README with full usage examples and CLI documentation`
* `ci: final cross-platform validation and smoke tests`

### Notifications

#### [NEW] Services/SlackNotificationService.cs
- Webhook-based Slack messages on backup completion/failure
- Config: `--slack-webhook` option
- Message includes: DB name, backup type, duration, status, file size

### Selective Restore

#### [MODIFY] RestoreCommand.cs
- Add `--tables` option — comma-separated list of tables/collections
- PostgreSQL: `pg_restore --table=<name>`
- MySQL: `mysql` with filtered dump
- MongoDB: `mongorestore --collection=<name>`

### Cross-Platform & Security

- Validate all commands work on Windows (PowerShell), Linux, and macOS
- Sanitize user inputs in shell commands to prevent injection
- Ensure passwords are not logged in plaintext

### Documentation

#### [MODIFY] README.md
- Update with final usage examples for all commands and providers
- Document all CLI options

### Phase 5 Deliverables
- [ ] Slack notification fires on backup completion
- [ ] `dotnet run -- restore --tables users,orders ...` → restores specific tables
- [ ] `dotnet run -- --help` shows comprehensive, clear documentation
- [ ] No passwords appear in log files
- [ ] Tool runs on Windows, Linux, and macOS

---

## Verification Plan

### Phase 1 — Core smoke tests
```bash
# Start test DB
docker compose up -d

# Test connection
dotnet run -- test --provider postgres --host localhost --port 5433 --user admin --password admin --database testdb

# Backup
dotnet run -- backup --provider postgres --host localhost --port 5433 --user admin --password admin --database testdb

# Verify backup file
ls -la Backups/

# Restore
dotnet run -- restore --provider postgres --host localhost --port 5433 --user admin --password admin --database testdb --file Backups/<file>

# Verify logs
cat Logs/dbbackup.log
```

### Later Phases
- Repeat the above cycle for each new DBMS provider (Phase 2)
- Test `--compress` and `--type` flags (Phase 3)
- Test cloud upload/download with real or mocked credentials (Phase 4)
- Verify Slack webhook and selective restore (Phase 5)

---

## Project Requirements Source

This plan implements all requirements from the official specification:

| Requirement | Phase |
|---|---|
| Multiple DBMS support (MySQL, PostgreSQL, MongoDB, SQLite) | Phase 1–2 |
| Connection parameters & testing | Phase 1 |
| Error handling for connection failures | Phase 1 |
| Full, incremental, differential backups | Phase 3 |
| Compression | Phase 3 |
| Local storage | Phase 1 (implicit) |
| Cloud storage (S3, GCS, Azure) | Phase 4 |
| Logging (start, end, duration, status, errors) | Phase 1 |
| Slack notifications | Phase 5 |
| Restore from backup | Phase 1 |
| Selective restore (tables/collections) | Phase 5 |
| Help command / user-friendly CLI | Phase 1 |
| Cross-platform compatibility | Phase 5 |
| Secure & reliable operations | Phase 5 |
| Handle large databases efficiently | Phase 3 |
