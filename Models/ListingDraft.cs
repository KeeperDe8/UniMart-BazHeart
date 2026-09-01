namespace FinalProject.Models;

public class SellingScheduleSlot
{
    public string Day { get; set; } = "Monday";
    public string TimeWindow { get; set; } = "10:00 AM – 2:00 PM";
    public string Location { get; set; } = "Main Building Lobby";
}

public class ListingDraft
{
    public string ImageSource { get; set; } = "";
    public List<string> AdditionalImages { get; set; } = [];
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; } = 0.00m;
    public int Quantity { get; set; } = 1;
    public string Category { get; set; } = "Food & Drinks";
    public string Condition { get; set; } = "Good Condition";
    public string Description { get; set; } = "";
    public List<SellingScheduleSlot> ScheduleSlots { get; set; } = [];
    public string MeetupLocation { get; set; } = "Main Building – Ground Floor Lobby";
    public string PickupInstructions { get; set; } = "Meet near the lobby benches";
    public int? BackendListingId { get; set; }
}

public class MarketplaceProduct : ListingDraft
{
    public string Status { get; set; } = "Active";
    public string Seller { get; set; } = "@matchabykai";
    public bool IsSaved { get; set; }
}
