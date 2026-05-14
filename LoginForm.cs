using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClothingStore;

namespace ClothingStoreIS
{
    public partial class LoginForm : Form
    {
        private Panel pnlCard;
        private Label lblTitle, lblSub, lblUser, lblPass, lblError;
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnForgot;
        private CheckBox chkShow;

        // Logged-in user info (accessible after login)
        public static int CurrentUserID { get; private set; }
        public static string CurrentUsername { get; private set; } = "";
        public static string CurrentRole { get; private set; } = "";

        public LoginForm()
        {
            InitUI();
        }

        private void InitUI()
        {
            Text = "Clothing Store IS — Login";
            Size = new Size(480, 580);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppTheme.BgLight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = AppTheme.FontBody;

            var topStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 8,
                BackColor = AppTheme.PrimaryDark
            };
            Controls.Add(topStrip);

            pnlCard = new Panel
            {
                Size = new Size(360, 460),
                BackColor = AppTheme.BgCard,
                Location = new Point(60, 50)
            };
            pnlCard.Paint += PnlCard_Paint;
            Controls.Add(pnlCard);

            var lblIcon = new Label
            {
                Text = "👗",
                Font = new Font("Segoe UI Emoji", 24f),
                Size = new Size(70, 70),
                Location = new Point(145, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlCard.Controls.Add(lblIcon);

            lblTitle = new Label
            {
                Text = "Clothing Store",
                Font = AppTheme.FontTitle,
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(0, 100),
                Size = new Size(360, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblTitle);

            lblSub = new Label
            {
                Text = "Information System",
                Font = AppTheme.FontItalic,
                ForeColor = AppTheme.TextMid,
                Location = new Point(0, 133),
                Size = new Size(360, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblSub);

            var div = new Panel
            {
                Location = new Point(80, 162),
                Size = new Size(200, 2),
                BackColor = AppTheme.Border
            };
            pnlCard.Controls.Add(div);

            lblUser = MakeLabel("Username", 60, 178);
            pnlCard.Controls.Add(lblUser);

            txtUsername = MakeTextBox(60, 198, false);
            pnlCard.Controls.Add(txtUsername);

            lblPass = MakeLabel("Password", 60, 248);
            pnlCard.Controls.Add(lblPass);

            txtPassword = MakeTextBox(60, 268, true);
            pnlCard.Controls.Add(txtPassword);

            chkShow = new CheckBox
            {
                Text = "Show password",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Location = new Point(60, 302),
                AutoSize = true
            };
            chkShow.CheckedChanged += (s, e) =>
                txtPassword.UseSystemPasswordChar = !chkShow.Checked;
            pnlCard.Controls.Add(chkShow);

            lblError = new Label
            {
                Text = "",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.Danger,
                Location = new Point(60, 323),
                Size = new Size(240, 18),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlCard.Controls.Add(lblError);

            btnLogin = MakeButton("LOGIN", 60, 348, AppTheme.PrimaryDark);
            btnLogin.Click += BtnLogin_Click;
            pnlCard.Controls.Add(btnLogin);

            btnForgot = new Button
            {
                Text = "Forgot password?",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.Accent,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(60, 398),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            btnForgot.FlatAppearance.BorderSize = 0;
            btnForgot.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnForgot.Click += (s, e) => new ForgotPasswordForm().ShowDialog();
            pnlCard.Controls.Add(btnForgot);

            AcceptButton = btnLogin;
        }

        // ── Login logic ───────────────────────────────────────────────────────
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Please fill in all fields.";
                return;
            }

            string hashedPassword = SecurityHelper.HashPassword(txtPassword.Text);

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var cmd = new MySqlCommand(
                        "SELECT UserID, Username, Role, IsActive " +
                        "FROM users WHERE Username=@u AND PasswordHash=@p",
                        conn);
                    cmd.Parameters.AddWithValue("@u", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", hashedPassword);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isActive = reader.GetBoolean("IsActive");
                            if (!isActive)
                            {
                                lblError.Text = "This account is inactive. Contact admin.";
                                return;
                            }

                            CurrentUserID = reader.GetInt32("UserID");
                            CurrentUsername = reader.GetString("Username");
                            CurrentRole = reader.GetString("Role");

                            Hide();
                            new DashboardForm().ShowDialog();
                            Show();
                            txtUsername.Clear();
                            txtPassword.Clear();
                        }
                        else
                        {
                            lblError.Text = "Invalid username or password.";
                            txtPassword.Clear();
                            txtPassword.Focus();
                            ShakeForm();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "DB Error: " + ex.Message;
            }
        }

        // ── Shake animation ───────────────────────────────────────────────────
        private async void ShakeForm()
        {
            int orig = pnlCard.Left;
            int[] offsets = { -8, 8, -6, 6, -4, 4, 0 };
            foreach (int o in offsets)
            {
                pnlCard.Left = orig + o;
                await System.Threading.Tasks.Task.Delay(40);
            }
            pnlCard.Left = orig;
        }

        // ── Card border paint ─────────────────────────────────────────────────
        private void PnlCard_Paint(object sender, PaintEventArgs e)
        {
            if (pnlCard.Width < 2 || pnlCard.Height < 2) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1);

            using (var pen = new Pen(AppTheme.Border, 1.5f))
                g.DrawRoundedRect(pen, rect, 16);
        }

        // ── UI factory helpers ────────────────────────────────────────────────
        private static Label MakeLabel(string text, int x, int y) => new Label
        {
            Text = text,
            Font = AppTheme.FontBold,
            ForeColor = AppTheme.TextDark,
            Location = new Point(x, y),
            AutoSize = true
        };

        private static TextBox MakeTextBox(int x, int y, bool isPassword) => new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(240, 28),
            Font = AppTheme.FontBody,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(255, 248, 251),
            UseSystemPasswordChar = isPassword
        };

        private static Button MakeButton(string text, int x, int y, Color bg)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(x, y),
                Size = new Size(240, 38),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }

    // ── Shared Graphics extension helpers (used by all forms) ─────────────────
    internal static class GraphicsExtensions
    {
        public static void DrawRoundedRect(this Graphics g, Pen pen, Rectangle r, int radius)
        {
            var path = SafeRoundedRect(r, radius);
            if (path != null) g.DrawPath(pen, path);
        }

        public static void FillRoundedRect(this Graphics g, Brush brush, Rectangle r, int radius)
        {
            var path = SafeRoundedRect(r, radius);
            if (path != null) g.FillPath(brush, path);
        }

        /// <summary>
        /// Builds a rounded-rectangle GraphicsPath.
        /// Returns null (instead of throwing) when the rectangle is too small.
        /// </summary>
        private static GraphicsPath SafeRoundedRect(Rectangle r, int radius)
        {
            // Guard: rectangle must have positive area
            if (r.Width < 2 || r.Height < 2) return null;

            // Clamp radius so diameter never exceeds the shorter side
            radius = Math.Max(1, Math.Min(radius, Math.Min(r.Width, r.Height) / 2));
            int d = radius * 2;

            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}