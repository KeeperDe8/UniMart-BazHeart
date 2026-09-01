using FinalProject.Controls;
using FinalProject.Models;
using FinalProject.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FinalProject;

public abstract class UniPage : ContentPage
{
    protected AppState State => AppState.Instance;
    protected Color Blue => (Color)Application.Current!.Resources["PrimaryBlue"];
    protected Color Text => (Color)Application.Current!.Resources["TextDark"];
    protected Color Muted => (Color)Application.Current!.Resources["TextMuted"];
    protected Color Line => (Color)Application.Current!.Resources["BorderLight"];
    protected Color CardColor => (Color)Application.Current!.Resources["CardBg"];
    protected UniPage() { SetDynamicResource(BackgroundColorProperty, "PageBg"); Shell.SetNavBarIsVisible(this, false); }
    protected async Task Go(string route) => await Shell.Current.GoToAsync(route);
    protected Border Card(View view, int padding=12) { var b=new Border { Stroke=Line, StrokeShape=new RoundRectangle { CornerRadius=16 }, Padding=padding, Content=view }; b.SetDynamicResource(Border.BackgroundColorProperty, "CardBg"); return b; }
    protected Label L(string t,double size=13,bool bold=false,Color? color=null) { var l=new Label { Text=t, FontSize=size, FontAttributes=bold?FontAttributes.Bold:FontAttributes.None }; if (color!=null) l.TextColor=color; else l.SetDynamicResource(Label.TextColorProperty, "TextDark"); return l; }
    protected Button Btn(string t,EventHandler h,bool outline=false) { var b=new Button { Text=t, HeightRequest=46, CornerRadius=13, FontAttributes=FontAttributes.Bold, BackgroundColor=outline?Colors.Transparent:Blue, TextColor=outline?Text:Colors.White, BorderColor=outline?Line:Colors.Transparent, BorderWidth=outline?1:0 }; b.Clicked+=h; return b; }
    protected Border Box(View v) { var b=new Border { Stroke=Line, StrokeShape=new RoundRectangle { CornerRadius=13 }, Padding=new Thickness(12,0), Content=v }; b.SetDynamicResource(Border.BackgroundColorProperty, "CardBg"); return b; }
    protected View Field(string label, View view) { var s=new VerticalStackLayout { Spacing=4 }; s.Add(L(label,12,true)); s.Add(Box(view)); return s; }
    protected static ColumnDefinitionCollection Cols(string x) { var c=new ColumnDefinitionCollection(); foreach(var p in x.Split(',')) c.Add(new ColumnDefinition { Width=p=="Auto"?GridLength.Auto:p=="*"?GridLength.Star:new GridLength(double.Parse(p)) }); return c; }
    protected static RowDefinitionCollection Rws(string x) { var r=new RowDefinitionCollection(); foreach(var p in x.Split(',')) r.Add(new RowDefinition { Height=p=="Auto"?GridLength.Auto:p=="*"?GridLength.Star:new GridLength(double.Parse(p)) }); return r; }
    protected Grid Header(string title) { var g=new Grid { Padding=new Thickness(16,10),ColumnDefinitions=Cols("42,*") };var b=Card(L("‹",30,false,Text),0);b.HeightRequest=40;b.WidthRequest=40;((Label)b.Content).HorizontalOptions=LayoutOptions.Center;((Label)b.Content).VerticalOptions=LayoutOptions.Center;var t=new TapGestureRecognizer();t.Tapped+=async(_,_)=>await Go("..");b.GestureRecognizers.Add(t);g.Add(b);var h=L(title,18,true);h.VerticalOptions=LayoutOptions.Center;h.Margin=new Thickness(10,0);g.Add(h,1);return g; }
    protected VerticalStackLayout Form(string title,string subtitle,int step) { var s=new VerticalStackLayout { Padding=new Thickness(16,0,16,24),Spacing=12 };s.Add(new ListingStepIndicator { CurrentStep=step });s.Add(L(title,19,true));s.Add(L(subtitle,13,false,Muted));return s; }
    protected void SetPage(string title,View body) { var root=new Grid { RowDefinitions=Rws("Auto,*") };root.Add(Header(title));Grid.SetRow(body,1);root.Add(body);Content=root; }
}

public class CreateListingPhotosPage : UniPage
{
    readonly List<string> selectedPhotos = [];
    readonly HorizontalStackLayout photosGallery = new() { Spacing = 10 };
    readonly Label countLabel;
    readonly Border addCard;

    public CreateListingPhotosPage()
    {
        var form = Form("Step 1: Product Photos (1 to 5)", "Upload 1 to 5 clear photos of your product from your device.", 1);
        
        countLabel = L("0 / 5 Photos Selected (Min 1, Max 5)", 12, true, Blue);
        form.Add(countLabel);

        var addLbl = L("📷\n+ Add Photo", 13, true, Blue);
        addLbl.HorizontalTextAlignment = TextAlignment.Center;
        addLbl.VerticalTextAlignment = TextAlignment.Center;
        
        addCard = Card(addLbl, 12);
        addCard.HeightRequest = 110;
        addCard.WidthRequest = 110;
        
        var tap = new TapGestureRecognizer();
        tap.Tapped += PickPhoto;
        addCard.GestureRecognizers.Add(tap);

        var scrollThumbs = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = photosGallery
        };
        form.Add(scrollThumbs);

        if (!string.IsNullOrWhiteSpace(State.Draft.ImageSource) && !selectedPhotos.Contains(State.Draft.ImageSource))
        {
            selectedPhotos.Add(State.Draft.ImageSource);
        }
        foreach (var img in State.Draft.AdditionalImages)
        {
            if (!selectedPhotos.Contains(img)) selectedPhotos.Add(img);
        }

        RefreshGallery();

        form.Add(L("Tap '+ Add Photo' to pick from phone storage. Maximum 5 photos.", 11, false, Muted));
        form.Add(Btn("Continue to Details →", async (_, _) =>
        {
            if (selectedPhotos.Count == 0)
            {
                selectedPhotos.Add("matcha.jpg"); // Fallback
            }
            State.Draft.ImageSource = selectedPhotos[0];
            State.Draft.AdditionalImages = selectedPhotos.Skip(1).ToList();
            await Go("create-listing/details");
        }));

        SetPage("Upload Photos", new ScrollView { Content = form });
    }

    void RefreshGallery()
    {
        photosGallery.Children.Clear();
        foreach (var photoPath in selectedPhotos.ToList())
        {
            var thumb = new Grid { HeightRequest = 110, WidthRequest = 110 };
            var img = new Image
            {
                Source = photoPath.StartsWith("http") || photoPath.EndsWith(".jpg") || photoPath.EndsWith(".png") 
                    ? ImageSource.FromFile(photoPath) 
                    : ImageSource.FromFile(photoPath),
                Aspect = Aspect.AspectFill
            };
            var frame = new Border { Stroke = Line, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 14 }, Content = img };
            thumb.Add(frame);

            var delBtn = new Border
            {
                BackgroundColor = Color.FromArgb("#EF4444"),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 11 },
                HeightRequest = 22,
                WidthRequest = 22,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(4)
            };
            delBtn.Content = L("✕", 10, true, Colors.White);
            ((Label)delBtn.Content).HorizontalOptions = LayoutOptions.Center;
            ((Label)delBtn.Content).VerticalOptions = LayoutOptions.Center;
            var delTap = new TapGestureRecognizer();
            delTap.Tapped += (_, _) =>
            {
                selectedPhotos.Remove(photoPath);
                RefreshGallery();
            };
            delBtn.GestureRecognizers.Add(delTap);
            thumb.Add(delBtn);

            photosGallery.Add(thumb);
        }

        if (selectedPhotos.Count < 5)
        {
            photosGallery.Add(addCard);
        }

        countLabel.Text = $"{selectedPhotos.Count} / 5 Photos Selected (Min 1, Max 5)";
    }

    async void PickPhoto(object? s, TappedEventArgs e)
    {
        if (selectedPhotos.Count >= 5)
        {
            if (Shell.Current != null) await Shell.Current.DisplayAlert("Photo Limit", "Maximum of 5 photos reached.", "OK");
            return;
        }

        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select Product Photo"
            });

            if (result != null && !selectedPhotos.Contains(result.FullPath))
            {
                selectedPhotos.Add(result.FullPath);
                RefreshGallery();
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Photo Picker", "Unable to open phone storage: " + ex.Message, "OK");
            }
        }
    }
}

