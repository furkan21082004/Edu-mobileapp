using System.Collections.ObjectModel;
using System.Windows.Input;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Edu_mobileapp;

public class Login : FmgLibContentPage
{
    // Tema renkleri
    private static readonly Color EdusamaRed = Color.FromArgb("#E50914");
    private static readonly Color LightGray = Color.FromArgb("#EAEAEA");

    // Basit binding alanlarý
    public ObservableCollection<string> Roles { get; } =
        new(new[] { "Yönetici", "Operasyon", "Muhasebe", "Destek" });

    public string SelectedRole { get; set; } = "Operasyon";
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;

    public ICommand LoginCommand { get; }

    public Login()
    {
        Title = "Giriþ";
        // 1) Kaydedilmiþ bilgileri önce yükle
        EmailOrUsername = Preferences.Get("remember_email", string.Empty);
        SelectedRole = Preferences.Get("remember_role", "Operasyon");
        BindingContext = this;
        this.InitializeHotReload(); // opsiyonel

        LoginCommand = new Command(async () => await OnLoginAsync());

        Build();
    }

    public override void Build()
    {
        this
        .BackgroundColor(Colors.White)
        .Content(
            new ScrollView()
            .Content(
                new VerticalStackLayout()
                .Padding(24)
                .Spacing(16)
                .Children(

                    // Üst baþlýk/bant
                    new Frame()
                        .Padding(new Thickness(16, 24))
                        .CornerRadius(16)
                        .BackgroundColor(EdusamaRed)
                        .Content(
                            new VerticalStackLayout()
                            .Spacing(6)
                            .Children(
                                new Label()
                                    .Text("Edusama AFS")
                                    .FontSize(28)
                                    .FontAttributes(FontAttributes.Bold)
                                    .TextColor(Colors.White)
                                    .HorizontalTextAlignment(TextAlignment.Center),

                                new Label()
                                    .Text("Hesabýnýza giriþ yapýn")
                                    .TextColor(Colors.White)
                                    .Opacity(0.9)
                                    .HorizontalTextAlignment(TextAlignment.Center)
                            )
                        ),

                    // Form: Rol
                    new Label().Text("Rol").FontSize(14),
                    new Picker()
                        .Title("Rol seçiniz")
                        .ItemsSource(e => e.Path(nameof(Roles)))
                        .SelectedItem(e => e.Path(nameof(SelectedRole)))
                        .BackgroundColor(LightGray)
                        .Margin(new Thickness(0, 0, 0, 6))
                        .HeightRequest(44),

                    // E-posta / Kullanýcý adý
                    new Label().Text("E-posta / Kullanýcý Adý").FontSize(14),
                    new Entry()
                        .Placeholder("ör. admin@edusama.com")
                        .Keyboard(Keyboard.Email)
                        .Text(e => e.Path(nameof(EmailOrUsername)))
                        .ClearButtonVisibility(ClearButtonVisibility.WhileEditing)
                        .BackgroundColor(LightGray)
                        .HeightRequest(44),

                    // Þifre
                    new Label().Text("Þifre").FontSize(14),
                    new Entry()
                        .Placeholder("Þifreniz")
                        .IsPassword(true)
                        .Text(e => e.Path(nameof(Password)))
                        .BackgroundColor(LightGray)
                        .HeightRequest(44)
                        .Margin(new Thickness(0, 0, 0, 4)),

                    // Beni hatýrla
                    new HorizontalStackLayout()
                        .Spacing(8)
                        .Children(
                            new CheckBox()
                                .IsChecked(e => e.Path(nameof(RememberMe)))
                                .Color(EdusamaRed),
                            new Label().Text("Beni hatýrla").VerticalTextAlignment(TextAlignment.Center)
                        ),

                    // Giriþ butonu (KIRMIZI)
                    new Button()
                        .Text("Giriþ Yap")
                        .FontAttributes(FontAttributes.Bold)
                        .CornerRadius(14)
                        .HeightRequest(50)
                        .BackgroundColor(EdusamaRed)
                        .TextColor(Colors.White)
                        .Command(e => e.Path(nameof(LoginCommand))),

                    // Alt bilgi
                    new Label()
                        .Text("© Edusama • 2025")
                        .FontSize(12)
                        .TextColor(Colors.Gray)
                        .HorizontalTextAlignment(TextAlignment.Center)
                        .Margin(new Thickness(0, 24, 0, 0))
                )
            )
        );
    }

   
        private async Task OnLoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Alan kontrolleri
            if (string.IsNullOrWhiteSpace(SelectedRole))
            {
                await DisplayAlert("Eksik bilgi", "Lütfen rol seçin.", "Tamam");
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailOrUsername))
            {
                await DisplayAlert("Eksik bilgi", "Lütfen e-posta / kullanýcý adý girin.", "Tamam");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 4)
            {
                await DisplayAlert("Eksik bilgi", "Lütfen þifrenizi girin (min. 4 karakter).", "Tamam");
                return;
            }

            // (Þimdilik) sahte doðrulama
            await Task.Delay(300);

            // Beni hatýrla
            try
            {
                if (RememberMe)
                {
                    Preferences.Set("remember_email", EmailOrUsername);
                    Preferences.Set("remember_role", SelectedRole);
                }
                else
                {
                    Preferences.Remove("remember_email");
                    Preferences.Remove("remember_role");
                }
            }
            catch { /* depolama hatalarýný görmezden gel */ }

            // Dashboard'a geçiþ
            if (Application.Current.MainPage is NavigationPage)
            {
                await Navigation.PushAsync(new Dashboard());           // geri tuþu Login'e döner
            }
            else
            {
                // NavigationPage yoksa kökü deðiþtir (geri dönülemez)
                Application.Current.MainPage = new NavigationPage(new Dashboard())
                {
                    BarBackgroundColor = Color.FromArgb("#E50914"),
                    BarTextColor = Colors.White
                };
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Giriþ sýrasýnda bir sorun oluþtu:\n{ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }


        if (IsBusy) return;
        try
        {
            IsBusy = true;

            // 1) Basit alan kontrolleri
            if (string.IsNullOrWhiteSpace(SelectedRole))
            { await DisplayAlert("Eksik bilgi", "Lütfen rol seçin.", "Tamam"); return; }

            if (string.IsNullOrWhiteSpace(EmailOrUsername))
            { await DisplayAlert("Eksik bilgi", "Lütfen e-posta / kullanýcý adý girin.", "Tamam"); return; }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 4)
            { await DisplayAlert("Eksik bilgi", "Lütfen þifrenizi girin (min. 4 karakter).", "Tamam"); return; }

            // 2) Örnek (lokal) doðrulama
            var ok =
                (EmailOrUsername == "admin@edusama.com" && Password == "1234" && SelectedRole == "Yönetici") ||
                (EmailOrUsername == "op@edusama.com" && Password == "1234" && SelectedRole == "Operasyon") ||
                (EmailOrUsername == "acc@edusama.com" && Password == "1234" && SelectedRole == "Muhasebe") ||
                (EmailOrUsername == "support@edusama.com" && Password == "1234" && SelectedRole == "Destek");

            if (!ok)
            {
                await DisplayAlert("Hatalý giriþ", "Bilgileri kontrol edin.", "Tamam");
                return;
            }

            // 3) Beni hatýrla
            try
            {
                if (RememberMe)
                {
                    Preferences.Set("remember_email", EmailOrUsername);
                    Preferences.Set("remember_role", SelectedRole);
                }
                else
                {
                    Preferences.Remove("remember_email");
                    Preferences.Remove("remember_role");
                }
            }
            catch { /* ignore */ }

            // 4) Dashboard'a geçiþ
            if (Application.Current.MainPage is NavigationPage)
                await Navigation.PushAsync(new Dashboard());
            else
                Application.Current.MainPage = new NavigationPage(new Dashboard())
                {
                    BarBackgroundColor = Color.FromArgb("#E50914"),
                    BarTextColor = Colors.White
                };
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
