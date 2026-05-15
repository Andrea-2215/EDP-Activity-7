using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClothingStore;

namespace ClothingStoreIS
{
    public class DashboardForm : Form
    {
        private Label lblTotalCustomers, lblTotalProducts, lblTotalOrders,
                      lblTotalRevenue, lblLowStock, lblTodayDate;
        private DataGridView dgvRecent;
        private Button btnCustomers, btnProducts, btnOrders,
                       btnReports, btnUsers, btnAbout, btnLogout;
        // ── transaction buttons ──────────────────────────────────────────
        private Button btnSale, btnRestock, btnReturn;

        public DashboardForm()
        {
            InitUI();
            LoadDashboard();
        }

        private void InitUI()
        {
            Text = "Clothing Store IS — Dashboard";
            Size = new Size(1100, 720);
            MinimumSize = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppTheme.BgLight;
            Font = AppTheme.FontBody;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // ── Sidebar ───────────────────────────────────────────────────────
            var sidebar = new Panel { Size = new Size(210, 720), Location = new Point(0, 0), BackColor = AppTheme.PrimaryDark };
            Controls.Add(sidebar);

            sidebar.Controls.Add(new Label
            {
                Text = "👗 ClothingIS",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 20),
                Size = new Size(210, 40),
                TextAlign = ContentAlignment.MiddleCenter
            });
            sidebar.Controls.Add(new Panel { Location = new Point(20, 65), Size = new Size(170, 1), BackColor = Color.FromArgb(255, 200, 215) });

            var lblCurrentUser = new Label
            {
                Text = $"👤 {LoginForm.CurrentUsername}",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 220, 230),
                Location = new Point(0, 70),
                Size = new Size(210, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            sidebar.Controls.Add(lblCurrentUser);

            // ── Separator label ──────
            int navY = 95;
            SideLabel(sidebar, "── TRANSACTIONS ──", navY); navY += 22;

            btnSale = NavButton("🛒  New Sale", navY); navY += 48;
            btnRestock = NavButton("📦  Restock", navY); navY += 48;
            btnReturn = NavButton("↩  Returns", navY); navY += 52;

            sidebar.Controls.AddRange(new Control[] { btnSale, btnRestock, btnReturn });

            btnSale.Click += (s, e) => new SalesTransactionForm().ShowDialog();
            btnRestock.Click += (s, e) => new RestockTransactionForm().ShowDialog();
            btnReturn.Click += (s, e) => new ReturnTransactionForm().ShowDialog();

            SideLabel(sidebar, "── MANAGEMENT ──", navY); navY += 22;

            btnCustomers = NavButton("👥  Customers", navY); navY += 48;
            btnProducts = NavButton("🧥  Products", navY); navY += 48;
            btnOrders = NavButton("📋  Orders", navY); navY += 48;
            btnReports = NavButton("📊  Reports", navY); navY += 48;
            btnUsers = NavButton("👤  Users", navY); navY += 48;
            btnAbout = NavButton("ℹ️   About", navY);

            sidebar.Controls.AddRange(new Control[] { btnCustomers, btnProducts, btnOrders, btnReports, btnUsers, btnAbout });

            btnCustomers.Click += (s, e) => OpenManage("customers");
            btnProducts.Click += (s, e) => OpenManage("products");
            btnOrders.Click += (s, e) => OpenManage("orders");
            btnReports.Click += (s, e) => new ReportForm().ShowDialog();
            btnUsers.Click += (s, e) => new UserManagementForm().ShowDialog();
            btnAbout.Click += (s, e) => new AboutForm().ShowDialog();

            btnUsers.Visible = LoginForm.CurrentRole == "Administrator";

            btnLogout = NavButton("🚪  Logout", 650);
            btnLogout.BackColor = Color.FromArgb(200, 80, 100);
            btnLogout.Click += (s, e) => { if (Confirm("Log out?")) Close(); };
            sidebar.Controls.Add(btnLogout);

            // ── Main area ─────────────────────────────────────────────────────
            var main = new Panel { Location = new Point(210, 0), Size = new Size(890, 720), BackColor = AppTheme.BgLight };
            Controls.Add(main);

            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            main.Controls.Add(header);

            header.Controls.Add(new Label
            {
                Text = "Dashboard",
                Font = AppTheme.FontTitle,
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(24, 10),
                AutoSize = true
            });

            lblTodayDate = new Label
            {
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Location = new Point(24, 40),
                AutoSize = true
            };
            header.Controls.Add(lblTodayDate);

           
            var kpiPanel = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = AppTheme.BgLight, Padding = new Padding(16, 10, 16, 0) };
            main.Controls.Add(kpiPanel);

