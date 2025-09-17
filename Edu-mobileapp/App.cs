namespace Edu_mobileapp;

public partial class App : Application
{
    public App()
    {
        // Login'i bir NavigationPage içinde açalım (üst barın rengi de kırmızı)
        MainPage = new NavigationPage(new Login())
        {
            BarBackgroundColor = Color.FromArgb("#E50914"),
            BarTextColor = Colors.White
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(MainPage);
}
