# Backup and Restore Guide

This guide explains how to backup and restore your Azure Naming Tool configuration data. The process varies depending on which storage provider you're using: **File System (JSON)** or **SQLite Database**.

## Understanding Storage Providers

The Azure Naming Tool supports two storage providers:

- **File System**: Uses JSON files stored in the `repository` folder (default for new installations)
- **SQLite**: Uses a database file (`azurenamingtool.db`) for improved performance and reliability

You can check which storage provider you're using in the **Admin** page under the "Storage Provider Migration" section.

---

## Backup and Restore with File System (JSON)

When using the File System storage provider, your configuration data is stored in JSON files.

### Creating a Backup

1. Navigate to the **Configuration** page
2. Scroll to the **Global Configuration** section
3. Click **Export** to download your complete configuration as a JSON file
4. *(Optional)* Check "Include Security/Identity Provider Settings?" if you want to include admin settings
5. Save the downloaded JSON file to a secure location

### Restoring from Backup

1. Navigate to the **Configuration** page
2. Scroll to the **Global Configuration** section
3. In the **Import Global Configuration** card, paste your JSON configuration into the text area
4. Click **Import**
5. Confirm the action (this will overwrite your current configuration)

---

## Backup and Restore with SQLite Database

When using the SQLite storage provider, you have multiple backup and restore options.

### Creating a Backup

You can create backups in two formats:

#### Option 1: Database File Backup (Recommended)
1. Navigate to the **Configuration** page
2. Scroll to the **Global Configuration** section
3. Under **Export the current Global Configuration**, find the **Download Database File** option
4. Click **Download Database** to save a complete copy of your database file
5. The file will be saved as `azurenamingtool-backup-[timestamp].db`

**When to use**: For quick disaster recovery and routine backups. This is the fastest and most reliable backup method.

#### Option 2: JSON Export
1. Navigate to the **Configuration** page
2. Scroll to the **Global Configuration** section
3. Under **Export the current Global Configuration**, find the **Export as JSON** option
4. *(Optional)* Check "Include Security/Identity Provider Settings?" if you want to include admin settings
5. Click **Export JSON** to download your configuration as a JSON file

**When to use**: For sharing configurations, version control, or migrating to a different instance.

### Restoring from Backup

You can restore from either a database file or a JSON file:

#### Option 1: Restore from Database File
1. Navigate to the **Configuration** page
2. Scroll to the **Global Configuration** section
3. Under **Import Global Configuration**, find the **Restore from Database File** option
4. Click **Choose File** and select your `.db` backup file
5. Click **Restore Database**
6. Confirm the action (a pre-restore backup will be created automatically)
7. The application will restart automatically

**Important**: This replaces your entire database. A pre-restore backup is automatically created for safety.

#### Option 2: Restore from JSON
1. Navigate to the **Configuration** page
2. Scroll to the **Global Configuration** section
3. Under **Import Global Configuration**, find the **Import from JSON** option
4. Paste your JSON configuration into the text area
5. Click **Import JSON**
6. Confirm the action
7. The page will refresh automatically

**Note**: This option works with both pre-migration JSON exports (from File System) and post-migration JSON exports (from SQLite).

---

## Migrating from File System to SQLite

If you want to migrate from File System storage to SQLite:

1. **Create a pre-migration backup** (recommended):
   - Go to the **Configuration** page
   - Export your current configuration as JSON
   - Save this file securely

2. **Navigate to the Admin page**
3. Scroll to the **Storage Provider Migration** section
4. Follow the two-step migration process:
   - **Step 1**: Create a mandatory pre-migration backup (JSON file will download automatically)
   - **Step 2**: Initiate the migration (requires double confirmation)
5. The application will restart automatically
6. Your configuration data will now be stored in the SQLite database

**Important**: Migration is one-way. You cannot automatically migrate back to File System storage. However, you can always restore from your JSON backup if needed.

---

## Best Practices

### For File System Users
- Export your configuration regularly (weekly or before major changes)
- Store backups in version control (Git) for tracking changes over time
- Test your backup files by importing them in a test environment

### For SQLite Users
- Create database backups before making significant configuration changes
- Keep multiple backup versions (daily, weekly, monthly)
- Store database backups in a secure, off-site location
- Export JSON backups periodically for portability
- Keep your pre-migration JSON backup indefinitely

### For All Users
- Label your backup files with dates and descriptions
- Test your restore process regularly to ensure backups are valid
- Keep backups in multiple locations (local, cloud storage, etc.)
- Document any customizations or special configurations

---

## Troubleshooting

### Backup Issues
- **Export fails**: Check your browser's download settings and ensure pop-ups are not blocked
- **Large backup files**: This is normal if you have many custom components or naming history

### Restore Issues
- **JSON import fails**: Verify the JSON format is valid (use a JSON validator)
- **Database restore fails**: Ensure the `.db` file is not corrupted and is a valid SQLite database
- **Application doesn't restart**: Manually restart the application or container

### Getting Help
If you encounter issues with backup or restore operations:
1. Check the **Admin Log** in the Admin page for detailed error messages
2. Review the backup file size and format
3. Ensure you have sufficient disk space
4. Consult the project documentation or submit an issue on GitHub

---

## Summary

| Storage Provider | Backup Format | Best Use Case |
|-----------------|---------------|---------------|
| File System | JSON | Version control, sharing configurations |
| SQLite | Database File (.db) | Quick backup/restore, disaster recovery |
| SQLite | JSON | Cross-instance migration, portability |

Choose the backup method that best fits your needs and always keep multiple backup versions for safety.
