namespace FinalProject.Controls;

public partial class ProductSkeletonView : ContentView
{
    bool isShimmering;

    public ProductSkeletonView()
    {
        InitializeComponent();
        Loaded += (_, _) => StartShimmer();
        Unloaded += (_, _) => isShimmering = false;
    }

    async void StartShimmer()
    {
        isShimmering = true;
        while (isShimmering && SkeletonGrid is not null)
        {
            try
            {
                await SkeletonGrid.FadeTo(0.45, 600, Easing.SinInOut);
                await SkeletonGrid.FadeTo(1.0, 600, Easing.SinInOut);
            }
            catch
            {
                break;
            }
        }
    }
}
