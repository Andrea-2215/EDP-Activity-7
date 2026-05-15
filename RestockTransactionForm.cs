using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClothingStore;

namespace ClothingStoreIS
{
    /// TRANSACTION 2 — Purchase / Restock Transaction

    public class RestockTransactionForm : Form
    {
        private ComboBox cboSupplier, cboProduct;
        private NumericUpDown nudQty;
        private NumericUpDown nudUnitCost;
        private DataGridView dgvItems;
        private Label lblCurrentStock, lblTotalCost;
        private Button btnAddLine, btnRemoveLine, btnPostRestock, btnClose;
        private DataTable _itemsTable;

        public RestockTransactionForm()
        {
            InitUI();
            LoadCombos();
        }

        private void InitUI()
        {
            Text = "Purchase / Restock Transaction";
            Size = new Size(980, 650);
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
                Text = "📦  Purchase / Restock Transaction",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(20, 12),
                AutoSize = true
            });

            // Left panel
            var left = new Panel { Location = new Point(0, 60), Size = new Size(330, 540), BackColor = AppTheme.BgCard };
            Controls.Add(left);
            int y = 14;

            left.Controls.Add(Lbl("Supplier", 16, y)); y += 24;
            cboSupplier = Combo(16, y); y += 38;
            left.Controls.Add(cboSupplier);

            left.Controls.Add(Lbl("Product to Restock", 16, y)); y += 24;
            cboProduct = Combo(16, y);
            cboProduct.SelectedIndexChanged += (s, e) => RefreshStock();
            y += 38;
            left.Controls.Add(cboProduct);

            lblCurrentStock = new Label { Text = "Current Stock: —", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMid, Location = new Point(16, y), AutoSize = true }; y += 26;
            left.Controls.Add(lblCurrentStock);

            left.Controls.Add(Lbl("Quantity Received", 16, y)); y += 24;
            nudQty = new NumericUpDown { Location = new Point(16, y), Size = new Size(120, 26), Minimum = 1, Maximum = 99999, Value = 1, Font = AppTheme.FontBody }; y += 38;
            left.Controls.Add(nudQty);

            left.Controls.Add(Lbl("Unit Cost (₱)", 16, y)); y += 24;
            nudUnitCost = new NumericUpDown { Location = new Point(16, y), Size = new Size(140, 26), Minimum = 0, Maximum = 999999, DecimalPlaces = 2, Value = 0, Font = AppTheme.FontBody }; y += 42;
            left.Controls.Add(nudUnitCost);

            btnAddLine = Btn("➕  Add Line", AppTheme.PrimaryDark, 16, y); y += 42;
            btnAddLine.Click += BtnAddLine_Click;
            left.Controls.Add(btnAddLine);

            btnRemoveLine = Btn("➖  Remove Selected", AppTheme.Danger, 16, y); y += 42;
            btnRemoveLine.Click += BtnRemoveLine_Click;
            left.Controls.Add(btnRemoveLine);

            left.Controls.Add(Lbl("Total Purchase Cost:", 16, y)); y += 22;
            lblTotalCost = new Label { Text = "₱ 0.00", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = AppTheme.PrimaryDark, Location = new Point(16, y), AutoSize = true };
            left.Controls.Add(lblTotalCost);

            // Right panel
            var right = new Panel { Location = new Point(330, 60), Size = new Size(634, 480), BackColor = Color.White };
            Controls.Add(right);
            right.Controls.Add(new Label { Text = "Restock Items", Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(10, 8), AutoSize = true });

            _itemsTable = new DataTable();
            _itemsTable.Columns.Add("ProductID", typeof(int));
            _itemsTable.Columns.Add("Product", typeof(string));
            _itemsTable.Columns.Add("Category", typeof(string));
            _itemsTable.Columns.Add("CurrentStock", typeof(int));
            _itemsTable.Columns.Add("QtyReceived", typeof(int));
            _itemsTable.Columns.Add("UnitCost", typeof(decimal));
            _itemsTable.Columns.Add("LineCost", typeof(decimal));

            dgvItems = new DataGridView
            {
                Location = new Point(10, 34),
                Size = new Size(610, 430),
                DataSource = _itemsTable,
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

            dgvItems.EnableHeadersVisualStyles = false;
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextDark;
            dgvItems.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBold;
            dgvItems.ColumnHeadersHeight = 32;

          
            right.Controls.Add(dgvItems);
            dgvItems.Columns["ProductID"].Visible = false;

      
            dgvItems.DataBindingComplete += (s, e) =>
            {
                if (dgvItems.Columns["ProductID"] != null)
                    dgvItems.Columns["ProductID"].Visible = false;
            };

            // Bottom bar
            var bottom = new Panel { Location = new Point(330, 540), Size = new Size(634, 60), BackColor = AppTheme.BgCard };
            Controls.Add(bottom);

            btnPostRestock = Btn("✔  Post Restock", AppTheme.Success, 10, 12);
            btnPostRestock.Size = new Size(170, 36);
            btnPostRestock.Click += BtnPostRestock_Click;
            bottom.Controls.Add(btnPostRestock);

            btnClose = Btn("✕  Close", AppTheme.Danger, 192, 12);
            btnClose.Click += (s, e) => Close();
            bottom.Controls.Add(btnClose);
        }