public class CreateListingDetailsPage : UniPage
{
    readonly Entry name;
    readonly Entry price;
    readonly Entry quantity;
    readonly Picker category;
    readonly Picker condition;
    readonly Editor description;

    public CreateListingDetailsPage()
    {
        var d = State.Draft;
        name = new Entry { Text = d.ProductName, Placeholder = "e.g. Madoka Plush Doll, Baked Treats", TextColor = Text };
        price = new Entry { Text = d.Price > 0 ? d.Price.ToString("0.00") : "", Placeholder = "0.00", Keyboard = Keyboard.Numeric, TextColor = Text };
        quantity = new Entry { Text = d.Quantity > 0 ? d.Quantity.ToString() : "1", Placeholder = "1", Keyboard = Keyboard.Numeric, TextColor = Text };
        category = new Picker { ItemsSource = new List<string> { "Food & Drinks", "Handmade", "Clothes", "Accessories", "School Supplies", "Other" }, SelectedItem = d.Category, TextColor = Text };
        condition = new Picker { ItemsSource = new List<string> { "Freshly Prepared / Baked", "Brand New", "Like New", "Good Condition", "Made to Order" }, SelectedItem = d.Condition, TextColor = Text };
        description = new Editor { Text = d.Description, Placeholder = "Describe the item specifications, size, condition, or flavors...", TextColor = Text, HeightRequest = 90 };

        var f = Form("Step 2: Product Information", "Enter product name, pricing with decimals, and stock quantity.", 2);
        f.Add(Field("Product Name *", name));

        var r = new Grid { ColumnDefinitions = Cols("*,*"), ColumnSpacing = 10 };
        r.Add(Field("Price (₱) *", price));
        r.Add(Field("Quantity in Stock *", quantity), 1);
        f.Add(r);

        f.Add(Field("Category *", category));
        f.Add(Field("Item Condition / Status", condition));
        f.Add(Field("Description", description));

        var nav = new Grid { ColumnDefinitions = Cols("*,*"), ColumnSpacing = 10 };
        nav.Add(Btn("Back", async (_, _) => await Go(".."), true));
        nav.Add(Btn("Continue to Schedule →", Next), 1);
        f.Add(nav);

        SetPage("Product Details", new ScrollView { Content = f });
    }

    async void Next(object? s, EventArgs e)
    {
        var cleanPrice = price.Text?.Replace("₱", "").Trim();
        if (string.IsNullOrWhiteSpace(name.Text))
        {
            if (Shell.Current != null) await Shell.Current.DisplayAlert("Missing Name", "Please enter a product name.", "OK");
            return;
        }

        if (!decimal.TryParse(cleanPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p) &&
            !decimal.TryParse(cleanPrice, out p) || p <= 0)
        {
            if (Shell.Current != null) await Shell.Current.DisplayAlert("Invalid Price", "Please enter a valid price (e.g. 95.00 or 750.50).", "OK");
            return;
        }

        if (!int.TryParse(quantity.Text?.Trim(), out var q) || q < 1)
        {
            q = 1;
        }

        var d = State.Draft;
        d.ProductName = name.Text.Trim();
        d.Price = p;
        d.Quantity = q;
        d.Category = category.SelectedItem?.ToString() ?? "Food & Drinks";
        d.Condition = condition.SelectedItem?.ToString() ?? "Good Condition";
        d.Description = description.Text ?? "";

        await Go("create-listing/schedule");
    }
}

public class CreateListingSchedulePage : UniPage
{
    readonly VerticalStackLayout slots = new() { Spacing = 8 };
    readonly Picker day = new() { ItemsSource = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }, SelectedIndex = 0 };
    readonly Entry time;

    public CreateListingSchedulePage()
    {
        time = new Entry { Text = "10:00 AM – 2:00 PM", TextColor = Text };
        Render();
        var f = Form("Step 3: Selling Schedule", "When can students meet you on campus to pick up their orders?", 3);
        f.Add(slots);
        var add = new VerticalStackLayout { Spacing = 8 };
        add.Add(L("+ Add Campus Selling Slot", 12, true, Blue));
        var r = new Grid { ColumnDefinitions = Cols("*,*"), ColumnSpacing = 8 };
        r.Add(Box(day));
        r.Add(Box(time), 1);
        add.Add(r);
        add.Add(Btn("+ Add to Schedule", Add, true));
        f.Add(Card(add));
        var nav = new Grid { ColumnDefinitions = Cols("*,*"), ColumnSpacing = 10 };
        nav.Add(Btn("Back", async (_, _) => await Go(".."), true));
        nav.Add(Btn("Continue to Meetup →", async (_, _) => await Go("create-listing/meetup")), 1);
        f.Add(nav);
        SetPage("Selling Schedule", new ScrollView { Content = f });
    }

    void Render()
    {
        slots.Clear();
        foreach (var x in State.Draft.ScheduleSlots.ToList())
        {
            var txt = new VerticalStackLayout { Spacing = 2 };
            txt.Add(L(x.Day + "   " + x.TimeWindow, 13, true));
            txt.Add(L("Hotspot: " + x.Location, 11, false, Muted));
            var row = new Grid { ColumnDefinitions = Cols("*,Auto") };
            row.Add(txt);
            var remove = Btn("×", (_, _) => { State.Draft.ScheduleSlots.Remove(x); Render(); }, true);
            remove.HeightRequest = 36;
            remove.WidthRequest = 40;
            row.Add(remove, 1);
            slots.Add(Card(row));
        }
    }

    void Add(object? s, EventArgs e)
    {
        var d = day.SelectedItem?.ToString();
        if (!string.IsNullOrWhiteSpace(d) && !string.IsNullOrWhiteSpace(time.Text) && !State.Draft.ScheduleSlots.Any(x => x.Day == d && x.TimeWindow == time.Text))
        {
            State.Draft.ScheduleSlots.Add(new SellingScheduleSlot { Day = d, TimeWindow = time.Text, Location = "Main Building Lobby" });
            Render();
        }
    }
}

public class CreateListingMeetupPage : UniPage
{
    public CreateListingMeetupPage()
    {
        var d = State.Draft;
        var loc = new Picker { ItemsSource = new List<string> { "Main Building – Ground Floor Lobby", "Student Activity Center (SAC)", "Library Entrance", "Cafeteria" }, SelectedItem = d.MeetupLocation, TextColor = Text };
        var note = new Entry { Text = d.PickupInstructions, Placeholder = "e.g. Meet near the lobby benches", TextColor = Text };
        var f = Form("Step 4: Campus Meetup Spot", "Choose where on NU Lipa campus you will meet student buyers.", 4);
        f.Add(Field("Meetup Hotspot *", loc));
        f.Add(Field("Specific Pickup Instructions", note));
        f.Add(L("Help buyers easily spot you during class breaks", 11, false, Muted));
        f.Add(Card(L("ℹ Campus Policy Reminder:\nUniMart is an in-person campus marketplace. No couriers or external delivery fees needed.", 12, false, Color.FromArgb("#087A59"))));
        var nav = new Grid { ColumnDefinitions = Cols("*,*"), ColumnSpacing = 10 };
        nav.Add(Btn("Back", async (_, _) => await Go(".."), true));
        nav.Add(Btn("Review Listing →", async (_, _) =>
        {
            d.MeetupLocation = loc.SelectedItem?.ToString() ?? "Main Building – Ground Floor Lobby";
            d.PickupInstructions = note.Text ?? "";
            await Go("create-listing/review");
        }), 1);
        f.Add(nav);
        SetPage("Meetup Location", new ScrollView { Content = f });
    }
}

