using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClothingStore;

namespace ClothingStoreIS
{
    /// TRANSACTION 1 — Sales Transaction
    public class SalesTransactionForm : Form
    {
        private ComboBox cboCustomer, cboProduct;
        private NumericUpDown nudQty;
        private DataGridView dgvCart;
        private Label lblGrandTotal, lblStock;
        private Button btnAddItem, btnRemoveItem, btnPostSale, btnClear, btnClose;
        private DataTable _cartTable;
        private decimal _grandTotal = 0m;

        public SalesTransactionForm()
        {
            InitUI();
            LoadCombos();
        }

        private void InitUI()
        {
            Text = "Sales Transaction";
            Size = new Size(950, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.BgLight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = AppTheme.FontBody;

            // Header
            var hdr = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            Controls.Add(hdr);
            hdr.Controls.Add(new Label
            {
                Text = "🛒  New Sales Transaction",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(20, 12),
                AutoSize = true
            });

            // ── Left panel (input) ────────────────────────────────────────────
            var left = new Panel { Location = new Point(0, 60), Size = new Size(320, 540), BackColor = AppTheme.BgCard };
            Controls.Add(left);
            int y = 16;

            left.Controls.Add(SectionLabel("Customer", 16, y)); y += 24;
            cboCustomer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, y), Size = new Size(284, 26), Font = AppTheme.FontBody };
            left.Controls.Add(cboCustomer);
            y += 36;

