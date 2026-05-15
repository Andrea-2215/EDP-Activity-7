using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClothingStore;

namespace ClothingStoreIS
{
    /// TRANSACTION 3 — Sales Return / Refund
    public class ReturnTransactionForm : Form
    {
        private ComboBox cboOrder;
        private DataGridView dgvOrderItems, dgvReturns;
        private NumericUpDown nudReturnQty;
        private TextBox txtReason;
        private Label lblOrderInfo, lblRefund;
        private Button btnLoadOrder, btnProcessReturn, btnClose;

        public ReturnTransactionForm()
        {
            InitUI();
            LoadOrders();
        }

        private void InitUI()
        {
            Text = "Sales Return / Refund";
            Size = new Size(1000, 680);
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
                Text = "↩  Sales Return / Refund",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(20, 12),
                AutoSize = true
            });

          
            var pnlTop = new Panel { Location = new Point(0, 60), Size = new Size(1000, 70), BackColor = AppTheme.BgCard };
            Controls.Add(pnlTop);
            pnlTop.Controls.Add(new Label { Text = "Step 1 — Select Order:", Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(16, 24), AutoSize = true });
            cboOrder = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(190, 20), Size = new Size(400, 26), Font = AppTheme.FontBody };
            pnlTop.Controls.Add(cboOrder);
            btnLoadOrder = MkBtn("Load Order Items", AppTheme.PrimaryDark, 604, 18, 160);
            btnLoadOrder.Click += BtnLoadOrder_Click;
            pnlTop.Controls.Add(btnLoadOrder);

            lblOrderInfo = new Label { Text = "", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMid, Location = new Point(776, 24), AutoSize = true };
            pnlTop.Controls.Add(lblOrderInfo);

        
            var pnlMid = new Panel { Location = new Point(0, 130), Size = new Size(580, 260), BackColor = Color.White };
            Controls.Add(pnlMid);
            pnlMid.Controls.Add(new Label { Text = "Step 2 — Order Items (select a row):", Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(10, 8), AutoSize = true });

            dgvOrderItems = MkGrid(10, 34, 558, 215);
            pnlMid.Controls.Add(dgvOrderItems);

           
            var pnlRight = new Panel { Location = new Point(580, 130), Size = new Size(420, 260), BackColor = AppTheme.BgCard };
            Controls.Add(pnlRight);
            pnlRight.Controls.Add(new Label { Text = "Step 3 — Return Details:", Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(14, 8), AutoSize = true });

            pnlRight.Controls.Add(new Label { Text = "Return Quantity:", Font = AppTheme.FontBody, ForeColor = AppTheme.TextDark, Location = new Point(14, 40), AutoSize = true });
            nudReturnQty = new NumericUpDown { Location = new Point(14, 60), Size = new Size(120, 26), Minimum = 1, Maximum = 9999, Value = 1, Font = AppTheme.FontBody };
            pnlRight.Controls.Add(nudReturnQty);

            pnlRight.Controls.Add(new Label { Text = "Reason for Return:", Font = AppTheme.FontBody, ForeColor = AppTheme.TextDark, Location = new Point(14, 96), AutoSize = true });
            txtReason = new TextBox { Location = new Point(14, 116), Size = new Size(384, 60), Multiline = true, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            pnlRight.Controls.Add(txtReason);

            lblRefund = new Label { Text = "Refund Amount: ₱ 0.00", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = AppTheme.PrimaryDark, Location = new Point(14, 188), AutoSize = true };
            pnlRight.Controls.Add(lblRefund);

            dgvOrderItems.SelectionChanged += (s, e) => UpdateRefundLabel();
            nudReturnQty.ValueChanged += (s, e) => UpdateRefundLabel();

            btnProcessReturn = MkBtn("✔  Process Return", AppTheme.Success, 14, 218, 180);
            btnProcessReturn.Click += BtnProcessReturn_Click;
            pnlRight.Controls.Add(btnProcessReturn);

            btnClose = MkBtn("✕  Close", AppTheme.Danger, 204, 218, 110);
            btnClose.Click += (s, e) => Close();
            pnlRight.Controls.Add(btnClose);
            
            
            var pnlBot = new Panel { Location = new Point(0, 390), Size = new Size(1000, 248), BackColor = Color.White };
            Controls.Add(pnlBot);
            pnlBot.Controls.Add(new Label { Text = "Recent Returns:", Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, Location = new Point(10, 8), AutoSize = true });
            dgvReturns = MkGrid(10, 34, 976, 200);
            pnlBot.Controls.Add(dgvReturns);
            LoadRecentReturns();
        }

        private DataGridView MkGrid(int x, int y, int w, int h)
        {
            var g = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = AppTheme.FontBody,
                MultiSelect = false
            };
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            g.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextDark;
            g.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBold;
            g.ColumnHeadersHeight = 32;
            return g;
        }

        private Button MkBtn(string t, Color bg, int x, int y, int w)
        {
            var b = new Button
            {
                Text = t,
                Font = AppTheme.FontBold,
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(x, y),
                Size = new Size(w, 32),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void LoadOrders()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var dt = new DataTable();
                    new MySqlDataAdapter(
                        "SELECT o.OrderID, CONCAT('Order #',o.OrderID,' — ',c.FirstName,' ',c.LastName,' (',DATE(o.OrderDate),')') AS Label " +
                        "FROM orders o JOIN customers c ON c.CustomerID=o.CustomerID ORDER BY o.OrderDate DESC LIMIT 200", conn).Fill(dt);
                    cboOrder.DataSource = dt;
                    cboOrder.DisplayMember = "Label";
                    cboOrder.ValueMember = "OrderID";
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnLoadOrder_Click(object sender, EventArgs e)
        {
            if (cboOrder.SelectedItem == null) return;
            int orderId = Convert.ToInt32(cboOrder.SelectedValue);
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var dt = new DataTable();
                    var cmd = new MySqlCommand(
                        "SELECT od.OrderDetailID, p.ProductName, p.Category, od.Quantity AS Ordered, " +
                        "od.Quantity - IFNULL(ret.qty,0) AS Available, od.Subtotal, " +
                        "ROUND(od.Subtotal / od.Quantity, 2) AS UnitPrice " +
                        "FROM orderdetails od " +
                        "JOIN products p ON p.ProductID = od.ProductID " +
                        "LEFT JOIN (SELECT OrderDetailID, SUM(QtyReturned) AS qty FROM returns GROUP BY OrderDetailID) ret " +
                        "  ON ret.OrderDetailID = od.OrderDetailID " +
                        "WHERE od.OrderID = @oid", conn);
                    cmd.Parameters.AddWithValue("@oid", orderId);
                    new MySqlDataAdapter(cmd).Fill(dt);
                    dgvOrderItems.DataSource = dt;
                    lblOrderInfo.Text = $"{dt.Rows.Count} line(s)";
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void UpdateRefundLabel()
        {
            if (dgvOrderItems.CurrentRow == null) { lblRefund.Text = "Refund Amount: ₱ 0.00"; return; }
            if (dgvOrderItems.DataSource == null) return;
            var row = ((DataTable)dgvOrderItems.DataSource).Rows[dgvOrderItems.CurrentRow.Index];
            decimal unit = Convert.ToDecimal(row["UnitPrice"]);
            int retQty = (int)nudReturnQty.Value;
            lblRefund.Text = $"Refund Amount: ₱ {(unit * retQty):N2}";
        }

        private void BtnProcessReturn_Click(object sender, EventArgs e)
        {
            if (dgvOrderItems.CurrentRow == null || dgvOrderItems.DataSource == null)
            { MessageBox.Show("Select an order item first.", "Validation"); return; }
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            { MessageBox.Show("Enter a reason for the return.", "Validation"); return; }

            var row = ((DataTable)dgvOrderItems.DataSource).Rows[dgvOrderItems.CurrentRow.Index];
            int detailId = Convert.ToInt32(row["OrderDetailID"]);
            int available = Convert.ToInt32(row["Available"]);
            int retQty = (int)nudReturnQty.Value;
            decimal unitPrice = Convert.ToDecimal(row["UnitPrice"]);

            if (retQty > available) { MessageBox.Show($"Only {available} unit(s) available to return.", "Validation"); return; }

            int orderId = Convert.ToInt32(cboOrder.SelectedValue);

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                using (var tx = conn.BeginTransaction())
                {
                   
                    var cmdPid = new MySqlCommand("SELECT ProductID FROM orderdetails WHERE OrderDetailID=@id", conn, tx);
                    cmdPid.Parameters.AddWithValue("@id", detailId);
                    int productId = Convert.ToInt32(cmdPid.ExecuteScalar());

                  
                    var cmdR = new MySqlCommand(
                        "INSERT INTO returns (OrderID, OrderDetailID, ProductID, QtyReturned, RefundAmount, Reason, ReturnDate) " +
                        "VALUES (@oid, @did, @pid, @qty, @ref, @rsn, NOW())", conn, tx);
                    cmdR.Parameters.AddWithValue("@oid", orderId);
                    cmdR.Parameters.AddWithValue("@did", detailId);
                    cmdR.Parameters.AddWithValue("@pid", productId);
                    cmdR.Parameters.AddWithValue("@qty", retQty);
                    cmdR.Parameters.AddWithValue("@ref", unitPrice * retQty);
                    cmdR.Parameters.AddWithValue("@rsn", txtReason.Text.Trim());
                    cmdR.ExecuteNonQuery();

                   
                    var cmdS = new MySqlCommand("UPDATE products SET Stock = Stock + @qty WHERE ProductID=@pid", conn, tx);
                    cmdS.Parameters.AddWithValue("@qty", retQty);
                    cmdS.Parameters.AddWithValue("@pid", productId);
                    cmdS.ExecuteNonQuery();

                    tx.Commit();
                    MessageBox.Show($"Return processed!\nQty: {retQty}  |  Refund: ₱{(unitPrice * retQty):N2}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtReason.Clear();
                    BtnLoadOrder_Click(null, null);
                    LoadRecentReturns();
                }
            }
            catch (Exception ex) { MessageBox.Show("Transaction failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void LoadRecentReturns()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var dt = new DataTable();
                    new MySqlDataAdapter(
                        "SELECT r.ReturnID, r.OrderID, p.ProductName, r.QtyReturned, r.RefundAmount, r.Reason, r.ReturnDate " +
                        "FROM returns r JOIN products p ON p.ProductID=r.ProductID " +
                        "ORDER BY r.ReturnDate DESC LIMIT 30", conn).Fill(dt);
                    dgvReturns.DataSource = dt;
                }
            }
            catch { }
        }
    }
}
