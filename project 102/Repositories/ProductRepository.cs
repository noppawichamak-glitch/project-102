using Dapper;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Data.SQLite;
using project_102.Models;

namespace project_102.Repositories
{
    public class ProductRepository
    {
        public void AddProduct(Product p)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            string sql = "INSERT INTO Products (Code, Name, Price, Stock, Category, IsActive) VALUES (@Code, @Name, @Price, @Stock, @Category, 1)";
            conn.Execute(sql, p);
        }

        public List<Product> GetAllProducts()
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            return conn.Query<Product>("SELECT * FROM Products WHERE IsActive = 1").ToList();
        }

        public void UpdateProduct(Product p)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            string sql = "UPDATE Products SET Name=@Name, Price=@Price, Category=@Category, Stock=@Stock WHERE Id=@Id";
            conn.Execute(sql, p);
        }

        public void SoftDeleteProduct(int id)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            conn.Execute("UPDATE Products SET IsActive = 0 WHERE Id = @Id", new { Id = id });
        }
    }
}
