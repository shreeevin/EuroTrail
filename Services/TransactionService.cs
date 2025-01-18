using EuroTrail.Models;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;

using System.Diagnostics;

namespace EuroTrail.Services
{
    public static class TransactionService
    {
        private static string BaseDirectory => AppContext.BaseDirectory;
        private static string DatabaseFolderPath => Path.Combine(BaseDirectory, "Database");
        private static string DatabaseFilePath => Path.Combine(DatabaseFolderPath, "EuroTrail.db");
        
        public static bool CreateTransactionByModel(Transaction transaction)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO transactions (user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at)
                        VALUES ($userId, $tnx, $type, $scope, $source, $tags, $note, $fee, $amount, $status, $createdAt, $updatedAt);
                    ";

                    command.Parameters.AddWithValue("$userId", transaction.UserId);
                    command.Parameters.AddWithValue("$tnx", transaction.Tnx);
                    command.Parameters.AddWithValue("$type", transaction.Type);
                    command.Parameters.AddWithValue("$scope", transaction.Scope);
                    command.Parameters.AddWithValue("$source", transaction.Source);
                    command.Parameters.AddWithValue("$tags", string.Join(",", transaction.Tags));
                    command.Parameters.AddWithValue("$note", transaction.Note);
                    command.Parameters.AddWithValue("$fee", transaction.Fee);
                    command.Parameters.AddWithValue("$amount", transaction.Amount);
                    command.Parameters.AddWithValue("$status", transaction.Status);
                    command.Parameters.AddWithValue("$createdAt", transaction.CreatedAt);
                    command.Parameters.AddWithValue("$updatedAt", transaction.UpdatedAt);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating transaction: {ex.Message}");
                return false;
            }
        }
        public static bool CreateTransaction(
            int userId, 
            string tnx, 
            string type, 
            string scope, 
            string source, 
            List<string> tags, 
            string note, 
            decimal fee, 
            decimal amount, 
            string status)
        {
            try
            {                
                if(scope == "expense"){

                    decimal currentUserBalance = AuthService.GetWalletBalance(userId);

                    if(currentUserBalance < amount)
                    {
                        Debug.WriteLine("Insufficient balance. Cannot proceed with transaction.");
                        return false;
                    }
                }

                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();
                    
                    string tagsJson = JsonConvert.SerializeObject(tags);
                    
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO transactions (user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at)
                        VALUES ($userId, $tnx, $type, $scope, $source, $tags, $note, $fee, $amount, $status, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                    ";
                    
                    command.Parameters.AddWithValue("$userId", userId);
                    command.Parameters.AddWithValue("$tnx", tnx);
                    command.Parameters.AddWithValue("$type", type);
                    command.Parameters.AddWithValue("$scope", scope);
                    command.Parameters.AddWithValue("$source", source);
                    command.Parameters.AddWithValue("$tags", tagsJson);  
                    command.Parameters.AddWithValue("$note", note);
                    command.Parameters.AddWithValue("$fee", fee);
                    command.Parameters.AddWithValue("$amount", amount);
                    command.Parameters.AddWithValue("$status", status);

                    var result = command.ExecuteNonQuery();

                    if(result > 0)
                    {
                        if(scope == "income" || scope == "expense"){
                            AuthService.UpdateWalletBalance(
                                userId,
                                amount,
                                (type == "debit") ? "add" : "reduce"
                            );
                        }
                    }

                    return result > 0;  
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating transaction: {ex.Message}");
                return false;
            }
        }

        public static Transaction? GetTransactionById(int transactionId, int userId)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT id, user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at
                        FROM transactions
                        WHERE id = $id AND user_id = $userId;
                    ";
                    command.Parameters.AddWithValue("$id", transactionId);
                    command.Parameters.AddWithValue("$userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var tagsColumn = reader.IsDBNull(6) ? "[]" : reader.GetString(6); 
                            List<string> tags = new List<string>();

                            try
                            {
                                tags = JsonConvert.DeserializeObject<List<string>>(tagsColumn) ?? new List<string>();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error deserializing tags: {ex.Message}");
                            }

                            return new Transaction
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Tnx = reader.GetString(2),
                                Type = reader.GetString(3),
                                Scope = reader.GetString(4),
                                Source = reader.GetString(5),
                                Tags = tags, 
                                Note = reader.GetString(7),
                                Fee = reader.GetDecimal(8),
                                Amount = reader.GetDecimal(9),
                                Status = reader.GetString(10),
                                CreatedAt = reader.GetDateTime(11),
                                UpdatedAt = reader.GetDateTime(12)
                            };
                        }
                        else
                        {
                            Debug.WriteLine("Transaction not found.");
                            return null; 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching transaction: {ex.Message}");
                return null;
            }
        }

        public static (List<Transaction> Transactions, int TotalPages) GetTransactionsByScope(int userId, string scope, int page, int pageSize, string? status = "completed")
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT id, user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at
                        FROM transactions
                        WHERE user_id = $userId AND scope = $scope AND status = $status
                        ORDER BY created_at DESC
                        LIMIT $PageSize OFFSET $Offset;
                    ";

                    command.Parameters.AddWithValue("$userId", userId);
                    command.Parameters.AddWithValue("$scope", scope);
                    command.Parameters.AddWithValue("$status", status);
                    command.Parameters.AddWithValue("$PageSize", pageSize);
                    command.Parameters.AddWithValue("$Offset", (page - 1) * pageSize);

                    var reader = command.ExecuteReader();
                    var transactions = new List<Transaction>();

                    while (reader.Read())
                    {
                        var transaction = new Transaction
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Tnx = reader.GetString(2),
                            Type = reader.GetString(3),
                            Scope = reader.GetString(4),
                            Source = reader.GetString(5),
                            Tags = JsonConvert.DeserializeObject<List<string>>(reader.GetString(6)) ?? new List<string>(),
                            Note = reader.GetString(7),
                            Fee = reader.GetDecimal(8),
                            Amount = reader.GetDecimal(9),
                            Status = reader.GetString(10),
                            CreatedAt = reader.GetDateTime(11),
                            UpdatedAt = reader.GetDateTime(12)
                        };
                        transactions.Add(transaction);
                    }

                    reader.Close(); 
                    command.CommandText = @"
                        SELECT COUNT(*)
                        FROM transactions
                        WHERE user_id = $userId AND scope = $scope AND status = $status;
                    ";

                    int totalRecords = Convert.ToInt32(command.ExecuteScalar());
                    int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                    return (transactions, totalPages);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving transactions by scope: {ex.Message}");
                return (new List<Transaction>(), 0);
            }
        }


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
                Debug.WriteLine($"Error counting transactions: {ex.Message}");
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
                Debug.WriteLine($"Error getting balance: {ex.Message}");
                return 0;
            }
        }
        public static List<Transaction> GetTransactionsBySource(int userId, string source)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT id, user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at
                        FROM transactions
                        WHERE user_id = $userId AND source = $source;
                    ";

                    command.Parameters.AddWithValue("$userId", userId);
                    command.Parameters.AddWithValue("$source", source);

                    var reader = command.ExecuteReader();
                    var transactions = new List<Transaction>();

                    while (reader.Read())
                    {
                        var transaction = new Transaction
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Tnx = reader.GetString(2),
                            Type = reader.GetString(3),
                            Scope = reader.GetString(4),
                            Source = reader.GetString(5),
                            Tags = JsonConvert.DeserializeObject<List<string>>(reader.GetString(6)) ?? new List<string>(),
                            Note = reader.GetString(7),
                            Fee = reader.GetDecimal(8),
                            Amount = reader.GetDecimal(9),
                            Status = reader.GetString(10),
                            CreatedAt = reader.GetDateTime(11),
                            UpdatedAt = reader.GetDateTime(12)
                        };
                        transactions.Add(transaction);
                    }

                    return transactions;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving transactions by source: {ex.Message}");
                return new List<Transaction>();
            }
        }

        public static (List<Transaction> Transactions, int TotalPages) GetTransactionsByType(int userId, string type, int page = 1, int pageSize = 5)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var countCommand = connection.CreateCommand();
                    countCommand.CommandText = @"
                        SELECT COUNT(*)
                        FROM transactions
                        WHERE user_id = $userId AND type = $type;
                    ";
                    countCommand.Parameters.AddWithValue("$userId", userId);
                    countCommand.Parameters.AddWithValue("$type", type);
                    int totalCount = Convert.ToInt32(countCommand.ExecuteScalar());

                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT id, user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at
                        FROM transactions
                        WHERE user_id = $userId AND type = $type
                        ORDER BY created_at DESC
                        LIMIT $PageSize OFFSET $Offset;
                    ";

                    int offset = (page - 1) * pageSize;
                    command.Parameters.AddWithValue("$userId", userId);
                    command.Parameters.AddWithValue("$type", type);
                    command.Parameters.AddWithValue("$PageSize", pageSize);
                    command.Parameters.AddWithValue("$Offset", offset);

                    var reader = command.ExecuteReader();
                    var transactions = new List<Transaction>();

                    while (reader.Read())
                    {
                        var transaction = new Transaction
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Tnx = reader.GetString(2),
                            Type = reader.GetString(3),
                            Scope = reader.GetString(4),
                            Source = reader.GetString(5),
                            Tags = JsonConvert.DeserializeObject<List<string>>(reader.GetString(6)) ?? new List<string>(),
                            Note = reader.GetString(7),
                            Fee = reader.GetDecimal(8),
                            Amount = reader.GetDecimal(9),
                            Status = reader.GetString(10),
                            CreatedAt = reader.GetDateTime(11),
                            UpdatedAt = reader.GetDateTime(12)
                        };
                        transactions.Add(transaction);
                    }

                    return (transactions, totalPages);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving transactions by type: {ex.Message}");
                return (new List<Transaction>(), 0);
            }
        }


        public static (List<Transaction> Transactions, int TotalPages) GetAllTransactions(int userId, int page = 1, int pageSize = 5)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var countCommand = connection.CreateCommand();
                    countCommand.CommandText = @"
                        SELECT COUNT(*)
                        FROM transactions
                        WHERE user_id = $userId;
                    ";
                    countCommand.Parameters.AddWithValue("$userId", userId);
                    int totalCount = Convert.ToInt32(countCommand.ExecuteScalar());

                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT id, user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at
                        FROM transactions
                        WHERE user_id = $userId
                        ORDER BY created_at DESC
                        LIMIT $PageSize OFFSET $Offset;
                    ";

                    int offset = (page - 1) * pageSize;
                    command.Parameters.AddWithValue("$userId", userId);
                    command.Parameters.AddWithValue("$PageSize", pageSize);
                    command.Parameters.AddWithValue("$Offset", offset);

                    var reader = command.ExecuteReader();
                    var transactions = new List<Transaction>();

                    while (reader.Read())
                    {
                        var transaction = new Transaction
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Tnx = reader.GetString(2),
                            Type = reader.GetString(3),
                            Scope = reader.GetString(4),
                            Source = reader.GetString(5),
                            Tags = JsonConvert.DeserializeObject<List<string>>(reader.GetString(6)) ?? new List<string>(),
                            Note = reader.GetString(7),
                            Fee = reader.GetDecimal(8),
                            Amount = reader.GetDecimal(9),
                            Status = reader.GetString(10),
                            CreatedAt = reader.GetDateTime(11),
                            UpdatedAt = reader.GetDateTime(12)
                        };
                        transactions.Add(transaction);
                    }

                    return (transactions, totalPages);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving all transactions: {ex.Message}");
                return (new List<Transaction>(), 0);
            }
        }


        public static (List<Transaction> Transactions, int TotalPages) GetTransactionsByMixedFilter(
            int userId, 
            string? searchKeyword = null,
            string? scope = null,
            string? type = null,  
            string? status = null,
            string? startDate = null,   
            string? endDate = null,
            int page = 1, 
            int pageSize = 5 
        )
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var query = "SELECT id, user_id, tnx, type, scope, source, tags, note, fee, amount, status, created_at, updated_at " +
                                "FROM transactions WHERE user_id = $userId";

                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        query += " AND (tnx LIKE $searchKeyword OR note LIKE $searchKeyword OR tags LIKE $searchKeyword)";
                    }

                    if (!string.IsNullOrEmpty(scope))
                    {
                        query += " AND scope = $scope";
                    }

                    if (!string.IsNullOrEmpty(type))
                    {
                        query += " AND type = $type";
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        query += " AND status = $status";
                    }

                    if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                    {
                        query += " AND created_at BETWEEN $startDate AND $endDate";
                    }

                    query += " ORDER BY created_at DESC LIMIT $PageSize OFFSET $Offset";

                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    command.Parameters.AddWithValue("$userId", userId);

                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        command.Parameters.AddWithValue("$searchKeyword", "%" + searchKeyword + "%");
                    }

                    if (!string.IsNullOrEmpty(scope))
                    {
                        command.Parameters.AddWithValue("$scope", scope);
                    }

                    if (!string.IsNullOrEmpty(type))
                    {
                        command.Parameters.AddWithValue("$type", type);
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        command.Parameters.AddWithValue("$status", status);
                    }

                    if (!string.IsNullOrEmpty(startDate))
                    {
                        command.Parameters.AddWithValue("$startDate", startDate);
                    }

                    if (!string.IsNullOrEmpty(endDate))
                    {
                        command.Parameters.AddWithValue("$endDate", endDate);
                    }

                    command.Parameters.AddWithValue("$PageSize", pageSize);
                    command.Parameters.AddWithValue("$Offset", (page - 1) * pageSize);

                    var reader = command.ExecuteReader();
                    var transactions = new List<Transaction>();

                    while (reader.Read())
                    {
                        var transaction = new Transaction
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Tnx = reader.GetString(2),
                            Type = reader.GetString(3),
                            Scope = reader.GetString(4),
                            Source = reader.GetString(5),
                            Tags = JsonConvert.DeserializeObject<List<string>>(reader.GetString(6)) ?? new List<string>(),
                            Note = reader.GetString(7),
                            Fee = reader.GetDecimal(8),
                            Amount = reader.GetDecimal(9),
                            Status = reader.GetString(10),
                            CreatedAt = reader.GetDateTime(11),
                            UpdatedAt = reader.GetDateTime(12)
                        };
                        transactions.Add(transaction);
                    }

                    var countQuery = "SELECT COUNT(*) FROM transactions WHERE user_id = $userId";

                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        countQuery += " AND (tnx LIKE $searchKeyword OR note LIKE $searchKeyword OR tags LIKE $searchKeyword)";
                    }

                    if (!string.IsNullOrEmpty(scope))
                    {
                        countQuery += " AND scope = $scope";
                    }

                    if (!string.IsNullOrEmpty(type))
                    {
                        countQuery += " AND type = $type";
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        countQuery += " AND status = $status";
                    }

                    if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                    {
                        countQuery += " AND created_at BETWEEN $startDate AND $endDate";
                    }

                    var countCommand = connection.CreateCommand();
                    countCommand.CommandText = countQuery;

                    countCommand.Parameters.AddWithValue("$userId", userId);
                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        countCommand.Parameters.AddWithValue("$searchKeyword", "%" + searchKeyword + "%");
                    }
                    if (!string.IsNullOrEmpty(scope))
                    {
                        countCommand.Parameters.AddWithValue("$scope", scope);
                    }
                    if (!string.IsNullOrEmpty(type))
                    {
                        countCommand.Parameters.AddWithValue("$type", type);
                    }
                    if (!string.IsNullOrEmpty(status))
                    {
                        countCommand.Parameters.AddWithValue("$status", status);
                    }
                    if (!string.IsNullOrEmpty(startDate))
                    {
                        countCommand.Parameters.AddWithValue("$startDate", startDate);
                    }
                    if (!string.IsNullOrEmpty(endDate))
                    {
                        countCommand.Parameters.AddWithValue("$endDate", endDate);
                    }

                    int totalCount = Convert.ToInt32(countCommand.ExecuteScalar());
                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    return (transactions, totalPages);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving transactions with filters: {ex.Message}");
                return (new List<Transaction>(), 0);
            }
        }


        public static bool UpdateTransactionById(int transactionId, int userId, string type, string scope, string source, List<string> tags, string note, decimal fee, decimal amount, string status)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    string serializedTags = JsonConvert.SerializeObject(tags);

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        UPDATE transactions
                        SET type = $type, scope = $scope, source = $source, tags = $tags, note = $note, fee = $fee, amount = $amount, status = $status, updated_at = CURRENT_TIMESTAMP
                        WHERE id = $id AND user_id = $userId;
                    ";

                    command.Parameters.AddWithValue("$id", transactionId);
                    command.Parameters.AddWithValue("$userId", userId);
                    command.Parameters.AddWithValue("$type", type);
                    command.Parameters.AddWithValue("$scope", scope);
                    command.Parameters.AddWithValue("$source", source);
                    command.Parameters.AddWithValue("$tags", serializedTags); 
                    command.Parameters.AddWithValue("$note", note);
                    command.Parameters.AddWithValue("$fee", fee);
                    command.Parameters.AddWithValue("$amount", amount);
                    command.Parameters.AddWithValue("$status", status);

                    var result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return true; 
                    }
                    else
                    {
                        Debug.WriteLine("No transaction found or no changes made.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating transaction: {ex.Message}");
                return false; 
            }
        }
        public static (bool status, string message) DeleteTransaction(int transactionId, int userId)
        {
            decimal? amount = null;
            string? type = null;
            string? scope = null;
            string? status = null;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var selectCommand = connection.CreateCommand();
                    selectCommand.CommandText = "SELECT scope, type, status, amount FROM transactions WHERE id = $id AND user_id = $userId;";
                    selectCommand.Parameters.AddWithValue("$id", transactionId);
                    selectCommand.Parameters.AddWithValue("$userId", userId);

                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            scope = reader.GetString(0);
                            type = reader.GetString(1);
                            status = reader.GetString(2);
                            amount = reader.GetDecimal(3);
                        }
                        else
                        {
                            Debug.WriteLine("Transaction not found.");
                            return (false, "Transaction not found.");
                        }
                    }

                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText = "DELETE FROM transactions WHERE id = $id AND user_id = $userId;";
                    deleteCommand.Parameters.AddWithValue("$id", transactionId);
                    deleteCommand.Parameters.AddWithValue("$userId", userId);

                    var rowsAffected = deleteCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Debug.WriteLine("Transaction deleted successfully.");
                    }
                    else
                    {
                        Debug.WriteLine("Transaction not found or does not belong to the user.");
                        return (false,"Transaction not found or does not belong to the user.");
                    }
                }

                if (amount.HasValue && scope != null && status != null && type != null)
                {
                    if (scope == "debt" && status == "completed")
                    {
                        AuthService.UpdateWalletBalance(
                            userId,
                            amount.Value,
                            "add"
                        );
                    }
                    else if (scope == "income" && status == "completed")
                    {
                        AuthService.UpdateWalletBalance(
                            userId,
                            amount.Value,
                            "reduce"
                        );
                    }
                    else if (scope == "expense" && status == "completed")
                    {
                        AuthService.UpdateWalletBalance(
                            userId,
                            amount.Value,
                            "add"
                        );
                    }
                }

                return (true, "Transaction deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting transaction: {ex.Message}");
                return (false, $"Error deleting transaction: {ex.Message}");
            }
        }    
        public static bool ClearDebt(int transactionId, int userId)
        {
            try
            {
                decimal amount;
                string type;
                string scope;
                string status;

                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var getTransactionCommand = connection.CreateCommand();
                    getTransactionCommand.CommandText = "SELECT amount, type, scope, status FROM transactions WHERE id = $id AND user_id = $userId;";
                    getTransactionCommand.Parameters.AddWithValue("$id", transactionId);
                    getTransactionCommand.Parameters.AddWithValue("$userId", userId);

                    using (var reader = getTransactionCommand.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Debug.WriteLine("Transaction not found or does not belong to the user.");
                            return false;
                        }

                        amount = reader.GetDecimal(0);
                        type = reader.GetString(1);
                        scope = reader.GetString(2);
                        status = reader.GetString(3);

                        if (scope != "debt" || status == "completed")
                        {
                            Debug.WriteLine("Transaction is not a pending debt or is already completed.");
                            return false;
                        }
                    }

                    var updateStatusCommand = connection.CreateCommand();
                    updateStatusCommand.CommandText = "UPDATE transactions SET status = 'completed' WHERE id = $id AND user_id = $userId;";
                    updateStatusCommand.Parameters.AddWithValue("$id", transactionId);
                    updateStatusCommand.Parameters.AddWithValue("$userId", userId);
                    updateStatusCommand.ExecuteNonQuery();
                } 
                
                AuthService.UpdateWalletBalance(
                    userId,
                    amount,
                    "reduce" 
                );

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing debt: {ex.Message}");
                return false;
            }
        }
        public static (int? maxTransactionId, int? minTransactionId) GetTransactionAmountExtremes(int userId, string scope)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = @"
                        SELECT 
                            MAX(CASE WHEN amount = (SELECT MAX(amount) FROM transactions WHERE user_id = $userId AND scope = $scope) THEN id END) AS MaxTransactionId,
                            MAX(CASE WHEN amount = (SELECT MIN(amount) FROM transactions WHERE user_id = $userId AND scope = $scope) THEN id END) AS MinTransactionId
                        FROM transactions
                        WHERE user_id = $userId AND scope = $scope;
                    ";

                    command.Parameters.AddWithValue("$userId", userId);
                    command.Parameters.AddWithValue("$scope", scope);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var maxTransactionId = reader["MaxTransactionId"] != DBNull.Value
                                ? Convert.ToInt32(reader["MaxTransactionId"])
                                : (int?)null;

                            var minTransactionId = reader["MinTransactionId"] != DBNull.Value
                                ? Convert.ToInt32(reader["MinTransactionId"])
                                : (int?)null;

                            return (maxTransactionId, minTransactionId);
                        }
                    }

                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving transaction extremes: {ex.Message}");
                return (null, null);
            }
        }
    }
}
