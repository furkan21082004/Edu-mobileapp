using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Edu_mobileapp;

public class AccountingPage : FmgLibContentPage
{
    // Tema
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    // --- Hesaplar ---
    public ObservableCollection<AccountRow> Accounts { get; } = new();
    public ObservableCollection<AccountRow> ViewAccounts { get; } = new();

    // --- Yevmiye (Gelir/Gider) ---
    public ObservableCollection<JournalRow> Journal { get; } = new();
    public ObservableCollection<JournalRow> ViewJournal { get; } = new();

    // Filtreler (yevmiye)
    string _q = "";
    public string Query { get => _q; set { _q = value; ApplyJournalFilter(); } }

    public ObservableCollection<string> Months { get; } =
        new(new[] { "Bu Ay", "Geçen Ay", "Tümü" });

    string _selectedMonth = "Bu Ay";
    public string SelectedMonth { get => _selectedMonth; set { _selectedMonth = value; ApplyJournalFilter(); } }

    // KPI’lar (dashboard)
    string _kpiIncome = "—"; public string KpiIncome { get => _kpiIncome; set { _kpiIncome = value; OnPropertyChanged(); } }
    string _kpiExpense = "—"; public string KpiExpense { get => _kpiExpense; set { _kpiExpense = value; OnPropertyChanged(); } }
    string _kpiNet = "—"; public string KpiNet { get => _kpiNet; set { _kpiNet = value; OnPropertyChanged(); } }
    string _kpiCash = "—"; public string KpiCash { get => _kpiCash; set { _kpiCash = value; OnPropertyChanged(); } }

    // Sekmeler
    bool _showDash = true, _showAcc = false, _showJrnl = false, _showRpt = false;
    public bool ShowDashboard { get => _showDash; set { _showDash = value; OnPropertyChanged(); } }
    public bool ShowAccounts { get => _showAcc; set { _showAcc = value; OnPropertyChanged(); } }
    public bool ShowJournal { get => _showJrnl; set { _showJrnl = value; OnPropertyChanged(); } }
    public bool ShowReports { get => _showRpt; set { _showRpt = value; OnPropertyChanged(); } }

    public AccountingPage()
    {
        Title = "Muhasebe";
        BindingContext = this;

        Seed();                // örnek veri
        RecalcAccounts();      // yevmiye -> hesap bakiyeleri
        ApplyJournalFilter();  // görünüm
        RecalcKpis();          // KPI’lar

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
                    new Label().Text("Muhasebe")
                               .FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White)
                               .HorizontalTextAlignment(TextAlignment.Center),
                    new Label().Text("Genel Bakýþ • Hesaplar • Yevmiye • Raporlar")
                               .TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // Sekmeler
        var tabs = new Grid { ColumnSpacing = 8, Margin = new Thickness(0, 0, 0, 4) };
        tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        Button TabBtn(string t, Action on)
        {
            var b = new Button { Text = t, CornerRadius = 12, HeightRequest = 44, BorderColor = Red, BorderWidth = 1, BackgroundColor = Colors.White, TextColor = Red };
            b.Clicked += (_, __) =>
            {
                ShowDashboard = ShowAccounts = ShowJournal = ShowReports = false;
                on();
            };
            return b;
        }

        var b1 = TabBtn("Genel Bakýþ", () => ShowDashboard = true);
        var b2 = TabBtn("Hesaplar", () => ShowAccounts = true);
        var b3 = TabBtn("Yevmiye", () => ShowJournal = true);
        var b4 = TabBtn("Raporlar", () => ShowReports = true);

        Grid.SetColumn(b1, 0); Grid.SetColumn(b2, 1); Grid.SetColumn(b3, 2); Grid.SetColumn(b4, 3);
        tabs.Children.Add(b1); tabs.Children.Add(b2); tabs.Children.Add(b3); tabs.Children.Add(b4);

