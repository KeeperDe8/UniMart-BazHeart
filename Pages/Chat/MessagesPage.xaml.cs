using FinalProject.Models;
using FinalProject.Services;
using FinalProject.Services.Api;
using Microsoft.Maui.Controls.Shapes;

namespace FinalProject;

public partial class MessagesPage : ContentPage
{
    public MessagesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadConversations();
    }

    private async void LoadConversations()
    {
        try
        {
            var conversations = await Task.Run(() => CampusApiService.Instance.GetConversationsAsync());
            Dispatcher.Dispatch(() =>
            {
                ConversationsContainer.Clear();

                if (conversations == null || conversations.Count == 0)
                {
                    EmptyStateView.IsVisible = true;
                    ConversationsContainer.IsVisible = false;
                    ChatCountLabel.Text = "0";
                    return;
                }

                EmptyStateView.IsVisible = false;
                ConversationsContainer.IsVisible = true;
                ChatCountLabel.Text = conversations.Count.ToString();

                foreach (var conv in conversations)
                {
                    var card = CreateConversationCard(conv);
                    ConversationsContainer.Add(card);
                }
            });
        }
        catch
        {
            Dispatcher.Dispatch(() =>
            {
                if (ConversationsContainer.Children.Count == 0)
                {
                    EmptyStateView.IsVisible = true;
                    ConversationsContainer.IsVisible = false;
                }
            });
        }
    }

    private View CreateConversationCard(ApiConversation conv)
    {
        var otherName = conv.OtherUser?.Name ?? "Campus User";
        var otherHandle = "@" + otherName.ToLower().Replace(" ", "");
        var lastMsg = conv.LatestMessage?.Body ?? "Tap to view conversation";
        var timeStr = conv.LatestMessage != null ? conv.LatestMessage.CreatedAt.ToString("h:mm tt") : "";

        var card = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["CardBg"],
            Stroke = (Color)Application.Current!.Resources["BorderLight"],
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(12, 10)
        };

        var grid = new Grid
        {
            Padding = new Thickness(0, 4),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(54) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Avatar
        var avatarGrid = new Grid { HeightRequest = 46, WidthRequest = 46 };
        var avatarBorder = new Border
        {
            BackgroundColor = Color.FromArgb("#1E3A8A"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 23 }
        };

        var avatarSrc = conv.OtherUser?.AvatarUrl;
        if (!string.IsNullOrWhiteSpace(avatarSrc))
        {
            avatarBorder.Content = new Image { Source = avatarSrc, Aspect = Aspect.AspectFill };
        }
        else
        {
            var initial = !string.IsNullOrWhiteSpace(otherName) ? otherName[0].ToString().ToUpper() : "U";
            avatarBorder.Content = new Label { Text = initial, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#93C5FD"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }

        avatarGrid.Add(avatarBorder);
        avatarGrid.Add(new Ellipse
        {
            Fill = (Color)Application.Current!.Resources["SuccessGreen"],
            HeightRequest = 9,
            WidthRequest = 9,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End
        });
        grid.Add(avatarGrid, 0);

        // Middle: Name & Last Message
        var middleStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        middleStack.Add(new Label
        {
            Text = $"{otherName} ({otherHandle})",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["TextDark"],
            LineBreakMode = LineBreakMode.TailTruncation
        });
        middleStack.Add(new Label
        {
            Text = lastMsg,
            FontSize = 11,
            TextColor = (Color)Application.Current!.Resources["TextMuted"],
            LineBreakMode = LineBreakMode.TailTruncation
        });
        grid.Add(middleStack, 1);

        // Right: Time & Unread Badge
        var rightStack = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
        if (!string.IsNullOrWhiteSpace(timeStr))
        {
            rightStack.Add(new Label { Text = timeStr, FontSize = 10, TextColor = (Color)Application.Current!.Resources["TextMuted"] });
        }

        if (conv.UnreadCount > 0)
        {
            var unreadBadge = new Border
            {
                BackgroundColor = (Color)Application.Current!.Resources["DangerRed"],
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                HeightRequest = 18,
                WidthRequest = 18,
                HorizontalOptions = LayoutOptions.End
            };
            unreadBadge.Content = new Label { Text = conv.UnreadCount.ToString(), FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            rightStack.Add(unreadBadge);
        }
        grid.Add(rightStack, 2);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            AppState.Instance.CurrentProduct = new MarketplaceProduct
            {
                Seller = otherHandle,
                SellerId = conv.OtherUser?.Id ?? 1,
                ProductName = conv.Listing?.Title ?? "Campus Listing",
                Price = conv.Listing?.Price ?? 0m,
                ImageSource = "homura.jpg"
            };
            await Shell.Current.GoToAsync("chat");
        };
        grid.GestureRecognizers.Add(tap);

        card.Content = grid;
        return card;
    }
}
