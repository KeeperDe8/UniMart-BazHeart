namespace FinalProject.Controls;

public partial class UniMartHeader : ContentView
{
    public UniMartHeader() => InitializeComponent();

    private async void OnSearchTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null) await Shell.Current.GoToAsync("//explore");
    }

    private async void OnMessagesTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null) await Shell.Current.GoToAsync("notifications");
    }
}
