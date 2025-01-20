using Microsoft.Data.Sqlite;

namespace EuroTrail.Services
{
    public static class AnalysisService
    {
        private static string BaseDirectory => AppContext.BaseDirectory;
        private static string DatabaseFolderPath => Path.Combine(BaseDirectory, "Database");
        private static string DatabaseFilePath => Path.Combine(DatabaseFolderPath, "EuroTrail.db");
        public static int GetTransactionCount(int userId, string filter)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    switch (filter.ToLower())
                    {   
                        case "all_transaction":
                            command.CommandText = @"
                                SELECT COUNT(*)
                                FROM transactions
                                WHERE user_id = $userId;
                            ";
                            break;

                        case "income":
                            command.CommandText = @"
                                SELECT COUNT(*)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'income';
                            ";
                            break;

                        case "expense":
                            command.CommandText = @"
                                SELECT COUNT(*)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'expense';
                            ";
                            break;

                        case "debt":
                            command.CommandText = @"
                                SELECT COUNT(*)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'debt';
                            ";
                            break;

                        case "debt_completed":
                            command.CommandText = @"
                                SELECT COUNT(*)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'debt' AND status = 'completed';
                            ";
                            break;

                        case "debt_pending":
                            command.CommandText = @"
                                SELECT COUNT(*)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'debt' AND status = 'pending';
                            ";
                            break;

                        default:
                            throw new ArgumentException("Invalid filter specified.");
                    }

                    command.Parameters.AddWithValue("$userId", userId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                ToasterService.ShowGlobalToast(
                    message: "Server Error", 
                    description: $"Error counting transactions: {ex.Message}",
                    type: "danger"
                );

                return 0;
            }
        }
        
        public static decimal GetTransactionBalance(int userId, string filter)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    switch (filter.ToLower())
                    {
                        case "income":
                            command.CommandText = @"
                                SELECT SUM(amount)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'income';
                            ";
                            break;

                        case "expense":
                            command.CommandText = @"
                                SELECT SUM(amount)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'expense';
                            ";
                            break;

                        case "debt":
                            command.CommandText = @"
                                SELECT SUM(amount)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'debt';
                            ";
                            break;

                        case "debt_completed":
                            command.CommandText = @"
                                SELECT SUM(amount)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'debt' AND status = 'completed';
                            ";
                            break;
                        
                        case "debt_pending":
                            command.CommandText = @"
                                SELECT SUM(amount)
                                FROM transactions
                                WHERE user_id = $userId AND scope = 'debt' AND status = 'pending';
                            ";
                            break;

                        case "balance":
                            command.CommandText = @"
                                SELECT wallet
                                FROM users
                                WHERE id = $userId;
                            ";
                            break;

                        default:
                            throw new ArgumentException("Invalid filter specified.");
                    }

                    command.Parameters.AddWithValue("$userId", userId);
                    var result = command.ExecuteScalar();

                    if (result == DBNull.Value || result == null)
                    {
                        return 0;
                    }

                    return Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                ToasterService.ShowGlobalToast(
                    message: "Server Error", 
                    description: $"Error getting balance: {ex.Message}",
                    type: "danger"
                );
                
                return 0;
            }
        }
    }
}
