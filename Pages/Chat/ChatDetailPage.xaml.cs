using FinalProject.Services;
using FinalProject.Services.Api;
using Microsoft.Maui.Controls.Shapes;

namespace FinalProject;

public partial class ChatDetailPage : ContentPage
{
    int activeConversationId = 0;
    int targetSellerId = 0;
    string targetSellerName = "Seller";
    IDispatcherTimer? pollTimer;
    bool isPolling = false;
    readonly HashSet<int> renderedMessageIds = [];

    public ChatDetailPage()
    {
        InitializeComponent();
        BindCurrentProductAndSeller();
        LoadInitialMessages();
        StartRealtimePolling();
    }

    void BindCurrentProductAndSeller()
    {
        var p = AppState.Instance.CurrentProduct;
        if (p != null)
        {
            var rawSeller = string.IsNullOrWhiteSpace(p.Seller) ? "Maria Santos" : p.Seller;
            var cleanSeller = rawSeller.StartsWith("@") ? rawSeller.Substring(1) : rawSeller;
            var displayName = cleanSeller.Equals("mariasantos", StringComparison.OrdinalIgnoreCase) ? "Maria Santos" :
                              cleanSeller.Equals("matchabykai", StringComparison.OrdinalIgnoreCase) ? "Kai dela Cruz" : cleanSeller;

            targetSellerName = displayName;
            targetSellerId = p.SellerId > 0 ? p.SellerId : (cleanSeller.ToLower().Contains("maria") ? 1 : (cleanSeller.ToLower().Contains("kai") ? 2 : 1));

            ChatUserName.Text = $"{displayName} (@{cleanSeller.ToLower()})";
            ChatAvatar.Source = cleanSeller.ToLower().Contains("maria") ? "profile_sdada.jpg" : "kai_avatar.jpg";
            ProductThumb.Source = p.ImageSource;
            ProductTitleText.Text = $"{p.ProductName} • ₱{p.Price:0.00}";
        }
        else
        {
            targetSellerName = "Maria Santos";
            targetSellerId = 1;
            ChatUserName.Text = "Maria Santos (@mariasantos)";
            ChatAvatar.Source = "profile_sdada.jpg";
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopRealtimePolling();
    }

    void StartRealtimePolling()
    {
        pollTimer = Dispatcher.CreateTimer();
        pollTimer.Interval = TimeSpan.FromSeconds(5);
        pollTimer.Tick += async (s, e) => await PollIncomingMessagesAsync();
        pollTimer.Start();
    }

    void StopRealtimePolling()
    {
        pollTimer?.Stop();
        pollTimer = null;
    }

    async void LoadInitialMessages()
    {
        MessagesLayout.Clear();
        renderedMessageIds.Clear();

        var p = AppState.Instance.CurrentProduct;

        // Fetch or create unique conversation for this specific seller
        try
        {
            var res = await Task.Run(() => CampusApiService.Instance.GetOrCreateConversationAsync(
                targetSellerId,
                targetSellerName,
                p?.BackendListingId
            ));

            if (res != null)
            {
                activeConversationId = res.ConversationId;

                if (res.Messages != null && res.Messages.Count > 0)
                {
                    MessagesLayout.Add(CreateTimePill("Today"));
                    foreach (var msg in res.Messages)
                    {
                        renderedMessageIds.Add(msg.Id);
                        bool isOutgoing = msg.SenderId == (AppState.Instance.CurrentUserId > 0 ? AppState.Instance.CurrentUserId : 4);
                        if (msg.MessageType == "meetup_card")
                        {
                            MessagesLayout.Add(CreateMeetupCard("Main Building – Ground Floor Lobby", msg.CreatedAt.ToString("h:mm tt"), msg.Body));
                        }
                        else
                        {
                            MessagesLayout.Add(CreateMessageBubble(msg.Body, msg.CreatedAt.ToString("h:mm tt"), isOutgoing));
                        }
                    }
                    await ScrollToBottomAsync();
                    return;
                }
            }
        }
        catch { }

        // Clean new conversation start with NO fake messages
        MessagesLayout.Add(CreateTimePill("New Conversation"));
        var startCard = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["InputBackground"],
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(14, 10),
            Margin = new Thickness(20, 12),
            HorizontalOptions = LayoutOptions.Center
        };
        var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center };
        stack.Add(new Label { Text = $"Chatting with {targetSellerName}", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = (Color)Application.Current!.Resources["TextDark"], HorizontalTextAlignment = TextAlignment.Center });
        stack.Add(new Label { Text = "Send a message or propose a meetup to arrange campus pickup.", FontSize = 11, TextColor = (Color)Application.Current!.Resources["TextMuted"], HorizontalTextAlignment = TextAlignment.Center });
        startCard.Content = stack;
        MessagesLayout.Add(startCard);
    }

    async Task PollIncomingMessagesAsync()
    {
        if (isPolling || activeConversationId == 0) return;
        isPolling = true;

        try
        {
            var res = await Task.Run(() => CampusApiService.Instance.GetMessagesAsync(activeConversationId));
            if (res?.Messages != null && res.Messages.Count > 0)
            {
                bool hasNew = false;
                foreach (var msg in res.Messages)
                {
                    if (!renderedMessageIds.Contains(msg.Id))
                    {
                        renderedMessageIds.Add(msg.Id);
                        bool isOutgoing = msg.SenderId == (AppState.Instance.CurrentUserId > 0 ? AppState.Instance.CurrentUserId : 4);
                        if (msg.MessageType == "meetup_card")
                        {
                            MessagesLayout.Add(CreateMeetupCard("Main Building – Ground Floor Lobby", msg.CreatedAt.ToString("h:mm tt"), msg.Body));
                        }
                        else
                        {
                            MessagesLayout.Add(CreateMessageBubble(msg.Body, msg.CreatedAt.ToString("h:mm tt"), isOutgoing));
                        }
                        hasNew = true;
                    }
                }
                if (hasNew)
                {
                    await ScrollToBottomAsync();
                }
            }
        }
        catch { }
        finally
        {
            isPolling = false;
        }
    }

    View CreateTimePill(string time)
    {
        var pill = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["InputBackground"],
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(10, 4),
            HorizontalOptions = LayoutOptions.Center
        };
        pill.Content = new Label { Text = time, FontSize = 10, TextColor = (Color)Application.Current!.Resources["TextMuted"] };
        return pill;
    }

    View CreateMessageBubble(string text, string time, bool isOutgoing)
    {
        var bubble = new Border
        {
            BackgroundColor = isOutgoing ? (Color)Application.Current!.Resources["PrimaryBlue"] : (Color)Application.Current!.Resources["CardBg"],
            Stroke = isOutgoing ? Colors.Transparent : (Color)Application.Current!.Resources["BorderLight"],
            StrokeThickness = isOutgoing ? 0 : 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(14, 10),
            HorizontalOptions = isOutgoing ? LayoutOptions.End : LayoutOptions.Start,
            MaximumWidthRequest = 290
        };

        var stack = new VerticalStackLayout { Spacing = 3 };
        stack.Add(new Label
        {
            Text = text,
            FontSize = 13,
            TextColor = isOutgoing ? Colors.White : (Color)Application.Current!.Resources["TextDark"]
        });
        stack.Add(new Label
        {
            Text = time,
            FontSize = 9,
            TextColor = isOutgoing ? Color.FromArgb("#DCE7FF") : (Color)Application.Current!.Resources["TextMuted"],
            HorizontalOptions = LayoutOptions.End
        });

        bubble.Content = stack;
        return bubble;
    }

    View CreateMeetupCard(string location, string datetime, string notes)
    {
        var card = new Border
        {
            BackgroundColor = Color.FromArgb("#065F46"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(14),
            Margin = new Thickness(10, 4),
            MaximumWidthRequest = 320,
            HorizontalOptions = LayoutOptions.Center
        };

        var stack = new VerticalStackLayout { Spacing = 6 };
        var header = new HorizontalStackLayout { Spacing = 8 };
        header.Add(new Label { Text = "📍", FontSize = 18 });
        var titleStack = new VerticalStackLayout { Spacing = 0 };
        titleStack.Add(new Label { Text = "Campus Meetup Scheduled", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        titleStack.Add(new Label { Text = location, FontSize = 11, TextColor = Color.FromArgb("#A7F3D0") });
        header.Add(titleStack);
        stack.Add(header);

        var timeBox = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(10, 6)
        };
        timeBox.Content = new Label { Text = $"⏰ {datetime}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#065F46") };
        stack.Add(timeBox);

        if (!string.IsNullOrWhiteSpace(notes))
        {
            stack.Add(new Label { Text = notes, FontSize = 10, TextColor = Color.FromArgb("#D1FAE5") });
        }

        // Action Buttons: Confirm & Change Time
        var btnRow = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Star } }, ColumnSpacing = 8, Margin = new Thickness(0, 4, 0, 0) };
        var confirmBtn = new Button { Text = "✓ Confirm", BackgroundColor = Color.FromArgb("#10B981"), TextColor = Colors.White, FontSize = 10, FontAttributes = FontAttributes.Bold, HeightRequest = 30, CornerRadius = 15 };
        confirmBtn.Clicked += async (_, _) =>
        {
            confirmBtn.Text = "Confirmed ✓";
            confirmBtn.IsEnabled = false;
            if (Shell.Current != null) await Shell.Current.DisplayAlert("Meetup Confirmed", "Your campus handover is scheduled. Please arrive on time at the hotspot!", "OK");
        };
        btnRow.Add(confirmBtn);

        var chatBtn = new Button { Text = "Adjust Time", BackgroundColor = Color.FromArgb("#047857"), TextColor = Colors.White, FontSize = 10, HeightRequest = 30, CornerRadius = 15 };
        chatBtn.Clicked += (_, _) =>
        {
            MessageEntry.Text = "Can we meet 15 minutes earlier?";
            MessageEntry.Focus();
        };
        btnRow.Add(chatBtn, 1);
        stack.Add(btnRow);

        card.Content = stack;
        return card;
    }

    async void OnSendClicked(object? sender, EventArgs e)
    {
        var text = MessageEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        // 1. Optimistic Instant UI Bubble (Zero latency)
        MessageEntry.Text = string.Empty;
        var bubble = CreateMessageBubble(text, DateTime.Now.ToString("h:mm tt"), true);
        MessagesLayout.Add(bubble);
        await ScrollToBottomAsync();

        // 2. Concurrently push to backend API
        var convId = activeConversationId;
        _ = Task.Run(async () =>
        {
            try
            {
                if (convId > 0)
                {
                    await CampusApiService.Instance.SendTextMessageAsync(convId, text);
                }
            }
            catch { }
        });
    }

    async void OnQuickChipClicked(object? sender, EventArgs e)
    {
        if (sender is Button b)
        {
            MessageEntry.Text = b.Text;
            OnSendClicked(sender, e);
        }
    }

    async void OnProposeMeetupQuickClicked(object? sender, EventArgs e)
    {
        var location = await DisplayActionSheetAsync(
            "Select Campus Hotspot",
            "Cancel",
            null,
            "Main Building – Ground Floor Lobby",
            "Student Activity Center (SAC)",
            "Library 2nd Floor Entrance",
            "Campus Cafeteria"
        );

        if (string.IsNullOrWhiteSpace(location) || location == "Cancel") return;

        var time = DateTime.Now.AddHours(1).ToString("h:mm tt");
        var meetupCard = CreateMeetupCard(location, $"Today at {time}", "Meetup proposed via quick chat schedule.");
        MessagesLayout.Add(meetupCard);
        await ScrollToBottomAsync();

        var convId = activeConversationId;
        _ = Task.Run(async () =>
        {
            try
            {
                if (convId > 0)
                {
                    await CampusApiService.Instance.SendMeetupCardAsync(convId, 1, DateTime.Now.AddHours(1), $"Pickup at {location}");
                }
            }
            catch { }
        });
    }

    async void OnLocationShortcutTapped(object? sender, TappedEventArgs e)
    {
        OnProposeMeetupQuickClicked(sender, EventArgs.Empty);
    }

    async Task ScrollToBottomAsync()
    {
        await Task.Delay(50);
        await ConversationScroll.ScrollToAsync(0, MessagesLayout.Height, true);
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");

    private async void OnFlagTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("Safety First", "Reporting user to Campus Admin for safety verification.", "OK");
        }
    }
}