public class CreateListingReviewPage : UniPage
{
    bool isProcessing = false;

    public CreateListingReviewPage()
    {
        var d = State.Draft;
        var p = new Grid { ColumnDefinitions = Cols("80,*") };
        var previewImg = new Image { Source = string.IsNullOrWhiteSpace(d.ImageSource) ? "matcha.jpg" : d.ImageSource, Aspect = Aspect.AspectFill, HeightRequest = 80, WidthRequest = 70 };
        p.Add(previewImg);
        var info = new VerticalStackLayout { Spacing = 3 };
        info.Add(L(d.Category.ToUpper(), 10, true, Blue));
        info.Add(L(d.ProductName, 16, true));
        info.Add(L($"₱{d.Price:0.00}", 15, true, Blue));
        info.Add(L($"{d.Quantity} in stock • {d.Condition}\nMeetup: {d.MeetupLocation}\nSchedule: {string.Join(", ", d.ScheduleSlots.Select(x => $"{x.Day} ({x.TimeWindow})"))}", 10, false, Muted));
        p.Add(info, 1);
        var f = Form("Step 5: Review Your Listing", "Check all product details before paying the listing fee.", 5);
        f.Add(Card(p));
        f.Add(Card(L("Mandatory Listing Fee\n₱25.00\nA small listing fee is required for every item on UniMart. Instant activation upon payment.", 12, false, Muted)));
        var nav = new Grid { ColumnDefinitions = Cols("*,*"), ColumnSpacing = 10 };
        nav.Add(Btn("Edit Details", async (_, _) => await Go("create-listing/details"), true));
        nav.Add(Btn("Proceed to Pay →", ProceedToPay), 1);
        f.Add(nav);
        SetPage("Review & Pay Fee", new ScrollView { Content = f });
    }

    async void ProceedToPay(object? s, EventArgs e)
    {
        if (isProcessing) return;
        isProcessing = true;
        var d = State.Draft;

        // Correct category IDs from unimart_db
        int catId = d.Category switch
        {
            "Food & Drinks" => 1,
            "Handmade" => 2,
            "Clothes" => 3,
            "Accessories" => 4,
            "School Supplies" => 5,
            _ => 6
        };

        // Only create draft in DB if not already created
        if (d.BackendListingId == null)
        {
            try
            {
                var created = await FinalProject.Services.Api.CampusApiService.Instance.CreateListingDraftAsync(
                    d.ProductName, d.Price, d.Quantity, catId, d.Condition, d.Description, d.ImageSource, $"{d.MeetupLocation} • {d.PickupInstructions}");
                if (created != null) d.BackendListingId = created.Id;
            }
            catch { }
        }

        await Go("gcash-payment");
        isProcessing = false;
    }
}

public class GCashPaymentPage : UniPage
{
 public GCashPaymentPage()
 {
     var content = new VerticalStackLayout { Padding = 16, Spacing = 14 };
     content.Add(new ListingStepIndicator { CurrentStep = 5 });
     var banner = new VerticalStackLayout { BackgroundColor = Color.FromArgb("#193F9D"), Padding = 18, Spacing = 4 };
     banner.Add(L("Pay Here", 22, true, Colors.White));
     banner.Add(L("Official Campus Listing Fee Gateway", 11, false, Colors.White));
     content.Add(banner);
     var pay = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
     pay.Add(L("UNIMART — NU LIPA", 14, true, Blue));
     pay.Add(L("Listing Publication Fee (₱25.00)", 12, false, Muted));
     pay.Add(L("₱25.00", 28, true, Blue));
     pay.Add(L("Pay via GCash, Maya, or Card", 11, false, Muted));
     var payBtn = Btn("Pay Here (₱25.00)", async (_, _) => await Go("paymongo-checkout"));
     payBtn.HeightRequest = 52;
     payBtn.FontSize = 15;
     pay.Add(payBtn);
     content.Add(Card(pay));
     content.Add(Card(L("In-App Secure Checkout:\nTap 'Pay Here' to open the secure payment sheet. You will remain inside the app and your listing will be published immediately upon completion.", 12, false, Muted)));
     SetPage("Pay Here", new ScrollView { Content = content });
 }
}

public class PayMongoCheckoutPage : UniPage
{
 readonly WebView webView = new();
 readonly ActivityIndicator loading = new() { IsRunning = true, Color = Color.FromArgb("#2456D8"), VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
 bool hasHandledPaymentSuccess = false;

 public PayMongoCheckoutPage()
 {
     var top = new Grid { Padding = new Thickness(12, 8), ColumnDefinitions = Cols("Auto,*,Auto"), BackgroundColor = (Color)Application.Current!.Resources["CardBg"] };
     var back = Btn("‹ Cancel", async (_, _) => await Go(".."), true);
     back.HeightRequest = 34;
     back.Padding = new Thickness(10, 0);
     top.Add(back);

     var title = L("PayMongo Checkout", 14, true);
     title.VerticalOptions = LayoutOptions.Center;
     title.HorizontalOptions = LayoutOptions.Center;
     top.Add(title, 1);

     webView.Navigating += OnNav;
     webView.Navigated += (_, _) => loading.IsRunning = false;
     var body = new Grid();
     body.Add(webView);
     body.Add(loading);
     var root = new Grid { RowDefinitions = Rws("Auto,*") };
     root.Add(top);
     Grid.SetRow(body, 1);
     root.Add(body);
     Content = root;
 }

 protected override async void OnAppearing()
 {
     base.OnAppearing();
     hasHandledPaymentSuccess = false;
     loading.IsRunning = true;
     var d = State.Draft;
     int targetId = d.BackendListingId ?? 1;

     try
     {
         var res = await FinalProject.Services.Api.CampusApiService.Instance.CreatePayMongoLinkAsync(targetId);
         if (res != null && !string.IsNullOrWhiteSpace(res.CheckoutUrl))
         {
             State.PaymentReference = res.ReferenceNumber;
             webView.Source = res.CheckoutUrl;
             return;
         }
     }
     catch { }

     webView.Source = "https://paymongo.com";
 }

 async Task CompletePaymentFlowAsync()
 {
     if (hasHandledPaymentSuccess) return;
     hasHandledPaymentSuccess = true;
     loading.IsRunning = true;
     int targetId = State.Draft.BackendListingId ?? 1;

     try
     {
         await FinalProject.Services.Api.CampusApiService.Instance.PayListingFeeAsync(targetId, State.PaymentReference);
     }
     catch { }

     State.PublishDraft();
     State.ResetDraft();
     await Go("payment-success");
 }

 async void OnNav(object? s, WebNavigatingEventArgs e)
 {
     if (hasHandledPaymentSuccess)
     {
         e.Cancel = true;
         return;
     }

     if (e.Url.Contains("payment-return") || e.Url.Contains("status=success") || e.Url.StartsWith("unimart://"))
     {
         e.Cancel = true;
         await CompletePaymentFlowAsync();
     }
 }
}

public class PaymentSuccessPage : UniPage
{
 public PaymentSuccessPage()
 {
     var s = new VerticalStackLayout { Padding = 16, Spacing = 16 };
     var banner = Card(L("✓ Payment Verified", 14, true, Colors.White));
     banner.BackgroundColor = Color.FromArgb("#087A59");
     s.Add(banner);
     s.Add(new ListingStepIndicator { CurrentStep = 6 });
     var check = L("✓", 54, false, Color.FromArgb("#10B981"));
     check.HorizontalOptions = LayoutOptions.Center;
     check.Margin = new Thickness(0, 45, 0, 0);
     s.Add(check);
     var title = L("Payment Successful", 19, true);
     title.HorizontalOptions = LayoutOptions.Center;
     s.Add(title);
     var message = L($"Your listing fee was confirmed. \"{State.CurrentProduct?.ProductName}\" is now live on the NU Lipa campus marketplace!", 13, false, Muted);
     message.HorizontalTextAlignment = TextAlignment.Center;
     s.Add(message);
     s.Add(Card(L($"Reference No:  {State.PaymentReference}\nAmount Paid:  ₱25.00\nPayment Channel:  Online Payment (PayMongo)\nStatus:  Active & Published in Database", 12, false, Blue)));
     s.Add(Btn("View My Listings", async (_, _) => await Shell.Current.GoToAsync("//listings")));
     Content = s;
 }
}

public class ProductDetailPage : UniPage
{
    public ProductDetailPage()
    {
        var p = State.CurrentProduct ?? new MarketplaceProduct
        {
            ProductName = "Iced Strawberry Matcha Latte",
            Price = 95,
            Quantity = 5,
            ImageSource = "matcha.jpg",
            Category = "Food & Drinks",
            Condition = "Freshly Prepared",
            Seller = "@matchabykai",
            Description = "Signature iced matcha layered with authentic Japanese Uji matcha and house-made strawberry puree with creamy fresh milk."
        };

        var currentUserName = State.CurrentUserName?.Trim() ?? "";
        var currentHandle = string.IsNullOrWhiteSpace(currentUserName) ? "" : "@" + currentUserName.ToLower().Replace(" ", "");

        bool isOwner = false;
        if (p.SellerId > 0 && State.CurrentUserId > 0)
        {
            isOwner = p.SellerId == State.CurrentUserId;
        }
        else if (!string.IsNullOrWhiteSpace(currentUserName))
        {
            isOwner = p.Seller.Equals(currentHandle, StringComparison.OrdinalIgnoreCase) 
                   || p.Seller.Equals(currentUserName, StringComparison.OrdinalIgnoreCase)
                   || p.Seller.Equals($"@{currentUserName}", StringComparison.OrdinalIgnoreCase);
        }

        // Top Action Header (Back & Share/Edit)
        var top = new Grid { Padding = new Thickness(16, 8), ColumnDefinitions = Cols("Auto,*,Auto"), BackgroundColor = Colors.Transparent };
        var backBtn = Card(L("‹", 26, false, Text), 0);
        backBtn.HeightRequest = 38; backBtn.WidthRequest = 38;
        ((Label)backBtn.Content).HorizontalOptions = LayoutOptions.Center;
        ((Label)backBtn.Content).VerticalOptions = LayoutOptions.Center;
        var backTap = new TapGestureRecognizer();
        backTap.Tapped += async (_, _) => await Go("..");
        backBtn.GestureRecognizers.Add(backTap);
        top.Add(backBtn);

        var topActions = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.End };
        if (isOwner)
        {
            var editTopBtn = Card(L("✏", 16, false, Text), 0);
            editTopBtn.HeightRequest = 38; editTopBtn.WidthRequest = 38;
            ((Label)editTopBtn.Content).HorizontalOptions = LayoutOptions.Center;
            ((Label)editTopBtn.Content).VerticalOptions = LayoutOptions.Center;
            var editTap = new TapGestureRecognizer();
            editTap.Tapped += async (_, _) => await Go("edit-listing");
            editTopBtn.GestureRecognizers.Add(editTap);
            topActions.Add(editTopBtn);
        }