            int cx = 16;
            lblTotalCustomers = KpiCard(kpiPanel, "Customers", "—", cx); cx += 170;
            lblTotalProducts = KpiCard(kpiPanel, "Products", "—", cx); cx += 170;
            lblTotalOrders = KpiCard(kpiPanel, "Orders", "—", cx); cx += 170;
            lblTotalRevenue = KpiCard(kpiPanel, "Revenue", "—", cx); cx += 170;
            lblLowStock = KpiCard(kpiPanel, "Low Stock", "—", cx);

            // ── Recent orders grid ────────────────────────────────────────────
            var gridHdr = new Label
            {
                Text = "Recent Orders",
                Font = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                BackColor = AppTheme.BgLight
            };
            main.Controls.Add(gridHdr);

            dgvRecent = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = AppTheme.Border,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = AppTheme.FontBody
            };
            dgvRecent.EnableHeadersVisualStyles = false;
            dgvRecent.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            dgvRecent.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextDark;
            dgvRecent.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBold;
            dgvRecent.ColumnHeadersHeight = 34;
            dgvRecent.DefaultCellStyle.SelectionBackColor = AppTheme.Primary;
            dgvRecent.RowTemplate.Height = 28;
            dgvRecent.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    dgvRecent.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                        e.RowIndex % 2 == 0 ? Color.White : AppTheme.RowAlt;
            };
            main.Controls.Add(dgvRecent);

            
            main.Controls.SetChildIndex(dgvRecent, 0);
            main.Controls.SetChildIndex(gridHdr, 1);
            main.Controls.SetChildIndex(kpiPanel, 2);
            main.Controls.SetChildIndex(header, 3);
        }

      
        private Label KpiCard(Panel parent, string caption, string value, int x)
        {
            var card = new Panel
            {
                Location = new Point(x, 6),
                Size = new Size(156, 88),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.Border, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var brush = new SolidBrush(AppTheme.PrimaryDark))
                    e.Graphics.FillRectangle(brush, 0, 0, card.Width, 4);
            };
            parent.Controls.Add(card);

            card.Controls.Add(new Label
            {
                Text = caption,
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Location = new Point(10, 12),
                AutoSize = true
            });
            var valLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(10, 32),
                AutoSize = true
            };
            card.Controls.Add(valLabel);
            return valLabel;
        }

        private void SideLabel(Panel parent, string text, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 215),
                Location = new Point(0, y),
                Size = new Size(210, 18),
                TextAlign = ContentAlignment.MiddleCenter
            });
        }

        // ── Nav button ────────────────────────────────────────────────
        private Button NavButton(string text, int y)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, y),
                Size = new Size(210, 44),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 100, 140);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 60, 90);
            return b;
        }

        // ── Load dashboard ──────────────────────────────────────────────
        private void LoadDashboard()
        {
            lblTodayDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    lblTotalCustomers.Text = Scalar("SELECT COUNT(*) FROM customers", conn);
                    lblTotalProducts.Text = Scalar("SELECT COUNT(*) FROM products", conn);
                    lblTotalOrders.Text = Scalar("SELECT COUNT(*) FROM orders", conn);
                    lblTotalRevenue.Text = "₱" + Scalar("SELECT IFNULL(SUM(TotalAmount),0) FROM orders", conn, fmt: "N0");
                    lblLowStock.Text = Scalar("SELECT COUNT(*) FROM products WHERE Stock<20", conn);

                    // Recent orders
                    var da = new MySqlDataAdapter(
                        @"SELECT o.OrderID, CONCAT(c.FirstName,' ',c.LastName) AS Customer,
                                 o.OrderDate, o.TotalAmount AS Total
                          FROM orders o JOIN customers c ON c.CustomerID=o.CustomerID
                          ORDER BY o.OrderDate DESC LIMIT 15", conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvRecent.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dashboard load error:\n" + ex.Message, "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string Scalar(string sql, MySqlConnection conn, string fmt = null)
        {
            try
            {
                var cmd = new MySqlCommand(sql, conn);
                var result = cmd.ExecuteScalar();
                if (fmt != null && decimal.TryParse(result?.ToString(), out decimal d))
                    return d.ToString(fmt);
                return result?.ToString() ?? "0";
            }
            catch { return "?"; }
        }

        private void OpenManage(string table) => new ManageDataForm(table).ShowDialog();

        private bool Confirm(string msg) =>
            MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