            left.Controls.Add(SectionLabel("Product", 16, y)); y += 24;
            cboProduct = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, y), Size = new Size(284, 26), Font = AppTheme.FontBody };
            cboProduct.SelectedIndexChanged += CboProduct_Changed;
            left.Controls.Add(cboProduct);
            y += 36;

            lblStock = new Label { Text = "Stock: —", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMid, Location = new Point(16, y), AutoSize = true };
            left.Controls.Add(lblStock);
            y += 22;

            left.Controls.Add(SectionLabel("Quantity", 16, y)); y += 24;
            nudQty = new NumericUpDown { Location = new Point(16, y), Size = new Size(100, 26), Minimum = 1, Maximum = 9999, Value = 1, Font = AppTheme.FontBody };
            left.Controls.Add(nudQty);
            y += 40;

            btnAddItem = PrimaryBtn("➕  Add to Cart", 16, y, AppTheme.PrimaryDark);
            btnAddItem.Click += BtnAddItem_Click;
            left.Controls.Add(btnAddItem);
            y += 44;

            btnRemoveItem = PrimaryBtn("➖  Remove Selected", 16, y, AppTheme.Danger);
            btnRemoveItem.Click += BtnRemoveItem_Click;
            left.Controls.Add(btnRemoveItem);
            y += 44;

            btnClear = PrimaryBtn("🗑  Clear Cart", 16, y, Color.Gray);
            btnClear.Click += (s, e) => ClearCart();
            left.Controls.Add(btnClear);
            y += 60;

            left.Controls.Add(new Label { Text = "Grand Total:", Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(16, y), AutoSize = true });
            lblGrandTotal = new Label { Text = "₱ 0.00", Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = AppTheme.PrimaryDark, Location = new Point(16, y + 20), AutoSize = true };
            left.Controls.Add(lblGrandTotal);

            // ── Right panel (cart) ────────────────────────────────────────────
            var right = new Panel { Location = new Point(320, 60), Size = new Size(614, 480), BackColor = Color.White };
            Controls.Add(right);
            right.Controls.Add(new Label { Text = "Shopping Cart", Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(10, 10), AutoSize = true });

            _cartTable = new DataTable();
            _cartTable.Columns.Add("ProductID", typeof(int));
            _cartTable.Columns.Add("Product", typeof(string));
            _cartTable.Columns.Add("Category", typeof(string));
            _cartTable.Columns.Add("Price", typeof(decimal));
            _cartTable.Columns.Add("Qty", typeof(int));
            _cartTable.Columns.Add("Subtotal", typeof(decimal));

            dgvCart = new DataGridView
            {
                Location = new Point(10, 34),
                Size = new Size(590, 430),
                DataSource = _cartTable,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = AppTheme.FontBody
            };
            dgvCart.EnableHeadersVisualStyles = false;
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextDark;
            dgvCart.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBold;
            dgvCart.ColumnHeadersHeight = 32;
            dgvCart.DataBindingComplete += (s, e) =>
            {
                if (dgvCart.Columns.Contains("ProductID"))
                    dgvCart.Columns["ProductID"].Visible = false;
            };
            right.Controls.Add(dgvCart);

            // ── Bottom bar ────────────────────────────────────────────────────
            var bottom = new Panel { Location = new Point(320, 540), Size = new Size(614, 60), BackColor = AppTheme.BgCard };
            Controls.Add(bottom);

            btnPostSale = PrimaryBtn("✔  Post Sale", 10, 12, AppTheme.Success);
            btnPostSale.Size = new Size(160, 36);
            btnPostSale.Click += BtnPostSale_Click;
            bottom.Controls.Add(btnPostSale);

            btnClose = PrimaryBtn("✕  Close", 180, 12, AppTheme.Danger);
            btnClose.Click += (s, e) => Close();
            bottom.Controls.Add(btnClose);
        }

        private Label SectionLabel(string text, int x, int y) =>
            new Label { Text = text, Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(x, y), AutoSize = true };

        private Button PrimaryBtn(string text, int x, int y, Color bg)
        {
            var b = new Button
            {
                Text = text,
                Font = AppTheme.FontBold,
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(x, y),
                Size = new Size(284, 34),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void LoadCombos()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var dt = new DataTable();
                    new MySqlDataAdapter("SELECT CustomerID, CONCAT(FirstName,' ',LastName) AS Name FROM customers ORDER BY FirstName", conn).Fill(dt);
                    cboCustomer.DataSource = dt;
                    cboCustomer.DisplayMember = "Name";
                    cboCustomer.ValueMember = "CustomerID";

                    var dp = new DataTable();
                    new MySqlDataAdapter("SELECT ProductID, CONCAT(ProductName,' [',Size,'] — ₱',Price) AS Label, Stock FROM products WHERE Stock>0 ORDER BY ProductName", conn).Fill(dp);
                    cboProduct.DataSource = dp;
                    cboProduct.DisplayMember = "Label";
                    cboProduct.ValueMember = "ProductID";
                }
            }
            catch (Exception ex) { MessageBox.Show("Load error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CboProduct_Changed(object sender, EventArgs e)
        {
            if (cboProduct.SelectedItem is DataRowView drv)
            {
                int stock = Convert.ToInt32(drv["Stock"]);
                lblStock.Text = $"Stock: {stock} units";
                lblStock.ForeColor = stock < 20 ? AppTheme.Danger : AppTheme.Success;
                nudQty.Maximum = stock > 0 ? stock : 1;
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (cboProduct.SelectedItem == null) return;
            var drv = (DataRowView)cboProduct.SelectedItem;
            int productId = Convert.ToInt32(drv["ProductID"]);
            int qty = (int)nudQty.Value;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var cmd = new MySqlCommand("SELECT ProductName, Category, Price FROM products WHERE ProductID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", productId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return;
                        string name = r["ProductName"].ToString();
                        string cat = r["Category"].ToString();
                        decimal price = Convert.ToDecimal(r["Price"]);
                        decimal sub = price * qty;

                        // Merge if already in cart
                        foreach (DataRow row in _cartTable.Rows)
                        {
                            if (Convert.ToInt32(row["ProductID"]) == productId)
                            {
                                row["Qty"] = Convert.ToInt32(row["Qty"]) + qty;
                                row["Subtotal"] = Convert.ToDecimal(row["Price"]) * Convert.ToInt32(row["Qty"]);
                                RecalcTotal();
                                return;
                            }
                        }
                        _cartTable.Rows.Add(productId, name, cat, price, qty, sub);
                        RecalcTotal();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow == null) return;
            int idx = dgvCart.CurrentRow.Index;
            if (idx >= 0 && idx < _cartTable.Rows.Count)
            {
                _cartTable.Rows[idx].Delete();
                RecalcTotal();
            }
        }

        private void ClearCart()
        {
            _cartTable.Clear();
            _grandTotal = 0m;
            lblGrandTotal.Text = "₱ 0.00";
        }

        private void RecalcTotal()
        {
            _grandTotal = 0m;
            foreach (DataRow row in _cartTable.Rows)
                if (row.RowState != DataRowState.Deleted)
                    _grandTotal += Convert.ToDecimal(row["Subtotal"]);
            lblGrandTotal.Text = "₱ " + _grandTotal.ToString("N2");
        }

        private void BtnPostSale_Click(object sender, EventArgs e)
        {
            if (cboCustomer.SelectedItem == null) { MessageBox.Show("Select a customer.", "Validation"); return; }
            if (_cartTable.Rows.Count == 0) { MessageBox.Show("Cart is empty.", "Validation"); return; }

            int customerId = Convert.ToInt32(cboCustomer.SelectedValue);

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                using (var tx = conn.BeginTransaction())
                {
                    var cmdO = new MySqlCommand(
                        "INSERT INTO orders (CustomerID, OrderDate, TotalAmount) VALUES (@cid, NOW(), @total); SELECT LAST_INSERT_ID();", conn, tx);
                    cmdO.Parameters.AddWithValue("@cid", customerId);
                    cmdO.Parameters.AddWithValue("@total", _grandTotal);
                    int orderId = Convert.ToInt32(cmdO.ExecuteScalar());

                    foreach (DataRow row in _cartTable.Rows)
                    {
                        int pid = Convert.ToInt32(row["ProductID"]);
                        int qty = Convert.ToInt32(row["Qty"]);
                        decimal sub = Convert.ToDecimal(row["Subtotal"]);

                        var cmdD = new MySqlCommand(
                            "INSERT INTO orderdetails (OrderID, ProductID, Quantity, Subtotal) VALUES (@oid,@pid,@qty,@sub)", conn, tx);
                        cmdD.Parameters.AddWithValue("@oid", orderId);
                        cmdD.Parameters.AddWithValue("@pid", pid);
                        cmdD.Parameters.AddWithValue("@qty", qty);
                        cmdD.Parameters.AddWithValue("@sub", sub);
                        cmdD.ExecuteNonQuery();

                        var cmdS = new MySqlCommand("UPDATE products SET Stock = Stock - @qty WHERE ProductID=@pid", conn, tx);
                        cmdS.Parameters.AddWithValue("@qty", qty);
                        cmdS.Parameters.AddWithValue("@pid", pid);
                        cmdS.ExecuteNonQuery();
                    }

                    tx.Commit();
                    MessageBox.Show($"✅ Sale posted!\nOrder ID: {orderId}\nTotal: ₱{_grandTotal:N2}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearCart();
                    LoadCombos();
                }
            }
            catch (Exception ex) { MessageBox.Show("Transaction failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}