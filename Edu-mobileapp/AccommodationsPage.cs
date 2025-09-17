using System.Collections.ObjectModel;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Edu_mobileapp;

public class AccommodationsPage : FmgLibContentPage
{
    // Tema renkleri
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    // Veri
    public ObservableCollection<AcItem> Items { get; } = new();

    public AccommodationsPage()
    {
        Title = "Konaklama";
        BindingContext = this;

        Seed();   // 10 kayıt
        Build();
    }

    public override void Build()
    {
        // Üst başlık
        var header = new Frame
        {
            CornerRadius = 16,
            Padding = new Thickness(16, 22),
            BackgroundColor = Red,
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label().Text("Konaklama Listesi")
                               .FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White)
                               .HorizontalTextAlignment(TextAlignment.Center),
                    new Label().Text("Okul yurtları ve yakın daireler")
                               .TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // Liste (kartlar)
        var cv = new CollectionView
        {
            Margin = new Thickness(0, 8, 0, 24),
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 16 },
            ItemTemplate = new DataTemplate(() =>
            {
                // Swipe: düzenle/sil istersen ekleyebilirsin (şimdilik yok)
                var card = new Frame
                {
                    CornerRadius = 18,
                    Padding = 10,
                    HasShadow = true,
                    BackgroundColor = Colors.White
                };

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{ Width = new GridLength(140) }, // görsel
                        new ColumnDefinition{ Width = GridLength.Star }      // metinler
                    }
                };

                // SOL: Görsel + rozet
                var imageFrame = new Frame
                {
                    Padding = 0,
                    CornerRadius = 16,
                    HasShadow = false,
                    IsClippedToBounds = true,
                    BackgroundColor = LightGray,
                    HeightRequest = 120,
                    WidthRequest = 140
                };
                var img = new Image { Aspect = Aspect.AspectFill };
                img.SetBinding(Image.SourceProperty, nameof(AcItem.ImageName));
                imageFrame.Content = img;

                var badge = new Label
                {
                    Text = "POPULAR",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 12,
                    TextColor = Colors.White,
                    BackgroundColor = Red,
                    Padding = new Thickness(8, 4),
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 6, 6, 0)
                };
                badge.SetBinding(IsVisibleProperty, nameof(AcItem.IsPopular));

                var left = new Grid();
                left.Children.Add(imageFrame);
                left.Children.Add(badge);

                Grid.SetColumn(left, 0);
                grid.Children.Add(left);

                // SAĞ: Başlık + bilgi satırları + açıklama + alt çizgi
                var right = new VerticalStackLayout { Spacing = 6, Padding = new Thickness(12, 0, 0, 0) };

                var title = new Label().FontAttributes(FontAttributes.Bold).FontSize(18).TextColor(Colors.Black);
                title.SetBinding(Label.TextProperty, nameof(AcItem.Title));
                right.Children.Add(title);

                right.Children.Add(InfoRow("🗓️", nameof(AcItem.Terms)));
                right.Children.Add(InfoRow("📘", nameof(AcItem.Types)));
                right.Children.Add(InfoRow("🏠", nameof(AcItem.Dorm)));
                right.Children.Add(InfoRow("💴", nameof(AcItem.Fee)));

