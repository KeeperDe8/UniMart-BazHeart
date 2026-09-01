namespace FinalProject;

public partial class ProfilePage : ContentPage
{
    bool isSeller;

    public ProfilePage()
    {
        InitializeComponent();
        isSeller = Services.AppState.Instance.IsSellerApproved;
        UpdateModeUI();

        DarkModeSwitch.IsToggled = Services.ThemeManager.IsDark;
        Services.ThemeManager.ThemeChanged += OnThemeChanged;
        Unloaded += (_, _) => Services.ThemeManager.ThemeChanged -= OnThemeChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshProfileData();
    }

    async void RefreshProfileData()
    {
        var name = string.IsNullOrWhiteSpace(Services.AppState.Instance.CurrentUserName) ? "Maria Santos" : Services.AppState.Instance.CurrentUserName;
        UserNameLabel.Text = name;
        UserHandleLabel.Text = "@" + name.ToLower().Replace(" ", "");

        try
        {
            var listings = await Services.Api.CampusApiService.Instance.GetSellerListingsAsync();
            ProfileListingsCountLabel.Text = (listings?.Count ?? Services.AppState.Instance.Listings.Count).ToString();
        }
        catch
        {
            ProfileListingsCountLabel.Text = Services.AppState.Instance.Listings.Count.ToString();
        }

        ProfileSavedCountLabel.Text = Services.AppState.Instance.SavedItems.Count.ToString();
        isSeller = Services.AppState.Instance.IsSellerApproved;
        UpdateModeUI();
    }

    void OnThemeChanged(object? sender, EventArgs e) => Dispatcher.Dispatch(() => DarkModeSwitch.IsToggled = Services.ThemeManager.IsDark);

    private void OnModeClicked(object? sender, EventArgs e)
    {
        isSeller = sender == SellerButton;
        UpdateModeUI();
    }

    void UpdateModeUI()
    {
        SellerButton.BackgroundColor = isSeller ? (Color)Application.Current!.Resources["DarkBlue"] : (Color)Application.Current!.Resources["InputBackground"];
        SellerButton.TextColor = isSeller ? Colors.White : (Color)Application.Current!.Resources["TextMuted"];
        BuyerButton.BackgroundColor = isSeller ? (Color)Application.Current!.Resources["InputBackground"] : (Color)Application.Current!.Resources["DarkBlue"];
        BuyerButton.TextColor = isSeller ? (Color)Application.Current!.Resources["TextMuted"] : Colors.White;

        SellerHubSection.IsVisible = isSeller;
        SellerOnboardingCard.IsVisible = !isSeller;
        RoleBadgeText.Text = isSeller ? "STUDENT SELLER" : "CAMPUS BUYER";
    }

    private async void OnApplySellerClicked(object? sender, EventArgs e)
    {
        try
        {
            await Services.Api.CampusApiService.Instance.ApplySellerAsync();
        }
        catch { }

        Services.AppState.Instance.IsSellerApproved = true;
        isSeller = true;
        UpdateModeUI();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlert(
                    "Verification Approved",
                    "Your Student Seller account is approved! The '+' Create Listing button is now visible on your bottom bar.",
                    "Get Started"
                );
            }
        });
    }

    private async void OnListingsClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//listings");
    private async void OnScheduleClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("selling-schedule");
    private async void OnShopClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("public-shop");
    private async void OnSavedClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("saved-items");
    private async void OnNotificationsClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("notifications");

    private void OnDarkModeToggled(object? sender, ToggledEventArgs e)
    {
        if (Services.ThemeManager.IsDark == e.Value) return;
        Services.ThemeManager.Apply(e.Value);
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        try { await Services.Api.CampusApiService.Instance.LogoutAsync(); } catch { }
        Services.AppState.Instance.ClearSession();
        Services.AppState.Instance.ShowLogoutMessage = true;
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null) window.Page = new LandingPage();
    }
}
