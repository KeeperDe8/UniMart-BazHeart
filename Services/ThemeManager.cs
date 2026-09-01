namespace FinalProject.Services;

public static class ThemeManager
{
    public static bool IsDark { get; private set; }
    public static event EventHandler? ThemeChanged;

    public static void Initialize() => Apply(Preferences.Default.Get("is_dark_mode", false), false);

    public static void Apply(bool dark, bool persist = true)
    {
        IsDark = dark;
        if (persist) Preferences.Default.Set("is_dark_mode", dark);
        if (Application.Current is null) return;

        Application.Current.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
        var r = Application.Current.Resources;

        // Page & Card Surfaces
        Set(r, "PageBg", dark ? "#0B111E" : "#F7F9FC");
        Set(r, "PageBackground", dark ? "#0B111E" : "#F7F9FC");
        Set(r, "CardBg", dark ? "#131D31" : "#FFFFFF");
        Set(r, "CardBackground", dark ? "#131D31" : "#FFFFFF");
        Set(r, "SurfaceBackground", dark ? "#131D31" : "#FFFFFF");
        Set(r, "ElevatedCardBackground", dark ? "#1A2740" : "#FFFFFF");

        // Typography
        Set(r, "TextDark", dark ? "#F8FAFC" : "#172033");
        Set(r, "PrimaryText", dark ? "#F8FAFC" : "#172033");
        Set(r, "TextMuted", dark ? "#94A3B8" : "#8A9AAF");
        Set(r, "SecondaryText", dark ? "#94A3B8" : "#8A9AAF");
        Set(r, "MutedText", dark ? "#64748B" : "#98A2B3");

        // Borders & Inputs
        Set(r, "BorderLight", dark ? "#223352" : "#DFE6EF");
        Set(r, "BorderColor", dark ? "#223352" : "#DFE6EF");
        Set(r, "DividerColor", dark ? "#1E2D48" : "#EDF1F5");
        Set(r, "InputBackground", dark ? "#17243B" : "#F1F5FA");
        Set(r, "InputBorder", dark ? "#2B3F63" : "#DFE6EF");
        Set(r, "NavBackground", dark ? "#0F172A" : "#FFFFFF");

        // Brand & Accents
        Set(r, "PrimaryBlue", dark ? "#38BDF8" : "#2456D8");
        Set(r, "AccentBlue", dark ? "#60A5FA" : "#2F6BFF");
        Set(r, "DarkBlue", dark ? "#2563EB" : "#193F9D");
        Set(r, "AccentGold", dark ? "#FBBF24" : "#E59A22");
        Set(r, "SuccessGreen", dark ? "#34D399" : "#16A66A");
        Set(r, "LightGreen", dark ? "#064E3B" : "#CFF7DF");
        Set(r, "DangerRed", dark ? "#FB7185" : "#FF496A");

        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    static void Set(ResourceDictionary r, string k, string v) => r[k] = Color.FromArgb(v);
}