                var desc = new Label
                {
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                // 2 satır hissi için çok uzun olmasın
                desc.SetBinding(Label.TextProperty, nameof(AcItem.Description));
                right.Children.Add(desc);

                // Alt aksiyonlar (isteğe bağlı)
                var actions = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 6, 0, 0) };
                actions.Children.Add(SmallRedButton("Detay", () => { }));
                actions.Children.Add(SmallRedButton("Kiralama", () => { }));
                right.Children.Add(actions);

                Grid.SetColumn(right, 1);
                grid.Children.Add(right);

                card.Content = grid;
                return card;
            })
        };
        cv.SetBinding(ItemsView.ItemsSourceProperty, nameof(Items));

        var pageStack = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        pageStack.Children.Add(header);
        pageStack.Children.Add(cv);

        this.BackgroundColor(Colors.White)
            .Content(new ScrollView { Content = pageStack });
    }

    // ---------- Yardımcı UI ----------
    private View InfoRow(string icon, string bindProp)
    {
        var row = new HorizontalStackLayout { Spacing = 10 };

        row.Children.Add(new Label { Text = icon, FontSize = 14, VerticalTextAlignment = TextAlignment.Center });

        var value = new Label { TextColor = Colors.Black };
        value.SetBinding(Label.TextProperty, bindProp);

        row.Children.Add(value);
        return row;
    }

    private Button SmallRedButton(string text, Action onClick) =>
        new Button
        {
            Text = text,
            BackgroundColor = Red,
            TextColor = Colors.White,
            CornerRadius = 12,
            Padding = new Thickness(12, 6),
            Command = new Command(onClick)
        };

    // ---------- Örnek veri (10 kart) ----------
    private void Seed()
    {
        Items.Add(new AcItem
        {
            Title = "TLS — Tokyo Kampüs",
            ImageName = "o_onizleme.jpg",
            IsPopular = true,
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca, Üniversite Hazırlık, Kısa Dönem",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 816.000 JPY",
            Description = "Tokyo'nun öğrenci dostu bölgesinde, toplu taşımaya yakın."
        });

        Items.Add(new AcItem
        {
            Title = "Aishin — Nagoya",
            ImageName = "aishin_onizleme.png",
            IsPopular = true,
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 780.000 JPY",
            Description = "Merkezi konum, modern sınıflar ve sosyal etkinlikler."
        });

        Items.Add(new AcItem
        {
            Title = "Akamonkai — Tokyo",
            ImageName = "akamonkai_onizleme.jpg",
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca, Hazırlık",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 840.000 JPY",
            Description = "Geniş kampüs, deneyimli eğitmen kadrosu."
        });

        Items.Add(new AcItem
        {
            Title = "Genki — Fukuoka",
            ImageName = "genki_onizleme_fukuoka.jpg",
            IsPopular = true,
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca, Yoğun Program",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 820.000 JPY",
            Description = "Sıcak iklim, sahile yakın şehir yaşamı."
        });

        Items.Add(new AcItem
        {
            Title = "Genki — Genel",
            ImageName = "genki_onizleme_genel.jpg",
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 790.000 JPY",
            Description = "Keyifli kampüs atmosferi, küçük sınıflar."
        });

        Items.Add(new AcItem
        {
            Title = "Genki — Kyoto",
            ImageName = "genki_onizleme_kyoto.jpg",
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca, Kültür Programı",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 830.000 JPY",
            Description = "Tarihi Kyoto’nun kalbinde, tapınaklara yakın."
        });

        Items.Add(new AcItem
        {
            Title = "Genki — Tokyo",
            ImageName = "genki_onizleme_tokyo.jpg",
            IsPopular = true,
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca, İş Japoncası",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 860.000 JPY",
            Description = "Shinjuku yakınında, hareketli şehir hayatı."
        });

        Items.Add(new AcItem
        {
            Title = "Human — Fukuoka",
            ImageName = "human_onizleme_fukuoka.jpg",
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 760.000 JPY",
            Description = "Şehir merkeziyle dengeli, sakin kampüs."
        });

        Items.Add(new AcItem
        {
            Title = "Human — Genel",
            ImageName = "human_onizleme_genel.jpg",
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 770.000 JPY",
            Description = "Çeşitli sosyal kulüpler ve atölyeler."
        });

        Items.Add(new AcItem
        {
            Title = "Nitto — Tokyo",
            ImageName = "nitto_onizleme.jpg",
            Terms = "Eğitim dönemleri : Ocak, Nisan, Temmuz, Ekim",
            Types = "Eğitim türleri : Genel Japonca, Hazırlık",
            Dorm = "Yurt imkanı : Var",
            Fee = "Yıllık Ücret : 800.000 JPY",
            Description = "Ulaşımı kolay, modern sınıf altyapısı."
        });
    }

    // Kartta kullanılan basit veri tipi
    public class AcItem
    {
        public string Title { get; set; } = "";
        public string Terms { get; set; } = "";
        public string Types { get; set; } = "";
        public string Dorm { get; set; } = "";
        public string Fee { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageName { get; set; } = ""; // Resources/Images içindeki dosya adı
        public bool IsPopular { get; set; }
    }
}

