using Dapper;
using System;
using System.Data.SQLite;

namespace project_102.Repositories
{
    public class InventoryRepository
    {
        public void UpdateStock(int productId, int quantity, string type, string note)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var transaction = conn.BeginTransaction();
            try
            {
                string sqlUpdate = type == "IN" ? "UPDATE Products SET Stock = Stock + @Qty WHERE Id = @Id" : "UPDATE Products SET Stock = Stock - @Qty WHERE Id = @Id";
                conn.Execute(sqlUpdate, new { Qty = quantity, Id = productId }, transaction);

                string sqlLog = "INSERT INTO StockTransactions (ProductId, Type, Quantity, Note) VALUES (@Pid, @Type, @Qty, @Note)";
                conn.Execute(sqlLog, new { Pid = productId, Type = type, Qty = quantity, Note = note }, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Stock Update Failed: " + ex.Message, ex);
            }
        }
    }
}
