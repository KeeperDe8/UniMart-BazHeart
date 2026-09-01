using System.Collections.ObjectModel;
using FinalProject.Models;
using FinalProject.Services.Api;

namespace FinalProject.Services;

public class AppState
{
    public static AppState Instance { get; } = new();

    public ListingDraft Draft { get; set; } = CreateDraft();
    public ObservableCollection<MarketplaceProduct> Listings { get; } = [];
    public ObservableCollection<MarketplaceProduct> SavedItems { get; } = [];
    public ObservableCollection<SellingScheduleSlot> SellingSchedule { get; } =
    [
        new() { Day = "Monday", TimeWindow = "10:00 AM – 2:00 PM" },
        new() { Day = "Wednesday", TimeWindow = "11:00 AM – 3:00 PM" },
        new() { Day = "Friday", TimeWindow = "1:00 PM – 4:30 PM", Location = "Student Activity Center (SAC)" }
    ];

    public MarketplaceProduct? CurrentProduct { get; set; }
    public bool DarkMode { get => ThemeManager.IsDark; set => ThemeManager.Apply(value); }
    public string PaymentReference { get; set; } = "PAY-2026-343562";
    public string CurrentUserName { get; set; } = "Student";
    public string CurrentEmail { get; set; } = "";
    public int CurrentUserId { get; set; }
    public string CurrentRole { get; set; } = "StudentSeller";
    public string SellerDisplayName { get; set; } = "";
    public string SellerBio { get; set; } = "";
    public string PreferredMeetupArea { get; set; } = "Main Building – Ground Floor Lobby";
    public bool ShowLogoutMessage { get; set; }

    public bool IsSellerApproved
    {
        get => Preferences.Default.Get("is_seller_approved", false);
        set
        {
            Preferences.Default.Set("is_seller_approved", value);
            SellerApprovalChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public event EventHandler? SellerApprovalChanged;

    public void SaveSession(string token, string name, string email, string role, int userId = 0, string? shopName = null, string? bio = null, string? meetupArea = null)
    {
        CurrentUserName = name;
        CurrentEmail = email;
        CurrentUserId = userId;
        CurrentRole = role;
        SellerDisplayName = shopName ?? "";
        SellerBio = bio ?? "";
        PreferredMeetupArea = meetupArea ?? PreferredMeetupArea;

        Preferences.Default.Set("auth_token", token);
        Preferences.Default.Set("user_name", name);
        Preferences.Default.Set("user_email", email);
        Preferences.Default.Set("user_id", userId);
        Preferences.Default.Set("user_role", role);
        Preferences.Default.Set("seller_shop", SellerDisplayName);
        Preferences.Default.Set("seller_bio", SellerBio);
        Preferences.Default.Set("meetup_area", PreferredMeetupArea);

        ApiClient.Instance.SetToken(token);
    }

    public bool RestoreSession()
    {
        var token = Preferences.Default.Get("auth_token", string.Empty);
        if (string.IsNullOrWhiteSpace(token)) return false;

        CurrentUserName = Preferences.Default.Get("user_name", "Student");
        CurrentEmail = Preferences.Default.Get("user_email", "");
        CurrentUserId = Preferences.Default.Get("user_id", 0);
        CurrentRole = Preferences.Default.Get("user_role", "Buyer");
        SellerDisplayName = Preferences.Default.Get("seller_shop", "");
        SellerBio = Preferences.Default.Get("seller_bio", "");
        PreferredMeetupArea = Preferences.Default.Get("meetup_area", "Main Building – Ground Floor Lobby");

        ApiClient.Instance.SetToken(token);

        // If user_id is missing (old login), fetch it in background
        if (CurrentUserId == 0)
        {
            _ = FetchAndSaveUserIdAsync();
        }

        return true;
    }

    private async Task FetchAndSaveUserIdAsync()
    {
        try
        {
            var me = await Api.CampusApiService.Instance.GetMeAsync();
            if (me != null && me.Id > 0)
            {
                CurrentUserId = me.Id;
                Preferences.Default.Set("user_id", me.Id);
            }
        }
        catch { }
    }

    public void ClearSession()
    {
        Preferences.Default.Remove("auth_token");
        Preferences.Default.Remove("user_name");
        Preferences.Default.Remove("user_email");
        Preferences.Default.Remove("user_id");
        Preferences.Default.Remove("user_role");
        Preferences.Default.Remove("seller_shop");
        Preferences.Default.Remove("seller_bio");
        Preferences.Default.Remove("meetup_area");

        CurrentUserId = 0;
        ApiClient.Instance.SetToken(null);
    }

    public void ResetDraft() => Draft = CreateDraft();

    public MarketplaceProduct PublishDraft()
    {
        var existing = Listings.FirstOrDefault(x => x.ProductName.Equals(Draft.ProductName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Status = "Active";
            existing.Price = Draft.Price;
            existing.Quantity = Draft.Quantity;
            existing.Category = string.IsNullOrWhiteSpace(Draft.Category) ? "General" : Draft.Category;
            existing.Condition = Draft.Condition;
            existing.Description = Draft.Description;
            CurrentProduct = existing;
            return existing;
        }

        var product = new MarketplaceProduct
        {
            ImageSource = string.IsNullOrWhiteSpace(Draft.ImageSource) ? "matcha.jpg" : Draft.ImageSource,
            ProductName = Draft.ProductName,
            Price = Draft.Price,
            Quantity = Draft.Quantity,
            Category = string.IsNullOrWhiteSpace(Draft.Category) ? "General" : Draft.Category,
            Condition = Draft.Condition,
            Description = Draft.Description,
            ScheduleSlots = Draft.ScheduleSlots.Select(x => new SellingScheduleSlot { Day = x.Day, TimeWindow = x.TimeWindow, Location = x.Location }).ToList(),
            MeetupLocation = Draft.MeetupLocation,
            PickupInstructions = Draft.PickupInstructions,
            Status = "Active",
            Seller = string.IsNullOrWhiteSpace(SellerDisplayName) ? ("@" + CurrentUserName.Replace(" ", "").ToLower()) : SellerDisplayName
        };
        Listings.Insert(0, product);
        CurrentProduct = product;
        return product;
    }

    public void ToggleSaved(MarketplaceProduct product)
    {
        product.IsSaved = !product.IsSaved;
        if (product.IsSaved && !SavedItems.Contains(product)) SavedItems.Add(product);
        if (!product.IsSaved) SavedItems.Remove(product);
    }

    static ListingDraft CreateDraft() => new()
    {
        ScheduleSlots = [new() { Day = "Monday", TimeWindow = "10:00 AM – 2:00 PM" }, new() { Day = "Wednesday", TimeWindow = "11:00 AM – 3:00 PM" }]
    };
}
