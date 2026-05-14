using System.Drawing;
using System.Windows.Forms;

namespace ClothingStoreIS
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            InitUI();
        }

        private void InitUI()
        {
            Text = "About – Clothing Store IS";
            Size = new Size(500, 560);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.BgLight;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = AppTheme.FontBody;

            // ── Pink header band ──────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = AppTheme.Primary
            };
            Controls.Add(header);

            var lblIcon = new Label
            {
                Text = "🛍️",
                Font = new Font("Segoe UI Emoji", 38f),
                Location = new Point(0, 18),
                Size = new Size(500, 56),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblIcon);

            var lblAppName = new Label
            {
                Text = "Clothing Store Information System",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppTheme.TextDark,
                Location = new Point(0, 80),
                Size = new Size(500, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblAppName);

            // ── Body content ──────────────────────────────────────────────────
            int y = 148;

            AddSection("Version", "1.0.0  (May 2026)", ref y);
            AddSection("Platform", "Windows Forms / C# (.NET 6)", ref y);
            AddSection("Database", "MySQL / MariaDB 10.4  via phpMyAdmin", ref y);
            AddSection("Description",
                "A desktop-based management system for a clothing retail store.\n" +
                "Manages customers, products, suppliers, orders, and generates\n" +
                "detailed sales reports with live MySQL connectivity.", ref y);
            AddSection("Modules",
                "• Login & Authentication\n" +
                "• Password Recovery\n" +
                "• Dashboard (KPI Overview)\n" +
                "• Data Management (Customers, Products, Orders)\n" +
                "• Report Generator", ref y);
            AddSection("Developer", "ANDREA / CUTE", ref y);
            AddSection("Institution", "BICOL UNIVERSITY", ref y);

            // ── Divider ───────────────────────────────────────────────────────
            var div = new Panel
            {
                Location = new Point(40, y + 6),
                Size = new Size(420, 1),
                BackColor = AppTheme.Border
            };
            Controls.Add(div);

            // ── Close button ──────────────────────────────────────────────────
            var btnClose = new Button
            {
                Text = "CLOSE",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppTheme.PrimaryDark,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 38),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Location = new Point((500 - 140) / 2, y + 18);
            btnClose.Click += (s, e) => Close();
            Controls.Add(btnClose);
        }

        private void AddSection(string heading, string value, ref int y)
        {
            var lblH = new Label
            {
                Text = heading,
                Font = AppTheme.FontBold,
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(50, y),
                Size = new Size(120, 18)
            };
            Controls.Add(lblH);

            var lblV = new Label
            {
                Text = value,
                Font = AppTheme.FontBody,
                ForeColor = AppTheme.TextDark,
                Location = new Point(176, y),
                Size = new Size(280, value.Contains("\n") ? 58 : 18)
            };
            Controls.Add(lblV);

            y += lblV.Height + 8;
        }
    }
}