        var shareBtn = Card(L("⤤", 18, false, Text), 0);
        shareBtn.HeightRequest = 38; shareBtn.WidthRequest = 38;
        ((Label)shareBtn.Content).HorizontalOptions = LayoutOptions.Center;
        ((Label)shareBtn.Content).VerticalOptions = LayoutOptions.Center;
        var shareTap = new TapGestureRecognizer();
        shareTap.Tapped += async (_, _) => { if (Shell.Current != null) await Shell.Current.DisplayAlert("Share", $"Share link to {p.ProductName} on campus!", "OK"); };
        shareBtn.GestureRecognizers.Add(shareTap);
        topActions.Add(shareBtn);
        top.Add(topActions, 2);

        // Body Scroll Content
        var scrollContent = new VerticalStackLayout { Spacing = 14, Padding = new Thickness(16, 0, 16, 20) };

        // 1. Hero Image Card
        var heroImage = new Image { Source = p.ImageSource, Aspect = Aspect.AspectFill, HeightRequest = 250 };
        var imageCard = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 22 }, Content = heroImage };
        scrollContent.Add(imageCard);

        // 2. Thumbnails Row (All images up to 5)
        var allImages = new List<string> { p.ImageSource };
        foreach (var img in p.AdditionalImages) { if (!allImages.Contains(img)) allImages.Add(img); }
        if (allImages.Count == 1)
        {
            if (p.ProductName.Contains("Matcha")) allImages.Add("brownies.jpg");
            else if (p.ProductName.Contains("Madoka")) allImages.Add("homura.jpg");
        }

        var thumbs = new HorizontalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
        foreach (var thumbSrc in allImages.Take(5))
        {
            var thumbImg = new Image { Source = thumbSrc, Aspect = Aspect.AspectFill, HeightRequest = 46, WidthRequest = 46 };
            var thumbBorder = new Border { Stroke = Line, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 12 }, Content = thumbImg };
            var selectThumbTap = new TapGestureRecognizer();
            selectThumbTap.Tapped += (_, _) => heroImage.Source = thumbSrc;
            thumbBorder.GestureRecognizers.Add(selectThumbTap);
            thumbs.Add(thumbBorder);
        }
        scrollContent.Add(thumbs);

        // 3. Title, Seller & Rating
        var titleRow = new Grid { ColumnDefinitions = Cols("*,Auto") };
        var titleCol = new VerticalStackLayout { Spacing = 2 };
        titleCol.Add(L(p.ProductName, 18, true));
        titleCol.Add(L($"By {p.Seller} • {p.Category}", 11, false, Muted));
        titleRow.Add(titleCol);

        var ratingBadge = new Border { BackgroundColor = (Color)Application.Current!.Resources["InputBackground"], StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(8, 4), VerticalOptions = LayoutOptions.Start, Content = L("★ 4.9", 11, true, (Color)Application.Current!.Resources["AccentGold"]) };
        titleRow.Add(ratingBadge, 1);
        scrollContent.Add(titleRow);

        // 4. Condition & Meetup Chips
        var chips = new HorizontalStackLayout { Spacing = 8 };
        chips.Add(new Border { BackgroundColor = (Color)Application.Current!.Resources["LightGreen"], StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 8 }, Padding = new Thickness(8, 3), Content = L($"{p.Quantity} in Stock", 10, true, (Color)Application.Current!.Resources["SuccessGreen"]) });
        chips.Add(new Border { BackgroundColor = (Color)Application.Current!.Resources["InputBackground"], StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 8 }, Padding = new Thickness(8, 3), Content = L(p.Condition, 10, false, Muted) });
        if (isOwner)
        {
            chips.Add(new Border { BackgroundColor = Color.FromArgb("#1E3A8A"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 8 }, Padding = new Thickness(8, 3), Content = L("Your Product Listing", 10, true, Color.FromArgb("#93C5FD")) });
        }
        else
        {
            chips.Add(new Border { BackgroundColor = (Color)Application.Current!.Resources["InputBackground"], StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 8 }, Padding = new Thickness(8, 3), Content = L("No Courier Fee", 10, false, Muted) });
        }
        scrollContent.Add(chips);

        // 5. Segmented Detail Tabs (About, Schedule, Meetup Spot)
        var descCard = L(string.IsNullOrWhiteSpace(p.Description) ? "Campus student listing. Local meetups inside campus." : p.Description, 13, false, Muted);
        var schedCard = L("Selling Schedule:\n• Monday: 10:00 AM – 2:00 PM\n• Wednesday: 11:00 AM – 3:00 PM\n• Friday: 1:00 PM – 4:30 PM", 12, false, Muted);
        schedCard.IsVisible = false;
        var meetupCard = L($"Pickup Location:\n{p.MeetupLocation ?? "Main Building – Ground Floor Lobby"}\n\nInstructions: {p.PickupInstructions ?? "Look for the seller with the BazHeart tote bag near the main lobby."}", 12, false, Muted);
        meetupCard.IsVisible = false;

        var tabAbout = new Button { Text = "About", BackgroundColor = Blue, TextColor = Colors.White, HeightRequest = 32, CornerRadius = 16, FontSize = 11, FontAttributes = FontAttributes.Bold, Padding = new Thickness(14, 0) };
        var tabSched = new Button { Text = "Schedule", BackgroundColor = Colors.Transparent, TextColor = Muted, HeightRequest = 32, CornerRadius = 16, FontSize = 11, Padding = new Thickness(12, 0) };
        var tabMeet = new Button { Text = "Meetup Spot", BackgroundColor = Colors.Transparent, TextColor = Muted, HeightRequest = 32, CornerRadius = 16, FontSize = 11, Padding = new Thickness(12, 0) };

        void SetTab(Button active)
        {
            foreach (var b in new[] { tabAbout, tabSched, tabMeet })
            {
                b.BackgroundColor = b == active ? Blue : Colors.Transparent;
                b.TextColor = b == active ? Colors.White : Muted;
                b.FontAttributes = b == active ? FontAttributes.Bold : FontAttributes.None;
            }
            descCard.IsVisible = active == tabAbout;
            schedCard.IsVisible = active == tabSched;
            meetupCard.IsVisible = active == tabMeet;
        }

        tabAbout.Clicked += (_, _) => SetTab(tabAbout);
        tabSched.Clicked += (_, _) => SetTab(tabSched);
        tabMeet.Clicked += (_, _) => SetTab(tabMeet);

        var tabs = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 4, 0, 0) };
        tabs.Add(tabAbout);
        tabs.Add(tabSched);
        tabs.Add(tabMeet);
        scrollContent.Add(tabs);

        // 6. Description Text & Info Panels
        scrollContent.Add(descCard);
        scrollContent.Add(schedCard);
        scrollContent.Add(meetupCard);

        // 7. Sticky Bottom Floating Action Bar
        var bottomBar = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["CardBg"],
            Stroke = Line,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = new Thickness(16, 10),
            Margin = new Thickness(16, 0, 16, 12),
            Shadow = new Shadow { Brush = Color.FromArgb("#000000"), Offset = new Point(0, 4), Radius = 12, Opacity = 0.15f }
        };

        var bottomGrid = new Grid { ColumnDefinitions = Cols("Auto,*,Auto"), ColumnSpacing = 12 };

        // Heart Wishlist Button
        var heartBtn = new Border { BackgroundColor = (Color)Application.Current!.Resources["InputBackground"], StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 18 }, HeightRequest = 44, WidthRequest = 44 };
        var heartLbl = L("♡", 20, false, Muted);
        heartLbl.HorizontalOptions = LayoutOptions.Center; heartLbl.VerticalOptions = LayoutOptions.Center;
        heartBtn.Content = heartLbl;
        var heartTap = new TapGestureRecognizer();
        heartTap.Tapped += async (_, _) =>
        {
            await heartBtn.ScaleToAsync(1.35, 90, Easing.SpringOut);
            AppState.Instance.ToggleSaved(p);
            heartLbl.Text = p.IsSaved ? "♥" : "♡";
            heartLbl.TextColor = p.IsSaved ? Color.FromArgb("#F43F5E") : Muted;
            await heartBtn.ScaleToAsync(1.0, 90, Easing.SpringIn);
        };
        heartBtn.GestureRecognizers.Add(heartTap);
        bottomGrid.Add(heartBtn);

        // Price Display
        var priceStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 0 };
        priceStack.Add(L("Total Price", 10, false, Muted));
        priceStack.Add(L($"₱{p.Price:0.00}", 18, true, Blue));
        bottomGrid.Add(priceStack, 1);

        // Action Button: Message Seller (if buyer) or Edit Listing & Stock (if owner)
        Button actionBtn;
        if (isOwner)
        {
            actionBtn = Btn("✏ Edit Listing & Stock", async (_, _) => await Go("edit-listing"));
        }
        else
        {
            actionBtn = Btn("Message Seller", async (_, _) =>
            {
                AppState.Instance.CurrentProduct = p;
                await Go("chat");
            });
        }
        actionBtn.HeightRequest = 46;
        actionBtn.Padding = new Thickness(16, 0);
        bottomGrid.Add(actionBtn, 2);

        bottomBar.Content = bottomGrid;

        var root = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        Grid.SetRow(top, 0);
        root.Add(top);

        var scrollView = new ScrollView { Content = scrollContent };
        Grid.SetRow(scrollView, 1);
        root.Add(scrollView);

        Grid.SetRow(bottomBar, 2);
        root.Add(bottomBar);

        Content = root;
    }
}

