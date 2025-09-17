using System;
using System.Threading.Tasks;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Edu_mobileapp;

public class Dashboard : FmgLibContentPage
{
    private static readonly Color Red = Color.FromArgb("#E50914");
    private static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    public Dashboard()
    {
        Title = "Ana Sayfa";
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

                    // Kırmızı başlık bandı — tıklanınca QuickAccessPage açılır
                    new Frame()
                        .CornerRadius(16)
                        .Padding(new Thickness(16, 22))
                        .BackgroundColor(Red)
                        .Content(
                            new VerticalStackLayout()
                            .Children(
                                new Label()
                                    .Text("Edusama AFS")
                                    .FontSize(26)
                                    .FontAttributes(FontAttributes.Bold)
                                    .TextColor(Colors.White)
                                    .HorizontalTextAlignment(TextAlignment.Center),

                                new Label()
                                    .Text("Modüllere hızlı erişim")
                                    .TextColor(Colors.White)
                                    .Opacity(0.9)
                                    .HorizontalTextAlignment(TextAlignment.Center)
                            )
                        )
                        .GestureRecognizers(
                            new TapGestureRecognizer
                            {
                                Command = new Command(async () =>
                                    await Navigation.PushAsync(new QuickAccessPage()))
                            }
                        ),

                    // Modül butonları
                    RedButton("👤 Müşteri Yönetimi", async () => await Navigation.PushAsync(new CustomersPage())),
                    RedButton("🏠 Konaklama", async () => await Navigation.PushAsync(new AccommodationsPage())),
                    RedButton("🪑 Demirbaş", async () => await Navigation.PushAsync(new AssetsPage())),
                    RedButton("📦 Kiralama", async () => await Navigation.PushAsync(new LeasePage())),
                    RedButton("💳 Ödemeler", async () => await Navigation.PushAsync(new PaymentsPage())),
                    RedButton("📊 Muhasebe", async () => await Navigation.PushAsync(new AccountingPage())),
                    RedButton("🎌 Online Japonca (N5/N4)", async () => await Navigation.PushAsync(new JapaneseCoursesPage())),
                    RedButton("📅 Ders Programları", async () => await Navigation.PushAsync(new SchedulesPage())),



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

    Button RedButton(string text, Func<Task> onClick) =>
        new Button()
            .Text(text)
            .FontAttributes(FontAttributes.Bold)
            .CornerRadius(14)
            .HeightRequest(54)
            .BackgroundColor(Red)
            .TextColor(Colors.White)
            .Command(new Command(async () => await onClick()));
}







//using FmgLib.MauiMarkup;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Graphics;

//namespace Edu_mobileapp;

//public class Dashboard : FmgLibContentPage
//{
//    private static readonly Color Red = Color.FromArgb("#E50914");
//    private static readonly Color LightGray = Color.FromArgb("#F3F4F6");

//    public Dashboard()
//    {
//        Title = "Ana Sayfa";
//        Build();
//    }

//    public override void Build()
//    {
//        this
//        .BackgroundColor(Colors.White)
//        .Content(
//            new ScrollView()
//            .Content(
//                new VerticalStackLayout()
//                .Padding(24)
//                .Spacing(16)
//                .Children(
//                    RedButton("👤 Müşteri Yönetimi", async () => await Navigation.PushAsync(new CustomersPage())),

//      RedButton("🏠 Konaklama", async () => await Navigation.PushAsync(new AccommodationsPage())),

//       RedButton("🪑 Demirbaş", async () => await Navigation.PushAsync(new AssetsPage())),

//        RedButton("📦 Kiralama", async () => await Navigation.PushAsync(new LeasePage())),


//        RedButton("💳 Ödemeler", async () => await Navigation.PushAsync(new PaymentsPage())),

//        RedButton("📊 Muhasebe", async () => await Navigation.PushAsync(new AccountingPage())),





//                    // Kırmızı başlık bandı
//                    new Frame()
//                        .CornerRadius(16)
//                        .Padding(new Thickness(16, 22))
//                        .BackgroundColor(Red)
//                        .Content(
//                            new VerticalStackLayout()
//                            .Children(
//                                new Label()
//                                    .Text("Edusama AFS")
//                                    .FontSize(26)
//                                    .FontAttributes(FontAttributes.Bold)
//                                    .TextColor(Colors.White)
//                                    .HorizontalTextAlignment(TextAlignment.Center),

//                                new Label()
//                                    .Text("Modüllere hızlı erişim")
//                                    .TextColor(Colors.White)
//                                    .Opacity(0.9)
//                                    .HorizontalTextAlignment(TextAlignment.Center)
//                            )
//                        ),


//        // Modül butonları (kırmızı)


//        new Label()
//                        .Text("© Edusama • 2025")
//                        .FontSize(12)
//                        .TextColor(Colors.Gray)
//                        .HorizontalTextAlignment(TextAlignment.Center)
//                        .Margin(new Thickness(0, 24, 0, 0))
//                )
//            )
//        );
//    }

//    Button RedButton(string text, Func<Task> onClick) =>
//        new Button()
//            .Text(text)
//            .FontAttributes(FontAttributes.Bold)
//            .CornerRadius(14)
//            .HeightRequest(54)
//            .BackgroundColor(Red)
//            .TextColor(Colors.White)
//            .Command(new Command(async () => await onClick()));
//}
