namespace FinalProject.Controls;
public partial class ListingStepIndicator : ContentView
{
 public static readonly BindableProperty CurrentStepProperty = BindableProperty.Create(nameof(CurrentStep), typeof(int), typeof(ListingStepIndicator), 1, propertyChanged: (bindable,_,_) => ((ListingStepIndicator)bindable).Render());
 public int CurrentStep { get => (int)GetValue(CurrentStepProperty); set => SetValue(CurrentStepProperty, value); }
 public ListingStepIndicator() { InitializeComponent(); Loaded += (_,_) => Render(); }
 void Render() { var nodes = new[] { S1,S2,S3,S4,S5 }; var lines = new[] { L1,L2,L3,L4 }; for(int i=0;i<nodes.Length;i++) { bool done=i+1<CurrentStep, current=i+1==CurrentStep; nodes[i].BackgroundColor=done?Color.FromArgb("#10B981"):current?Color.FromArgb("#2E65E8"):Color.FromArgb("#EDF2F8"); ((Label)nodes[i].Content).TextColor=done||current?Colors.White:Color.FromArgb("#9AA9BC"); if(i<4) lines[i].Color=done?Color.FromArgb("#10B981"):Color.FromArgb("#DDE6F1"); } }
}
