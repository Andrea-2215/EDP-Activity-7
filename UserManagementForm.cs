using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClothingStore;

namespace ClothingStoreIS
{
    public partial class UserManagementForm : Form
    {
        // ── Controls ──────────────────────────────────────────────────────────
        private DataGridView dgv;
        private TextBox txtSearch, txtUsername, txtEmail, txtPassword, txtFullName;
        private ComboBox cmbRole, cmbStatus;
        private Button btnAdd, btnUpdate, btnToggleStatus, btnClear, btnClose, btnRefresh;
        private Label lblStatus, lblCount;
        private Panel pnlForm;
        private DataTable _allData;
        private int _selectedUserID = -1;

        public UserManagementForm()
        {
            InitUI();
            LoadUsers();
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI Construction
        // ─────────────────────────────────────────────────────────────────────
        private void InitUI()
        {
            Text            = "User Management";
            Size            = new Size(1100, 660);
            MinimumSize     = new Size(1100, 660);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = AppTheme.BgLight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            Font            = AppTheme.FontBody;

            // ── Header bar ───────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            Controls.Add(header);

            header.Controls.Add(new Label
            {
                Text      = "👤  User Management",
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = AppTheme.PrimaryDark,
                Location  = new Point(20, 12),
                AutoSize  = true
            });

            // ── Left panel: form ──────────────────────────────────────────────
            pnlForm = new Panel
            {
                Size      = new Size(310, 600),
                Location  = new Point(0, 56),
                BackColor = Color.White
            };
            pnlForm.Paint += Panel_Paint;
            Controls.Add(pnlForm);

            int fy = 20;
            AddFormLabel("Full Name",  fy,      pnlForm); txtFullName = AddFormBox(fy + 20,  false, pnlForm); fy += 60;
            AddFormLabel("Username",   fy,      pnlForm); txtUsername = AddFormBox(fy + 20,  false, pnlForm); fy += 60;
            AddFormLabel("Email",      fy,      pnlForm); txtEmail    = AddFormBox(fy + 20,  false, pnlForm); fy += 60;
            AddFormLabel("Password",   fy,      pnlForm); txtPassword = AddFormBox(fy + 20,  true,  pnlForm); fy += 60;

            AddFormLabel("Role", fy, pnlForm);
            cmbRole = new ComboBox
            {
                Location = new Point(20, fy + 20), Size = new Size(270, 26),
                Font = AppTheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbRole.Items.AddRange(new object[] { "Administrator", "Staff", "Cashier" });
            cmbRole.SelectedIndex = 1;
            pnlForm.Controls.Add(cmbRole);
            fy += 60;

            AddFormLabel("Status", fy, pnlForm);
            cmbStatus = new ComboBox
            {
                Location = new Point(20, fy + 20), Size = new Size(270, 26),
                Font = AppTheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;
            pnlForm.Controls.Add(cmbStatus);
            fy += 70;

            lblStatus = new Label
            {
                Location  = new Point(20, fy),
                Size      = new Size(270, 18),
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.Danger
            };
            pnlForm.Controls.Add(lblStatus);
            fy += 24;

            // Action buttons
            btnAdd = MakeBtn("➕ Add User", AppTheme.PrimaryDark, 20, fy, 130);
            btnAdd.Click += BtnAdd_Click;
            pnlForm.Controls.Add(btnAdd);

            btnUpdate = MakeBtn("💾 Update", AppTheme.Accent, 160, fy, 130);
            btnUpdate.Enabled = false;
            btnUpdate.Click += BtnUpdate_Click;
            pnlForm.Controls.Add(btnUpdate);
            fy += 46;

            btnToggleStatus = MakeBtn("🔄 Toggle Status", Color.FromArgb(80, 150, 200), 20, fy, 130);
            btnToggleStatus.Enabled = false;
            btnToggleStatus.Click += BtnToggle_Click;
            pnlForm.Controls.Add(btnToggleStatus);

            btnClear = MakeBtn("✖ Clear", Color.FromArgb(160, 160, 165), 160, fy, 130);
            btnClear.Click += (s, e) => ClearForm();
            pnlForm.Controls.Add(btnClear);

            // ── Right panel: grid ─────────────────────────────────────────────
            var pnlRight = new Panel
            {
                Location  = new Point(310, 56),
                Size      = new Size(790, 604),
                BackColor = AppTheme.BgLight
            };
            Controls.Add(pnlRight);

            // Toolbar row
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = AppTheme.BgCard };
            pnlRight.Controls.Add(toolbar);

            toolbar.Controls.Add(new Label
            {
                Text      = "Search:",
                Font      = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Location  = new Point(12, 14),
                AutoSize  = true
            });

            txtSearch = new TextBox
            {
                Location      = new Point(68, 11),
                Size          = new Size(220, 26),
                Font          = AppTheme.FontBody,
                BorderStyle   = BorderStyle.FixedSingle,
                BackColor     = Color.FromArgb(255, 248, 251)
            };
            txtSearch.TextChanged += TxtSearch_Changed;
            toolbar.Controls.Add(txtSearch);

            btnRefresh = new Button
            {
                Text        = "↻ Refresh",
                Font        = AppTheme.FontSmall,
                ForeColor   = AppTheme.PrimaryDark,
                BackColor   = Color.White,
                FlatStyle   = FlatStyle.Flat,
                Location    = new Point(300, 10),
                Size        = new Size(84, 27),
                Cursor      = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderColor = AppTheme.Border;
            btnRefresh.Click += (s, e) => { LoadUsers(); ClearForm(); };
            toolbar.Controls.Add(btnRefresh);

            lblCount = new Label
            {
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMid,
                Location  = new Point(396, 16),
                AutoSize  = true
            };
            toolbar.Controls.Add(lblCount);

            btnClose = MakeBtn("Close", AppTheme.PrimaryDark, 694, 10, 84);
            btnClose.Click += (s, e) => Close();
            toolbar.Controls.Add(btnClose);

            // Grid
            dgv = new DataGridView
            {
                Dock                      = DockStyle.Fill,
                BackgroundColor           = Color.White,
                BorderStyle               = BorderStyle.None,
                CellBorderStyle           = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle  = DataGridViewHeaderBorderStyle.Single,
                GridColor                 = AppTheme.Border,
                ReadOnly                  = true,
                AllowUserToAddRows        = false,
                AllowUserToDeleteRows     = false,
                SelectionMode             = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible         = false,
                Font                      = AppTheme.FontBody
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextDark;
            dgv.ColumnHeadersDefaultCellStyle.Font      = AppTheme.FontBold;
            dgv.ColumnHeadersHeight                     = 34;
            dgv.DefaultCellStyle.SelectionBackColor     = AppTheme.Primary;
            dgv.DefaultCellStyle.SelectionForeColor     = AppTheme.TextDark;
            dgv.RowTemplate.Height                      = 28;
            dgv.CellClick                              += Dgv_CellClick;
            dgv.RowPrePaint                            += Dgv_RowPrePaint;
            pnlRight.Controls.Add(dgv);

            pnlRight.Controls.SetChildIndex(dgv, 0);
            pnlRight.Controls.SetChildIndex(toolbar, 1);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Data Operations
        // ─────────────────────────────────────────────────────────────────────
        private void LoadUsers()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var da = new MySqlDataAdapter(
                        @"SELECT UserID, FullName, Username, Email, Role,
                                 CASE WHEN IsActive=1 THEN 'Active' ELSE 'Inactive' END AS Status,
                                 CreatedAt
                          FROM users ORDER BY UserID", conn);
                    _allData = new DataTable();
                    da.Fill(_allData);
                    dgv.DataSource = _allData;
                    lblCount.Text  = _allData.Rows.Count + " user(s)";
                    txtSearch.Clear();
                    StyleGrid();
                }
            }
            catch (Exception ex)
            {
                Msg("DB Error:\n" + ex.Message, true);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateForm(requirePassword: true)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    // Check duplicate username
                    var chk = new MySqlCommand("SELECT COUNT(*) FROM users WHERE Username=@u", conn);
                    chk.Parameters.AddWithValue("@u", txtUsername.Text.Trim());
                    if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                    { Msg("Username already exists.", true); return; }

                    var cmd = new MySqlCommand(
                        @"INSERT INTO users (FullName, Username, Email, PasswordHash, Role, IsActive, CreatedAt)
                          VALUES (@fn, @u, @e, @p, @r, @a, NOW())", conn);
                    cmd.Parameters.AddWithValue("@fn", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@u",  txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@e",  txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@p",  SecurityHelper.HashPassword(txtPassword.Text));
                    cmd.Parameters.AddWithValue("@r",  cmbRole.Text);
                    cmd.Parameters.AddWithValue("@a",  cmbStatus.SelectedIndex == 0 ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }

                Msg("✓ User added successfully!", false);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex) { Msg("DB Error: " + ex.Message, true); }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedUserID < 0) { Msg("Select a user first.", true); return; }
            if (!ValidateForm(requirePassword: false)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    // Check duplicate username (excluding current user)
                    var chk = new MySqlCommand(
                        "SELECT COUNT(*) FROM users WHERE Username=@u AND UserID<>@id", conn);
                    chk.Parameters.AddWithValue("@u",  txtUsername.Text.Trim());
                    chk.Parameters.AddWithValue("@id", _selectedUserID);
                    if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                    { Msg("Username already taken.", true); return; }

                    string sql;
                    MySqlCommand cmd;

                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        // Update with new password
                        sql = @"UPDATE users SET FullName=@fn, Username=@u, Email=@e,
                                    PasswordHash=@p, Role=@r, IsActive=@a, UpdatedAt=NOW()
                                WHERE UserID=@id";
                        cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@p",  SecurityHelper.HashPassword(txtPassword.Text));
                    }
                    else
                    {
                        // Update without changing password
                        sql = @"UPDATE users SET FullName=@fn, Username=@u, Email=@e,
                                    Role=@r, IsActive=@a, UpdatedAt=NOW()
                                WHERE UserID=@id";
                        cmd = new MySqlCommand(sql, conn);
                    }

                    cmd.Parameters.AddWithValue("@fn", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@u",  txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@e",  txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@r",  cmbRole.Text);
                    cmd.Parameters.AddWithValue("@a",  cmbStatus.SelectedIndex == 0 ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", _selectedUserID);
                    cmd.ExecuteNonQuery();
                }

                Msg("✓ User updated successfully!", false);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex) { Msg("DB Error: " + ex.Message, true); }
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            if (_selectedUserID < 0) { Msg("Select a user first.", true); return; }

            string action = cmbStatus.SelectedIndex == 0 ? "deactivate" : "activate";
            if (MessageBox.Show($"Are you sure you want to {action} this account?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    var cmd = new MySqlCommand(
                        "UPDATE users SET IsActive=@a, UpdatedAt=NOW() WHERE UserID=@id", conn);
                    cmd.Parameters.AddWithValue("@a",  cmbStatus.SelectedIndex == 0 ? 0 : 1);
                    cmd.Parameters.AddWithValue("@id", _selectedUserID);
                    cmd.ExecuteNonQuery();
                }
                Msg("✓ Account status updated!", false);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex) { Msg("DB Error: " + ex.Message, true); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Grid Events
        // ─────────────────────────────────────────────────────────────────────
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];

            _selectedUserID       = Convert.ToInt32(row.Cells["UserID"].Value);
            txtFullName.Text      = row.Cells["FullName"].Value?.ToString() ?? "";
            txtUsername.Text      = row.Cells["Username"].Value?.ToString() ?? "";
            txtEmail.Text         = row.Cells["Email"].Value?.ToString() ?? "";
            txtPassword.Text      = "";          // never pre-fill password
            cmbRole.Text          = row.Cells["Role"].Value?.ToString() ?? "Staff";
            cmbStatus.SelectedIndex = (row.Cells["Status"].Value?.ToString() == "Active") ? 0 : 1;

            btnUpdate.Enabled        = true;
            btnToggleStatus.Enabled  = true;
            btnToggleStatus.Text     = cmbStatus.SelectedIndex == 0
                                        ? "🔴 Deactivate"
                                        : "🟢 Activate";
            lblStatus.Text = "";
        }

        private void Dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                e.RowIndex % 2 == 0 ? Color.White : AppTheme.RowAlt;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Search
        // ─────────────────────────────────────────────────────────────────────
        private void TxtSearch_Changed(object sender, EventArgs e)
        {
            if (_allData == null) return;
            string q = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(q))
            {
                dgv.DataSource = _allData;
                lblCount.Text  = _allData.Rows.Count + " user(s)";
                return;
            }

            var view = new DataView(_allData);
            view.RowFilter =
                $"CONVERT(FullName, System.String) LIKE '%{q}%' OR " +
                $"CONVERT(Username, System.String) LIKE '%{q}%' OR " +
                $"CONVERT(Email, System.String)    LIKE '%{q}%' OR " +
                $"CONVERT(Role, System.String)     LIKE '%{q}%' OR " +
                $"CONVERT(Status, System.String)   LIKE '%{q}%'";

            dgv.DataSource = view;
            lblCount.Text  = view.Count + " of " + _allData.Rows.Count + " user(s)";
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────
        private bool ValidateForm(bool requirePassword)
        {
            lblStatus.Text = "";
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { Msg("Full Name is required.", true); return false; }
            if (string.IsNullOrWhiteSpace(txtUsername.Text)) { Msg("Username is required.", true);  return false; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))    { Msg("Email is required.", true);     return false; }
            if (!txtEmail.Text.Contains("@"))                { Msg("Invalid email address.", true); return false; }
            if (requirePassword && string.IsNullOrWhiteSpace(txtPassword.Text))
            { Msg("Password is required.", true); return false; }
            if (!string.IsNullOrWhiteSpace(txtPassword.Text) && txtPassword.Text.Length < 6)
            { Msg("Password must be at least 6 characters.", true); return false; }
            return true;
        }

        private void Msg(string text, bool isError)
        {
            lblStatus.ForeColor = isError ? AppTheme.Danger : AppTheme.Success;
            lblStatus.Text      = text;
        }

        private void ClearForm()
        {
            _selectedUserID         = -1;
            txtFullName.Text        = "";
            txtUsername.Text        = "";
            txtEmail.Text           = "";
            txtPassword.Text        = "";
            cmbRole.SelectedIndex   = 1;
            cmbStatus.SelectedIndex = 0;
            lblStatus.Text          = "";
            btnUpdate.Enabled       = false;
            btnToggleStatus.Enabled = false;
            btnToggleStatus.Text    = "🔄 Toggle Status";
        }

        private void StyleGrid()
        {
            if (dgv.Columns.Contains("UserID"))
                dgv.Columns["UserID"].Visible = false;       // hide PK column
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, pnlForm.Width - 1, pnlForm.Height - 1);
            using (var pen = new Pen(AppTheme.Border, 1f))
                g.DrawRoundedRect(pen, r, 0);
        }

        // ── UI factory helpers ────────────────────────────────────────────────
        private static void AddFormLabel(string text, int y, Panel p)
        {
            p.Controls.Add(new Label
            {
                Text      = text,
                Font      = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Location  = new Point(20, y),
                AutoSize  = true
            });
        }

        private static TextBox AddFormBox(int y, bool isPassword, Panel p)
        {
            var tb = new TextBox
            {
                Location              = new Point(20, y),
                Size                  = new Size(270, 26),
                Font                  = AppTheme.FontBody,
                BorderStyle           = BorderStyle.FixedSingle,
                BackColor             = Color.FromArgb(255, 248, 251),
                UseSystemPasswordChar = isPassword
            };
            p.Controls.Add(tb);
            return tb;
        }

        private static Button MakeBtn(string text, Color bg, int x, int y, int w)
        {
            var b = new Button
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(x, y),
                Size      = new Size(w, 34),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
