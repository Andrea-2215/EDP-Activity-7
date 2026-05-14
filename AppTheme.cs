using System.Drawing;

namespace ClothingStoreIS
{

    public static class AppTheme
    {
        // ── Colours ───────────────────────────────────────────────────────────
        public static readonly Color BgLight = Color.FromArgb(255, 245, 248);  // near-white warm
        public static readonly Color BgCard = Color.White;
        public static readonly Color Primary = Color.FromArgb(255, 182, 193);  // light pink
        public static readonly Color PrimaryDark = Color.FromArgb(219, 112, 147);  // pale violet-red
        public static readonly Color Accent = Color.FromArgb(255, 105, 135);  // deep rose accent
        public static readonly Color TextDark = Color.FromArgb(80, 40, 55);   // warm dark wine
        public static readonly Color TextMid = Color.FromArgb(160, 100, 120);
        public static readonly Color TextLight = Color.FromArgb(200, 160, 175);
        public static readonly Color Border = Color.FromArgb(255, 210, 220);
        public static readonly Color Success = Color.FromArgb(100, 190, 150);
        public static readonly Color Danger = Color.FromArgb(230, 80, 100);
        public static readonly Color RowAlt = Color.FromArgb(255, 240, 245);

        // ── Fonts ─────────────────────────────────────────────────────────────
        public static readonly Font FontTitle = new Font("Segoe UI", 20f, FontStyle.Bold);
        public static readonly Font FontSub = new Font("Segoe UI", 11f, FontStyle.Regular);
        public static readonly Font FontBold = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 9f, FontStyle.Regular);
        public static readonly Font FontSmall = new Font("Segoe UI", 8f, FontStyle.Regular);
        public static readonly Font FontItalic = new Font("Segoe UI", 9f, FontStyle.Italic);
    }
}