public class EditListingPage : UniPage
{
    int currentStock = 1;
    string selectedCategory = "Food & Drinks";
    string selectedCondition = "Good Condition";
    string selectedDay = "Monday to Friday";
    int startHour = 10;
    string startPeriod = "AM";
    int endHour = 3;
    string endPeriod = "PM";
    string selectedLocation = "Main Building – Ground Floor Lobby";

    readonly Entry nameEntry;
    readonly Entry priceEntry;
    readonly Editor descEditor;
    readonly Editor instructionsEditor;
    readonly Label stockCountLabel;
    readonly Label timePreviewLabel;
    readonly List<string> photos = [];
    readonly HorizontalStackLayout photosRow = new() { Spacing = 10 };
    readonly Border addPhotoBtn;
    readonly Label photosCountLabel;

    public EditListingPage()
    {
        var p = State.CurrentProduct ?? new MarketplaceProduct();
        currentStock = p.Quantity;
        selectedCategory = p.Category ?? "Food & Drinks";
        selectedCondition = p.Condition ?? "Good Condition";
        selectedLocation = p.MeetupLocation ?? "Main Building – Ground Floor Lobby";

        nameEntry = new Entry { Text = p.ProductName, TextColor = Text };
        priceEntry = new Entry { Text = p.Price.ToString("0.00"), Keyboard = Keyboard.Numeric, TextColor = Text };
        descEditor = new Editor { Text = p.Description, HeightRequest = 75, TextColor = Text };
        instructionsEditor = new Editor { Text = p.PickupInstructions ?? "Meet near the lobby benches or seller booth.", HeightRequest = 60, TextColor = Text };

        if (!string.IsNullOrWhiteSpace(p.ImageSource) && !photos.Contains(p.ImageSource)) photos.Add(p.ImageSource);
        foreach (var img in p.AdditionalImages) { if (!photos.Contains(img)) photos.Add(img); }

        var addLbl = L("📷\n+ Add", 11, true, Blue);
        addLbl.HorizontalTextAlignment = TextAlignment.Center;
        addLbl.VerticalTextAlignment = TextAlignment.Center;
        addPhotoBtn = Card(addLbl, 6);
        addPhotoBtn.HeightRequest = 75;
        addPhotoBtn.WidthRequest = 75;
        var addPhotoTap = new TapGestureRecognizer();
        addPhotoTap.Tapped += PickPhoto;
        addPhotoBtn.GestureRecognizers.Add(addPhotoTap);

        photosCountLabel = L("Photos (1 to 5)", 12, true, Blue);

        var form = new VerticalStackLayout { Padding = new Thickness(16, 4, 16, 20), Spacing = 14 };

        // ================= 1. PHOTOS & BASIC INFO CARD =================
        var detailsStack = new VerticalStackLayout { Spacing = 10 };
        detailsStack.Add(photosCountLabel);
        detailsStack.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = photosRow });
        RefreshPhotos();

        detailsStack.Add(Field("Product Name / Title *", nameEntry));
        detailsStack.Add(Field("Price (₱) *", priceEntry));

        // Category Chips
        detailsStack.Add(L("Category", 12, true));
        var catChips = new HorizontalStackLayout { Spacing = 8 };
        var categories = new[] { "Food & Drinks", "Handmade", "Clothes", "Accessories", "Other" };
        foreach (var cat in categories)
        {
            var btn = new Button
            {
                Text = cat,
                HeightRequest = 32,
                CornerRadius = 16,
                FontSize = 11,
                Padding = new Thickness(12, 0),
                BackgroundColor = cat == selectedCategory ? Blue : (Color)Application.Current!.Resources["InputBackground"],
                TextColor = cat == selectedCategory ? Colors.White : Text
            };
            btn.Clicked += async (_, _) =>
            {
                await btn.ScaleToAsync(0.92, 50, Easing.CubicOut);
                await btn.ScaleToAsync(1.0, 60, Easing.CubicIn);
                selectedCategory = cat;
                foreach (var child in catChips.Children.OfType<Button>())
                {
                    child.BackgroundColor = child.Text == selectedCategory ? Blue : (Color)Application.Current!.Resources["InputBackground"];
                    child.TextColor = child.Text == selectedCategory ? Colors.White : Text;
                }
            };
            catChips.Add(btn);
        }
        detailsStack.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = catChips });

        // Condition Chips
        detailsStack.Add(L("Item Condition", 12, true));
        var condChips = new HorizontalStackLayout { Spacing = 8 };
        var conditions = new[] { "Brand New", "Like New", "Good Condition", "Freshly Prepared / Baked", "Made to Order" };
        foreach (var cond in conditions)
        {
            var btn = new Button
            {
                Text = cond,
                HeightRequest = 32,
                CornerRadius = 16,
                FontSize = 11,
                Padding = new Thickness(12, 0),
                BackgroundColor = cond == selectedCondition ? Blue : (Color)Application.Current!.Resources["InputBackground"],
                TextColor = cond == selectedCondition ? Colors.White : Text
            };
            btn.Clicked += async (_, _) =>
            {
                await btn.ScaleToAsync(0.92, 50, Easing.CubicOut);
                await btn.ScaleToAsync(1.0, 60, Easing.CubicIn);
                selectedCondition = cond;
                foreach (var child in condChips.Children.OfType<Button>())
                {
                    child.BackgroundColor = child.Text == selectedCondition ? Blue : (Color)Application.Current!.Resources["InputBackground"];
                    child.TextColor = child.Text == selectedCondition ? Colors.White : Text;
                }
            };
            condChips.Add(btn);
        }
        detailsStack.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = condChips });
        detailsStack.Add(Field("Description", descEditor));

        form.Add(Card(detailsStack, 14));

        // ================= 2. STOCK & INVENTORY CARD =================
        stockCountLabel = L(currentStock.ToString(), 30, true, Blue);
        stockCountLabel.HorizontalOptions = LayoutOptions.Center;

        var minusBtn = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["InputBackground"],
            Stroke = Line,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            HeightRequest = 44,
            WidthRequest = 44
        };
        var minusLbl = L("−", 24, true, Text);
        minusLbl.HorizontalOptions = LayoutOptions.Center; minusLbl.VerticalOptions = LayoutOptions.Center;
        minusBtn.Content = minusLbl;
        var minusTap = new TapGestureRecognizer();
        minusTap.Tapped += async (_, _) =>
        {
            await minusBtn.ScaleToAsync(0.85, 60, Easing.CubicOut);
            if (currentStock > 0) currentStock--;
            stockCountLabel.Text = currentStock.ToString();
            await minusBtn.ScaleToAsync(1.0, 70, Easing.CubicIn);
        };
        minusBtn.GestureRecognizers.Add(minusTap);

        var plusBtn = new Border
        {
            BackgroundColor = Blue,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            HeightRequest = 44,
            WidthRequest = 44,
            Shadow = new Shadow { Brush = Color.FromArgb("#2563EB"), Offset = new Point(0, 3), Radius = 8, Opacity = 0.3f }
        };
        var plusLbl = L("+", 22, true, Colors.White);
        plusLbl.HorizontalOptions = LayoutOptions.Center; plusLbl.VerticalOptions = LayoutOptions.Center;
        plusBtn.Content = plusLbl;
        var plusTap = new TapGestureRecognizer();
        plusTap.Tapped += async (_, _) =>
        {
            await plusBtn.ScaleToAsync(0.85, 60, Easing.CubicOut);
            currentStock++;
            stockCountLabel.Text = currentStock.ToString();
            await plusBtn.ScaleToAsync(1.0, 70, Easing.CubicIn);
        };
        plusBtn.GestureRecognizers.Add(plusTap);

        var stepperRow = new Grid { ColumnDefinitions = Cols("Auto,*,Auto"), ColumnSpacing = 16, HorizontalOptions = LayoutOptions.Center, WidthRequest = 220 };
        stepperRow.Add(minusBtn, 0);
        
        var stockStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        stockStack.Add(stockCountLabel);
        stockStack.Add(L("Units Available", 11, false, Muted));
        stepperRow.Add(stockStack, 1);
        
        stepperRow.Add(plusBtn, 2);

        // Preset buttons
        var presets = new[] { 0, 3, 5, 10, 20 };
        var presetRow = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center };
        foreach (var qty in presets)
        {
            var pBtn = new Button
            {
                Text = qty == 0 ? "0 (Sold Out)" : $"+{qty}",
                HeightRequest = 30,
                CornerRadius = 15,
                FontSize = 10.5,
                BackgroundColor = (Color)Application.Current!.Resources["InputBackground"],
                TextColor = Text
            };
            pBtn.Clicked += async (_, _) =>
            {
                await pBtn.ScaleToAsync(0.9, 50, Easing.CubicOut);
                currentStock = qty == 0 ? 0 : currentStock + qty;
                stockCountLabel.Text = currentStock.ToString();
                await pBtn.ScaleToAsync(1.0, 60, Easing.CubicIn);
            };
            presetRow.Add(pBtn);
        }

        var stockCard = Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                L("📦 Stock & Inventory", 14, true),
                stepperRow,
                presetRow
            }
        }, 14);
        form.Add(stockCard);

        // ================= 3. SCHEDULE & MEETUP CARD =================
        var schedStack = new VerticalStackLayout { Spacing = 12 };
        schedStack.Add(L("📍 Campus Schedule & Meetup Spot", 14, true));

        // Selling Days Chips
        schedStack.Add(L("Selling Days", 12, true));
        var dayChips = new HorizontalStackLayout { Spacing = 8 };
        var daysList = new[] { "Monday to Friday", "Monday, Wednesday, Friday", "Tuesday, Thursday", "Everyday" };
        foreach (var d in daysList)
        {
            var btn = new Button
            {
                Text = d,
                HeightRequest = 32,
                CornerRadius = 16,
                FontSize = 11,
                Padding = new Thickness(12, 0),
                BackgroundColor = d == selectedDay ? Blue : (Color)Application.Current!.Resources["InputBackground"],
                TextColor = d == selectedDay ? Colors.White : Text
            };
            btn.Clicked += async (_, _) =>
            {
                await btn.ScaleToAsync(0.92, 50, Easing.CubicOut);
                await btn.ScaleToAsync(1.0, 60, Easing.CubicIn);
                selectedDay = d;
                foreach (var child in dayChips.Children.OfType<Button>())
                {
                    child.BackgroundColor = child.Text == selectedDay ? Blue : (Color)Application.Current!.Resources["InputBackground"];
                    child.TextColor = child.Text == selectedDay ? Colors.White : Text;
                }
            };
            dayChips.Add(btn);
        }
        schedStack.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = dayChips });

        // Time Range with AM/PM and Hour Steppers
        timePreviewLabel = L($"{startHour:00}:00 {startPeriod} – {endHour:00}:00 {endPeriod}", 13.5, true, Blue);
        timePreviewLabel.HorizontalOptions = LayoutOptions.Center;

        var startHourLabel = L($"{startHour:00}", 15, true);
        var endHourLabel = L($"{endHour:00}", 15, true);

        View CreateHourControl(string label, Label valLbl, Action<int> onDelta, Func<string> getPeriod, Action<string> setPeriod)
        {
            var minus = new Button { Text = "▼", HeightRequest = 28, WidthRequest = 28, CornerRadius = 14, Padding = 0, BackgroundColor = (Color)Application.Current!.Resources["InputBackground"], TextColor = Text };
            var plus = new Button { Text = "▲", HeightRequest = 28, WidthRequest = 28, CornerRadius = 14, Padding = 0, BackgroundColor = (Color)Application.Current!.Resources["InputBackground"], TextColor = Text };
            
            minus.Clicked += (_, _) => { onDelta(-1); UpdateTimePreview(); };
            plus.Clicked += (_, _) => { onDelta(1); UpdateTimePreview(); };

            var amBtn = new Button { Text = "AM", HeightRequest = 26, WidthRequest = 38, CornerRadius = 13, Padding = 0, FontSize = 9.5, FontAttributes = FontAttributes.Bold, BackgroundColor = getPeriod() == "AM" ? Blue : Colors.Transparent, TextColor = getPeriod() == "AM" ? Colors.White : Muted };
            var pmBtn = new Button { Text = "PM", HeightRequest = 26, WidthRequest = 38, CornerRadius = 13, Padding = 0, FontSize = 9.5, FontAttributes = FontAttributes.Bold, BackgroundColor = getPeriod() == "PM" ? Blue : Colors.Transparent, TextColor = getPeriod() == "PM" ? Colors.White : Muted };

            amBtn.Clicked += (_, _) => { setPeriod("AM"); amBtn.BackgroundColor = Blue; amBtn.TextColor = Colors.White; pmBtn.BackgroundColor = Colors.Transparent; pmBtn.TextColor = Muted; UpdateTimePreview(); };
            pmBtn.Clicked += (_, _) => { setPeriod("PM"); pmBtn.BackgroundColor = Blue; pmBtn.TextColor = Colors.White; amBtn.BackgroundColor = Colors.Transparent; amBtn.TextColor = Muted; UpdateTimePreview(); };

            var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };
            stack.Add(L(label, 10.5, true, Muted));
            
            var hourRow = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center };
            hourRow.Add(minus);
            valLbl.VerticalOptions = LayoutOptions.Center;
            hourRow.Add(valLbl);
            hourRow.Add(plus);
            stack.Add(hourRow);

            var periodRow = new HorizontalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };
            periodRow.Add(amBtn);
            periodRow.Add(pmBtn);
            stack.Add(periodRow);

            return Card(stack, 8);
        }

        void UpdateTimePreview()
        {
            startHourLabel.Text = $"{startHour:00}";
            endHourLabel.Text = $"{endHour:00}";
            timePreviewLabel.Text = $"{startHour:00}:00 {startPeriod} – {endHour:00}:00 {endPeriod}";
        }

        var startPicker = CreateHourControl("From (Start)", startHourLabel, delta => { startHour = Math.Clamp(startHour + delta, 1, 12); }, () => startPeriod, p => startPeriod = p);
        var endPicker = CreateHourControl("To (End)", endHourLabel, delta => { endHour = Math.Clamp(endHour + delta, 1, 12); }, () => endPeriod, p => endPeriod = p);

        var timeGrid = new Grid { ColumnDefinitions = Cols("*,*"), ColumnSpacing = 10 };
        timeGrid.Add(startPicker, 0);
        timeGrid.Add(endPicker, 1);

        schedStack.Add(L("Handover Time Window", 12, true));
        schedStack.Add(timeGrid);
        schedStack.Add(timePreviewLabel);

        // Location Chips
        schedStack.Add(L("Campus Pickup Location", 12, true));
        var locChips = new HorizontalStackLayout { Spacing = 8 };
        var locations = new[] { "Main Building – Ground Floor Lobby", "Student Activity Center (SAC)", "University Library Entrance", "Campus Plaza Benches" };
        foreach (var loc in locations)
        {
            var btn = new Button
            {
                Text = loc,
                HeightRequest = 32,
                CornerRadius = 16,
                FontSize = 11,
                Padding = new Thickness(12, 0),
                BackgroundColor = loc == selectedLocation ? Blue : (Color)Application.Current!.Resources["InputBackground"],
                TextColor = loc == selectedLocation ? Colors.White : Text
            };
            btn.Clicked += async (_, _) =>
            {
                await btn.ScaleToAsync(0.92, 50, Easing.CubicOut);
                await btn.ScaleToAsync(1.0, 60, Easing.CubicIn);
                selectedLocation = loc;
                foreach (var child in locChips.Children.OfType<Button>())
                {
                    child.BackgroundColor = child.Text == selectedLocation ? Blue : (Color)Application.Current!.Resources["InputBackground"];
                    child.TextColor = child.Text == selectedLocation ? Colors.White : Text;
                }
            };
            locChips.Add(btn);
        }
        schedStack.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = locChips });
        schedStack.Add(Field("Pickup Instructions", instructionsEditor));

        form.Add(Card(schedStack, 14));

        // ================= ROOT ASSEMBLE =================
        var saveBtn = Btn("Save Changes ✓", OnSaveChanges);
        saveBtn.HeightRequest = 48;
        saveBtn.Margin = new Thickness(16, 6, 16, 12);

        var root = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        Grid.SetRow(Header("Edit Listing & Stock"), 0);
        root.Add(Header("Edit Listing & Stock"));

        var scrollView = new ScrollView { Content = form };
        Grid.SetRow(scrollView, 1);
        root.Add(scrollView);

        Grid.SetRow(saveBtn, 2);
        root.Add(saveBtn);

        Content = root;
    }

    void RefreshPhotos()
    {
        photosRow.Children.Clear();
        foreach (var ph in photos.ToList())
        {
            var cell = new Grid { HeightRequest = 80, WidthRequest = 80 };
            var img = new Image
            {
                Source = ph.StartsWith("http") || ph.EndsWith(".jpg") || ph.EndsWith(".png") ? ImageSource.FromFile(ph) : ImageSource.FromFile(ph),
                Aspect = Aspect.AspectFill
            };
            cell.Add(new Border { Stroke = Line, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 14 }, Content = img });

            var delBtn = new Border
            {
                BackgroundColor = Color.FromArgb("#EF4444"),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                HeightRequest = 22,
                WidthRequest = 22,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = 3
            };
            delBtn.Content = L("✕", 9.5, true, Colors.White);
            ((Label)delBtn.Content).HorizontalOptions = LayoutOptions.Center;
            ((Label)delBtn.Content).VerticalOptions = LayoutOptions.Center;
            var delTap = new TapGestureRecognizer();
            delTap.Tapped += (_, _) => { photos.Remove(ph); RefreshPhotos(); };
            delBtn.GestureRecognizers.Add(delTap);
            cell.Add(delBtn);

            photosRow.Add(cell);
        }

        if (photos.Count < 5) photosRow.Add(addPhotoBtn);
        photosCountLabel.Text = $"Photos ({photos.Count}/5 Selected)";
    }

    async void PickPhoto(object? s, TappedEventArgs e)
    {
        if (photos.Count >= 5)
        {
            if (Shell.Current != null) await Shell.Current.DisplayAlert("Photo Limit", "Maximum of 5 photos allowed.", "OK");
            return;
        }

        try
        {
            var res = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Add Listing Photo" });
            if (res != null && !photos.Contains(res.FullPath))
            {
                photos.Add(res.FullPath);
                RefreshPhotos();
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null) await Shell.Current.DisplayAlert("Photo Picker", ex.Message, "OK");
        }
    }

    async void OnSaveChanges(object? s, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameEntry.Text))
        {
            await Shell.Current.DisplayAlert("Missing Field", "Please enter product name.", "OK");
            return;
        }

        if (!decimal.TryParse(priceEntry.Text, out decimal parsedPrice) || parsedPrice <= 0)
        {
            await Shell.Current.DisplayAlert("Invalid Price", "Please enter a valid price.", "OK");
            return;
        }

        var p = State.CurrentProduct;
        if (p != null)
        {
            p.ProductName = nameEntry.Text.Trim();
            p.Price = parsedPrice;
            p.Quantity = currentStock;
            p.Category = selectedCategory;
            p.Condition = selectedCondition;
            p.Description = descEditor.Text?.Trim() ?? "";
            p.MeetupLocation = selectedLocation;
            p.PickupInstructions = instructionsEditor.Text?.Trim() ?? "";
            if (photos.Count > 0)
            {
                p.ImageSource = photos[0];
                p.AdditionalImages = photos.Skip(1).ToList();
            }

            var existing = State.Listings.FirstOrDefault(x => x.ProductName.Equals(p.ProductName, StringComparison.OrdinalIgnoreCase) || (p.BackendListingId.HasValue && x.BackendListingId == p.BackendListingId));
            if (existing != null)
            {
                existing.ProductName = p.ProductName;
                existing.Price = p.Price;
                existing.Quantity = p.Quantity;
                existing.Category = p.Category;
                existing.Condition = p.Condition;
                existing.Description = p.Description;
                existing.ImageSource = p.ImageSource;
                existing.MeetupLocation = p.MeetupLocation;
            }
        }

        await Shell.Current.DisplayAlert("Success", "Listing and stock details updated successfully!", "OK");
        await Go("..");
    }
}

