using ClothingStoreIS;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ClothingStore
{
    public class ManageDataForm : Form
    {
        private readonly string _table;
        private DataGridView dgv;
        private Label lblCount;
        private TextBox txtSearch;
        private Button btnRefresh, btnClose;
        private DataTable _fullData;

        private static readonly Dictionary<string, string> Queries =
            new Dictionary<string, string>
            {
                {
                    "customers",
                    @"SELECT c.CustomerID, c.FirstName, c.LastName,
                             c.ContactNo, c.Email,
                             COUNT(o.OrderID) AS Orders,
                             IFNULL(SUM(o.TotalAmount),0) AS TotalSpent
                      FROM customers c
                      LEFT JOIN orders o ON o.CustomerID = c.CustomerID
                      GROUP BY c.CustomerID, c.FirstName, c.LastName,
                               c.ContactNo, c.Email
                      ORDER BY c.CustomerID"
                },
                {
                    "products",
                    @"SELECT ProductID, ProductName, Category, Size,
                             Price, Stock,
                             CASE WHEN Stock < 20 THEN 'Low Stock' ELSE 'OK' END AS StockStatus
                      FROM products
                      ORDER BY ProductID"
                },
                {
                    "orders",
                    @"SELECT o.OrderID, CONCAT(c.FirstName,' ',c.LastName) AS Customer,
                             o.OrderDate, o.TotalAmount,
                             GROUP_CONCAT(p.ProductName SEPARATOR ', ') AS Items
                      FROM orders o
                      JOIN customers c ON c.CustomerID = o.CustomerID
                      JOIN orderdetails od ON od.OrderID = o.OrderID
                      JOIN products p ON p.ProductID = od.ProductID
                      GROUP BY o.OrderID, o.OrderDate, o.TotalAmount
                      ORDER BY o.OrderDate DESC"
                }
            };

        public ManageDataForm(string table)
        {
            _table = table;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
          
            string cap = _table.Length > 0
                ? char.ToUpper(_table[0]) + _table.Substring(1)
                : _table;

            Text = "Manage " + cap;
            Size = new Size(980, 580);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.BgLight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = AppTheme.FontBody;

            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White
            };
            Controls.Add(header);

            var lblH = new Label
            {
                Text = cap,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(20, 12),
                AutoSize = true
            };
            header.Controls.Add(lblH);

            // ── Toolbar ───────────────────────────────────────────────────────
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = AppTheme.BgCard
            };
            Controls.Add(toolbar);

            var lblSrch = new Label
            {
                Text = "Search:",
                Font = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Location = new Point(16, 15),
                AutoSize = true
            };
            toolbar.Controls.Add(lblSrch);

            txtSearch = new TextBox
            {
                Location = new Point(72, 12),
                Size = new Size(250, 26),
                Font = AppTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 248, 251)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            toolbar.Controls.Add(txtSearch);

            btnRefresh = new Button
            {
                Text = "Refresh",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.PrimaryDark,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(334, 11),
                Size = new Size(84, 27),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderColor = AppTheme.Border;
            btnRefresh.Click += (s, e) => LoadData();
            toolbar.Controls.Add(btnRefresh);

            lblCount = new Label
            {
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Location = new Point(430, 16),
                AutoSize = true
            };
            toolbar.Controls.Add(lblCount);

            btnClose = new Button
            {
                Text = "Close",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppTheme.PrimaryDark,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(868, 10),
                Size = new Size(84, 27),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();
            toolbar.Controls.Add(btnClose);

            // ── Grid ──────────────────────────────────────────────────────────
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                GridColor = AppTheme.Border,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false,
                Font = AppTheme.FontBody
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextDark;
            dgv.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBold;
            dgv.ColumnHeadersHeight = 34;
            dgv.DefaultCellStyle.SelectionBackColor = AppTheme.Primary;
            dgv.DefaultCellStyle.SelectionForeColor = AppTheme.TextDark;
            dgv.RowTemplate.Height = 28;
            dgv.RowPrePaint += Dgv_RowPrePaint;
            Controls.Add(dgv);

           
            Controls.SetChildIndex(dgv, 0);
            Controls.SetChildIndex(toolbar, 1);
            Controls.SetChildIndex(header, 2);
        }

        private void Dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                    e.RowIndex % 2 == 0 ? Color.White : AppTheme.RowAlt;
            }
        }

        private void LoadData()
        {
            if (!Queries.ContainsKey(_table)) return;
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(Queries[_table], conn);
                    _fullData = new DataTable();
                    da.Fill(_fullData);
                    dgv.DataSource = _fullData;
                    lblCount.Text = _fullData.Rows.Count + " record(s)";
                    txtSearch.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB Error:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_fullData == null) return;

            string q = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(q))
            {
                dgv.DataSource = _fullData;
                lblCount.Text = _fullData.Rows.Count + " record(s)";
                return;
            }

            DataView view = new DataView(_fullData);
            List<string> cols = new List<string>();

            foreach (DataColumn col in _fullData.Columns)
            {
                if (col.DataType == typeof(string))
                    cols.Add("CONVERT([" + col.ColumnName + "], System.String) LIKE '%" + q + "%'");
            }

            if (cols.Count > 0)
            {
                view.RowFilter = string.Join(" OR ", cols);
                dgv.DataSource = view;
                lblCount.Text = view.Count + " of " + _fullData.Rows.Count + " record(s)";
            }
        }
    }
}
