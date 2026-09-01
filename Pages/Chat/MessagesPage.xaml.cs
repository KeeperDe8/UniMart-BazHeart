namespace FinalProject;
public partial class MessagesPage : ContentPage { public MessagesPage() => InitializeComponent(); private async void OnKaiTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("chat"); }