        private Label Lbl(string t, int x, int y) =>
            new Label { Text = t, Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(x, y), AutoSize = true };

        private ComboBox Combo(int x, int y) =>
            new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(x, y), Size = new Size(295, 26), Font = AppTheme.FontBody };

        private Button Btn(string text, Color bg, int x, int y)
        {
            var b = new Button
            {
                Text = text,
                Font = AppTheme.FontBold,
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(x, y),
                Size = new Size(160, 34),
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
                    var ds = new DataTable();
                    new MySqlDataAdapter("SELECT SupplierID, SupplierName FROM suppliers ORDER BY SupplierName", conn).Fill(ds);
                    cboSupplier.DataSource = ds;
                    cboSupplier.DisplayMember = "SupplierName";
                    cboSupplier.ValueMember = "SupplierID";

                    var dp = new DataTable();
                    new MySqlDataAdapter("SELECT ProductID, CONCAT(ProductName,' [',Size,']') AS Label, Stock, Category FROM products ORDER BY ProductName", conn).Fill(dp);
                    cboProduct.DataSource = dp;
                    cboProduct.DisplayMember = "Label";
                    cboProduct.ValueMember = "ProductID";
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RefreshStock()
        {
            if (cboProduct.SelectedItem is DataRowView drv)
            {
                int stock = Convert.ToInt32(drv["Stock"]);
                lblCurrentStock.Text = $"Current Stock: {stock} units";
                lblCurrentStock.ForeColor = stock < 20 ? AppTheme.Danger : AppTheme.TextMid;
            }
        }

        private void BtnAddLine_Click(object sender, EventArgs e)
        {
            if (cboProduct.SelectedItem == null) return;
            var drv = (DataRowView)cboProduct.SelectedItem;
            int pid = Convert.ToInt32(drv["ProductID"]);
            string label = (string)drv["Label"];
            string cat = drv["Category"].ToString();
            int curStock = Convert.ToInt32(drv["Stock"]);
            int qty = (int)nudQty.Value;
            decimal cost = nudUnitCost.Value;
            decimal line = cost * qty;

           
            foreach (DataRow row in _itemsTable.Rows)
            {
                if (Convert.ToInt32(row["ProductID"]) == pid)
                {
                    row["QtyReceived"] = Convert.ToInt32(row["QtyReceived"]) + qty;
                    row["LineCost"] = Convert.ToDecimal(row["UnitCost"]) * Convert.ToInt32(row["QtyReceived"]);
                    RecalcCost();
                    return;
                }
            }

            _itemsTable.Rows.Add(pid, label.Split('[')[0].Trim(), cat, curStock, qty, cost, line);
            RecalcCost();
        }

        private void BtnRemoveLine_Click(object sender, EventArgs e)
        {
            if (dgvItems.CurrentRow == null) return;
            int idx = dgvItems.CurrentRow.Index;
            if (idx >= 0 && idx < _itemsTable.Rows.Count)
            {
                _itemsTable.Rows[idx].Delete();
                RecalcCost();
            }
        }

        private void RecalcCost()
        {
            decimal total = 0m;
            foreach (DataRow row in _itemsTable.Rows)
                if (row.RowState != DataRowState.Deleted)
                    total += Convert.ToDecimal(row["LineCost"]);
            lblTotalCost.Text = "₱ " + total.ToString("N2");
        }

        private void BtnPostRestock_Click(object sender, EventArgs e)
        {
            if (cboSupplier.SelectedItem == null) { MessageBox.Show("Select a supplier.", "Validation"); return; }
            if (_itemsTable.Rows.Count == 0) { MessageBox.Show("Add at least one item.", "Validation"); return; }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                using (var tx = conn.BeginTransaction())
                {
                    foreach (DataRow row in _itemsTable.Rows)
                    {
                        int pid = Convert.ToInt32(row["ProductID"]);
                        int qty = Convert.ToInt32(row["QtyReceived"]);

                        var cmd = new MySqlCommand("UPDATE products SET Stock = Stock + @qty WHERE ProductID = @pid", conn, tx);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@pid", pid);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                    MessageBox.Show(" Restock posted! Stock levels updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _itemsTable.Clear();
                    lblTotalCost.Text = "₱ 0.00";
                    LoadCombos();
                }
            }
            catch (Exception ex) { MessageBox.Show("Transaction failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
