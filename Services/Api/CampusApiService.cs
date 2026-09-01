using System.Text.Json.Serialization;

namespace FinalProject.Services.Api;

public class CampusApiService
{
    public static CampusApiService Instance { get; } = new();

    private readonly ApiClient _api = ApiClient.Instance;

    // --- AUTHENTICATION & OTP ---
    public async Task<bool> SendOtpAsync(string email)
    {
        var res = await _api.PostAsync<object, object>("otp/send", new { email });
        return res != null;
    }

    public async Task<bool> VerifyOtpAsync(string email, string code)
    {
        var res = await _api.PostAsync<object, object>("otp/verify", new { email, code });
        return res != null;
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var res = await _api.PostAsync<object, AuthResponse>("login", new { email, password });
        if (res?.Token != null)
        {
            _api.SetToken(res.Token);
        }
        return res;
    }

    public async Task<AuthResponse?> RegisterAsync(string name, string email, string password, string role = "buyer", string? studentNumber = null, string? shopName = null, string? bio = null, string? meetupArea = null)
    {
        var res = await _api.PostAsync<object, AuthResponse>("register", new
        {
            name,
            email,
            password,
            student_number = studentNumber,
            role,
            seller_shop_name = shopName,
            seller_bio = bio,
            preferred_meetup_area = meetupArea
        });
        if (res?.Token != null)
        {
            _api.SetToken(res.Token);
        }
        return res;
    }

    public async Task<bool> LogoutAsync()
    {
        await _api.PostAsync<object, object>("logout", new { });
        _api.SetToken(null);
        return true;
    }

    // --- USER INFO ---
    public async Task<ApiUser?> GetMeAsync()
    {
        var email = AppState.Instance.CurrentEmail;
        var url = string.IsNullOrWhiteSpace(email) ? "me" : $"me?email={Uri.EscapeDataString(email)}";
        var res = await _api.GetAsync<MeResponse>(url);
        return res?.User;
    }

    // --- LISTINGS & FEED ---
    public async Task<List<ApiListing>> GetFeedAsync(string? categorySlug = null, string? search = null)
    {
        var url = "listings?";
        if (!string.IsNullOrWhiteSpace(categorySlug)) url += $"category_slug={categorySlug}&";
        if (!string.IsNullOrWhiteSpace(search)) url += $"search={search}&";

        var res = await _api.GetAsync<ListingFeedResponse>(url);
        return res?.Listings ?? [];
    }

    public async Task<List<ApiListing>> GetSellerListingsAsync()
    {
        var sellerId = AppState.Instance.CurrentUserId;
        var url = sellerId > 0 ? $"seller/my-listings?seller_id={sellerId}" : "seller/my-listings";
        var res = await _api.GetAsync<ListingFeedResponse>(url);
        return res?.Listings ?? [];
    }

    public async Task<ApiListing?> CreateListingDraftAsync(string title, decimal price, int stock, int categoryId = 1, string condition = "Freshly Prepared / Baked", string? description = null, string? imagePath = null, string? pickupInstructions = null)
    {
        var res = await _api.PostAsync<object, CreateListingResponse>("listings", new
        {
            title,
            price,
            stock_quantity = stock,
            category_id = categoryId,
            item_condition = condition,
            description,
            image_path = imagePath,
            pickup_instructions = pickupInstructions,
            seller_id = AppState.Instance.CurrentUserId > 0 ? AppState.Instance.CurrentUserId : (int?)null
        });
        return res?.Listing;
    }

    public async Task<PayMongoLinkResponse?> CreatePayMongoLinkAsync(int listingId)
    {
        return await _api.PostAsync<object, PayMongoLinkResponse>($"listings/{listingId}/paymongo-link", new { });
    }

    public async Task<PaymentCheckResponse?> CheckPaymentStatusAsync(int listingId, string sessionId)
    {
        return await _api.GetAsync<PaymentCheckResponse>($"listings/{listingId}/check-payment?session_id={sessionId}");
    }

    public async Task<bool> PayListingFeeAsync(int listingId, string referenceNumber = "", decimal amount = 25.00m)
    {
        var res = await _api.PostAsync<object, object>($"listings/{listingId}/pay-fee", new
        {
            reference_number = referenceNumber,
            amount
        });
        return res != null;
    }

    public async Task<bool> ApplySellerAsync(string? shopName = null, string? bio = null)
    {
        var res = await _api.PostAsync<object, object>("seller/apply", new
        {
            shop_name = shopName,
            bio
        });
        return res != null;
    }

    // --- CHAT & MESSAGING ---
    public async Task<List<ApiConversation>> GetConversationsAsync()
    {
        var uid = AppState.Instance.CurrentUserId;
        var url = uid > 0 ? $"conversations?user_id={uid}" : "conversations";
        var res = await _api.GetAsync<ConversationsResponse>(url);
        return res?.Conversations ?? [];
    }

    public async Task<ConversationDetailResponse?> GetMessagesAsync(int conversationId)
    {
        var uid = AppState.Instance.CurrentUserId;
        var url = uid > 0 ? $"conversations/{conversationId}/messages?user_id={uid}" : $"conversations/{conversationId}/messages";
        return await _api.GetAsync<ConversationDetailResponse>(url);
    }