public class SellingSchedulePage : UniPage
{
 readonly VerticalStackLayout list=new(){Spacing=9};
 public SellingSchedulePage(){Render();var time=new Entry{Text="10:00 AM – 2:00 PM",TextColor=Text};var day=new Picker{ItemsSource=new List<string>{"Monday","Tuesday","Wednesday","Thursday","Friday"},SelectedIndex=0,TextColor=Text};var loc=new Picker{ItemsSource=new List<string>{"Main Building Lobby","Student Activity Center (SAC)","Library Entrance"},SelectedIndex=0,TextColor=Text};var f=new VerticalStackLayout{Padding=16,Spacing=12};f.Add(L("Campus Selling Hours",18,true));f.Add(L("Set your availability on campus so buyers know when you can hand over products.",12,false,Muted));f.Add(list);f.Add(Field("Day of Week",day));f.Add(Field("Time Window",time));f.Add(Field("Location on Campus",loc));f.Add(Btn("Add Schedule Slot",(_,_)=>{State.SellingSchedule.Add(new SellingScheduleSlot{Day=day.SelectedItem?.ToString()??"Monday",TimeWindow=time.Text??"",Location=loc.SelectedItem?.ToString()??"Main Building Lobby"});Render();}));SetPage("My Selling Schedule",new ScrollView{Content=f});}
 void Render(){list.Clear();foreach(var x in State.SellingSchedule.ToList()){var row=new Grid{ColumnDefinitions=Cols("*,Auto")};row.Add(L($"{x.Day}   {x.TimeWindow}\n📍 {x.Location}",13,true));row.Add(Btn("🗑",(_,_)=>{State.SellingSchedule.Remove(x);Render();},true),1);list.Add(Card(row));}}
}

