using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClothingStore;

namespace ClothingStoreIS
{
    public class ForgotPasswordForm : Form
    {
        private Label lblTitle, lblSub, lblEmail, lblNewPass, lblConfirm, lblStatus;
        private TextBox txtEmail, txtNewPassword, txtConfirm;
        private Button btnReset, btnBack;
        private Panel pnlCard;

        public ForgotPasswordForm()
        {
            InitUI();
        }

        private void InitUI()
        {
            Text = "Password Recovery";
            Size = new Size(440, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.BgLight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = AppTheme.FontBody;

            var topStrip = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = AppTheme.PrimaryDark };
            Controls.Add(topStrip);

            pnlCard = new Panel { Size = new Size(360, 420), BackColor = AppTheme.BgCard, Location = new Point(40, 45) };
            pnlCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawRoundedRect(new Pen(AppTheme.Border, 1.5f),
                    new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1), 16);
            };
            Controls.Add(pnlCard);

            var lblIcon = new Label
            {
                Text = "🔒",
                Font = new Font("Segoe UI Emoji", 28f),
                Location = new Point(0, 22),
                Size = new Size(360, 48),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblIcon);

            lblTitle = new Label
            {
                Text = "Reset Password",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location = new Point(0, 76),
                Size = new Size(360, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblTitle);

            lblSub = new Label
            {
                Text = "Enter your email and a new password.",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Location = new Point(30, 110),
                Size = new Size(300, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlCard.Controls.Add(lblSub);

            var div = new Panel { Location = new Point(80, 132), Size = new Size(200, 2), BackColor = AppTheme.Border };
            pnlCard.Controls.Add(div);

            // Email
            AddFieldLabel("Email Address", 148, pnlCard);
            txtEmail = AddTextBox(168, false, pnlCard);

            // New Password
            AddFieldLabel("New Password", 200, pnlCard);
            txtNewPassword = AddTextBox(220, true, pnlCard);

            // Confirm Password
            AddFieldLabel("Confirm Password", 252, pnlCard);
            txtConfirm = AddTextBox(272, true, pnlCard);

            lblStatus = new Label
            {
                Text = "",
                Font = AppTheme.FontSmall,
                Location = new Point(60, 308),
                Size = new Size(240, 18)
            };
            pnlCard.Controls.Add(lblStatus);

            btnReset = new Button
            {
                Text = "RESET PASSWORD",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppTheme.PrimaryDark,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(60, 330),
                Size = new Size(240, 38),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;
            pnlCard.Controls.Add(btnReset);

            btnBack = new Button
            {
                Text = "← Back to Login",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.Accent,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(60, 380),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnBack.Click += (s, e) => Close();
            pnlCard.Controls.Add(btnBack);

            AcceptButton = btnReset;
        }

        private static void AddFieldLabel(string text, int y, Panel parent)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Font = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Location = new Point(60, y),
                AutoSize = true
            });
        }

        private static TextBox AddTextBox(int y, bool isPassword, Panel parent)
        {
            var tb = new TextBox
            {
                Location = new Point(60, y),
                Size = new Size(240, 28),
                Font = AppTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 248, 251),
                UseSystemPasswordChar = isPassword
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            lblStatus.ForeColor = AppTheme.Danger;
            lblStatus.Text = "";

            if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtNewPassword.Text) ||
                string.IsNullOrWhiteSpace(txtConfirm.Text))
            {
                lblStatus.Text = "Please fill in all fields.";
                return;
            }

            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                lblStatus.Text = "Please enter a valid email address.";
                return;
            }

            if (txtNewPassword.Text != txtConfirm.Text)
            {
                lblStatus.Text = "Passwords do not match.";
                return;
            }

            if (txtNewPassword.Text.Length < 6)
            {
                lblStatus.Text = "Password must be at least 6 characters.";
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    // Check email exists
                    var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE Email=@e", conn);
                    checkCmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        lblStatus.Text = "No account found with that email.";
                        return;
                    }

                    // Update password
                    string newHash = SecurityHelper.HashPassword(txtNewPassword.Text);
                    var updateCmd = new MySqlCommand(
                        "UPDATE users SET PasswordHash=@p, UpdatedAt=NOW() WHERE Email=@e", conn);
                    updateCmd.Parameters.AddWithValue("@p", newHash);
                    updateCmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                    updateCmd.ExecuteNonQuery();

                    lblStatus.ForeColor = AppTheme.Success;
                    lblStatus.Text = "✓ Password reset successfully!";
                    btnReset.Enabled = false;

                    MessageBox.Show("Your password has been reset successfully!\nYou can now log in with your new password.",
                        "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "DB Error: " + ex.Message;
            }
        }
    }
}
