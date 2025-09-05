using SurrealDb.Net;

namespace LiquidStorageCloud.Core.Database
{
    /// <summary>
    /// Defines permission levels for database tables
    /// </summary>
    public enum TablePermission
    {
        Select = 1,
        Create = 2,
        Update = 4,
        Delete = 8
    }

    /// <summary>
    /// Manages table-level permissions for SurrealDB
    /// </summary>
    public static class TablePermissions
    {
        /// <summary>
        /// Applies read-only permissions to a new or existing table
        /// </summary>
        public static async Task ApplyReadOnlyPermissions(this ISurrealDbClient db, string tableName)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));

            // Define read-only permissions (only SELECT allowed)
            var query = $@"
                DEFINE TABLE {tableName} PERMISSIONS
                    FOR select FULL,
                    FOR create NONE,
                    FOR update NONE,
                    FOR delete NONE;
            ";

            await db.RawQuery(query);
        }

        /// <summary>
        /// Applies InsertAndUpdate permissions to a new or existing table (CREATE, UPDATE, and SELECT allowed, DELETE restricted)
        /// </summary>
        public static async Task ApplyInsertAndUpdatePermissions(this ISurrealDbClient db, string tableName)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));

            // Define InsertAndUpdate permissions (SELECT, CREATE, UPDATE allowed, DELETE restricted)
            var query = $@"
                DEFINE TABLE {tableName} PERMISSIONS
                    FOR select FULL,
                    FOR create FULL,
                    FOR update FULL,
                    FOR delete NONE;
            ";

            await db.RawQuery(query);
        }

        /// <summary>
        /// Creates a new table with default read-only permissions and is_ledger field
        /// </summary>
        public static async Task CreateTable(this ISurrealDbClient db, string tableName, bool isLedger = false)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));

            // Create table with is_ledger field and default read-only permissions
            var query = $@"
                DEFINE TABLE {tableName} SCHEMALESS
                    PERMISSIONS
                        FOR select FULL,
                        FOR create NONE,
                        FOR update NONE,
                        FOR delete NONE;
                
                DEFINE FIELD is_ledger ON {tableName} TYPE bool
                    VALUE $value OR {isLedger.ToString().ToLower()};
            ";

            await db.RawQuery(query);
        }

        /// <summary>
        /// Adds or updates the is_ledger field for an existing table
        /// </summary>
        public static async Task SetLedgerStatus(this ISurrealDbClient db, string tableName, bool isLedger)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));

            // Add or update is_ledger field
            var query = $@"
                DEFINE FIELD is_ledger ON {tableName} TYPE bool
                    VALUE $value OR {isLedger.ToString().ToLower()};

                -- Update existing records
                UPDATE {tableName} SET is_ledger = {isLedger.ToString().ToLower()};
            ";

            await db.RawQuery(query);
        }

        /// <summary>
        /// Lists all tables that are marked as ledger tables
        /// </summary>
        public static async Task<IEnumerable<string>> GetLedgerTables(this ISurrealDbClient db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            // Query to find all tables with is_ledger = true
            var query = @"
                SELECT 
                    meta::tb() as table_name 
                FROM information_schema.tables 
                WHERE meta::field_value(meta::tb(), 'is_ledger') = true;
            ";

            var result = await db.RawQuery(query);
            var tables = result.GetValue<IEnumerable<dynamic>>(0);
            return tables?.Select(t => (string)t.table_name) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Ensures all tables have the is_ledger field and default permissions set
        /// </summary>
        public static async Task InitializeAllTables(this ISurrealDbClient db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            // Get all tables
            var query = @"
                SELECT meta::tb() as table_name 
                FROM information_schema.tables;
            ";

            var result = await db.RawQuery(query);
            var tables = result.GetValue<IEnumerable<dynamic>>(0);

            if (tables != null)
            {
                foreach (var table in tables)
                {
                    string tableName = table.table_name;

                    // Add is_ledger field if it doesn't exist (defaults to false)
                    await db.RawQuery($@"
                        DEFINE FIELD is_ledger ON {tableName} TYPE bool
                            VALUE $value OR false;
                    ");

                    // Set default read-only permissions if no permissions are defined
                    await ApplyReadOnlyPermissions(db, tableName);
                }
            }
        }

        /// <summary>
        /// Gets the current permissions for a table
        /// </summary>
        public static async Task<Dictionary<string, string>> GetTablePermissions(this ISurrealDbClient db, string tableName)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));

            var query = $@"
                INFO FOR TABLE {tableName};
            ";

            var result = await db.RawQuery(query);
            var info = result.GetValue<dynamic>(0);

            var permissions = new Dictionary<string, string>
            {
                { "select", info?.permissions?.select?.ToString() ?? "NONE" },
                { "create", info?.permissions?.create?.ToString() ?? "NONE" },
                { "update", info?.permissions?.update?.ToString() ?? "NONE" },
                { "delete", info?.permissions?.delete?.ToString() ?? "NONE" }
            };

            return permissions;
        }
    }
}
