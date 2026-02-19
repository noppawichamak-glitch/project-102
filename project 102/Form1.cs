using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using project_102.Models;
using project_102.Repositories;
using project_102.Services;

namespace project_102
{
    public partial class Form1 : Form
    {
        private readonly ProductRepository _repo = new ProductRepository();
        private readonly InventoryRepository _invRepo = new InventoryRepository();
        private readonly ReceiptService _receiptService = new ReceiptService();
        private readonly BindingSource _bs = new BindingSource();
        private List<Product> _products = new List<Product>();

        public Form1()
        {
            InitializeComponent();

            // Wire up button events
            button1.Click += BtnAdd_Click;       // Add
            button2.Click += BtnUpdate_Click;    // Update
            button3.Click += BtnDelete_Click;    // Soft Delete
            button4.Click += BtnStockIn_Click;   // Stock IN
            button5.Click += BtnStockOut_Click;  // Stock OUT
            button6.Click += BtnPdf_Click;       // Generate PDF

            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                DatabaseConfig.InitializeDatabase();
                LoadDataGrid();
                label1.Text = "Code";
                label2.Text = "Name";
                label3.Text = "Price";
                label4.Text = "Qty";
                label5.Text = "Category";
                this.Text = "Products"; // use Form title instead of missing label6
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize application: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataGrid()
        {
            try
            {
                _products = _repo.GetAllProducts();
                _bs.DataSource = _products;
                dataGridView1.DataSource = _bs;

                // Hide internal columns if they exist
                if (dataGridView1.Columns.Contains("IsActive"))
                    dataGridView1.Columns["IsActive"].Visible = false;

                if (dataGridView1.Columns.Contains("Id"))
                    dataGridView1.Columns["Id"].Visible = false;

                // Adjust header text
                if (dataGridView1.Columns.Contains("Code"))
                    dataGridView1.Columns["Code"].HeaderText = "Code";
                if (dataGridView1.Columns.Contains("Name"))
                    dataGridView1.Columns["Name"].HeaderText = "Name";
                if (dataGridView1.Columns.Contains("Price"))
                    dataGridView1.Columns["Price"].HeaderText = "Price";
                if (dataGridView1.Columns.Contains("Stock"))
                    dataGridView1.Columns["Stock"].HeaderText = "Stock";
                if (dataGridView1.Columns.Contains("Category"))
                    dataGridView1.Columns["Category"].HeaderText = "Category";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("กรุณากรอกรหัสสินค้า", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("กรุณากรอกชื่อสินค้า", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(textBox3.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("ราคาต้องเป็นตัวเลขและห้ามติดลบ", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(textBox4.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("จำนวนต้องเป็นจำนวนเต็มและห้ามติดลบ", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var p = new Product
                {
                    Code = textBox1.Text.Trim(),
                    Name = textBox2.Text.Trim(),
                    Price = price,
                    Stock = stock,
                    Category = textBox5.Text?.Trim() ?? string.Empty,
                    IsActive = true
                };

                _repo.AddProduct(p);
                MessageBox.Show("บันทึกสำเร็จ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataGrid();
            }
            catch (System.Data.SQLite.SQLiteException sqlEx)
            {
                if (sqlEx.Message.Contains("UNIQUE constraint failed"))
                {
                    MessageBox.Show("รหัสสินค้านี้มีอยู่แล้ว กรุณาใช้รหัสอื่น", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Database error: " + sqlEx.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_bs.Current is Product current))
                {
                    MessageBox.Show("กรุณาเลือกสินค้าที่ต้องการแก้ไข", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(textBox3.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("ราคาต้องเป็นตัวเลขและห้ามติดลบ", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(textBox4.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("จำนวนต้องเป็นจำนวนเต็มและห้ามติดลบ", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                current.Name = textBox2.Text.Trim();
                current.Price = price;
                current.Stock = stock;
                current.Category = textBox5.Text?.Trim() ?? string.Empty;

                _repo.UpdateProduct(current);
                MessageBox.Show("อัปเดตเรียบร้อย", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_bs.Current is Product current))
                {
                    MessageBox.Show("กรุณาเลือกสินค้าที่ต้องการลบ", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dr = MessageBox.Show($"ต้องการลบสินค้ารหัส {current.Code} ใช่หรือไม่?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;

                _repo.SoftDeleteProduct(current.Id);
                MessageBox.Show("ลบเรียบร้อย", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStockIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_bs.Current is Product current))
                {
                    MessageBox.Show("กรุณาเลือกสินค้า", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(textBox4.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("จำนวนต้องเป็นตัวเลขบวก", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _invRepo.UpdateStock(current.Id, qty, "IN", "Stock IN via UI");
                MessageBox.Show("เพิ่มสต็อกเรียบร้อย", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการอัพเดตสต็อก: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStockOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_bs.Current is Product current))
                {
                    MessageBox.Show("กรุณาเลือกสินค้า", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(textBox4.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("จำนวนต้องเป็นตัวเลขบวก", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (current.Stock < qty)
                {
                    MessageBox.Show("จำนวนในสต็อกไม่เพียงพอ", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _invRepo.UpdateStock(current.Id, qty, "OUT", "Sale via UI");
                MessageBox.Show("เบิกออกเรียบร้อย", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการอัพเดตสต็อก: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPdf_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_bs.Current is Product current))
                {
                    MessageBox.Show("กรุณาเลือกสินค้าที่ต้องการออกใบเสร็จ", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(textBox4.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("กรุณาระบุจำนวนที่จะออกใบเสร็จ", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create a shallow copy to represent the cart item (use Stock property to carry qty)
                var cartItem = new Product
                {
                    Id = current.Id,
                    Code = current.Code,
                    Name = current.Name,
                    Price = current.Price,
                    Stock = qty,
                    Category = current.Category,
                    IsActive = current.IsActive
                };

                var cart = new List<Product> { cartItem };
                var total = qty * current.Price;
                var receiptNo = DateTime.Now.ToString("yyyyMMddHHmmss");

                _receiptService.GenerateReceiptPDF(receiptNo, cart, total);
                MessageBox.Show($"สร้างไฟล์ receipt_{receiptNo}.pdf เรียบร้อย", "PDF Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถสร้างไฟล์ PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_SelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                if (!(_bs.Current is Product current))
                {
                    // Clear inputs
                    textBox1.Text = string.Empty;
                    textBox2.Text = string.Empty;
                    textBox3.Text = string.Empty;
                    textBox4.Text = string.Empty;
                    textBox5.Text = string.Empty;
                    return;
                }

                textBox1.Text = current.Code;
                textBox2.Text = current.Name;
                textBox3.Text = current.Price.ToString();
                textBox4.Text = current.Stock.ToString();
                textBox5.Text = current.Category;
            }
            catch
            {
                // swallow UI update errors
            }
        }
    }
}
