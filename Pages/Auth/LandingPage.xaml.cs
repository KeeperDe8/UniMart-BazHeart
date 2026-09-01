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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (HeroSection != null)
        {
            HeroSection.Opacity = 0;
            HeroSection.Scale = 0.88;
            HeroSection.TranslationY = -12;
            await Task.WhenAll(
                HeroSection.FadeToAsync(1, 350, Easing.CubicOut),
                HeroSection.ScaleToAsync(1, 450, Easing.SpringOut),
                HeroSection.TranslateToAsync(0, 0, 350, Easing.CubicOut)
            );
        }
    }

    private async Task HideLogoutBannerAsync()
    {
        await Task.Delay(2500);
        LogoutBanner.IsVisible = false;
    }

    private void OnLoginSegmentTapped(object? s, TappedEventArgs e) => ShowLogin();
    private void OnRegisterSegmentTapped(object? s, TappedEventArgs e) => ShowRegister();

    private async void ShowLogin()
    {
        if (LoginPanel.IsVisible && !OtpPanel.IsVisible) return;
        var blue = (Color)Application.Current!.Resources["PrimaryBlue"];
        var muted = (Color)Application.Current!.Resources["TextMuted"];
        var card = (Color)Application.Current!.Resources["CardBg"];

        LoginTabPill.BackgroundColor = card;
        RegisterTabPill.BackgroundColor = Colors.Transparent;
        LoginTabText.TextColor = blue;
        RegisterTabText.TextColor = muted;

        RegisterPanel.IsVisible = false;
        OtpPanel.IsVisible = false;

        LoginPanel.Opacity = 0;
        LoginPanel.TranslationY = 12;
        LoginPanel.IsVisible = true;

        await Task.WhenAll(
            LoginPanel.FadeToAsync(1, 200, Easing.CubicOut),
            LoginPanel.TranslateToAsync(0, 0, 200, Easing.CubicOut)
        );
    }

    private async void ShowRegister()
    {
        if (RegisterPanel.IsVisible && !OtpPanel.IsVisible) return;
        var blue = (Color)Application.Current!.Resources["PrimaryBlue"];
        var muted = (Color)Application.Current!.Resources["TextMuted"];
        var card = (Color)Application.Current!.Resources["CardBg"];

        RegisterTabPill.BackgroundColor = card;
        LoginTabPill.BackgroundColor = Colors.Transparent;
        RegisterTabText.TextColor = blue;
        LoginTabText.TextColor = muted;

        LoginPanel.IsVisible = false;
        OtpPanel.IsVisible = false;

        RegisterPanel.Opacity = 0;
        RegisterPanel.TranslationY = 12;
        RegisterPanel.IsVisible = true;

        await Task.WhenAll(
            RegisterPanel.FadeToAsync(1, 200, Easing.CubicOut),
            RegisterPanel.TranslateToAsync(0, 0, 200, Easing.CubicOut)
        );
    }

    private void OnBuyerRoleTapped(object? s, TappedEventArgs e) => SetRole(false);
    private void OnSellerRoleTapped(object? s, TappedEventArgs e) => SetRole(true);

    private async void SetRole(bool seller)
    {
        sellerRole = seller;
        var blue = (Color)Application.Current!.Resources["PrimaryBlue"];
        var muted = (Color)Application.Current!.Resources["TextMuted"];
        var card = (Color)Application.Current!.Resources["CardBg"];

        BuyerSegment.BackgroundColor = seller ? Colors.Transparent : card;
        SellerSegment.BackgroundColor = seller ? card : Colors.Transparent;
        BuyerRoleText.TextColor = seller ? muted : blue;
        SellerRoleText.TextColor = seller ? blue : muted;

        if (seller)
        {
            SellerFields.Opacity = 0;
            SellerFields.TranslationY = 10;
            SellerFields.IsVisible = true;
            await Task.WhenAll(
                SellerFields.FadeToAsync(1, 180, Easing.CubicOut),
                SellerFields.TranslateToAsync(0, 0, 180, Easing.CubicOut)
            );
        }
        else
        {
            SellerFields.IsVisible = false;
        }
    }

    // --- VECTOR SHOW / HIDE PASSWORD TOGGLES ---
    private const string EyeOpenPath = "M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z";
    private const string EyeClosedPath = "M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.7-2.89 3.43-4.75-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 1.55 0 3.03-.3 4.38-.84l.42.42L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.31-.78l3.15 3.15.02-.16c0-1.66-1.34-3-3-3l-.17.01z";

    private void OnToggleLoginPassword(object? s, TappedEventArgs e)
    {
        LoginPassword.IsPassword = !LoginPassword.IsPassword;
        LoginEyePath.Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(LoginPassword.IsPassword ? EyeOpenPath : EyeClosedPath);
    }

    private void OnToggleRegisterPassword(object? s, TappedEventArgs e)
    {
        RegisterPassword.IsPassword = !RegisterPassword.IsPassword;
        RegisterEyePath.Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(RegisterPassword.IsPassword ? EyeOpenPath : EyeClosedPath);
    }

    private void OnToggleConfirmPassword(object? s, TappedEventArgs e)
    {
        ConfirmPassword.IsPassword = !ConfirmPassword.IsPassword;
        ConfirmEyePath.Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(ConfirmPassword.IsPassword ? EyeOpenPath : EyeClosedPath);
    }

    private string pendingEmail = "";
    private AuthResponse? pendingAuthResponse;

    private async void OnForgotTapped(object? s, TappedEventArgs e)
    {
        var email = LoginEmail.Text?.Trim();
        if (ValidEmail(email))
        {
            try
            {
                await CampusApiService.Instance.SendOtpAsync(email!);
                pendingEmail = email!;
                ShowOtp(email!);
                await DisplayAlertAsync("Reset Code Sent", $"We sent a verification code to {email}.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        else
        {
            await DisplayAlertAsync("Forgot Password", "Enter your email address in the email field first to receive a verification code.", "OK");
        }
    }

    private async void ShowOtp(string email)
    {
        pendingEmail = email;
        LoginPanel.IsVisible = false;
        RegisterPanel.IsVisible = false;
        OtpPanel.Opacity = 0;
        OtpPanel.TranslationY = 12;
        OtpPanel.IsVisible = true;
        OtpSubtitle.Text = $"We sent a 6-digit verification code to {email}. Check your email inbox.";
        OtpInput.Text = "";

        await Task.WhenAll(
            OtpPanel.FadeToAsync(1, 200, Easing.CubicOut),
            OtpPanel.TranslateToAsync(0, 0, 200, Easing.CubicOut)
        );
    }

    private void OnBackToLoginFromOtp(object? s, EventArgs e)
    {
        OtpPanel.IsVisible = false;
        ShowLogin();
    }

    private async void OnResendOtpTapped(object? s, TappedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(pendingEmail)) return;
        try
        {
            await CampusApiService.Instance.SendOtpAsync(pendingEmail);
            await DisplayAlertAsync("Code Resent", $"A new verification code was sent to {pendingEmail}.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private async void OnVerifyOtpClicked(object? s, EventArgs e)
    {
        var code = OtpInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
        {
            await DisplayAlertAsync("Invalid Code", "Please enter the 6-digit verification code.", "OK");
            return;
        }

        VerifyOtpButton.IsEnabled = false;
        try
        {
            var verified = await CampusApiService.Instance.VerifyOtpAsync(pendingEmail, code);
            if (verified)
            {
                if (pendingAuthResponse?.User != null && !string.IsNullOrWhiteSpace(pendingAuthResponse.Token))
                {
                    AppState.Instance.SaveSession(
                        pendingAuthResponse.Token,
                        pendingAuthResponse.User.Name,
                        pendingAuthResponse.User.Email,
                        pendingAuthResponse.User.Role == "seller" ? "StudentSeller" : "Buyer",
                        pendingAuthResponse.User.Id,
                        pendingAuthResponse.User.SellerShopName,
                        pendingAuthResponse.User.SellerBio,
                        pendingAuthResponse.User.PreferredMeetupArea
                    );
                }

                await DisplayAlertAsync("Verified ✓", "Your email has been verified. Welcome to BazHeart!", "Enter Marketplace");
                EnterApp();
            }
            else
            {
                await DisplayAlertAsync("Verification Failed", "The code you entered is invalid or has expired.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Verification Error", ex.Message, "OK");
        }
        finally
        {
            VerifyOtpButton.IsEnabled = true;
        }
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
            if (res?.RequiresOtp == true)
            {
                pendingEmail = email;
                pendingAuthResponse = res;
                ShowOtp(email);
                await DisplayAlertAsync("Email Verification Required", "Please enter the 6-digit code sent to your email to verify your account.", "OK");
                return;
            }

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
                pendingEmail = email;
                pendingAuthResponse = res;
                ShowOtp(email);
                await DisplayAlertAsync("Account Created!", $"We sent a 6-digit verification code to {email}. Please enter it below to activate your account.", "OK");
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

    private void EnterApp()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            window.Page = new AppShell();
        }
    }
}