public class PublicShopPage : UniPage
{
 public PublicShopPage(){var f=new VerticalStackLayout{Padding=16,Spacing=12};f.Add(L("Kai's Café & Matcha ✓",21,true));f.Add(L("@matchabykai • BS Business Administration • 3rd Year",11,false,Muted));f.Add(L("Freshly whisked Uji matcha drinks & homemade fudgy baked treats on campus! Always freshly prepared on selling days. ✨",13));f.Add(Card(L("7 Listings      ★ 4.9      ~5 mins Response",13,true,Blue)));foreach(var p in Products()){var card=Card(new Grid{ColumnDefinitions=Cols("90,*")});var g=(Grid)card.Content;g.Add(new Image{Source=p.ImageSource,Aspect=Aspect.AspectFill,HeightRequest=80,WidthRequest=80});g.Add(L($"{p.ProductName}\n₱{p.Price:0}\n5 in Stock",14,true),1);var t=new TapGestureRecognizer();t.Tapped+=async(_,_)=>{State.CurrentProduct=p;await Go("product");};card.GestureRecognizers.Add(t);f.Add(card);}SetPage("Seller Public Shop",new ScrollView{Content=f});}
 static IEnumerable<MarketplaceProduct> Products()=>new[]{new MarketplaceProduct{ProductName="Iced Strawberry Matcha Latte",Price=95,ImageSource="matcha.jpg",Quantity=5},new MarketplaceProduct{ProductName="Handmade Crochet Daisy Bouquet",Price=160,ImageSource="crochet_bouquet.jpg",Quantity=5}};
}

