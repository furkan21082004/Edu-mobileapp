using System.Collections.Generic;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Edu_mobileapp;

public class SchedulesPage : FmgLibContentPage
{
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    public SchedulesPage()
    {
        Title = "Ders Programları";
        Build();
    }

    public override void Build()
    {
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
                    new Label().Text("Edusama AFS")
                               .FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White)
                               .HorizontalTextAlignment(TextAlignment.Center),

                    new Label().Text("N5 • N4 • Konuşma Kulübü")
                               .TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // --- Kısa içerik metinleri ---
        var n5Rows = new List<string[]>
        {
            new[] { "Pzt", "19:00–21:00", "2s", "Dilbilgisi: は/が/を/に/へ", "Mini quiz 10dk" },
            new[] { "Sal", "19:00–21:00", "2s", "Kelime + Kanji (temel)", "Yazma sayfası" },
            new[] { "Per", "19:00–21:00", "2s", "Dinleme + Telaffuz", "" },
            new[] { "Cum", "19:00–21:00", "2s", "Okuma + Kısa yazma", "Geri bildirim" },
            new[] { "Toplam", "", "8s", "", "" },
        };
        var n5 = ProgramCard("JLPT N5 – Haftaiçi Akşam", "4×2 saat = 8 saat/hafta",
            new[] { "Gün", "Saat", "Süre", "İçerik / Odak", "Not" }, n5Rows);

        var n4Rows = new List<string[]>
        {
            new[] { "Sal", "18:30–22:30", "4s", "Dilbilgisi: ～て形/～ながら/～そうだ/～らしい; Kanji; Okuma", "Saat başı 10dk ara" },
            new[] { "Cum", "18:30–22:30", "4s", "Dinleme • Okuduğunu anlama • Yazma (e-posta/duyuru)", "Quiz 20dk" },
            new[] { "Toplam", "", "8s", "", "" },
        };
        var n4 = ProgramCard("JLPT N4 – Yoğun", "2×4 saat = 8 saat/hafta",
            new[] { "Gün", "Saat", "Süre", "İçerik / Odak", "Not" }, n4Rows);

        var clubRows = new List<string[]>
        {
            new[] { "Cmt", "10:00–12:00", "2s", "Serbest konuşma • Rol-play • Günlük diyalog", "Maks. 10 kişi" }
        };
        var club = ProgramCard("Konuşma Kulübü", "Hafta sonu 2 saat",
            new[] { "Gün", "Saat", "Süre", "İçerik / Odak", "Not" }, clubRows);

        var root = new VerticalStackLayout
        {
            Padding = 16,
            Spacing = 12,
            Children =
            {
                header, n5, n4, club,
                new Label().Text("Not: Tüm saatler TRT (GMT+3) örneğidir.")
                           .FontSize(12).TextColor(Colors.Gray)
                           .HorizontalTextAlignment(TextAlignment.Center)
                           .Margin(new Thickness(0,6,0,0))
            }
        };

        this.BackgroundColor(Colors.White)
            .Content(new ScrollView { Content = root });
    }

    // ---------- Yardımcılar ----------

    private View ProgramCard(string title, string subtitle, string[] headers, List<string[]> rows)
    {
        var card = new Frame
        {
            CornerRadius = 16,
            Padding = 14,
            BackgroundColor = Colors.White,
            BorderColor = LightGray
        };

        var stack = new VerticalStackLayout { Spacing = 10 };

        stack.Children.Add(
            new VerticalStackLayout
            {
                Spacing = -2,
                Children =
                {
                    new Label().Text(title).FontAttributes(FontAttributes.Bold).FontSize(18),
                    new Label().Text(subtitle).TextColor(Colors.Gray).FontSize(13)
                }
            });

        // tabloyu yatay kaydırılabilir yap
        var table = MakeTable(headers, rows);
        var hScroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = table
        };

        stack.Children.Add(hScroll);
        card.Content = stack;
        return card;
    }

    private View MakeTable(string[] headers, List<string[]> rows)
    {
        var g = new Grid { ColumnSpacing = 8, RowSpacing = 6 };

        // Tüm kolonlar Auto —> genişlik toplamı ekranı aşarsa yatay scroll devreye girer
        for (int i = 0; i < headers.Length; i++)
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        int r = 0;

        // Başlık satırı
        for (int c = 0; c < headers.Length; c++)
        {
            var head = new Frame
            {
                BackgroundColor = LightGray,
                Padding = new Thickness(8, 6),
                CornerRadius = 8,
                HasShadow = false,
                Content = new Label
                {
                    Text = headers[c],
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.TailTruncation
                }
            };
            Grid.SetRow(head, r);
            Grid.SetColumn(head, c);
            g.Children.Add(head);
        }
        r++;

        // Veri satırları (nowrap + kısaltma)
        foreach (var row in rows)
        {
            for (int c = 0; c < headers.Length; c++)
            {
                var lbl = new Label
                {
                    Text = c < row.Length ? row[c] : "",
                    LineBreakMode = LineBreakMode.TailTruncation,  // taşan yerde … göster
                    FontAttributes = row[0] == "Toplam" ? FontAttributes.Bold : FontAttributes.None
                };

                var cell = new Frame
                {
                    Padding = new Thickness(6, 4),
                    HasShadow = false,
                    CornerRadius = 8,
                    BackgroundColor = Colors.Transparent,
                    Content = lbl
                };

                Grid.SetRow(cell, r);
                Grid.SetColumn(cell, c);
                g.Children.Add(cell);
            }
            r++;
        }

        for (int i = 0; i < r; i++)
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        return g;
    }
}
