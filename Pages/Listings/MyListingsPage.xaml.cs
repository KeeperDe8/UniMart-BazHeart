using FinalProject.Models;
using FinalProject.Services;

namespace FinalProject;

public partial class MyListingsPage : ContentPage
{
    string currentFilter = "all";

    public MyListingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadListings();
    }

    async void LoadListings()
    {
        // 1. Ensure active seller ID is 1 (Maria Santos)
        if (AppState.Instance.CurrentUserId <= 0)
        {
            AppState.Instance.CurrentUserId = 1;
            AppState.Instance.CurrentUserName = "Maria Santos";
            Preferences.Default.Set("user_id", 1);
        }

        // 2. Fetch live listings from backend
        try
        {
            var remoteListings = await Services.Api.CampusApiService.Instance.GetSellerListingsAsync();
            if (remoteListings != null && remoteListings.Count > 0)
            {
                AppState.Instance.Listings.Clear();
                foreach (var r in remoteListings)
                {
                    var imgSrc = "matcha.jpg";
                    if (!string.IsNullOrWhiteSpace(r.PrimaryImage?.ImagePath))
                    {
                        var path = r.PrimaryImage.ImagePath;
                        imgSrc = (path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) && !path.Contains('/') 
                            ? path 
                            : "matcha.jpg";
                    }

                    AppState.Instance.Listings.Add(new MarketplaceProduct
                    {
                        ProductName = r.Title,
                        Price = r.Price,
                        Quantity = r.StockQuantity,
                        Category = r.Category?.Name ?? "Campus Item",
                        Condition = r.ItemCondition ?? "Good Condition",
                        Description = r.Description,
                        Status = r.Status == "active" ? "Active" : "Payment Required",
                        Seller = "@" + (string.IsNullOrWhiteSpace(AppState.Instance.CurrentUserName) ? "mariasantos" : AppState.Instance.CurrentUserName.ToLower().Replace(" ", "")),
                        ImageSource = imgSrc,
                        BackendListingId = r.Id
                    });
                }
            }
        }
        catch { }

        await MainThread.InvokeOnMainThreadAsync(RenderListings);
    }

    void RenderListings()
    {
        var all = AppState.Instance.Listings.ToList();
        var active = all.Where(x => x.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();
        var paymentReq = all.Where(x => !x.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();

        ActiveCountLabel.Text = $"{active.Count}";
        PaymentReqCountLabel.Text = $"{paymentReq.Count}";
        SoldOutCountLabel.Text = "0";

        AllButton.Text = $"All ({all.Count})";
        ActiveButton.Text = $"Active ({active.Count})";
        PaymentButton.Text = $"Payment Required ({paymentReq.Count})";
        SoldButton.Text = "Sold Out (0)";

        List<MarketplaceProduct> filtered = currentFilter switch
        {
            "active" => active,
            "payment" => paymentReq,
            "sold" => [],
            _ => all
        };

        ListingsListContainer.Clear();

        if (filtered.Count == 0)
        {
            EmptyStateView.IsVisible = true;
            ListingsListContainer.IsVisible = false;
        }
        else
        {
            EmptyStateView.IsVisible = false;
            ListingsListContainer.IsVisible = true;

            foreach (var item in filtered)
            {
                var card = CreateListingCard(item);
                ListingsListContainer.Add(card);
            }
        }
    }

    View CreateListingCard(MarketplaceProduct item)
    {
        var card = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["CardBg"],
            Stroke = (Color)Application.Current!.Resources["BorderLight"],
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(12)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(72) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 12
        };

        var img = new Image
        {
            Source = string.IsNullOrWhiteSpace(item.ImageSource) ? "matcha.jpg" : item.ImageSource,
            Aspect = Aspect.AspectFill,
            HeightRequest = 72,
            WidthRequest = 72
        };
        var imgBorder = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Content = img
        };
        grid.Add(imgBorder);

        var details = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        details.Add(new Label { Text = item.ProductName, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = (Color)Application.Current!.Resources["TextDark"], MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation });
        details.Add(new Label { Text = $"₱{item.Price:0.00}", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = (Color)Application.Current!.Resources["PrimaryBlue"] });
        details.Add(new Label { Text = $"{item.Quantity} in stock • {item.Category}", FontSize = 10, TextColor = (Color)Application.Current!.Resources["TextMuted"] });
        grid.Add(details, 1);

        bool isActive = item.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        if (isActive)
        {
            var statusBadge = new Border
            {
                BackgroundColor = (Color)Application.Current!.Resources["LightGreen"],
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(8, 4),
                VerticalOptions = LayoutOptions.Center
            };
            statusBadge.Content = new Label
            {
                Text = "Active",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current!.Resources["SuccessGreen"]
            };
            grid.Add(statusBadge, 2);
        }
        else
        {
            var payFeeBtn = new Button
            {
                Text = "Pay ₱25",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.FromArgb("#2563EB"),
                TextColor = Colors.White,
                HeightRequest = 32,
                CornerRadius = 16,
                Padding = new Thickness(10, 0),
                VerticalOptions = LayoutOptions.Center
            };
            payFeeBtn.Clicked += async (_, _) =>
            {
                payFeeBtn.IsEnabled = false;
                payFeeBtn.Text = "Paying...";
                try
                {
                    await Services.Api.CampusApiService.Instance.PayListingFeeAsync(item.BackendListingId ?? 1, "PM-VERIFIED");
                }
                catch { }

                item.Status = "Active";
                LoadListings();
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Listing Published", $"₱25 fee paid! '{item.ProductName}' is now active on campus feed.", "OK");
                }
            };
            grid.Add(payFeeBtn, 2);
        }

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            AppState.Instance.CurrentProduct = item;
            await Shell.Current.GoToAsync("product");
        };
        card.GestureRecognizers.Add(tap);

        card.Content = grid;
        return card;
    }

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        AppState.Instance.Draft = new ListingDraft();
        await Shell.Current.GoToAsync("create-listing/photos");
    }

    private void OnFilterClicked(object? sender, EventArgs e)
    {
        if (sender is not Button selected) return;

        foreach (var button in new[] { AllButton, ActiveButton, PaymentButton, SoldButton })
        {
            button.BackgroundColor = (Color)Application.Current!.Resources["CardBg"];
            button.TextColor = (Color)Application.Current!.Resources["TextMuted"];
            button.BorderWidth = 1;
        }

        selected.BackgroundColor = (Color)Application.Current!.Resources["DarkBlue"];
        selected.TextColor = Colors.White;
        selected.BorderWidth = 0;

        currentFilter = selected == ActiveButton ? "active" : selected == PaymentButton ? "payment" : selected == SoldButton ? "sold" : "all";
        RenderListings();
    }
}
