using System.Collections.ObjectModel;
using System.Linq;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Edu_mobileapp;

public class AssetsPage : FmgLibContentPage
{
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    public ObservableCollection<Edu_mobileapp.Models.Asset> Items { get; } = new();

    public AssetsPage()
    {
        Title = "Demirbaşlar";
        BindingContext = this;

        Seed();     // her görsel 1 kez
        Build();
    }

    public override void Build()
    {
        // Üst kart
        var header = new Frame
        {
            CornerRadius = 16,
            Padding = new Thickness(16, 22),
            BackgroundColor = Red,
            Content = new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    new Label().Text("Demirbaş Listesi")
                               .FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White)
                               .HorizontalTextAlignment(TextAlignment.Center),
                    new Label().Text("Sol: görsel • Sağ: adet, durum, marka, özellik")
                               .TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // Liste
        var list = new CollectionView
        {
            ItemsSource = Items,
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 12 },
            ItemTemplate = new DataTemplate(() =>
            {
                var card = new Frame
                {
                    CornerRadius = 16,
                    Padding = 12,
                    BackgroundColor = Colors.White,
                    BorderColor = LightGray
                };

                // 2 kolon: sol görsel, sağ bilgiler
                var g = new Grid { ColumnSpacing = 12 };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(112) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                // Sol görsel kutusu
                var imgFrame = new Frame
                {
                    Padding = 0,
                    CornerRadius = 12,
                    HasShadow = false,
                    IsClippedToBounds = true,
                    BackgroundColor = LightGray,
                    WidthRequest = 112,
                    HeightRequest = 88
                };
                var img = new Image { Aspect = Aspect.AspectFill };
                img.SetBinding(Image.SourceProperty, nameof(Models.Asset.Image));
                imgFrame.Content = img;
                Grid.SetColumn(imgFrame, 0);
                g.Children.Add(imgFrame);

                // Sağ bölüm
                var right = new VerticalStackLayout { Spacing = 6 };

                // Başlık
                var title = new Label { FontAttributes = FontAttributes.Bold, FontSize = 18 };
                title.SetBinding(Label.TextProperty, nameof(Models.Asset.Name));
                right.Children.Add(title);

                // Bilgi satırları
                right.Children.Add(BindRow("📦", "Adet: {0}", nameof(Models.Asset.Quantity)));
                right.Children.Add(BindRow("📍", "Konum: {0}", nameof(Models.Asset.Location)));
                right.Children.Add(BindRow("🏷️", "Marka: {0}", nameof(Models.Asset.Brand)));
                right.Children.Add(BindRow("⚙️", "Özellik: {0}", nameof(Models.Asset.Spec)));

                // Durum çipi
                var statusText = new Label { FontSize = 12, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
                statusText.SetBinding(Label.TextProperty, nameof(Models.Asset.Status));

                var chip = new Frame { Padding = new Thickness(8, 3), CornerRadius = 8, Content = statusText };
                chip.SetBinding(VisualElement.BackgroundColorProperty,
                    new Binding(nameof(Models.Asset.Status), converter: new StatusToColor()));

                right.Children.Add(chip);

                Grid.SetColumn(right, 1);
                g.Children.Add(right);

                card.Content = g;
                return card;
            })
        };

        var root = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        root.Children.Add(header);
        root.Children.Add(list);

        this.BackgroundColor(Colors.White).Content(new ScrollView { Content = root });
    }

    // Küçük yardımcı: bağlanan satır
    private View BindRow(string emoji, string format, string bindPath)
    {
        var row = new HorizontalStackLayout { Spacing = 6 };
        row.Children.Add(new Label { Text = emoji, WidthRequest = 18, HorizontalTextAlignment = TextAlignment.Center });

        var val = new Label { TextColor = Colors.Black };
        val.SetBinding(Label.TextProperty, new Binding(bindPath, stringFormat: format));
        row.Children.Add(val);

        return row;
    }