        // Dashboard
        var dash = DashboardView(); dash.SetBinding(IsVisibleProperty, nameof(ShowDashboard));
        // Accounts
        var acc = AccountsView(); acc.SetBinding(IsVisibleProperty, nameof(ShowAccounts));
        // Journal
        var jrnl = JournalView(); jrnl.SetBinding(IsVisibleProperty, nameof(ShowJournal));
        // Reports
        var rpt = ReportsView(); rpt.SetBinding(IsVisibleProperty, nameof(ShowReports));

        var root = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        root.Children.Add(header);
        root.Children.Add(tabs);
        root.Children.Add(dash);
        root.Children.Add(acc);
        root.Children.Add(jrnl);
        root.Children.Add(rpt);

        this.BackgroundColor(Colors.White)
            .Content(new ScrollView { Content = root });
    }

    // ==================== VIEWS ====================

    private View DashboardView()
    {
        View KpiCard(string title, string bind)
        {
            var f = new Frame { CornerRadius = 16, Padding = 16, BackgroundColor = Colors.White, BorderColor = LightGray };
            var v = new VerticalStackLayout { Spacing = 6 };
            v.Children.Add(new Label { Text = title, TextColor = Colors.Gray, FontSize = 13 });
            var val = new Label { FontAttributes = FontAttributes.Bold, FontSize = 20 };
            val.SetBinding(Label.TextProperty, bind);
            v.Children.Add(val);
            f.Content = v;
            return f;
        }

        var g = new Grid { ColumnSpacing = 12, RowSpacing = 12, Margin = new Thickness(0, 0, 0, 8) };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var c1 = KpiCard("Gelir", nameof(KpiIncome));
        var c2 = KpiCard("Gider", nameof(KpiExpense));
        var c3 = KpiCard("Net", nameof(KpiNet));
        var c4 = KpiCard("Kasa+Banka", nameof(KpiCash));
        Grid.SetColumn(c1, 0); Grid.SetRow(c1, 0);
        Grid.SetColumn(c2, 1); Grid.SetRow(c2, 0);
        Grid.SetColumn(c3, 0); Grid.SetRow(c3, 1);
        Grid.SetColumn(c4, 1); Grid.SetRow(c4, 1);
        g.Children.Add(c1); g.Children.Add(c2); g.Children.Add(c3); g.Children.Add(c4);

        // Para Hesaplarý mini liste
        var lbl = new Label { Text = "Para Hesaplarý", FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 4, 0, 0) };
        var list = new CollectionView
        {
            ItemsSource = ViewAccounts,
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 6 },
            ItemTemplate = new DataTemplate(() =>
            {
                var f = new Frame { CornerRadius = 12, Padding = 10, BackgroundColor = Colors.White, BorderColor = LightGray };
                var row = new Grid { ColumnSpacing = 8 };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var name = new Label { FontAttributes = FontAttributes.Bold };
                name.SetBinding(Label.TextProperty, nameof(AccountRow.Name));
                Grid.SetColumn(name, 0);

                var bal = new Label { TextColor = Colors.Black, HorizontalTextAlignment = TextAlignment.End };
                bal.SetBinding(Label.TextProperty, nameof(AccountRow.BalanceDisplay));
                Grid.SetColumn(bal, 1);

                row.Children.Add(name); row.Children.Add(bal);
                f.Content = row;
                return f;
            })
        };

        var box = new VerticalStackLayout();
        box.Children.Add(g);
        box.Children.Add(lbl);
        box.Children.Add(list);
        return box;
    }

    private View AccountsView()
    {
        var list = new CollectionView
        {
            ItemsSource = ViewAccounts,
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 },
            ItemTemplate = new DataTemplate(() =>
            {
                var f = new Frame { CornerRadius = 16, Padding = 12, BackgroundColor = Colors.White, BorderColor = LightGray };

                var g = new Grid { ColumnSpacing = 12 };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var left = new VerticalStackLayout { Spacing = 4 };
                var name = new Label { FontAttributes = FontAttributes.Bold, FontSize = 17 };
                name.SetBinding(Label.TextProperty, nameof(AccountRow.Name));

                var info = new Label { TextColor = Colors.Gray, FontSize = 13 };
                info.SetBinding(Label.TextProperty, nameof(AccountRow.InfoLine));

                left.Children.Add(name);
                left.Children.Add(info);

                var chipText = new Label { FontSize = 12, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
                chipText.SetBinding(Label.TextProperty, nameof(AccountRow.Type));
                var chip = new Frame { Padding = new Thickness(8, 2), CornerRadius = 8, Content = chipText };
                chip.SetBinding(VisualElement.BackgroundColorProperty, new Binding(nameof(AccountRow.Type), converter: new TypeToColor()));
                left.Children.Add(chip);

                Grid.SetColumn(left, 0); g.Children.Add(left);

                var bal = new Label { FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End };
                bal.SetBinding(Label.TextProperty, nameof(AccountRow.BalanceDisplay));
                Grid.SetColumn(bal, 1); g.Children.Add(bal);

                f.Content = g;
                return f;
            })
        };

        return list;
    }

    private View JournalView()
    {
        var top = new Grid { ColumnSpacing = 8, RowSpacing = 8, Margin = new Thickness(0, 0, 0, 4) };
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        top.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        top.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var search = new Entry { Placeholder = "Açýklama / hesap ara...", BackgroundColor = LightGray, HeightRequest = 44 };
        search.SetBinding(Entry.TextProperty, nameof(Query));
        Grid.SetColumnSpan(search, 3);
        Grid.SetRow(search, 0);
        top.Children.Add(search);

        var pickMonth = new Picker { Title = "Ay", BackgroundColor = LightGray, HeightRequest = 44 };
        pickMonth.SetBinding(Picker.ItemsSourceProperty, nameof(Months));
        pickMonth.SetBinding(Picker.SelectedItemProperty, nameof(SelectedMonth));
        Grid.SetColumn(pickMonth, 0); Grid.SetRow(pickMonth, 1);
        top.Children.Add(pickMonth);

        var btnIncome = new Button { Text = "Gelir Ekle", BackgroundColor = Red, TextColor = Colors.White, CornerRadius = 10, Padding = new Thickness(12, 6) };
        btnIncome.Clicked += async (_, __) => await QuickAddAsync("Gelir");
        Grid.SetColumn(btnIncome, 1); Grid.SetRow(btnIncome, 1);
        top.Children.Add(btnIncome);

        var btnExpense = new Button { Text = "Gider Ekle", BackgroundColor = LightGray, TextColor = Colors.Black, CornerRadius = 10, Padding = new Thickness(12, 6) };
        btnExpense.Clicked += async (_, __) => await QuickAddAsync("Gider");
        Grid.SetColumn(btnExpense, 2); Grid.SetRow(btnExpense, 1);
        top.Children.Add(btnExpense);

        var list = new CollectionView
        {
            ItemsSource = ViewJournal,
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 },
            ItemTemplate = new DataTemplate(() =>
            {
                var f = new Frame { CornerRadius = 16, Padding = 12, BackgroundColor = Colors.White, BorderColor = LightGray };

                var g = new Grid { ColumnSpacing = 12, RowSpacing = 2 };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var left = new VerticalStackLayout { Spacing = 2 };
                var line1 = new Label { FontAttributes = FontAttributes.Bold, FontSize = 16 };
                line1.SetBinding(Label.TextProperty, new Binding(nameof(JournalRow.Date), stringFormat: "{0:dd.MM.yyyy}"));

                var line2 = new Label { TextColor = Colors.Gray, FontSize = 13 };
                line2.SetBinding(Label.TextProperty, nameof(JournalRow.Description));

                var line3 = new Label { TextColor = Colors.Gray, FontSize = 13 };
                line3.SetBinding(Label.TextProperty, new Binding(nameof(JournalRow.Account), stringFormat: "Hesap: {0}"));

                left.Children.Add(line1); left.Children.Add(line2); left.Children.Add(line3);

                var right = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };

                var amt = new Label { FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End };
                amt.SetBinding(Label.TextProperty, nameof(JournalRow.AmountDisplay));

                var chipText = new Label { FontSize = 12, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
                chipText.SetBinding(Label.TextProperty, nameof(JournalRow.Kind));
                var chip = new Frame { Padding = new Thickness(8, 2), CornerRadius = 8, Content = chipText };
                chip.SetBinding(VisualElement.BackgroundColorProperty, new Binding(nameof(JournalRow.Kind), converter: new KindToColor()));

                right.Children.Add(amt);
                right.Children.Add(chip);

                Grid.SetColumn(left, 0); g.Children.Add(left);
                Grid.SetColumn(right, 1); g.Children.Add(right);

                f.Content = g;
                return f;
            })
        };

        var box = new VerticalStackLayout();
        box.Children.Add(top);
        box.Children.Add(list);
        return box;
    }

    private View ReportsView()
    {
        // Basit Aylýk Gelir-Gider Özeti
        var card = new Frame { CornerRadius = 16, Padding = 16, BackgroundColor = Colors.White, BorderColor = LightGray };
        var v = new VerticalStackLayout { Spacing = 8 };

        var title = new Label { Text = "Aylýk Gelir - Gider Özeti", FontAttributes = FontAttributes.Bold, FontSize = 18 };
        v.Children.Add(title);

        var monthLbl = new Label();
        monthLbl.Text = $"Dönem: {DateTime.Today:MMMM yyyy}";
        v.Children.Add(monthLbl);

        // TOPLAMLAR (para birimine göre)
        var incomeLbl = new Label { FontAttributes = FontAttributes.Bold, TextColor = Colors.SeaGreen };
        var expenseLbl = new Label { FontAttributes = FontAttributes.Bold, TextColor = Colors.IndianRed };
        var netLbl = new Label { FontAttributes = FontAttributes.Bold };

        incomeLbl.Text = "Gelir:   " + SumByCurrency(Journal.Where(x => IsInCurrentMonth(x.Date) && x.Kind == "Gelir"), j => j.Amount);
        expenseLbl.Text = "Gider:   " + SumByCurrency(Journal.Where(x => IsInCurrentMonth(x.Date) && x.Kind == "Gider"), j => j.Amount);
        netLbl.Text = "Net:     " + SumByCurrency(Journal.Where(x => IsInCurrentMonth(x.Date)), j => j.Kind == "Gelir" ? j.Amount : -j.Amount);

        v.Children.Add(incomeLbl);
        v.Children.Add(expenseLbl);
        v.Children.Add(netLbl);

        // Kategori kýrýlýmý (ilk 6 satýr)
        var br = new BoxView { HeightRequest = 1, BackgroundColor = LightGray, Margin = new Thickness(0, 6, 0, 6) };
        v.Children.Add(br);

        v.Children.Add(new Label { Text = "Kategoriye Göre (ilk 6)", FontAttributes = FontAttributes.Bold });

        var list = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 6 },
            ItemTemplate = new DataTemplate(() =>
            {
                var row = new Grid { ColumnSpacing = 8 };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var name = new Label();
                name.SetBinding(Label.TextProperty, "Name");
                var val = new Label { FontAttributes = FontAttributes.Bold };
                val.SetBinding(Label.TextProperty, "AmountText");

                Grid.SetColumn(name, 0); Grid.SetColumn(val, 1);
                row.Children.Add(name); row.Children.Add(val);

                var f = new Frame { CornerRadius = 12, Padding = 10, BackgroundColor = Colors.White, BorderColor = LightGray, Content = row };
                return f;
            })
        };

        var top6 = Journal
            .Where(x => IsInCurrentMonth(x.Date))
            .GroupBy(x => $"{x.Kind} • {x.Category}")
            .Select(g => new { Name = g.Key, Total = g.Sum(x => x.Amount), Cur = g.First().Currency })
            .OrderByDescending(x => x.Total)
            .Take(6)
            .Select(x => new ReportRow { Name = x.Name, AmountText = $"{x.Total:N0} {x.Cur}" })
            .ToList();

        list.ItemsSource = top6;

        v.Children.Add(list);
        card.Content = v;

        return card;
    }

    // ==================== ÝÞ MANTIÐI ====================

    async Task QuickAddAsync(string kind) // "Gelir" | "Gider"
    {
        if (Accounts.Count == 0) return;

        // Hesap seç
        var selected = await DisplayActionSheet("Hesap Seç", "Vazgeç", null, Accounts.Select(a => a.Name).ToArray());
        if (string.IsNullOrWhiteSpace(selected) || selected == "Vazgeç") return;

        var amtStr = await DisplayPromptAsync(kind, "Tutar giriniz:", "Kaydet", "Ýptal", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(amtStr)) return;
        if (!decimal.TryParse(amtStr.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                              NumberStyles.Any, CultureInfo.CurrentCulture, out var amount)) return;
        if (amount <= 0) return;

        var desc = await DisplayPromptAsync(kind, "Açýklama (isteðe baðlý):", "Devam", "Atla") ?? "";

        var acc = Accounts.First(a => a.Name == selected);

        Journal.Add(new JournalRow
        {
            Date = DateTime.Today,
            Kind = kind,
            Category = kind == "Gelir" ? "Genel" : "Genel",
            Description = string.IsNullOrWhiteSpace(desc) ? $"{kind} kaydý" : desc,
            Account = acc.Name,
            Currency = acc.Currency,
            Amount = amount
        });

        RecalcAccounts();
        ApplyJournalFilter();
        RecalcKpis();
    }

    void ApplyJournalFilter()
    {
        ViewJournal.Clear();
        IEnumerable<JournalRow> src = Journal;

        // Metin arama
        var q = Query?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(q))
            src = src.Where(x =>
                (x.Description?.ToLowerInvariant().Contains(q) ?? false) ||
                (x.Account?.ToLowerInvariant().Contains(q) ?? false) ||
                (x.Category?.ToLowerInvariant().Contains(q) ?? false));

        // Dönem
        var today = DateTime.Today;
        if (SelectedMonth == "Bu Ay")
            src = src.Where(x => x.Date.Year == today.Year && x.Date.Month == today.Month);
        else if (SelectedMonth == "Geçen Ay")
        {
            var d = today.AddMonths(-1);
            src = src.Where(x => x.Date.Year == d.Year && x.Date.Month == d.Month);
        }

        foreach (var r in src.OrderByDescending(x => x.Date))
            ViewJournal.Add(r);

        // Hesap listesi görünümü (dashboard ve hesaplar için)
        ViewAccounts.Clear();
        foreach (var a in Accounts.OrderBy(x => x.Name))
            ViewAccounts.Add(a);
    }

    void RecalcAccounts()
    {
        // Tüm bakiyeleri sýfýrla
        foreach (var a in Accounts) a.Balance = 0;

        foreach (var j in Journal)
        {
            var acc = Accounts.FirstOrDefault(x => x.Name == j.Account && x.Currency == j.Currency);
            if (acc == null) continue;

            if (j.Kind == "Gelir") acc.Balance += j.Amount;
            else if (j.Kind == "Gider") acc.Balance -= j.Amount;
        }
    }

    void RecalcKpis()
    {
        // Para birimine göre özet metinleri
        string SumBy(Func<JournalRow, bool> pred, bool sign = false)
        {
            var parts = Journal
                .Where(pred)
                .GroupBy(x => x.Currency)
                .Select(g =>
                {
                    var sum = g.Sum(x => x.Amount * (sign && x.Kind == "Gider" ? -1 : 1));
                    return $"{g.Key} {sum:N0}";
                })
                .ToList();

            return parts.Count > 0 ? string.Join(" | ", parts) : "—";
        }

        KpiIncome = SumBy(x => x.Kind == "Gelir");
        KpiExpense = SumBy(x => x.Kind == "Gider");
        KpiNet = SumBy(x => true, sign: true);

        var cashParts = Accounts
            .Where(a => a.Type is "Kasa" or "Banka")
            .GroupBy(a => a.Currency)
            .Select(g => $"{g.Key} {g.Sum(a => a.Balance):N0}")
            .ToList();
        KpiCash = cashParts.Count > 0 ? string.Join(" | ", cashParts) : "—";
    }

    static bool IsInCurrentMonth(DateTime d) =>
        d.Year == DateTime.Today.Year && d.Month == DateTime.Today.Month;

    string SumByCurrency(IEnumerable<JournalRow> rows, Func<JournalRow, decimal> sel)
    {
        var parts = rows.GroupBy(r => r.Currency)
                        .Select(g => $"{g.Sum(sel):N0} {g.Key}")
                        .ToList();
        return parts.Count > 0 ? string.Join(" | ", parts) : "—";
    }

    // ==================== SEED ====================
    void Seed()
    {
        // Hesaplar
        Accounts.Add(new AccountRow { Name = "Kasa TRY", Currency = "TRY", Type = "Kasa" });
        Accounts.Add(new AccountRow { Name = "Banka TRY", Currency = "TRY", Type = "Banka" });
        Accounts.Add(new AccountRow { Name = "Banka JPY", Currency = "JPY", Type = "Banka" });
        Accounts.Add(new AccountRow { Name = "Ödenecekler", Currency = "TRY", Type = "Borç" });

        // Yevmiye (örnek)
        var t = DateTime.Today;
        Journal.Add(new JournalRow { Date = t.AddDays(-4), Kind = "Gelir", Category = "Kira", Description = "Öðrenci kira tahsilatý", Account = "Banka JPY", Currency = "JPY", Amount = 140000 });
        Journal.Add(new JournalRow { Date = t.AddDays(-3), Kind = "Gider", Category = "Okul", Description = "Okul ödemesi", Account = "Banka TRY", Currency = "TRY", Amount = 28000 });
        Journal.Add(new JournalRow { Date = t.AddDays(-2), Kind = "Gelir", Category = "Okul", Description = "Kayýt ücreti", Account = "Kasa TRY", Currency = "TRY", Amount = 12000 });
        Journal.Add(new JournalRow { Date = t.AddDays(-1), Kind = "Gider", Category = "Ofis", Description = "Elektrik faturasý", Account = "Banka TRY", Currency = "TRY", Amount = 3500 });
        Journal.Add(new JournalRow { Date = t, Kind = "Gelir", Category = "Kira", Description = "Kira tahsilatý", Account = "Banka JPY", Currency = "JPY", Amount = 160000 });
        Journal.Add(new JournalRow { Date = t.AddDays(1), Kind = "Gider", Category = "Muhasebe", Description = "Muhasebe hizmeti", Account = "Banka TRY", Currency = "TRY", Amount = 2500 });
    }

    // ==================== MODELLER & CONVERTER ====================

    public class AccountRow
    {
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "TRY";
        public string Type { get; set; } = "Kasa"; // Kasa | Banka | Borç | Alacak | Diðer
        public decimal Balance { get; set; }

        public string BalanceDisplay => $"{Balance:N0} {Currency}";
        public string InfoLine => $"{Type} • {Currency}";
    }

    public class JournalRow
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public string Kind { get; set; } = "Gelir"; // Gelir | Gider
        public string Category { get; set; } = "Genel";
        public string Description { get; set; } = "";
        public string Account { get; set; } = "";
        public string Currency { get; set; } = "TRY";
        public decimal Amount { get; set; }

        public string AmountDisplay => $"{(Kind == "Gider" ? "-" : "")}{Amount:N0} {Currency}";
    }

    // <<< EKLENEN KÜÇÜK MODEL >>>
    public class ReportRow
    {
        public string Name { get; set; } = "";
        public string AmountText { get; set; } = "";
    }

    public class KindToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (value as string)?.ToLowerInvariant() ?? "";
            return s == "gelir" ? Colors.SeaGreen : Colors.IndianRed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TypeToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (value as string)?.ToLowerInvariant() ?? "";
            return s switch
            {
                "kasa" => Colors.SteelBlue,
                "banka" => Colors.MediumPurple,
                "borç" => Colors.DarkGray,
                "alacak" => Colors.Orange,
                _ => Colors.Gray
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
