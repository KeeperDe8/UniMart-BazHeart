using FinalProject.Models;
using FinalProject.Services;
using FinalProject.Services.Api;
using Microsoft.Maui.Controls.Shapes;

namespace FinalProject;

public partial class ExplorePage : ContentPage
{
    string activeCategory = "All";
    string searchQuery = "";
    readonly List<MarketplaceProduct> loadedProducts = [];

    public ExplorePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProductsAsync();
    }

    async void LoadProductsAsync()
    {
        loadedProducts.Clear();

        // 1. Fetch live listings from backend database
        try
        {
            var apiListings = await CampusApiService.Instance.GetFeedAsync();
            if (apiListings != null && apiListings.Count > 0)
            {
                foreach (var apiItem in apiListings)
                {
                    if (!loadedProducts.Any(x => x.ProductName.Equals(apiItem.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        var imgSrc = "matcha.jpg";
                        if (!string.IsNullOrWhiteSpace(apiItem.PrimaryImage?.ImagePath))
                        {
                            var path = apiItem.PrimaryImage.ImagePath;
                            imgSrc = (path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) && !path.Contains('/') 
                                ? path 
                                : "matcha.jpg";
                        }

                        loadedProducts.Add(new MarketplaceProduct
                        {
                            ProductName = apiItem.Title,
                            Price = apiItem.Price,
                            Quantity = apiItem.StockQuantity,
                            ImageSource = imgSrc,
                            Category = apiItem.Category?.Name ?? "Campus Item",
                            Condition = apiItem.ItemCondition ?? "Good Condition",
                            Seller = apiItem.Seller?.Name != null ? ("@" + apiItem.Seller.Name.Replace(" ", "").ToLower()) : "@campus_seller",
                            Description = apiItem.Description ?? "Student listing on NU Lipa marketplace.",
                            BackendListingId = apiItem.Id
                        });
                    }
                }
            }
        }
        catch { }

        // 2. Add local active listings from AppState if not already present
        foreach (var local in AppState.Instance.Listings.Where(x => x.Status == "Active"))
        {
            if (!loadedProducts.Any(x => x.ProductName.Equals(local.ProductName, StringComparison.OrdinalIgnoreCase)))
            {
                loadedProducts.Insert(0, local);
            }
        }

        // 3. Fallback default curated items if database is completely empty
        if (loadedProducts.Count == 0)
        {
            loadedProducts.Add(new MarketplaceProduct { ProductName = "Madoka Plush", Price = 700, Quantity = 5, ImageSource = "madoka.jpg", Category = "Clothes", Condition = "Good Condition", Seller = "@mariasantos", Description = "Original Madoka Kaname plushie doll." });
            loadedProducts.Add(new MarketplaceProduct { ProductName = "Homura Plush", Price = 760, Quantity = 5, ImageSource = "homura.jpg", Category = "Clothes", Condition = "Good Condition", Seller = "@mariasantos", Description = "Original Homura Akemi plushie doll." });
            loadedProducts.Add(new MarketplaceProduct { ProductName = "Iced Strawberry Matcha Latte", Price = 95, Quantity = 5, ImageSource = "matcha.jpg", Category = "Food & Drinks", Condition = "Freshly Prepared", Seller = "@matchabykai", Description = "Signature iced matcha." });
            loadedProducts.Add(new MarketplaceProduct { ProductName = "Handmade Crochet Daisy Bouquet", Price = 160, Quantity = 5, ImageSource = "crochet_bouquet.jpg", Category = "Handmade", Condition = "Made to Order", Seller = "@crochetbysam", Description = "Everlasting pastel daisy bouquet." });
        }

        RenderProductGrid();
    }

    void RenderProductGrid()
    {
        var filtered = loadedProducts.Where(p =>
        {
            bool matchesCategory = activeCategory == "All" || p.Category.Contains(activeCategory, StringComparison.OrdinalIgnoreCase) || (activeCategory == "Food & Drinks" && p.Category.Contains("Food", StringComparison.OrdinalIgnoreCase));
            bool matchesSearch = string.IsNullOrWhiteSpace(searchQuery) || p.ProductName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || p.Seller.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || p.Category.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
            return matchesCategory && matchesSearch;
        }).ToList();

        DynamicProductsGrid.Children.Clear();
        DynamicProductsGrid.RowDefinitions.Clear();

        int rows = (filtered.Count + 1) / 2;
        for (int i = 0; i < rows; i++)
        {
            DynamicProductsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (int i = 0; i < filtered.Count; i++)
        {
            var product = filtered[i];
            var card = CreateProductCard(product);
            int row = i / 2;
            int col = i % 2;

            Grid.SetRow(card, row);
            Grid.SetColumn(card, col);
            
            card.Opacity = 0;
            card.TranslationY = 12;
            DynamicProductsGrid.Children.Add(card);
            
            // Staggered fluid entrance
            _ = card.FadeToAsync(1.0, (uint)(180 + i * 35), Easing.CubicOut);
            _ = card.TranslateToAsync(0, 0, (uint)(180 + i * 35), Easing.CubicOut);
        }
    }

    View CreateProductCard(MarketplaceProduct p)
    {
        var card = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["CardBg"],
            Stroke = (Color)Application.Current!.Resources["BorderLight"],
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = 0,
            Shadow = new Shadow { Brush = Color.FromArgb("#000000"), Offset = new Point(0, 4), Radius = 10, Opacity = 0.07f }
        };

        var stack = new VerticalStackLayout { Spacing = 0 };

        var imgGrid = new Grid { HeightRequest = 148 };
        var img = new Image { Source = string.IsNullOrWhiteSpace(p.ImageSource) ? "matcha.jpg" : p.ImageSource, Aspect = Aspect.AspectFill };
        imgGrid.Add(img);

        var stockBadge = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["LightGreen"],
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(7, 3),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(8)
        };
        stockBadge.Content = new Label { Text = $"{p.Quantity} in Stock", FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = (Color)Application.Current!.Resources["SuccessGreen"] };
        imgGrid.Add(stockBadge);

        var heartBtn = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["CardBg"],
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            HeightRequest = 32,
            WidthRequest = 32,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(8),
            Shadow = new Shadow { Brush = Color.FromArgb("#000000"), Offset = new Point(0, 2), Radius = 6, Opacity = 0.15f }
        };
        var heartLbl = new Label { Text = p.IsSaved ? "♥" : "♡", FontSize = 18, TextColor = p.IsSaved ? Color.FromArgb("#F43F5E") : (Color)Application.Current!.Resources["TextMuted"], HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        heartBtn.Content = heartLbl;
        var heartTap = new TapGestureRecognizer();
        heartTap.Tapped += async (_, _) =>
        {
            await heartBtn.ScaleToAsync(1.35, 90, Easing.SpringOut);
            AppState.Instance.ToggleSaved(p);
            heartLbl.Text = p.IsSaved ? "♥" : "♡";
            heartLbl.TextColor = p.IsSaved ? Color.FromArgb("#F43F5E") : (Color)Application.Current!.Resources["TextMuted"];
            await heartBtn.ScaleToAsync(1.0, 90, Easing.CubicIn);
        };
        heartBtn.GestureRecognizers.Add(heartTap);
        imgGrid.Add(heartBtn);
        stack.Add(imgGrid);

        var details = new VerticalStackLayout { Padding = new Thickness(12, 9), Spacing = 3 };
        details.Add(new Label { Text = p.ProductName, FontSize = 13.5, FontAttributes = FontAttributes.Bold, TextColor = (Color)Application.Current!.Resources["TextDark"], MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation });
        details.Add(new Label { Text = $"₱{p.Price:0.00}", FontSize = 14.5, FontAttributes = FontAttributes.Bold, TextColor = (Color)Application.Current!.Resources["PrimaryBlue"] });
        details.Add(new Label { Text = p.Seller, FontSize = 10.5, TextColor = (Color)Application.Current!.Resources["TextMuted"] });
        stack.Add(details);

        card.Content = stack;

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            await card.ScaleToAsync(0.95, 60, Easing.CubicOut);
            await card.ScaleToAsync(1.0, 70, Easing.CubicIn);
            AppState.Instance.CurrentProduct = p;
            await Shell.Current.GoToAsync("product");
        };
        card.GestureRecognizers.Add(tap);

        return card;
    }

    void OnCategoryCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string cat)
        {
            activeCategory = cat;
            UpdateFilterPillButtons();
            RenderProductGrid();
        }
    }

    async void OnFilterPillClicked(object? sender, EventArgs e)
    {
        if (sender is Button b)
        {
            await b.ScaleToAsync(0.92, 50, Easing.CubicOut);
            await b.ScaleToAsync(1.0, 60, Easing.CubicIn);
            activeCategory = b.Text.Replace("&amp;", "&");
            UpdateFilterPillButtons();
            RenderProductGrid();
        }
    }

    void UpdateFilterPillButtons()
    {
        var buttons = new[] { AllFilterBtn, FoodFilterBtn, HandmadeFilterBtn, ClothesFilterBtn, AccessoriesFilterBtn };
        foreach (var btn in buttons)
        {
            bool isActive = btn.Text.Equals(activeCategory, StringComparison.OrdinalIgnoreCase) || (activeCategory.StartsWith("Food") && btn == FoodFilterBtn);
            btn.BackgroundColor = isActive ? (Color)Application.Current!.Resources["DarkBlue"] : (Color)Application.Current!.Resources["CardBg"];
            btn.TextColor = isActive ? Colors.White : (Color)Application.Current!.Resources["TextDark"];
            btn.BorderWidth = isActive ? 0 : 1;
            btn.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
        }
    }

    void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        searchQuery = e.NewTextValue?.Trim() ?? "";
        RenderProductGrid();
    }
}
