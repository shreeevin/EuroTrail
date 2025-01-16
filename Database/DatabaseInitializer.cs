using Microsoft.Data.Sqlite;

namespace EuroTrail.Database
{
    public static class DatabaseInitializer
    {
        private static string BaseDirectory => AppContext.BaseDirectory;
        private static string DatabaseFolderPath => Path.Combine(BaseDirectory, "Database");
        private static string DatabaseFilePath => Path.Combine(DatabaseFolderPath, "EuroTrail.db");
        
        public static void Initialize()
        {
            if (!File.Exists(DatabaseFilePath))
            {
                Directory.CreateDirectory(DatabaseFolderPath);

                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS users (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT,
                            email TEXT,
                            phone TEXT,
                            username TEXT UNIQUE NOT NULL,
                            password TEXT NOT NULL,
                            wallet REAL DEFAULT 0,
                            currency TEXT DEFAULT 'USD',
                            status TEXT DEFAULT 'active',
                            blocked BOOLEAN DEFAULT FALSE,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );
                    ";
                    command.ExecuteNonQuery();

                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS transactions (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            user_id INTEGER NOT NULL,
                            tnx TEXT UNIQUE NOT NULL,
                            type TEXT NOT NULL,
                            scope TEXT NOT NULL,
                            source TEXT NOT NULL,
                            tags TEXT,
                            note TEXT,
                            fee REAL DEFAULT 0,
                            amount REAL DEFAULT 0,
                            status TEXT DEFAULT 'pending',
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={DatabaseFilePath}");
        }
    }
}
