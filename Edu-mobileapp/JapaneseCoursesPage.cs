using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Edu_mobileapp;

public class JapaneseCoursesPage : FmgLibContentPage
{
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    public JapaneseCoursesPage()
    {
        Title = "Online Japonca (N5/N4)";
        Build();
    }

    public override void Build()
    {
        // Başlık
        var header = new Frame
        {
            CornerRadius = 16,
            Padding = new Thickness(16, 22),
            BackgroundColor = Red,
            HasShadow = false,
            Content = new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    new Label
                    {
                        Text = "🎌 オンライン日本語 — N5 / N4",
                        FontSize = 22,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Label
                    {
                        Text = "Canlı ders • Kayıt tekrarları • Ödev & Quiz • Konuşma atölyesi",
                        TextColor = Colors.White, Opacity = 0.9,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            }
        };

        // Yardımcı chip
        View Chip(string text, Color? bg = null) => new Frame
        {
            Padding = new Thickness(10, 6),
            CornerRadius = 12,
            HasShadow = false,
            BackgroundColor = bg ?? LightGray,
            Content = new Label { Text = text, FontSize = 12, TextColor = Colors.Black }
        };

        Label Section(string t) => new Label
        {
            Text = t,
            TextColor = Red,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 10, 0, 4)
        };

        // N5 kartı
        var n5Card = new Frame
        {
            CornerRadius = 16,
            Padding = 16,
            BackgroundColor = Colors.White,
            BorderColor = LightGray,
            HasShadow = false,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label { Text = "🎯 N5 — Başlangıç", FontSize = 18, FontAttributes = FontAttributes.Bold },
                    new Label { Text = "Hedef: Temel günlük konuşmalar • JLPT N5", TextColor = Colors.Gray, FontSize = 13 },
                    new HorizontalStackLayout
                    {
                        Spacing = 6,
                        Children = { Chip("🗓️ 12 hafta"), Chip("🕒 Haftada 3×2 saat"), Chip("👩‍🏫 Canlı + kayıt") }
                    },
                    Section("Müfredat"),
                    new Label { Text = "🔤 ひらがな・カタカナ (Hiragana/Katakana)" },
                    new Label { Text = "🧩 〜です／〜ます, これ・それ・あれ, 〜が あります/います" },
                    new Label { Text = "📚 Sayılar, tarih-saat, alışveriş, yön sorma" },
                    new Label { Text = "🗣️ Selamlaşma, kendini tanıtma, basit istek/soru" }
                }
            }
        };

        // N4 kartı
        var n4Card = new Frame
        {
            CornerRadius = 16,
            Padding = 16,
            BackgroundColor = Colors.White,
            BorderColor = LightGray,
            HasShadow = false,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label { Text = "🎯 N4 — Orta Öncesi", FontSize = 18, FontAttributes = FontAttributes.Bold },
                    new Label { Text = "Hedef: Günlük hayatı detaylı anlatım • JLPT N4", TextColor = Colors.Gray, FontSize = 13 },
                    new HorizontalStackLayout
                    {
                        Spacing = 6,
                        Children = { Chip("🗓️ 12 hafta"), Chip("🕒 Haftada 3×2 saat"), Chip("🗣️ Konuşma atölyesi") }
                    },
                    Section("Müfredat"),
                    new Label { Text = "🧩 て/ない/た-form, 〜と思う・〜かもしれない" },
                    new Label { Text = "📚 比較(もっと/ずっと), 可能/受け身/使役’e giriş" },
                    new Label { Text = "🗣️ Deneyim anlatma, sebep–sonuç, rica & öneri" }
                }
            }
        };

        // Genel özellikler
        var featuresCard = new Frame
        {
            CornerRadius = 16,
            Padding = 16,
            BackgroundColor = Colors.White,
            BorderColor = LightGray,
            HasShadow = false,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    Section("Öne Çıkanlar"),
                    new HorizontalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            Chip("💻 Zoom"),
                            Chip("🎥 Ders kayıtları"),
                            Chip("📝 Ödev + Quiz"),
                            Chip("🗣️ Konuşma kulübü"),
                            Chip("📥 PDF + Anki")
                        }
                    },
                    Section("Sınav & Sertifika"),
                    new Label { Text = "🧪 Modül sonu mini denemeler • Dönem sonu JLPT denemesi" },
                    new Label { Text = "📜 Kur bitirme sertifikası (Edusama)" }
                }
            }
        };

        // CTA (tamamen Stack tabanlı)
        Button RedBtn(string t, Func<Task> on) => new Button
        {
            Text = t,
            BackgroundColor = Red,
            TextColor = Colors.White,
            CornerRadius = 14,
            Padding = new Thickness(16, 10),
            Command = new Command(async () => await on())
        };
        Button GrayBtn(string t, Func<Task> on) => new Button
        {
            Text = t,
            BackgroundColor = LightGray,
            TextColor = Colors.Black,
            CornerRadius = 14,
            Padding = new Thickness(16, 10),
            Command = new Command(async () => await on())
        };

        var cta = new Frame
        {
            CornerRadius = 16,
            Padding = 16,
            BackgroundColor = Colors.White,
            BorderColor = LightGray,
            HasShadow = false,
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    Section("Dönem & Ücret"),
                    new Label { Text = "📅 Yeni dönem: Her ayın ilk haftası (kontenjan sınırlı)" },
                    new Label { Text = "💳 Taksit / peşin seçenekleri • Kur başı uygun fiyat", TextColor = Colors.Gray, FontSize = 13 },
                    new HorizontalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            RedBtn("🎯 Deneme Dersi", async () =>
                            {
                                await DisplayAlert("Deneme Dersi", "Danışmanımız sizinle iletişime geçecek. 🙌", "Tamam");
                            }),
                            GrayBtn("Bilgi Al", async () =>
                            {
                                await DisplayAlert("Bilgi Formu", "Talebiniz alındı. E-posta ile dönüş yapacağız.", "Kapat");
                            })
                        }
                    }
                }
            }
        };

        // Kök
        var root = new VerticalStackLayout
        {
            Padding = 16,
            Spacing = 12,
            Children = { header, n5Card, n4Card, featuresCard, cta }
        };

        this.BackgroundColor(Colors.White)
            .Content(new ScrollView { Content = root });
    }
}
