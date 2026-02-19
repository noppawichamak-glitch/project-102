using System;
using System.IO;
using System.Data.SQLite;

namespace project_102
{
    public static class DatabaseConfig
    {
        public static string DbFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ShopDB.db");
        public static string ConnectionString => $"Data Source={DbFile};Version=3;";

        public static SQLiteConnection GetConnection() => new SQLiteConnection(ConnectionString);

        public static void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(DbFile))
                {
                    SQLiteConnection.CreateFile(DbFile);
                }

                using var conn = GetConnection();
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"-- ตารางสินค้า (มี IsActive เพื่อทำ Soft Delete)
CREATE TABLE IF NOT EXISTS Products (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT UNIQUE NOT NULL,
    Name TEXT NOT NULL,
    Price REAL NOT NULL,
    Stock INTEGER DEFAULT 0,
    Category TEXT,
    IsActive BOOLEAN DEFAULT 1
);

-- ตารางประวัติสต็อก (รองรับรายงานความเคลื่อนไหว)
CREATE TABLE IF NOT EXISTS StockTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductId INTEGER,
    Type TEXT,
    Quantity INTEGER,
    TransactionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    Note TEXT,
    FOREIGN KEY(ProductId) REFERENCES Products(Id)
);

-- ตารางหัวบิล (Receipt)
CREATE TABLE IF NOT EXISTS Sales (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReceiptNumber TEXT UNIQUE,
    SaleDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    TotalAmount REAL
);

-- ตารางรายละเอียดบิล
CREATE TABLE IF NOT EXISTS SaleDetails (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaleId INTEGER,
    ProductId INTEGER,
    ProductName TEXT,
    Quantity INTEGER,
    UnitPrice REAL,
    FOREIGN KEY(SaleId) REFERENCES Sales(Id)
);";

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // If initialization fails, bubble up so UI can show friendly message
                throw new Exception("Failed to initialize database: " + ex.Message, ex);
            }
        }
    }
}
