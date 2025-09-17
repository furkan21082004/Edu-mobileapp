using System.Collections.ObjectModel;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Edu_mobileapp.Models;

namespace Edu_mobileapp;

public class LeasePage : FmgLibContentPage
{
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<AcOption> Accommodations { get; } = new();

    Customer? _selectedCustomer;
    AcOption? _selectedAccommodation;

    DateTime _startDate = DateTime.Today;
    int _months = 12;

    Label? _lblSelectedCustomer, _lblSelectedAccommodation, _lblMonthly, _lblDeposit, _lblEnd, _lblTotal, _lblTotalWithDep;
    View? _customMonthsArea;
    Stepper? _stepperMonths;

    public LeasePage(IEnumerable<Customer>? incomingCustomers = null)
    {
        Title = "Kiralama";
        BindingContext = this;

        if (incomingCustomers != null) foreach (var c in incomingCustomers) Customers.Add(c);
        else SeedCustomers();

        SeedAccommodations();

        Build();
        Recalc();
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
                Children =
                {
                    new Label().Text("Kiralama").FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White).HorizontalTextAlignment(TextAlignment.Center),
                    new Label().Text("Müşteri seç → Konaklama seç → Süre & Onay")
                               .TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // Özet kartı
        _lblSelectedCustomer = new Label { TextColor = Colors.Black, LineBreakMode = LineBreakMode.TailTruncation };
        _lblSelectedAccommodation = new Label { TextColor = Colors.Black, LineBreakMode = LineBreakMode.TailTruncation };

        var summaryGridTop = new Grid();
        summaryGridTop.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        summaryGridTop.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        summaryGridTop.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        summaryGridTop.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        var ico1 = new Label { Text = "👤", FontSize = 16 };
        Grid.SetRow(ico1, 0); Grid.SetColumn(ico1, 0);
        var lab1 = _lblSelectedCustomer!;
        Grid.SetRow(lab1, 0); Grid.SetColumn(lab1, 1);
        var ico2 = new Label { Text = "🏠", FontSize = 16 };
        Grid.SetRow(ico2, 1); Grid.SetColumn(ico2, 0);
        var lab2 = _lblSelectedAccommodation!;
        Grid.SetRow(lab2, 1); Grid.SetColumn(lab2, 1);

        summaryGridTop.Children.Add(ico1); summaryGridTop.Children.Add(lab1);
        summaryGridTop.Children.Add(ico2); summaryGridTop.Children.Add(lab2);

        var summaryCard = new Frame
        {
            CornerRadius = 14,
            BackgroundColor = Colors.White,
            BorderColor = LightGray,
            Padding = 12,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label().Text("Seçimler").FontAttributes(FontAttributes.Bold).TextColor(Red),
                    summaryGridTop
                }
            }
        };

        // 1) Müşteri seç
        var customerList = new CollectionView
        {
            ItemsSource = Customers,
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 },
            ItemTemplate = new DataTemplate(() =>
            {
                var frame = new Frame { CornerRadius = 12, Padding = 12, BackgroundColor = Colors.White, BorderColor = LightGray };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var left = new VerticalStackLayout { Spacing = 2 };
                var name = new Label().FontAttributes(FontAttributes.Bold).FontSize(18);
                name.SetBinding(Label.TextProperty, nameof(Customer.FullName));
                var email = new Label().TextColor(Colors.Gray); email.SetBinding(Label.TextProperty, nameof(Customer.Email));
                var phone = new Label().TextColor(Colors.Gray); phone.SetBinding(Label.TextProperty, nameof(Customer.Phone));
                left.Children.Add(name); left.Children.Add(email); left.Children.Add(phone);

                Grid.SetColumn(left, 0);
                grid.Children.Add(left);

                var btn = new Button
                {
                    Text = "Seç",
                    BackgroundColor = Red,
                    TextColor = Colors.White,
                    CornerRadius = 10,
                    Padding = new Thickness(12, 8)
                };
                btn.SetBinding(Button.CommandParameterProperty, new Binding("."));
                btn.Clicked += (_, __) =>
                {
                    if (btn.CommandParameter is Customer c)
                    {
                        _selectedCustomer = c;
                        Recalc();
                    }
                };
                Grid.SetColumn(btn, 1);
                grid.Children.Add(btn);

                frame.Content = grid;
                return frame;
            })
        };

        // 2) Konaklama seç
        var acList = new CollectionView
        {
            ItemsSource = Accommodations,
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 12 },
            ItemTemplate = new DataTemplate(() =>
            {
                var card = new Frame { CornerRadius = 14, Padding = 10, BackgroundColor = Colors.White, HasShadow = true };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var imgFrame = new Frame { Padding = 0, CornerRadius = 12, HasShadow = false, IsClippedToBounds = true, BackgroundColor = LightGray, HeightRequest = 90, WidthRequest = 120 };
                var img = new Image { Aspect = Aspect.AspectFill };
                img.SetBinding(Image.SourceProperty, nameof(AcOption.ImageName));
                imgFrame.Content = img;
                Grid.SetColumn(imgFrame, 0);
                grid.Children.Add(imgFrame);

                var right = new VerticalStackLayout { Spacing = 2, Padding = new Thickness(10, 0, 0, 0) };
                var title = new Label().FontAttributes(FontAttributes.Bold).FontSize(16).TextColor(Colors.Black);
                title.SetBinding(Label.TextProperty, nameof(AcOption.Title));
                var rent = new Label().TextColor(Colors.Black);
                rent.SetBinding(Label.TextProperty, new Binding(nameof(AcOption.RentMonthly), stringFormat: "Aylık: {0:N0} JPY"));
                var dep = new Label().TextColor(Colors.Gray);
                dep.SetBinding(Label.TextProperty, new Binding(nameof(AcOption.Deposit), stringFormat: "Depozito: {0:N0} JPY"));
                right.Children.Add(title); right.Children.Add(rent); right.Children.Add(dep);
                Grid.SetColumn(right, 1);
                grid.Children.Add(right);

                var select = new Button
                {
                    Text = "Seç",
                    BackgroundColor = Red,
                    TextColor = Colors.White,
                    CornerRadius = 10,
                    Padding = new Thickness(12, 8)
                };
                select.SetBinding(Button.CommandParameterProperty, new Binding("."));
                select.Clicked += (_, __) =>
                {
                    if (select.CommandParameter is AcOption a)
                    {
                        _selectedAccommodation = a;
                        Recalc();
                    }
                };
                Grid.SetColumn(select, 2);
                grid.Children.Add(select);

                card.Content = grid;
                return card;
            })
        };

        // 3) Süre & Tarihler + Özet
        var dpStart = new DatePicker { BackgroundColor = LightGray, HeightRequest = 44, Format = "dd.MM.yyyy", Date = _startDate };
        dpStart.DateSelected += (_, __) => { _startDate = dpStart.Date; Recalc(); };

        var monthPicker = new Picker
        {
            Title = "Süre",
            BackgroundColor = LightGray,
            HeightRequest = 44,
            ItemsSource = new[] { "3", "6", "9", "12", "Özel" }
        };
        monthPicker.SelectedIndex = 3;

        _stepperMonths = new Stepper { Minimum = 1, Maximum = 36, Increment = 1, Value = 12 };
        var lblStepperVal = new Label { TextColor = Colors.Black, Text = "12 ay" };
        _stepperMonths.ValueChanged += (_, e) =>
        {
            _months = (int)e.NewValue;
            lblStepperVal.Text = $"{_months} ay";
            Recalc();
        };

        _customMonthsArea = new HorizontalStackLayout
        {
            Spacing = 10,
            IsVisible = false,
            Children = { new Label().Text("Özel süre:"), _stepperMonths, lblStepperVal }
        };

        monthPicker.SelectedIndexChanged += (_, __) =>
        {
            if (monthPicker.SelectedIndex < 0) return;
            var val = (string)monthPicker.ItemsSource![monthPicker.SelectedIndex];
            if (val == "Özel")
            {
                _customMonthsArea.IsVisible = true;
                _months = (int)_stepperMonths!.Value;
            }
            else
            {
                _customMonthsArea.IsVisible = false;
                _months = int.Parse(val);
            }
            Recalc();
        };

        _lblMonthly = new Label().TextColor(Colors.Black);
        _lblDeposit = new Label().TextColor(Colors.Black);
        _lblEnd = new Label().TextColor(Colors.Black);
        _lblTotal = new Label().FontAttributes(FontAttributes.Bold).TextColor(Red);
        _lblTotalWithDep = new Label().FontAttributes(FontAttributes.Bold).TextColor(Red);

        var details = new Grid { ColumnSpacing = 12 };
        details.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        details.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        var leftDetails = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { new Label().Text("Başlangıç Tarihi").FontAttributes(FontAttributes.Bold).TextColor(Red), dpStart }
        };
        Grid.SetColumn(leftDetails, 0);
        details.Children.Add(leftDetails);

        var rightDetails = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { new Label().Text("Süre (Ay)").FontAttributes(FontAttributes.Bold).TextColor(Red), monthPicker, _customMonthsArea }
        };
        Grid.SetColumn(rightDetails, 1);
        details.Children.Add(rightDetails);

        var summary = new Grid { Margin = new Thickness(0, 8, 0, 0) };
        summary.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        summary.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        summary.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        summary.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        summary.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        summary.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        summary.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        void AddRow(string label, View value, int row)
        {
            var l = new Label { Text = label, TextColor = Colors.Black };
            Grid.SetRow(l, row); Grid.SetColumn(l, 0);
            Grid.SetRow(value, row); Grid.SetColumn(value, 1);
            summary.Children.Add(l); summary.Children.Add(value);
        }

        AddRow("Aylık Kira:", _lblMonthly!, 0);
        AddRow("Depozito:", _lblDeposit!, 1);
        AddRow("Bitiş Tarihi:", _lblEnd!, 2);
        AddRow("Toplam (kira x ay):", _lblTotal!, 3);
        AddRow("Toplam + Depozito:", _lblTotalWithDep!, 4);

        var confirm = new Button
        {
            Text = "Onayla ve Kaydet",
            BackgroundColor = Red,
            TextColor = Colors.White,
            CornerRadius = 14,
            Padding = new Thickness(16, 10),
            Margin = new Thickness(0, 10, 0, 20)
        };
        confirm.Clicked += async (_, __) =>
        {
            if (_selectedCustomer is null) { await DisplayAlert("Uyarı", "Lütfen müşteri seçin.", "Tamam"); return; }
            if (_selectedAccommodation is null) { await DisplayAlert("Uyarı", "Lütfen konaklama seçin.", "Tamam"); return; }

            await DisplayAlert(
                "Kiralama Oluşturuldu",
                $"Müşteri: {_selectedCustomer.FullName}\n" +
                $"Konaklama: {_selectedAccommodation.Title}\n" +
                $"Süre: {_months} ay\n" +
                $"Başlangıç: {_startDate:dd.MM.yyyy}\n" +
                $"Bitiş: {_startDate.AddMonths(_months):dd.MM.yyyy}\n" +
                $"Aylık Kira: {_selectedAccommodation.RentMonthly:N0} JPY\n" +
                $"Depozito: {_selectedAccommodation.Deposit:N0} JPY\n" +
                $"Toplam (kira x ay): {(_selectedAccommodation.RentMonthly * _months):N0} JPY\n" +
                $"Toplam + Depozito: {(_selectedAccommodation.RentMonthly * _months + _selectedAccommodation.Deposit):N0} JPY",
                "Kapat");
        };

        var page = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        page.Children.Add(header);
        page.Children.Add(summaryCard);

        page.Children.Add(new Label().Text("1) Müşteri Seç").FontAttributes(FontAttributes.Bold).TextColor(Red));
        page.Children.Add(customerList);

        page.Children.Add(new Label().Text("2) Konaklama Seç").FontAttributes(FontAttributes.Bold).TextColor(Red));
        page.Children.Add(acList);

        page.Children.Add(new Label().Text("3) Süre & Tarihler").FontAttributes(FontAttributes.Bold).TextColor(Red));
        page.Children.Add(details);
        page.Children.Add(summary);
        page.Children.Add(confirm);

        this.BackgroundColor(Colors.White).Content(new ScrollView { Content = page });
    }

    private void Recalc()
    {
        _lblSelectedCustomer!.Text = _selectedCustomer is null ? "— Müşteri seçilmedi" : _selectedCustomer.FullName;
        _lblSelectedAccommodation!.Text = _selectedAccommodation is null ? "— Konaklama seçilmedi" : _selectedAccommodation.Title;

        var monthly = _selectedAccommodation?.RentMonthly ?? 0m;
        var deposit = _selectedAccommodation?.Deposit ?? 0m;

        _lblMonthly!.Text = $"{monthly:N0} JPY";
        _lblDeposit!.Text = $"{deposit:N0} JPY";
        _lblEnd!.Text = $"{_startDate.AddMonths(_months):dd.MM.yyyy}";
        _lblTotal!.Text = $"{(monthly * _months):N0} JPY";
        _lblTotalWithDep!.Text = $"{(monthly * _months + deposit):N0} JPY";
    }

    // -------- Seed veriler (müşteriler senin verdiğinle aynı) --------
    private void SeedCustomers()
    {
        Customers.Add(new Customer { FirstName = "Aylin", LastName = "Tok", Gender = "Kadın", Email = "aylin.t@ex.com", Phone = "+81 90 111 22 33", PassportNo = "TR123456", LanguageSchoolName = "Shibuya Language School" });
        Customers.Add(new Customer { FirstName = "Mehmet", LastName = "Yılmaz", Gender = "Erkek", Email = "mehmet.y@ex.com", Phone = "+90 532 000 00 00", PassportNo = "TR789012", LanguageSchoolName = "Tokyo Language Academy" });
        Customers.Add(new Customer { FirstName = "Ece", LastName = "Kaya", Gender = "Kadın", Email = "ece.kaya@ex.com", Phone = "+90 555 111 22 33", PassportNo = "TR456789", LanguageSchoolName = "Kyoto Language Center" });
        Customers.Add(new Customer { FirstName = "Emre", LastName = "Demir", Gender = "Erkek", Email = "emre.demir@ex.com", Phone = "+90 532 222 33 44", PassportNo = "TR654321", LanguageSchoolName = "Osaka Nihongo Gakkou" });
        Customers.Add(new Customer { FirstName = "Yavuz", LastName = "Nil", Gender = "Kadın", Email = "yuki.n@ex.com", Phone = "+81 80 555 66 77", PassportNo = "JP998877", LanguageSchoolName = "Shinjuku Japanese Institute" });
        Customers.Add(new Customer { FirstName = "Kerim", LastName = "Salih", Gender = "Erkek", Email = "kenji.s@ex.com", Phone = "+81 80 444 55 66", PassportNo = "JP112233", LanguageSchoolName = "Fukuoka Language School" });
        Customers.Add(new Customer { FirstName = "Zeynep", LastName = "Aydın", Gender = "Kadın", Email = "zeynep.aydin@ex.com", Phone = "+90 533 444 55 66", PassportNo = "TR102938", LanguageSchoolName = "Nagoya Language Academy" });
        Customers.Add(new Customer { FirstName = "Ali", LastName = "Can", Gender = "Erkek", Email = "ali.can@ex.com", Phone = "+90 534 777 88 99", PassportNo = "TR564738", LanguageSchoolName = "Sapporo Nihongo Center" });
        Customers.Add(new Customer { FirstName = "Elif", LastName = "Çelik", Gender = "Kadın", Email = "elif.celik@ex.com", Phone = "+90 542 111 44 55", PassportNo = "TR847362", LanguageSchoolName = "Hiroshima Language School" });
        Customers.Add(new Customer { FirstName = "Hilmi", LastName = "Susuz", Gender = "Erkek", Email = "hilmi.s@.com", Phone = "+81 70 222 33 44", PassportNo = "JP334455", LanguageSchoolName = "Kobe Japanese Academy" });
    }

    private void SeedAccommodations()
    {
        Accommodations.Add(new AcOption { Title = "TLS — Tokyo Kampüsü", ImageName = "aishin_onizleme.png", RentMonthly = 816000m, Deposit = 100000m });
        Accommodations.Add(new AcOption { Title = "Akamonkai — Tokyo", ImageName = "akamonkai_onizleme.jpg", RentMonthly = 840000m, Deposit = 120000m });
        Accommodations.Add(new AcOption { Title = "Genki — Fukuoka", ImageName = "genki_onizleme_fukuoka.jpg", RentMonthly = 820000m, Deposit = 90000m });
        Accommodations.Add(new AcOption { Title = "Genki — Kyoto", ImageName = "genki_onizleme_kyoto.jpg", RentMonthly = 830000m, Deposit = 95000m });
        Accommodations.Add(new AcOption { Title = "Genki — Tokyo", ImageName = "genki_onizleme_tokyo.jpg", RentMonthly = 860000m, Deposit = 110000m });
        Accommodations.Add(new AcOption { Title = "Nitto — Tokyo", ImageName = "nitto_onizleme.jpg", RentMonthly = 800000m, Deposit = 85000m });
    }

    // Basit DTO
    public class AcOption
    {
        public string Title { get; set; } = "";
        public string ImageName { get; set; } = "";
        public decimal RentMonthly { get; set; }
        public decimal Deposit { get; set; }
    }
}