    public async Task<ApiMessage?> SendTextMessageAsync(int conversationId, string body)
    {
        var uid = AppState.Instance.CurrentUserId;
        var res = await _api.PostAsync<object, SendMessageResponse>("messages", new
        {
            conversation_id = conversationId,
            sender_id = uid > 0 ? uid : 1,
            message_type = "text",
            body
        });
        return res?.Message;
    }

    public async Task<ApiMessage?> SendMeetupCardAsync(int conversationId, int locationId, DateTime scheduledTime, string notes)
    {
        var uid = AppState.Instance.CurrentUserId;
        var res = await _api.PostAsync<object, SendMessageResponse>("messages", new
        {
            conversation_id = conversationId,
            sender_id = uid > 0 ? uid : 1,
            message_type = "meetup_card",
            body = $"Campus Meetup Scheduled for {scheduledTime:dddd, MMMM d} at {scheduledTime:h:mm tt}",
            meetup = new
            {
                location_id = locationId,
                scheduled_datetime = scheduledTime.ToString("yyyy-MM-dd HH:mm:ss"),
                notes
            }
        });
        return res?.Message;
    }

    // --- NOTIFICATIONS ---
    public async Task<List<ApiNotification>> GetNotificationsAsync()
    {
        var res = await _api.GetAsync<NotificationsResponse>("notifications");
        return res?.Notifications ?? [];
    }

    public async Task MarkNotificationsReadAsync()
    {
        await _api.PostAsync<object, object>("notifications/mark-all-read", new { });
    }

    public async Task RegisterDeviceTokenAsync(string token, string type = "android")
    {
        await _api.PostAsync<object, object>("device-tokens", new
        {
            device_token = token,
            device_type = type
        });
    }
}

// --- DATA TRANSFER OBJECTS (DTOs) ---
public class AuthResponse
{
    public string Message { get; set; } = "";
    public string Token { get; set; } = "";
    public ApiUser? User { get; set; }
}

public class ApiUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "buyer";
    [JsonPropertyName("seller_shop_name")]
    public string? SellerShopName { get; set; }
    [JsonPropertyName("seller_bio")]
    public string? SellerBio { get; set; }
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
    [JsonPropertyName("preferred_meetup_area")]
    public string? PreferredMeetupArea { get; set; }
}

public class MeResponse
{
    public ApiUser? User { get; set; }
}

public class ListingFeedResponse
{
    public List<ApiListing> Listings { get; set; } = [];
}

public class CreateListingResponse
{
    public string Message { get; set; } = "";
    public ApiListing? Listing { get; set; }
}

public class ApiListing
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    [JsonPropertyName("stock_quantity")]
    public int StockQuantity { get; set; }
    public string Status { get; set; } = "pending_payment";
    [JsonPropertyName("item_condition")]
    public string ItemCondition { get; set; } = "";
    public string? Description { get; set; }
    [JsonPropertyName("pickup_instructions")]
    public string? PickupInstructions { get; set; }
    public ApiUser? Seller { get; set; }
    public ApiCategory? Category { get; set; }
    [JsonPropertyName("primary_image")]
    public ApiListingImage? PrimaryImage { get; set; }
}

public class ApiCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class ApiListingImage
{
    public int Id { get; set; }
    [JsonPropertyName("image_path")]
    public string ImagePath { get; set; } = "";
}

public class ConversationsResponse
{
    public List<ApiConversation> Conversations { get; set; } = [];
}

public class ApiConversation
{
    public int Id { get; set; }
    [JsonPropertyName("unread_count")]
    public int UnreadCount { get; set; }
    [JsonPropertyName("other_user")]
    public ApiUser? OtherUser { get; set; }
    [JsonPropertyName("latest_message")]
    public ApiMessage? LatestMessage { get; set; }
}

public class ConversationDetailResponse
{
    public ApiConversation? Conversation { get; set; }
    public List<ApiMessage> Messages { get; set; } = [];
}

public class SendMessageResponse
{
    public ApiMessage? Message { get; set; }
}

public class ApiMessage
{
    public int Id { get; set; }
    [JsonPropertyName("conversation_id")]
    public int ConversationId { get; set; }
    [JsonPropertyName("sender_id")]
    public int SenderId { get; set; }
    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = "text";
    public string Body { get; set; } = "";
    [JsonPropertyName("is_read")]
    public bool IsRead { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class PayMongoLinkResponse
{
    [JsonPropertyName("checkout_url")]
    public string CheckoutUrl { get; set; } = "";
    public decimal Amount { get; set; } = 25.00m;
    [JsonPropertyName("reference_number")]
    public string ReferenceNumber { get; set; } = "";
}

public class PaymentCheckResponse
{
    public bool Paid { get; set; }
    public string Status { get; set; } = "pending";
    public string Message { get; set; } = "";
}

public class NotificationsResponse
{
    public List<ApiNotification> Notifications { get; set; } = [];
}

public class ApiNotification
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Type { get; set; } = "";
    [JsonPropertyName("read_at")]
    public DateTime? ReadAt { get; set; }
}
