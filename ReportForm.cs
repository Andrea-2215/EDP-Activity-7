using ClothingStore;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ClothingStoreIS
{
    
    /// Report Generation Module — DataGrid view + Excel export.
    public class ReportForm : Form
    {
        private ComboBox cboReport;
        private Button btnRun, btnExport, btnClose;
        private DataGridView dgvResult;
        private Label lblInfo;
        private Panel pnlToolbar;

        // ── Report definitions ────────────────────────────────────────────────
        private readonly (string Label, string SQL, string ChartColX, string ChartColY)[] Reports =
        {
            (
                "Customer Orders Summary",
                @"SELECT c.CustomerID, CONCAT(c.FirstName,' ',c.LastName) AS Customer,
                         c.Email, COUNT(o.OrderID) AS TotalOrders,
                         IFNULL(SUM(o.TotalAmount),0) AS TotalSpent
                  FROM customers c
                  LEFT JOIN orders o ON o.CustomerID = c.CustomerID
                  GROUP BY c.CustomerID, c.FirstName, c.LastName, c.Email
                  ORDER BY TotalSpent DESC",
                "Customer", "TotalSpent"
            ),
            (
                "Product Sales Report",
                @"SELECT p.ProductID, p.ProductName, p.Category, p.Size,
                         p.Price, IFNULL(SUM(od.Quantity),0) AS TotalSold,
                         IFNULL(SUM(od.Subtotal),0) AS TotalRevenue
                  FROM products p
                  LEFT JOIN orderdetails od ON od.ProductID = p.ProductID
                  GROUP BY p.ProductID, p.ProductName, p.Category, p.Size, p.Price
                  ORDER BY TotalRevenue DESC",
                "ProductName", "TotalRevenue"
            ),
            (
                "Sales Return Report",
                @"SELECT r.ReturnID, r.OrderID, p.ProductName, r.QtyReturned,
                         r.RefundAmount, r.Reason, DATE(r.ReturnDate) AS ReturnDate
                  FROM returns r
                  JOIN products p ON p.ProductID = r.ProductID
                  ORDER BY r.ReturnDate DESC",
                "ProductName", "RefundAmount"
            ),
            (
                "Revenue by Category",
                @"SELECT p.Category,
                         IFNULL(SUM(od.Quantity),0) AS UnitsSold,
                         IFNULL(SUM(od.Subtotal),0) AS Revenue
                  FROM products p
                  LEFT JOIN orderdetails od ON od.ProductID = p.ProductID
                  GROUP BY p.Category
                  ORDER BY Revenue DESC",
                "Category", "Revenue"
            ),
            (
                "Low Stock Products (< 30 units)",
                @"SELECT ProductID, ProductName, Category, Size, Price, Stock
                  FROM products
                  WHERE Stock < 30
                  ORDER BY Stock ASC",
                "ProductName", "Stock"
            ),
            (
                "Order Details Report",
                @"SELECT o.OrderID, CONCAT(c.FirstName,' ',c.LastName) AS Customer,
                         o.OrderDate, p.ProductName, p.Category,
                         od.Quantity, od.Subtotal
                  FROM orders o
                  JOIN customers c ON c.CustomerID = o.CustomerID
                  JOIN orderdetails od ON od.OrderID = o.OrderID
                  JOIN products p ON p.ProductID = od.ProductID
                  ORDER BY o.OrderDate DESC",
                "Customer", "Subtotal"
            ),
        };

        public ReportForm()
        {
            InitUI();
        }

        private void InitUI()
        {
            Text = "Report Generator";
            Size = new Size(1060, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.BgLight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = AppTheme.FontBody;

            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };
            Controls.Add(header);
            header.Controls.Add(new Label
            {
                Text = "📊  Report Generator",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(20, 12),
                AutoSize = true
            });
            header.Controls.Add(new Label
            {
                Text = "Select a report, click Run, then Export to Excel (.xlsx).",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Location = new Point(22, 42),
                AutoSize = true
            });

            // ── Toolbar ───────────────────────────────────────────────────────
            pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard };
            Controls.Add(pnlToolbar);

            pnlToolbar.Controls.Add(new Label
            {
                Text = "Report:",
                Font = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Location = new Point(16, 16),
                AutoSize = true
            });

            cboReport = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.FontBody,
                Location = new Point(74, 12),
                Size = new Size(380, 28),
                BackColor = Color.FromArgb(255, 248, 251)
            };
            foreach (var r in Reports) cboReport.Items.Add(r.Label);
            if (cboReport.Items.Count > 0) cboReport.SelectedIndex = 0;
            pnlToolbar.Controls.Add(cboReport);

            btnRun = ToolBtn("▶ Run Report", AppTheme.PrimaryDark, 466);
            btnRun.Click += BtnRun_Click;
            pnlToolbar.Controls.Add(btnRun);

            btnExport = ToolBtn("📥 Export Excel", AppTheme.Success, 580);
            btnExport.Click += BtnExport_Click;
            pnlToolbar.Controls.Add(btnExport);

            btnClose = ToolBtn("✕ Close", AppTheme.Danger, 700);
            btnClose.Click += (s, e) => Close();
            pnlToolbar.Controls.Add(btnClose);

            // ── Info strip ────────────────────────────────────────────────────
            lblInfo = new Label
            {
                Text = "No report loaded yet.",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                BackColor = AppTheme.RowAlt
            };
            Controls.Add(lblInfo);

            // ── DataGridView ──────────────────────────────────────────────────
            dgvResult = new DataGridView
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
            dgvResult.EnableHeadersVisualStyles = false;
            dgvResult.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            dgvResult.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextDark;
            dgvResult.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBold;
            dgvResult.ColumnHeadersHeight = 34;
            dgvResult.DefaultCellStyle.SelectionBackColor = AppTheme.Primary;
            dgvResult.DefaultCellStyle.SelectionForeColor = AppTheme.TextDark;
            dgvResult.RowTemplate.Height = 28;
            dgvResult.RowPrePaint += (sender, e) =>
            {
                if (e.RowIndex >= 0)
                    dgvResult.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                        e.RowIndex % 2 == 0 ? Color.White : AppTheme.RowAlt;
            };
            Controls.Add(dgvResult);

            // Z-order
            Controls.SetChildIndex(dgvResult, 0);
            Controls.SetChildIndex(lblInfo, 1);
            Controls.SetChildIndex(pnlToolbar, 2);
            Controls.SetChildIndex(header, 3);
        }

        // ── Run report ────────────────────────────────────────────────────────
        private void BtnRun_Click(object sender, EventArgs e)
        {
            if (cboReport.SelectedIndex < 0) return;
            var rep = Reports[cboReport.SelectedIndex];
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var da = new MySqlDataAdapter(rep.SQL, conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvResult.DataSource = dt;
                    lblInfo.Text = $"✓ {rep.Label}  —  {dt.Rows.Count} row(s)  ·  Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    lblInfo.ForeColor = AppTheme.Success;
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text = "✗ Error: " + ex.Message;
                lblInfo.ForeColor = AppTheme.Danger;
            }
        }

        // ── Export to Excel ───────────────────────────────────────────────────
        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvResult.DataSource == null)
            {
                MessageBox.Show("Please run a report first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                string safeName = Reports[cboReport.SelectedIndex].Label.Replace(" ", "_").Replace("/", "-");
                dlg.Filter = "Excel files (*.xlsx)|*.xlsx";
                dlg.FileName = $"Report_{safeName}_{DateTime.Now:yyyyMMdd}.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    DataTable dt = (DataTable)dgvResult.DataSource;
                    var rep = Reports[cboReport.SelectedIndex];

                    using (var pkg = new ExcelPackage())
                    {
                        // ══ Sheet 1 — Report Data ════════════════════════════
                        var ws = pkg.Workbook.Worksheets.Add("Report");

                       
                        ws.Cells["A1:B3"].Merge = true;
                        ws.Cells["A1"].Value = "👚"; 
                        ws.Cells["A1"].Style.Font.Size = 28;
                        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Row(1).Height = 22;
                        ws.Row(2).Height = 22;
                        ws.Row(3).Height = 22;

                        // Company name
                        ws.Cells["C1:J1"].Merge = true;
                        ws.Cells["C1"].Value = "CLOTHINGIS — Clothing Store Information System";
                        ws.Cells["C1"].Style.Font.Bold = true;
                        ws.Cells["C1"].Style.Font.Size = 16;
                        ws.Cells["C1"].Style.Font.Color.SetColor(Color.FromArgb(219, 112, 147));
                        ws.Cells["C1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells["C2:J2"].Merge = true;
                        ws.Cells["C2"].Value = "Legazpi City, Albay  |  Tel: (+63) 927-4146-455  |  clothingis@store.ph";
                        ws.Cells["C2"].Style.Font.Size = 9;
                        ws.Cells["C2"].Style.Font.Color.SetColor(Color.Gray);
                        ws.Cells["C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells["C3:J3"].Merge = true;
                        ws.Cells["C3"].Value = "www.clothingis.store.ph";
                        ws.Cells["C3"].Style.Font.Size = 9;
                        ws.Cells["C3"].Style.Font.Color.SetColor(Color.Gray);
                        ws.Cells["C3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        // Divider
                        ws.Cells[4, 1, 4, dt.Columns.Count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[4, 1, 4, dt.Columns.Count].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(219, 112, 147));
                        ws.Row(4).Height = 3;

                        // Report title
                        ws.Cells["A5"].Value = rep.Label;
                        ws.Cells["A5"].Style.Font.Bold = true;
                        ws.Cells["A5"].Style.Font.Size = 13;
                        ws.Cells["A5"].Style.Font.Color.SetColor(Color.FromArgb(80, 40, 55));

                        ws.Cells["A6"].Value = $"Generated by: {LoginForm.CurrentUsername}   |   Date: {DateTime.Now:MMMM dd, yyyy  HH:mm}";
                        ws.Cells["A6"].Style.Font.Size = 9;
                        ws.Cells["A6"].Style.Font.Color.SetColor(Color.Gray);
                        ws.Row(7).Height = 6; // spacer

                        // ── Column headers (row 8) ────────────────────────────
                        int headerRow = 8;
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            var cell = ws.Cells[headerRow, c + 1];
                            cell.Value = dt.Columns[c].ColumnName;
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Color.SetColor(Color.FromArgb(80, 40, 55));
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 182, 193));
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                            cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(219, 112, 147));
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        // ── Data rows ─────────────────────────────────────────
                        int dataStartRow = headerRow + 1;
                        for (int r = 0; r < dt.Rows.Count; r++)
                        {
                            bool alt = r % 2 != 0;
                            for (int c = 0; c < dt.Columns.Count; c++)
                            {
                                var cell = ws.Cells[dataStartRow + r, c + 1];
                                var val = dt.Rows[r][c];
                                cell.Value = val is DBNull ? "" : val;
                                if (alt)
                                {
                                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 240, 245));
                                }
                                // Right-align numbers
                                if (val is decimal || val is double || val is int || val is long)
                                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                // Format currency columns
                                string colName = dt.Columns[c].ColumnName.ToLower();
                                if (colName.Contains("amount") || colName.Contains("spent") ||
                                    colName.Contains("revenue") || colName.Contains("price") ||
                                    colName.Contains("subtotal") || colName.Contains("cost") ||
                                    colName.Contains("refund"))
                                    cell.Style.Numberformat.Format = "#,##0.00";
                            }
                        }

                        int lastDataRow = dataStartRow + dt.Rows.Count - 1;

                        // Table border
                        if (dt.Rows.Count > 0)
                        {
                            var tableRange = ws.Cells[headerRow, 1, lastDataRow, dt.Columns.Count];
                            tableRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(219, 112, 147));
                        }

                        // Auto-fit columns
                        ws.Cells[ws.Dimension.Address].AutoFitColumns(8, 40);

                        // ── Signature block ───────────────────────────────────
                        int sigRow = lastDataRow + 3;
                        ws.Cells[sigRow, 1].Value = "Prepared by:";
                        ws.Cells[sigRow, 1].Style.Font.Bold = true;
                        ws.Cells[sigRow + 2, 1].Value = LoginForm.CurrentUsername.ToUpper();
                        ws.Cells[sigRow + 2, 1].Style.Font.Bold = true;
                        ws.Cells[sigRow + 3, 1].Value = "Report Generator / " + LoginForm.CurrentRole;
                        ws.Cells[sigRow + 3, 1].Style.Font.Size = 8;
                        // Signature line
                        ws.Cells[sigRow + 2, 1, sigRow + 2, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        ws.Cells[sigRow + 2, 1, sigRow + 2, 3].Style.Border.Bottom.Color.SetColor(Color.Black);

                        // Approved by block (offset)
                        int ac = dt.Columns.Count - 2;
                        if (ac < 5) ac = 5;
                        ws.Cells[sigRow, ac].Value = "Approved by:";
                        ws.Cells[sigRow, ac].Style.Font.Bold = true;
                        ws.Cells[sigRow + 2, ac].Value = "___________________________";
                        ws.Cells[sigRow + 3, ac].Value = "Store Manager / Authorized Signatory";
                        ws.Cells[sigRow + 3, ac].Style.Font.Size = 8;

                        // ══ Sheet 2 — Chart ══════════════════════════════════
                        var wsChart = pkg.Workbook.Worksheets.Add("Chart");

                        // Copy summary columns for chart (ColX label, ColY value)
                        int xIdx = -1, yIdx = -1;
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            if (dt.Columns[c].ColumnName == rep.ChartColX) xIdx = c;
                            if (dt.Columns[c].ColumnName == rep.ChartColY) yIdx = c;
                        }
                        
                        if (xIdx < 0) xIdx = 0;
                        if (yIdx < 0) yIdx = dt.Columns.Count - 1;

                       
                        wsChart.Cells["A1"].Value = dt.Columns[xIdx].ColumnName;
                        wsChart.Cells["B1"].Value = dt.Columns[yIdx].ColumnName;
                        wsChart.Cells["A1"].Style.Font.Bold = true;
                        wsChart.Cells["B1"].Style.Font.Bold = true;

                        int chartRows = Math.Min(dt.Rows.Count, 20);
                        for (int r = 0; r < chartRows; r++)
                        {
                            wsChart.Cells[r + 2, 1].Value = dt.Rows[r][xIdx]?.ToString();
                            var numVal = dt.Rows[r][yIdx];
                            wsChart.Cells[r + 2, 2].Value = numVal is DBNull ? 0 : Convert.ToDouble(numVal);
                        }
                        wsChart.Cells[1, 1, chartRows + 1, 2].AutoFitColumns(10, 40);

                        // Chart title panel
                        wsChart.Cells["D1:L2"].Merge = true;
                        wsChart.Cells["D1"].Value = "CLOTHINGIS — " + rep.Label;
                        wsChart.Cells["D1"].Style.Font.Bold = true;
                        wsChart.Cells["D1"].Style.Font.Size = 13;
                        wsChart.Cells["D1"].Style.Font.Color.SetColor(Color.FromArgb(219, 112, 147));
                        wsChart.Cells["D1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsChart.Cells["D1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        wsChart.Cells["D3"].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy}   By: {LoginForm.CurrentUsername}";
                        wsChart.Cells["D3"].Style.Font.Size = 9;
                        wsChart.Cells["D3"].Style.Font.Color.SetColor(Color.Gray);

                        // Bar chart
                        var chart = wsChart.Drawings.AddChart("ReportChart", eChartType.ColumnClustered) as ExcelBarChart;
                        if (chart != null)
                        {
                            chart.SetPosition(3, 0, 3, 0);   // row 4, col D
                            chart.SetSize(700, 380);
                            chart.Title.Text = rep.Label;
                            chart.Title.Font.Bold = true;

                            var series = chart.Series.Add(
                                wsChart.Cells[2, 2, chartRows + 1, 2],
                                wsChart.Cells[2, 1, chartRows + 1, 1]);
                            series.Header = dt.Columns[yIdx].ColumnName;

                            chart.XAxis.Title.Text = dt.Columns[xIdx].ColumnName;
                            chart.YAxis.Title.Text = dt.Columns[yIdx].ColumnName;
                            chart.Legend.Remove();
                        }
                        else
                        {
                            // Fallback: any chart type (EPPlus version difference)
                            var chartAny = wsChart.Drawings.AddChart("ReportChart", eChartType.BarClustered);
                            chartAny.SetPosition(3, 0, 3, 0);
                            chartAny.SetSize(700, 380);
                            chartAny.Title.Text = rep.Label;
                            var series = chartAny.Series.Add(
                                wsChart.Cells[2, 2, chartRows + 1, 2],
                                wsChart.Cells[2, 1, chartRows + 1, 1]);
                            series.Header = dt.Columns[yIdx].ColumnName;
                        }

                        // Save
                        pkg.SaveAs(new FileInfo(dlg.FileName));
                    }

                    MessageBox.Show($"✅ Excel report exported to:\n{dlg.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static Button ToolBtn(string text, Color bg, int x)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(x, 10),
                Size = new Size(106, 30),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