    // Durumu renge çeviren dönüştürücü
    public class StatusToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = (value as string)?.ToLowerInvariant() ?? "";
            return s switch
            {
                "bakımda" => Colors.Orange,
                "yedekte" => Colors.SlateGray,
                "hurda" => Colors.Gray,
                _ => Colors.SeaGreen   // Kullanımda
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ------------ Örnek veriler (her görsel 1 kez) ------------
    private void Seed()
    {
        // Not: dosya adları senin klasörüne göre (tamamı küçük harf ve ascii).
        Items.Add(new Models.Asset { Name = "Dizüstü Bilgisayar", Category = "BT", Image = "dizustu_bilgisayar.jpeg", Quantity = 7, Location = "Açık Ofis", Status = "Kullanımda", Brand = "Lenovo ThinkPad", Spec = "i7 • 16 GB • 512 GB" });
        Items.Add(new Models.Asset { Name = "Masaüstü Bilgisayar", Category = "BT", Image = "masaustu_bilgisayar.jpeg", Quantity = 5, Location = "Açık Ofis", Status = "Kullanımda", Brand = "Dell OptiPlex", Spec = "i5 • 16 GB • 512 GB" });
        Items.Add(new Models.Asset { Name = "Kurumsal Telefon", Category = "Cihaz", Image = "telefon.jpeg", Quantity = 6, Location = "Saha Ekipleri", Status = "Kullanımda", Brand = "Samsung A34", Spec = "Dual SIM • 128 GB" });
        Items.Add(new Models.Asset { Name = "Tablet", Category = "Cihaz", Image = "tablet.jpeg", Quantity = 5, Location = "Saha Ekipleri", Status = "Yedekte", Brand = "iPad 9th", Spec = "10.2\" • 64 GB" });
        Items.Add(new Models.Asset { Name = "Lazer Yazıcı", Category = "Ofis", Image = "yazici.jpeg", Quantity = 2, Location = "Muhasebe", Status = "Kullanımda", Brand = "HP LaserJet", Spec = "A4 • 30ppm" });
        Items.Add(new Models.Asset { Name = "Güvenlik Kamerası", Category = "Güvenlik", Image = "guvenlik_kamerasi.jpg", Quantity = 4, Location = "Giriş-Çıkış", Status = "Kullanımda", Brand = "Hikvision", Spec = "1080p • IR" });
        Items.Add(new Models.Asset { Name = "Fotoğraf Kamerası", Category = "Prodüksiyon", Image = "fotograf_kamerasi.jpeg", Quantity = 2, Location = "Stüdyo", Status = "Yedekte", Brand = "Sony A7C", Spec = "Full Frame" });
        Items.Add(new Models.Asset { Name = "Modem", Category = "Ağ", Image = "modem.jpeg", Quantity = 1, Location = "Sunucu Odası", Status = "Kullanımda", Brand = "Zyxel", Spec = "VDSL/ADSL" });
        Items.Add(new Models.Asset { Name = "Ağ Anahtarı", Category = "Ağ", Image = "ag_anahtari.jpeg", Quantity = 3, Location = "Sunucu Odası", Status = "Kullanımda", Brand = "Cisco 24P PoE", Spec = "Gigabit • PoE" });
        Items.Add(new Models.Asset { Name = "Ofis Sandalyesi", Category = "Mobilya", Image = "ofis_sandalyesi.jpeg", Quantity = 7, Location = "Açık Ofis", Status = "Kullanımda", Brand = "Ergoseat", Spec = "Ergonomik" });
        Items.Add(new Models.Asset { Name = "Havuz Araç", Category = "Ulaşım", Image = "arac.jpeg", Quantity = 2, Location = "Otopark", Status = "Kullanımda", Brand = "Toyota Corolla", Spec = "Benzinli" });
        Items.Add(new Models.Asset { Name = "Merkez Ofis", Category = "Ofis", Image = "ofis.jpeg", Quantity = 1, Location = "Merkez", Status = "Kullanımda", Brand = "—", Spec = "Toplantı/Ortak alan" });
    }
}
