using FinalProject.Services;
using FinalProject.Services.Api;

namespace FinalProject;

public partial class LandingPage : ContentPage
{
    private bool sellerRole;

    public LandingPage()
    {
        InitializeComponent();
        MeetupArea.ItemsSource = new List<string>
        {
            "Main Building – Ground Floor Lobby",
            "Main Building Lobby",
            "Student Activity Center (SAC)",
            "Cafeteria",
            "Library Entrance"
        };
        MeetupArea.SelectedIndex = 0;

        if (AppState.Instance.ShowLogoutMessage)
        {
            AppState.Instance.ShowLogoutMessage = false;
            LogoutBanner.IsVisible = true;
            _ = HideLogoutBannerAsync();
        }
    }

    private async Task HideLogoutBannerAsync()
    {
        await Task.Delay(2500);
        LogoutBanner.IsVisible = false;
    }

    private void OnLoginSegmentTapped(object? s, TappedEventArgs e) => ShowLogin();
    private void OnRegisterSegmentTapped(object? s, TappedEventArgs e) => ShowRegister();

    private void ShowLogin()
    {
        LoginPanel.IsVisible = true;
        RegisterPanel.IsVisible = false;
    }

    private void ShowRegister()
    {
        LoginPanel.IsVisible = false;
        RegisterPanel.IsVisible = true;
    }

    private void OnBuyerRoleTapped(object? s, TappedEventArgs e) => SetRole(false);
    private void OnSellerRoleTapped(object? s, TappedEventArgs e) => SetRole(true);

    private void SetRole(bool seller)
    {
        sellerRole = seller;
        SellerFields.IsVisible = seller;
        BuyerSegment.BackgroundColor = seller ? Colors.Transparent : (Color)Application.Current!.Resources["CardBg"];
        SellerSegment.BackgroundColor = seller ? (Color)Application.Current!.Resources["CardBg"]: Colors.Transparent;
    }

    // --- SHOW / HIDE PASSWORD TOGGLES ---
    private void OnToggleLoginPassword(object? s, TappedEventArgs e)
    {
        LoginPassword.IsPassword = !LoginPassword.IsPassword;
        LoginEyeBtn.Text = LoginPassword.IsPassword ? "👁" : "🙈";
    }

    private void OnToggleRegisterPassword(object? s, TappedEventArgs e)
    {
        RegisterPassword.IsPassword = !RegisterPassword.IsPassword;
        RegisterEyeBtn.Text = RegisterPassword.IsPassword ? "👁" : "🙈";
    }

    private void OnToggleConfirmPassword(object? s, TappedEventArgs e)
    {
        ConfirmPassword.IsPassword = !ConfirmPassword.IsPassword;
        ConfirmEyeBtn.Text = ConfirmPassword.IsPassword ? "👁" : "🙈";
    }

    private async void OnForgotTapped(object? s, TappedEventArgs e)
    {
        await DisplayAlertAsync("Forgot Password", "Enter your email to receive an OTP verification code for password reset.", "OK");
    }

    private async void OnLoginClicked(object? s, EventArgs e)
    {
        var email = LoginEmail.Text?.Trim();
        var password = LoginPassword.Text?.Trim();

        if (!ValidEmail(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Check login", "Please enter your email and password.", "OK");
            return;
        }

        LoginButton.IsEnabled = false;
        try
        {
            var res = await CampusApiService.Instance.LoginAsync(email, password);
            if (res?.User != null && !string.IsNullOrWhiteSpace(res.Token))
            {
                AppState.Instance.SaveSession(
                    res.Token,
                    res.User.Name,
                    res.User.Email,
                    res.User.Role == "seller" ? "StudentSeller" : "Buyer",
                    res.User.Id,
                    res.User.SellerShopName,
                    res.User.SellerBio,
                    res.User.PreferredMeetupArea
                );
                EnterApp();
            }
            else
            {
                await DisplayAlertAsync("Login Failed", "Invalid email or password. Please verify your credentials.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Connection Error", $"Could not connect to backend: {ex.Message}", "OK");
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async void OnCreateClicked(object? s, EventArgs e)
    {
        var name = FullName.Text?.Trim();
        var email = RegisterEmail.Text?.Trim();
        var studentNo = StudentNumber.Text?.Trim();
        var pass = RegisterPassword.Text?.Trim();
        var confirm = ConfirmPassword.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name) || !ValidEmail(email))
        {
            await DisplayAlertAsync("Check details", "Please complete your name and valid email.", "OK");
            return;
        }

        if (sellerRole && (string.IsNullOrWhiteSpace(SellerName.Text) || string.IsNullOrWhiteSpace(SellerBio.Text) || MeetupArea.SelectedItem is null))
        {
            await DisplayAlertAsync("Seller details needed", "Please enter your seller display name, bio, and meetup area.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(pass) || pass.Length < 6)
        {
            await DisplayAlertAsync("Password", "Password must be at least 6 characters.", "OK");
            return;
        }

        if (pass != confirm)
        {
            await DisplayAlertAsync("Passwords do not match", "Re-enter the same password to continue.", "OK");
            return;
        }

        CreateButton.IsEnabled = false;
        try
        {
            var res = await CampusApiService.Instance.RegisterAsync(
                name,
                email,
                pass,
                sellerRole ? "seller" : "buyer",
                studentNo,
                SellerName.Text?.Trim(),
                SellerBio.Text?.Trim(),
                MeetupArea.SelectedItem?.ToString()
            );

            if (res?.User != null && !string.IsNullOrWhiteSpace(res.Token))
            {
                AppState.Instance.SaveSession(
                    res.Token,
                    name!,
                    email!,
                    sellerRole ? "StudentSeller" : "Buyer",
                    res.User.Id,
                    SellerName.Text,
                    SellerBio.Text,
                    MeetupArea.SelectedItem?.ToString()
                );
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("Account created", "Welcome to UniMart!", "Continue");
                });
                EnterApp();
            }
            else
            {
                await DisplayAlertAsync("Registration Failed", "Unable to create account. Email may already be registered.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Connection Error", $"Could not connect to backend: {ex.Message}", "OK");
        }
        finally
        {
            CreateButton.IsEnabled = true;
        }
    }

    private static bool ValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');

    private static void SetIdentity(string name, string email, string role)
    {
        var state = AppState.Instance;
        state.CurrentUserName = name;
        state.CurrentEmail = email;
        state.CurrentRole = role;
    }

    private void EnterApp()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            window.Page = new AppShell();
        }
    }
}