public class SavedItemsPage : UniPage
{
 public SavedItemsPage(){if(!State.SavedItems.Any()){State.SavedItems.Add(new MarketplaceProduct{ProductName="Iced Matcha Latte (16oz)",Price=85,ImageSource="matcha.jpg",Quantity=5,IsSaved=true,Seller="@matchabykai"});State.SavedItems.Add(new MarketplaceProduct{ProductName="Handmade Crochet Tulip Flower Bouquet",Price=150,ImageSource="crochet_bouquet.jpg",Quantity=5,IsSaved=true,Seller="@crochetbysam"});}var f=new VerticalStackLayout{Padding=16,Spacing=12};f.Add(L($"My Wishlist  •  {State.SavedItems.Count} saved",20,true));foreach(var p in State.SavedItems.ToList()){var card=Card(new Grid{ColumnDefinitions=Cols("100,*,Auto")});var g=(Grid)card.Content;g.Add(new Image{Source=p.ImageSource,Aspect=Aspect.AspectFill,HeightRequest=95,WidthRequest=90});g.Add(L($"Available Today\n{p.ProductName}\n₱{p.Price:0}\n{p.Seller}",13,true),1);g.Add(Btn("♥",(_,_)=>{State.SavedItems.Remove(p);Navigation.RemovePage(this);},true),2);f.Add(card);}SetPage("Saved Items",new ScrollView{Content=f});}
}

public class NotificationsPage : UniPage
{
 public NotificationsPage()
 {
     var f = new VerticalStackLayout { Padding = new Thickness(16, 10, 16, 20), Spacing = 10 };
     var head = new Grid { ColumnDefinitions = Cols("*,Auto") };
     head.Add(L("Notifications", 20, true));
     var mark = L("Mark all as read", 11, true, Blue);
     var markTap = new TapGestureRecognizer();
     markTap.Tapped += (_, _) =>
     {
         mark.Text = "All caught up ✓";
         mark.TextColor = (Color)Application.Current!.Resources["SuccessGreen"];
     };
     mark.GestureRecognizers.Add(markTap);
     head.Add(mark, 1);
     f.Add(head);

     f.Add(L("Recent Activity", 11, true, Muted));

     var notifications = new[]
     {
         ("Listing Published", "Your listing 'Iced Strawberry Matcha Latte' is active on campus feed.", "Just now", "//listings"),
         ("Payment Verified", "₱25.00 listing fee verified via PayMongo. Transaction active in database.", "10 mins ago", "//listings"),
         ("New Message from Kai", "Sure! I have 3 cups left today at the Main Building Lobby.", "25 mins ago", "chat"),
         ("Campus Meetup Reminder", "You have an upcoming meetup today at 2:00 PM at Main Building Lobby.", "1 hour ago", "chat")
     };

     foreach (var item in notifications)
     {
         var card = Card(new VerticalStackLayout
         {
             Spacing = 3,
             Children =
             {
                 new Grid
                 {
                     ColumnDefinitions = Cols("*,Auto"),
                     Children =
                     {
                         L(item.Item1, 13, true),
                         new Label { Text = item.Item3, FontSize = 10, TextColor = Muted, HorizontalOptions = LayoutOptions.End }
                     }
                 },
                 L(item.Item2, 12, false, Muted)
             }
         });

         var tap = new TapGestureRecognizer();
         tap.Tapped += async (_, _) => await Go(item.Item4);
         card.GestureRecognizers.Add(tap);
         f.Add(card);
     }

     SetPage("Notifications", new ScrollView { Content = f });
 }
}
