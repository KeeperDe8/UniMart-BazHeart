namespace FinalProject.Pages.Splash;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Initial setup for animation
        LogoContainer.Scale = 0.5;
        LogoContainer.Opacity = 0;
        TextContainer.Opacity = 0;
        TextContainer.TranslationY = 20;

        // Sequence 1: Logo spring pop
        await Task.WhenAll(
            LogoContainer.FadeToAsync(1.0, 450, Easing.CubicOut),
            LogoContainer.ScaleToAsync(1.1, 550, Easing.SpringOut)
        );
        await LogoContainer.ScaleToAsync(1.0, 200, Easing.CubicIn);

        // Sequence 2: Text slide and fade in
        await Task.WhenAll(
            TextContainer.FadeToAsync(1.0, 400, Easing.CubicOut),
            TextContainer.TranslateToAsync(0, 0, 400, Easing.CubicOut)
        );

        // Hold splash momentarily for smooth experience
        await Task.Delay(900);

        // Sequence 3: Fade out and transition
        await SplashRoot.FadeToAsync(0.0, 350, Easing.CubicIn);

        // Navigate to Home
        await Shell.Current.GoToAsync("//home");
    }
}
