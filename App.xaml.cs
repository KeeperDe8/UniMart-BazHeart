namespace FinalProject;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Services.ThemeManager.Initialize();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var hasSession = Services.AppState.Instance.RestoreSession();
        return new Window(hasSession ? new AppShell() : new LandingPage());
    }
}
