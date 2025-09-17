using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Edu_mobileapp;

public class PaymentsPage : FmgLibContentPage
{
    // Tema
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    // --- Veri ---
    public ObservableCollection<PayRow> Items { get; } = new();
    public ObservableCollection<PayRow> ViewItems { get; } = new();

    // Filtreler
    string _query = "";
    public string Query { get => _query; set { _query = value; ApplyFilter(); } }

    public ObservableCollection<string> Statuses { get; } =
        new(new[] { "Tümü", "Ödendi", "Parçalý", "Gecikmiþ", "Planlý" });

    string _selectedStatus = "Tümü";
    public string SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; ApplyFilter(); } }

    public ObservableCollection<string> Months { get; } =
        new(new[] { "Bu Ay", "Geçen Ay", "Tümü" });

    string _selectedMonth = "Bu Ay";
    public string SelectedMonth { get => _selectedMonth; set { _selectedMonth = value; ApplyFilter(); } }

    // KPI’lar
    string _kpiCollected = "—";
    public string KpiCollected { get => _kpiCollected; set { _kpiCollected = value; OnPropertyChanged(); } }

    string _kpiPending = "—";
    public string KpiPending { get => _kpiPending; set { _kpiPending = value; OnPropertyChanged(); } }

    string _kpiOverdue = "—";
    public string KpiOverdue { get => _kpiOverdue; set { _kpiOverdue = value; OnPropertyChanged(); } }

    string _kpiRefunds = "—";
    public string KpiRefunds { get => _kpiRefunds; set { _kpiRefunds = value; OnPropertyChanged(); } }

    // Gösterim: Genel Bakýþ / Liste
    bool _showDashboard = true;
    public bool ShowDashboard { get => _showDashboard; set { _showDashboard = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowList)); } }
    public bool ShowList => !ShowDashboard;

    public PaymentsPage()
    {
        Title = "Ödemeler";
        BindingContext = this;

        Seed();           // örnek veriler
        ApplyFilter();
        RecalcKpis();

        Build();
    }

    public override void Build()
    {
        // --- Baþlýk ---
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
                    new Label().Text("Ödemeler")
                               .FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White)
                               .HorizontalTextAlignment(TextAlignment.Center),
                    new Label().Text("Genel Bakýþ ve Tahsilatlar")
                               .TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // --- Sekmeler ---
        var tabs = new Grid { ColumnSpacing = 8 };
        tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        var btnDash = new Button { Text = "Genel Bakýþ", CornerRadius = 12, HeightRequest = 44, BorderColor = Red, BorderWidth = 1, BackgroundColor = Colors.White, TextColor = Red };
        btnDash.Clicked += (_, __) => ShowDashboard = true;
        Grid.SetColumn(btnDash, 0);

        var btnList = new Button { Text = "Tahsilatlar", CornerRadius = 12, HeightRequest = 44, BorderColor = Red, BorderWidth = 1, BackgroundColor = Colors.White, TextColor = Red };
        btnList.Clicked += (_, __) => ShowDashboard = false;
        Grid.SetColumn(btnList, 1);

        tabs.Children.Add(btnDash);
        tabs.Children.Add(btnList);

        // --- Dashboard ---
        var dash = DashboardView();
        dash.SetBinding(IsVisibleProperty, nameof(ShowDashboard));

        // --- Liste üstü: Arama + Filtreler ---
        var searchBar = new Grid { ColumnSpacing = 8, RowSpacing = 8 };
        searchBar.SetBinding(IsVisibleProperty, nameof(ShowList));
        searchBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        searchBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        searchBar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        searchBar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var txtSearch = new Entry { Placeholder = "Öðrenci / açýklama ara...", BackgroundColor = LightGray, HeightRequest = 44 };
        txtSearch.SetBinding(Entry.TextProperty, nameof(Query));
        Grid.SetColumnSpan(txtSearch, 2);
        Grid.SetRow(txtSearch, 0);
        searchBar.Children.Add(txtSearch);

        var pickStatus = new Picker { Title = "Durum", BackgroundColor = LightGray, HeightRequest = 44 };
        pickStatus.SetBinding(Picker.ItemsSourceProperty, nameof(Statuses));
        pickStatus.SetBinding(Picker.SelectedItemProperty, nameof(SelectedStatus));
        Grid.SetColumn(pickStatus, 0);
        Grid.SetRow(pickStatus, 1);
        searchBar.Children.Add(pickStatus);

        var pickMonth = new Picker { Title = "Ay", BackgroundColor = LightGray, HeightRequest = 44 };
        pickMonth.SetBinding(Picker.ItemsSourceProperty, nameof(Months));
        pickMonth.SetBinding(Picker.SelectedItemProperty, nameof(SelectedMonth));
        Grid.SetColumn(pickMonth, 1);
        Grid.SetRow(pickMonth, 1);
        searchBar.Children.Add(pickMonth);

        // --- Liste ---
        var list = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 },
            ItemTemplate = new DataTemplate(() =>
            {
                var card = new Frame { CornerRadius = 16, Padding = 12, BackgroundColor = Colors.White, BorderColor = LightGray };

                var g = new Grid { ColumnSpacing = 12, RowSpacing = 6 };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Sol: metinler
                var left = new VerticalStackLayout { Spacing = 2 };

                var name = new Label { FontAttributes = FontAttributes.Bold, FontSize = 17 };
                name.SetBinding(Label.TextProperty, nameof(PayRow.Student));

                var tags = new Label { TextColor = Colors.Gray, FontSize = 13 };
                tags.SetBinding(Label.TextProperty, nameof(PayRow.TagLine)); // Kategori • Para Birimi

                var due = new Label { TextColor = Colors.Gray, FontSize = 13 };
                due.SetBinding(Label.TextProperty, new Binding(nameof(PayRow.DueDate), stringFormat: "Vade: {0:dd.MM.yyyy}"));

                var desc = new Label { TextColor = Colors.Black, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, FontSize = 13 };
                desc.SetBinding(Label.TextProperty, nameof(PayRow.Description));

                left.Children.Add(name);
                left.Children.Add(tags);
                left.Children.Add(due);
                left.Children.Add(desc);

                Grid.SetColumn(left, 0);
                g.Children.Add(left);

                // Sað: tutarlar + durum çipi + aksiyonlar
                var right = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };

                var amount = new Label { FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End };
                amount.SetBinding(Label.TextProperty, nameof(PayRow.AmountDisplay));

                var paid = new Label { TextColor = Colors.Gray, FontSize = 12, HorizontalTextAlignment = TextAlignment.End };
                paid.SetBinding(Label.TextProperty, nameof(PayRow.PaidDisplay));

                var bal = new Label { TextColor = Colors.Gray, FontSize = 12, HorizontalTextAlignment = TextAlignment.End };
                bal.SetBinding(Label.TextProperty, nameof(PayRow.BalanceDisplay));

                var statusText = new Label { FontSize = 12, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
                statusText.SetBinding(Label.TextProperty, nameof(PayRow.Status));

                var chip = new Frame { Padding = new Thickness(8, 2), CornerRadius = 8, Content = statusText };
                chip.SetBinding(VisualElement.BackgroundColorProperty, new Binding(nameof(PayRow.Status), converter: new StatusToColor()));

                // Aksiyonlar
                var btnRow = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.End };
                var btnPay = new Button { Text = "Tahsilat", BackgroundColor = Red, TextColor = Colors.White, CornerRadius = 10, Padding = new Thickness(12, 6) };
                btnPay.Clicked += async (s, e) =>
                {
                    var item = (s as Button)?.BindingContext as PayRow;
                    if (item != null) await OnTakePaymentAsync(item);
                };
                var btnRcpt = new Button { Text = "Makbuz", BackgroundColor = LightGray, TextColor = Colors.Black, CornerRadius = 10, Padding = new Thickness(12, 6) };
                btnRcpt.Clicked += async (s, e) =>
                {
                    var item = (s as Button)?.BindingContext as PayRow;
                    if (item != null) await DisplayAlert("Makbuz", $"{item.Student}\n{item.AmountDisplay}\nDurum: {item.Status}", "Kapat");
                };
                btnRow.Children.Add(btnPay); btnRow.Children.Add(btnRcpt);

                right.Children.Add(amount);
                right.Children.Add(paid);
                right.Children.Add(bal);
                right.Children.Add(chip);
                right.Children.Add(btnRow);

                Grid.SetColumn(right, 1);
                g.Children.Add(right);

                card.Content = g;
                return card;
            })
        };
        list.SetBinding(ItemsView.ItemsSourceProperty, nameof(ViewItems));
        list.SetBinding(IsVisibleProperty, nameof(ShowList));

        // --- Kök yerleþim ---
        var root = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        root.Children.Add(header);
        root.Children.Add(tabs);
        root.Children.Add(dash);
        root.Children.Add(searchBar);
        root.Children.Add(list);

        this.BackgroundColor(Colors.White)
            .Content(new ScrollView { Content = root });
    }

    private View DashboardView()
    {
        // 4 KPI kartý (tümünü string property olarak baðlýyoruz)
        View kpi(string title, string bindProp)
        {
            var card = new Frame { CornerRadius = 16, Padding = 16, BackgroundColor = Colors.White, BorderColor = LightGray };
            var v = new VerticalStackLayout { Spacing = 6 };
            v.Children.Add(new Label { Text = title, TextColor = Colors.Gray, FontSize = 13 });
            var val = new Label { FontAttributes = FontAttributes.Bold, FontSize = 20 };
            val.SetBinding(Label.TextProperty, bindProp);
            v.Children.Add(val);
            card.Content = v;
            return card;
        }

        var g = new Grid { ColumnSpacing = 12, RowSpacing = 12 };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var c1 = kpi("Tahsil Edilen", nameof(KpiCollected));
        var c2 = kpi("Bekleyen", nameof(KpiPending));
        var c3 = kpi("Geciken", nameof(KpiOverdue));
        var c4 = kpi("Ýade", nameof(KpiRefunds));

        Grid.SetColumn(c1, 0); Grid.SetRow(c1, 0); g.Children.Add(c1);
        Grid.SetColumn(c2, 1); Grid.SetRow(c2, 0); g.Children.Add(c2);
        Grid.SetColumn(c3, 0); Grid.SetRow(c3, 1); g.Children.Add(c3);
        Grid.SetColumn(c4, 1); Grid.SetRow(c4, 1); g.Children.Add(c4);

        var container = new VerticalStackLayout();
        container.Children.Add(g);
        container.SetBinding(IsVisibleProperty, nameof(ShowDashboard));
        return container;
    }

    // ---------- Ýþ mantýðý ----------
    private async Task OnTakePaymentAsync(PayRow row)
    {
        if (row is null) return;

        var input = await DisplayPromptAsync("Tahsilat", $"{row.Student}\nKalan: {row.BalanceDisplay}\nTahsil edilecek tutar:", "Kaydet", "Vazgeç", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(input)) return;
        if (!decimal.TryParse(input.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                                   .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                              NumberStyles.Any, CultureInfo.CurrentCulture, out var amt)) return;

        if (amt <= 0) return;
        if (amt > row.Balance) amt = row.Balance;

        row.Paid += amt;
        row.Balance = Math.Max(0, row.Amount - row.Paid);

        // Durum güncelle
        row.Status = row.Balance == 0 ? "Ödendi"
                   : row.Paid > 0 ? "Parçalý"
                   : row.DueDate < DateTime.Today ? "Gecikmiþ"
                   : "Planlý";

        // UI yenile
        OnPropertyChanged(nameof(ViewItems));
        RecalcKpis();
        ApplyFilter(); // filtreler etkilenebilir
        await Task.CompletedTask;
    }

    private void ApplyFilter()
    {
        ViewItems.Clear();

        IEnumerable<PayRow> src = Items;

        // Arama
        var q = Query?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(q))
            src = src.Where(x =>
                (x.Student?.ToLowerInvariant().Contains(q) ?? false) ||
                (x.Description?.ToLowerInvariant().Contains(q) ?? false));

        // Durum
        if (SelectedStatus != "Tümü")
            src = src.Where(x => x.Status == SelectedStatus);

        // Ay
        var today = DateTime.Today;
        if (SelectedMonth == "Bu Ay")
            src = src.Where(x => x.DueDate.Year == today.Year && x.DueDate.Month == today.Month);
        else if (SelectedMonth == "Geçen Ay")
        {
            var d = today.AddMonths(-1);
            src = src.Where(x => x.DueDate.Year == d.Year && x.DueDate.Month == d.Month);
        }

        foreach (var r in src.OrderBy(x => x.DueDate).ThenBy(x => x.Student))
            ViewItems.Add(r);
    }

    private void RecalcKpis()
    {
        // Toplamlarý para birimine göre yazdýr (TRY 12.000 | JPY 820.000)
        string SumBy(Func<PayRow, decimal> selector)
        {
            var parts = Items
                .GroupBy(x => x.Currency)
                .Select(g => $"{g.Key} {g.Sum(selector):N0}")
                .ToList();
            return parts.Count > 0 ? string.Join(" | ", parts) : "—";
        }

        KpiCollected = SumBy(x => x.Paid);
        KpiPending = SumBy(x => x.Balance);
        KpiOverdue = SumBy(x => x.Status == "Gecikmiþ" ? x.Balance : 0);
        KpiRefunds = "—"; // þimdilik demo
    }

    private void Seed()
    {
        // Öðrenci – açýklama – tutar – para – vade – durum
        Items.Add(new PayRow("Aylin Tok", "Kira (Tokyo 1+1)", 180000, "JPY", DateTime.Today.AddDays(2), "Planlý"));
        Items.Add(new PayRow("Mehmet Yýlmaz", "Dil okulu taksit", 32000, "TRY", DateTime.Today.AddDays(-3), "Gecikmiþ"));
        Items.Add(new PayRow("Ece Kaya", "Depozito iadesi (-)", -8000, "TRY", DateTime.Today.AddDays(5), "Planlý"));
        Items.Add(new PayRow("Emre Demir", "Kira (Osaka 1+0)", 160000, "JPY", DateTime.Today.AddDays(1), "Parçalý", paid: 60000));
        Items.Add(new PayRow("Yavuz Nil", "Okul kayýt ücreti", 54000, "TRY", DateTime.Today.AddDays(-1), "Ödendi", paid: 54000));
        Items.Add(new PayRow("Kerim Salih", "Kira (Fukuoka 1+1)", 150000, "JPY", DateTime.Today.AddDays(9), "Planlý"));
        Items.Add(new PayRow("Zeynep Aydýn", "Okul materyal ücreti", 9000, "TRY", DateTime.Today.AddDays(6), "Planlý"));
        Items.Add(new PayRow("Ali Can", "Kira (Sapporo 1+1)", 140000, "JPY", DateTime.Today.AddDays(-4), "Gecikmiþ"));
        Items.Add(new PayRow("Elif Çelik", "Kira (Hiroshima 1+0)", 120000, "JPY", DateTime.Today.AddDays(3), "Parçalý", paid: 30000));
        Items.Add(new PayRow("Hilmi Susuz", "Dil okulu taksit", 28000, "TRY", DateTime.Today.AddDays(0), "Planlý"));
    }

    // --- Yardýmcý veri sýnýfý ---
    public class PayRow
    {
        public string Student { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }     // negatif ise iade
        public string Currency { get; set; } = "TRY";
        public DateTime DueDate { get; set; }

        public decimal Paid { get; set; }
        public decimal Balance { get; set; }

        public string Status { get; set; }      // Ödendi | Parçalý | Gecikmiþ | Planlý

        public string TagLine => $"{(Amount < 0 ? "Ýade" : "Tahsilat")} • {Currency}";
        public string AmountDisplay => $"{Amount:N0} {Currency}";
        public string PaidDisplay => $"Tahsil: {Paid:N0} {Currency}";
        public string BalanceDisplay => $"Kalan:  {Balance:N0} {Currency}";

        public PayRow(string student, string desc, decimal amount, string currency, DateTime due, string status, decimal paid = 0)
        {
            Student = student;
            Description = desc;
            Amount = Math.Abs(amount);
            Currency = currency;
            DueDate = due;

            Paid = Math.Max(0, Math.Min(paid, Amount));
            Balance = Amount - Paid;
            Status = status;
        }
    }

    // Durum -> renk dönüþtürücü
    public class StatusToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (value as string)?.ToLowerInvariant() ?? "";
            return s switch
            {
                "ödendi" => Colors.SeaGreen,
                "parçalý" => Colors.SteelBlue,
                "gecikmiþ" => Colors.IndianRed,
                _ => Colors.Orange // Planlý
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
