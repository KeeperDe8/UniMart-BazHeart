namespace FinalProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("chat", typeof(ChatDetailPage));
            Routing.RegisterRoute("product", typeof(ProductDetailPage));
            Routing.RegisterRoute("create-listing", typeof(CreateListingPhotosPage));
            Routing.RegisterRoute("create-listing/photos", typeof(CreateListingPhotosPage));
            Routing.RegisterRoute("create-listing/details", typeof(CreateListingDetailsPage));
            Routing.RegisterRoute("create-listing/schedule", typeof(CreateListingSchedulePage));
            Routing.RegisterRoute("create-listing/meetup", typeof(CreateListingMeetupPage));
            Routing.RegisterRoute("create-listing/review", typeof(CreateListingReviewPage));
            Routing.RegisterRoute("gcash-payment", typeof(GCashPaymentPage));
            Routing.RegisterRoute("paymongo-checkout", typeof(PayMongoCheckoutPage));
            Routing.RegisterRoute("payment-success", typeof(PaymentSuccessPage));
            Routing.RegisterRoute("selling-schedule", typeof(SellingSchedulePage));
            Routing.RegisterRoute("public-shop", typeof(PublicShopPage));
            Routing.RegisterRoute("saved-items", typeof(SavedItemsPage));
            Routing.RegisterRoute("notifications", typeof(NotificationsPage));
        }
    }
}
