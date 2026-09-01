namespace FinalProject.Controls;

public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab), typeof(string), typeof(BottomNavBar), "Home", propertyChanged: OnActiveTabChanged);

    public string ActiveTab { get => (string)GetValue(ActiveTabProperty); set => SetValue(ActiveTabProperty, value); }

    public BottomNavBar()
    {
        InitializeComponent();
        UpdateAppearance();
        Services.ThemeManager.ThemeChanged += (_, _) => UpdateAppearance();
        Services.AppState.Instance.SellerApprovalChanged += (_, _) => UpdateAppearance();
        Services.AppState.Instance.RoleChanged += (_, _) => UpdateAppearance();
    }

    static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue) => ((BottomNavBar)bindable).UpdateAppearance();

    void UpdateAppearance()
    {
        SetPill(HomePill, HomeIcon, HomeText, "Home");
        SetPill(ExplorePill, ExploreIcon, ExploreText, "Explore");
        SetPill(MessagesPill, MessagesIcon, MessagesText, "Messages");
        SetPill(ProfilePill, ProfileIcon, ProfileText, "Profile");
        MessageBadge.IsVisible = !string.Equals(ActiveTab, "Messages", StringComparison.OrdinalIgnoreCase);

        // Only show '+' create listing button if account is an approved seller!
        bool isSeller = Services.AppState.Instance.IsSeller;
        if (CreateButtonContainer is not null)
        {
            CreateButtonContainer.IsVisible = isSeller;
        }
    }

    void SetPill(Border pill, Microsoft.Maui.Controls.Shapes.Path icon, Label text, string tab)
    {
        bool isActive = string.Equals(ActiveTab, tab, StringComparison.OrdinalIgnoreCase);
        var blue = (Color)Application.Current!.Resources["PrimaryBlue"];
        var muted = (Color)Application.Current!.Resources["TextMuted"];

        pill.BackgroundColor = isActive ? blue : Colors.Transparent;
        pill.Padding = isActive ? new Thickness(12, 6) : new Thickness(8, 4);
        icon.Fill = isActive ? Colors.White : muted;
        text.TextColor = isActive ? Colors.White : muted;
        text.IsVisible = isActive;
    }

    async void Navigate(string route)
    {
        if (!string.Equals(ActiveTab, route, StringComparison.OrdinalIgnoreCase))
        {
            await Shell.Current.GoToAsync($"//{route.ToLowerInvariant()}");
        }
    }

    async void OnCreateListingTapped(object? s, TappedEventArgs e)
    {
        if (CreateButton is not null)
        {
            await CreateButton.ScaleToAsync(0.85, 80, Easing.CubicOut);
            await CreateButton.ScaleToAsync(1.1, 90, Easing.SpringOut);
            await CreateButton.ScaleToAsync(1.0, 70, Easing.CubicIn);
        }

        // Navigate to My Listings page
        await Shell.Current.GoToAsync("//listings");
    }

    async void OnHomeTapped(object? s, TappedEventArgs e)
    {
        if (HomePill != null) { await HomePill.ScaleToAsync(0.9, 50, Easing.CubicOut); await HomePill.ScaleToAsync(1.0, 60, Easing.CubicIn); }
        Navigate("Home");
    }

    async void OnExploreTapped(object? s, TappedEventArgs e)
    {
        if (ExplorePill != null) { await ExplorePill.ScaleToAsync(0.9, 50, Easing.CubicOut); await ExplorePill.ScaleToAsync(1.0, 60, Easing.CubicIn); }
        Navigate("Explore");
    }

    async void OnMessagesTapped(object? s, TappedEventArgs e)
    {
        if (MessagesPill != null) { await MessagesPill.ScaleToAsync(0.9, 50, Easing.CubicOut); await MessagesPill.ScaleToAsync(1.0, 60, Easing.CubicIn); }
        Navigate("Messages");
    }

    async void OnProfileTapped(object? s, TappedEventArgs e)
    {
        if (ProfilePill != null) { await ProfilePill.ScaleToAsync(0.9, 50, Easing.CubicOut); await ProfilePill.ScaleToAsync(1.0, 60, Easing.CubicIn); }
        Navigate("Profile");
    }
}
