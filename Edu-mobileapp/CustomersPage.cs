using System.Collections.ObjectModel;
using System.Windows.Input;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using Edu_mobileapp.Models;

namespace Edu_mobileapp;

public class CustomersPage : FmgLibContentPage
{
    // Tema
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    // Liste veri kaynakları
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Customer> ViewCustomers { get; } = new();

    // Filtreler
    string _searchText = "";
    public string SearchText { get => _searchText; set { _searchText = value; ApplyFilter(); } }

    public ObservableCollection<string> Genders { get; } =
        new(new[] { "Tümü", "Erkek", "Kadın", "Diğer" });

    string _selectedGenderFilter = "Tümü";
    public string SelectedGenderFilter { get => _selectedGenderFilter; set { _selectedGenderFilter = value; ApplyFilter(); } }

    // Sekme: Liste / Yeni
    bool _showForm;
    public bool ShowForm { get => _showForm; set { _showForm = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowList)); } }
    public bool ShowList => !ShowForm;

    // Form alanları
    public string FFirstName { get; set; } = "";
    public string FLastName { get; set; } = "";
    public string FGender { get; set; } = "Diğer";
    public string FAddressTR { get; set; } = "";
    public string FEmail { get; set; } = "";
    public string FPhone { get; set; } = "";
    public string FAddressJP { get; set; } = "";
    public string FLangName { get; set; } = "";
    public string FLangAddr { get; set; } = "";
    public string FPassportNo { get; set; } = "";
    public DateTime? FPassportIssued { get; set; }
    public DateTime? FPassportExpiry { get; set; }
    public string FCoaPath { get; set; } = "";
    public string FCoePath { get; set; } = "";
    public string FEmergencyName { get; set; } = "";
    public string FEmergencyPhone { get; set; } = "";
    public string FEmergencyRel { get; set; } = "";

    // Picker kaynakları
    public ObservableCollection<string> GenderOptions { get; } =
        new(new[] { "Erkek", "Kadın", "Diğer" });

    // Komutlar
    public ICommand SaveCommand { get; }
    public ICommand PickCoaCommand { get; }
    public ICommand PickCoeCommand { get; }
    public ICommand ClearFormCommand { get; }

    public CustomersPage()
    {
        Title = "Müşteri Yönetimi";
        BindingContext = this;

        SaveCommand = new Command(async () => await OnSaveAsync());
        PickCoaCommand = new Command(async () => await PickFileAsync("COA"));
        PickCoeCommand = new Command(async () => await PickFileAsync("COE"));
        ClearFormCommand = new Command(ClearForm);

        Seed();       // örnek veri
        ApplyFilter();

        Build();
    }

    public override void Build()
    {
        // --- Üst başlık ---
        var header = new Frame
        {
            CornerRadius = 16,
            Padding = new Thickness(16, 22),
            BackgroundColor = Red,
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label().Text("Müşteri Yönetimi").FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White).HorizontalTextAlignment(TextAlignment.Center),
                    new Label().Text("Liste / Arama / Ekle").TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // --- Sekmeler ---
        var tabs = TabsGrid();

        var headerStack = new VerticalStackLayout
        {
            Padding = 10,
            Spacing = 12,
            Children = { header, tabs }
        };

        // --- Liste alanı ---
        var listRoot = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        listRoot.SetBinding(IsVisibleProperty, nameof(ShowList));

        listRoot.Children.Add(SearchFilterGrid()); // Arama + Cinsiyet filtresi

        // CollectionView (kart + swipe)
        var cv = new CollectionView
        {
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 },
            ItemTemplate = new DataTemplate(() =>
            {
                var swipe = new SwipeView();

                var edit = new SwipeItem { Text = "Düzenle", BackgroundColor = Colors.Orange };
                edit.Command = new Command<object?>(item => EditCustomer(item as Customer));
                edit.SetBinding(SwipeItem.CommandParameterProperty, new Binding("."));
                swipe.LeftItems = new SwipeItems { edit };

                var del = new SwipeItem { Text = "Sil", BackgroundColor = Colors.Red };
                del.Command = new Command<object?>(item => DeleteCustomer(item as Customer));
                del.SetBinding(SwipeItem.CommandParameterProperty, new Binding("."));
                swipe.RightItems = new SwipeItems { del };

                var frame = new Frame { CornerRadius = 14, Padding = 12, BackgroundColor = Colors.White, BorderColor = LightGray };

                var inner = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{ Width = GridLength.Star },
                        new ColumnDefinition{ Width = GridLength.Auto }
                    }
                };

                var left = new VerticalStackLayout { Spacing = 4 };
                var name = new Label().FontAttributes(FontAttributes.Bold).FontSize(18);
                name.SetBinding(Label.TextProperty, nameof(Customer.FullName));
                var email = new Label().TextColor(Colors.Gray); email.SetBinding(Label.TextProperty, nameof(Customer.Email));
                var phone = new Label().TextColor(Colors.Gray); phone.SetBinding(Label.TextProperty, nameof(Customer.Phone));

                var badges = new HorizontalStackLayout { Spacing = 6 };
                badges.Children.Add(Badge("Cinsiyet:", nameof(Customer.Gender)));
                badges.Children.Add(Badge("Pasaport:", nameof(Customer.PassportNo)));

                left.Children.Add(name);
                left.Children.Add(email);
                left.Children.Add(phone);
                left.Children.Add(badges);

                Grid.SetColumn(left, 0);
                inner.Children.Add(left);

                var detailBtn = new Button
                {
                    Text = "Detay",
                    BackgroundColor = Red,
                    TextColor = Colors.White,
                    CornerRadius = 12,
                    Padding = new Thickness(14, 8)
                };
                detailBtn.Command = new Command<object?>(item => ShowDetail(item as Customer));
                detailBtn.SetBinding(Button.CommandParameterProperty, new Binding("."));
                Grid.SetColumn(detailBtn, 1);
                inner.Children.Add(detailBtn);

                frame.Content = inner;
                swipe.Content = frame;
                return swipe;
            })
        };
        cv.SetBinding(ItemsView.ItemsSourceProperty, nameof(ViewCustomers));
        listRoot.Children.Add(cv);

        // --- Form alanı ---
        var formRoot = new ScrollView();
        formRoot.SetBinding(IsVisibleProperty, nameof(ShowForm));

        var formStack = new VerticalStackLayout { Padding = 16, Spacing = 10 };

        formStack.Children.Add(SectionTitle("Kimlik & İletişim"));
        formStack.Children.Add(TwoCols(
            LabeledEntry("Ad", nameof(FFirstName)),
            LabeledEntry("Soyad", nameof(FLastName))
        ));

        var rowGenderPhone = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star }
            }
        };
        var gender = LabeledPicker("Cinsiyet", nameof(GenderOptions), nameof(FGender));
        Grid.SetColumn(gender, 0);
        var phone = LabeledEntry("Telefon", nameof(FPhone));
        Grid.SetColumn(phone, 1);
        rowGenderPhone.Children.Add(gender);
        rowGenderPhone.Children.Add(phone);
        formStack.Children.Add(rowGenderPhone);

        formStack.Children.Add(LabeledEntry("E-posta", nameof(FEmail), Keyboard.Email));
        formStack.Children.Add(LabeledEditor("Türkiye Adresi", nameof(FAddressTR)));
        formStack.Children.Add(LabeledEditor("Japonya’daki Adres", nameof(FAddressJP)));

        formStack.Children.Add(SectionTitle("Okul Bilgisi"));
        formStack.Children.Add(LabeledEntry("Dil Okulu Adı", nameof(FLangName)));
        formStack.Children.Add(LabeledEditor("Dil Okulu Adresi", nameof(FLangAddr)));

        formStack.Children.Add(SectionTitle("Pasaport Bilgisi"));
        formStack.Children.Add(TwoCols(
            LabeledEntry("Pasaport No", nameof(FPassportNo)),
            LabeledDate("Düzenlenme", nameof(FPassportIssued))
        ));
        formStack.Children.Add(LabeledDate("Geçerlilik", nameof(FPassportExpiry)));

        formStack.Children.Add(SectionTitle("Belgeler"));
        formStack.Children.Add(FilePickRow("COA Belgesi", nameof(FCoaPath), PickCoaCommand));
        formStack.Children.Add(FilePickRow("COE Belgesi", nameof(FCoePath), PickCoeCommand));

        formStack.Children.Add(SectionTitle("Acil Durum"));
        formStack.Children.Add(TwoCols(
            LabeledEntry("Kişi Adı", nameof(FEmergencyName)),
            LabeledEntry("Yakınlık", nameof(FEmergencyRel))
        ));
        formStack.Children.Add(LabeledEntry("Telefon", nameof(FEmergencyPhone)));

        var actions = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Auto }
            },
            Margin = new Thickness(0, 10, 0, 24)
        };
        var clearBtn = new Button().Text("Temizle").CornerRadius(14).BackgroundColor(LightGray).TextColor(Colors.Black);
        clearBtn.SetBinding(Button.CommandProperty, nameof(ClearFormCommand));
        Grid.SetColumn(clearBtn, 0);
        var saveBtn = new Button().Text("Kaydet").CornerRadius(14).BackgroundColor(Red).TextColor(Colors.White);
        saveBtn.SetBinding(Button.CommandProperty, nameof(SaveCommand));
        Grid.SetColumn(saveBtn, 1);
        actions.Children.Add(clearBtn);
        actions.Children.Add(saveBtn);
        formStack.Children.Add(actions);

        formRoot.Content = formStack;

        // --- Kök Grid: 2 satır (header, içerik) ---
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Star }
            }
        };

        Grid.SetRow(headerStack, 0);
        root.Children.Add(headerStack);

        var contentGrid = new Grid(); // iki view'yi üst üste koyuyoruz, IsVisible ile biri açık
        Grid.SetRow(contentGrid, 1);

        contentGrid.Children.Add(listRoot);
        contentGrid.Children.Add(formRoot);
        root.Children.Add(contentGrid);

        this.BackgroundColor(Colors.White).Content(root);
    }

    // ---------- Yardımcı UI parçaları ----------
    private View TabsGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star }
            },
            ColumnSpacing = 12,                 // butonlar arası boşluk
            Margin = new Thickness(0, 6, 0, 0)
        };

        var btnListe = new Button
        {
            Text = "Liste",
            CornerRadius = 12,
            HeightRequest = 44,
            BorderColor = Red,
            BorderWidth = 1,
            TextColor = Red,
            BackgroundColor = Colors.White,
            Command = new Command(() => ShowForm = false)
        };

        var btnYeni = new Button
        {
            Text = "Yeni",
            CornerRadius = 12,
            HeightRequest = 44,
            BorderColor = Red,
            BorderWidth = 1,
            TextColor = Red,
            BackgroundColor = Colors.White,
            Command = new Command(() => ShowForm = true)
        };

        Grid.SetColumn(btnListe, 0);
        Grid.SetColumn(btnYeni, 1);
        grid.Children.Add(btnListe);
        grid.Children.Add(btnYeni);
        return grid;
    }

    private View SearchFilterGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 12
        };

        var txt = new Entry { Placeholder = "İsim / Pasaport / E-posta ara...", BackgroundColor = LightGray, HeightRequest = 44 };
        txt.SetBinding(Entry.TextProperty, nameof(SearchText));
        Grid.SetColumn(txt, 0);

        var pick = new Picker { Title = "Cinsiyet", BackgroundColor = LightGray, HeightRequest = 44 };
        pick.SetBinding(Picker.ItemsSourceProperty, nameof(Genders));
        pick.SetBinding(Picker.SelectedItemProperty, nameof(SelectedGenderFilter));
        Grid.SetColumn(pick, 1);

        grid.Children.Add(txt);
        grid.Children.Add(pick);
        return grid;
    }

    private View Badge(string label, string bindPath) =>
        new Frame
        {
            Padding = new Thickness(10, 4),
            CornerRadius = 10,
            BackgroundColor = LightGray,
            Content = new HorizontalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label().Text(label).FontSize(12).TextColor(Colors.Black),
                    new Label()
                        .FontSize(12)
                        .TextColor(Colors.Black)
                        .Text(e => e.Path(bindPath))
                }
            }
        };

    private View SectionTitle(string text) =>
        new Label().Text(text).FontSize(16).FontAttributes(FontAttributes.Bold).TextColor(Red).Margin(new Thickness(0, 10, 0, 0));

    private Grid TwoCols(View left, View right)
    {
        var g = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star }
            }
        };
        Grid.SetColumn(left, 0); g.Children.Add(left);
        Grid.SetColumn(right, 1); g.Children.Add(right);
        return g;
    }

    private View LabeledEntry(string label, string bindPath, Keyboard? kb = null)
    {
        var entry = new Entry { BackgroundColor = LightGray, HeightRequest = 44 };
        entry.SetBinding(Entry.TextProperty, bindPath);
        if (kb != null) entry.Keyboard = kb;

        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                entry
            }
        };
    }

    private View LabeledEditor(string label, string bindPath)
    {
        var editor = new Editor { BackgroundColor = LightGray, HeightRequest = 80, AutoSize = EditorAutoSizeOption.TextChanges };
        editor.SetBinding(Editor.TextProperty, bindPath);

        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                editor
            }
        };
    }

    private View LabeledPicker(string label, string itemsSourcePath, string selectedItemPath)
    {
        var picker = new Picker { BackgroundColor = LightGray, HeightRequest = 44 };
        picker.SetBinding(Picker.ItemsSourceProperty, itemsSourcePath);
        picker.SetBinding(Picker.SelectedItemProperty, selectedItemPath);

        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                picker
            }
        };
    }

    private View LabeledDate(string label, string bindPath)
    {
        var dp = new DatePicker { BackgroundColor = LightGray, HeightRequest = 44, Format = "dd.MM.yyyy" };
        dp.SetBinding(DatePicker.DateProperty, bindPath);
        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                dp
            }
        };
    }

    private View FilePickRow(string label, string pathBind, ICommand command)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Auto },
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Auto }
            }
        };

        var l1 = new Label { Text = label, VerticalTextAlignment = TextAlignment.Center };
        Grid.SetColumn(l1, 0);

        var l2 = new Label { TextColor = Colors.Gray, LineBreakMode = LineBreakMode.TailTruncation };
        l2.SetBinding(Label.TextProperty, pathBind);
        Grid.SetColumn(l2, 1);

        var btn = new Button { Text = "Seç", BackgroundColor = Red, TextColor = Colors.White, CornerRadius = 10, Command = command };
        Grid.SetColumn(btn, 2);

        grid.Children.Add(l1); grid.Children.Add(l2); grid.Children.Add(btn);
        return grid;
    }

    // ---------- İş mantığı ----------
    private async Task PickFileAsync(string which)
    {
        var pdfTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android,    new[] { "application/pdf" } },
            { DevicePlatform.iOS,        new[] { "com.adobe.pdf"  } },
            { DevicePlatform.MacCatalyst,new[] { "com.adobe.pdf"  } },
            { DevicePlatform.WinUI,      new[] { ".pdf"           } },
        });

        var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = $"{which} Belgesi Seç", FileTypes = pdfTypes });
        if (result is null) return;

        if (which == "COA") FCoaPath = result.FullPath;
        else FCoePath = result.FullPath;

        OnPropertyChanged(nameof(FCoaPath));
        OnPropertyChanged(nameof(FCoePath));
    }

    private async Task OnSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FFirstName) || string.IsNullOrWhiteSpace(FEmail))
        { await DisplayAlert("Eksik bilgi", "Ad ve E-posta zorunludur.", "Tamam"); return; }

        var c = new Customer
        {
            FirstName = FFirstName,
            LastName = FLastName,
            Gender = FGender,
            AddressTR = FAddressTR,
            Email = FEmail,
            Phone = FPhone,
            AddressJP = FAddressJP,
            LanguageSchoolName = FLangName,
            LanguageSchoolAddress = FLangAddr,
            PassportNo = FPassportNo,
            PassportIssuedDate = FPassportIssued,
            PassportExpiryDate = FPassportExpiry,
            CoaFilePath = FCoaPath,
            CoeFilePath = FCoePath,
            EmergencyContactName = FEmergencyName,
            EmergencyContactPhone = FEmergencyPhone,
            EmergencyContactRelation = FEmergencyRel
        };

        Customers.Add(c);
        ApplyFilter();
        await DisplayAlert("Kaydedildi", $"{c.FullName} eklendi.", "Tamam");
        ClearForm();
        ShowForm = false;
    }

    private void ClearForm()
    {
        FFirstName = FLastName = FAddressTR = FEmail = FPhone = FAddressJP = "";
        FLangName = FLangAddr = FPassportNo = "";
        FGender = "Diğer";
        FPassportIssued = FPassportExpiry = null;
        FCoaPath = FCoePath = "";
        FEmergencyName = FEmergencyPhone = FEmergencyRel = "";
        OnPropertyChanged(null);
    }

    private void ApplyFilter()
    {
        ViewCustomers.Clear();
        var q = _searchText?.Trim().ToLowerInvariant() ?? "";

        IEnumerable<Customer> src = Customers;

        if (!string.IsNullOrEmpty(q))
        {
            src = src.Where(c =>
                (c.FullName?.ToLowerInvariant().Contains(q) ?? false) ||
                (c.Email?.ToLowerInvariant().Contains(q) ?? false) ||
                (c.PassportNo?.ToLowerInvariant().Contains(q) ?? false) ||
                (c.Phone?.ToLowerInvariant().Contains(q) ?? false));
        }

        if (SelectedGenderFilter != "Tümü")
            src = src.Where(c => string.Equals(c.Gender, SelectedGenderFilter, StringComparison.OrdinalIgnoreCase));

        foreach (var c in src) ViewCustomers.Add(c);
    }

    private void EditCustomer(Customer? c)
    {
        if (c is null) return;

        FFirstName = c.FirstName; FLastName = c.LastName; FGender = c.Gender;
        FAddressTR = c.AddressTR; FEmail = c.Email; FPhone = c.Phone;
        FAddressJP = c.AddressJP; FLangName = c.LanguageSchoolName; FLangAddr = c.LanguageSchoolAddress;
        FPassportNo = c.PassportNo; FPassportIssued = c.PassportIssuedDate; FPassportExpiry = c.PassportExpiryDate;
        FCoaPath = c.CoaFilePath; FCoePath = c.CoeFilePath;
        FEmergencyName = c.EmergencyContactName; FEmergencyPhone = c.EmergencyContactPhone; FEmergencyRel = c.EmergencyContactRelation;

        Customers.Remove(c);
        ApplyFilter();

        ShowForm = true;
        OnPropertyChanged(null);
    }

    private async void DeleteCustomer(Customer? c)
    {
        if (c is null) return;
        var ok = await DisplayAlert("Sil", $"{c.FullName} kaydını silmek istiyor musunuz?", "Evet", "Hayır");
        if (!ok) return;
        Customers.Remove(c);
        ApplyFilter();
    }

    private async void ShowDetail(Customer? c)
    {
        if (c is null) return;
        await DisplayAlert("Müşteri Detayı",
            $"{c.FullName}\nE-posta: {c.Email}\nTelefon: {c.Phone}\nPasaport: {c.PassportNo}\nCinsiyet: {c.Gender}",
            "Kapat");
    }

    private void Seed()
    {
        Customers.Add(new Customer
        {
            FirstName = "Aylin",
            LastName = "Tok",
            Gender = "Kadın",
            Email = "aylin.t@ex.com",
            Phone = "+81 90 111 22 33",
            PassportNo = "TR123456",
            LanguageSchoolName = "Shibuya Language School"
        });
        Customers.Add(new Customer
        {
            FirstName = "Mehmet",
            LastName = "Yılmaz",
            Gender = "Erkek",
            Email = "mehmet.y@ex.com",
            Phone = "+90 532 000 00 00",
            PassportNo = "TR789012",
            LanguageSchoolName = "Tokyo Language Academy"
        });
        Customers.Add(new Customer
        {
            FirstName = "Ece",
            LastName = "Kaya",
            Gender = "Kadın",
            Email = "ece.kaya@ex.com",
            Phone = "+90 555 111 22 33",
            PassportNo = "TR456789",
            LanguageSchoolName = "Kyoto Language Center"
        });
        Customers.Add(new Customer
        {
            FirstName = "Emre",
            LastName = "Demir",
            Gender = "Erkek",
            Email = "emre.demir@ex.com",
            Phone = "+90 532 222 33 44",
            PassportNo = "TR654321",
            LanguageSchoolName = "Osaka Nihongo Gakkou"
        });
        Customers.Add(new Customer
        {
            FirstName = "Yavuz",
            LastName = "Nil",
            Gender = "Kadın",
            Email = "yuki.n@ex.com",
            Phone = "+81 80 555 66 77",
            PassportNo = "JP998877",
            LanguageSchoolName = "Shinjuku Japanese Institute"
        });
        Customers.Add(new Customer
        {
            FirstName = "Kerim",
            LastName = "Salih",
            Gender = "Erkek",
            Email = "kenji.s@ex.com",
            Phone = "+81 80 444 55 66",
            PassportNo = "JP112233",
            LanguageSchoolName = "Fukuoka Language School"
        });
        Customers.Add(new Customer
        {
            FirstName = "Zeynep",
            LastName = "Aydın",
            Gender = "Kadın",
            Email = "zeynep.aydin@ex.com",
            Phone = "+90 533 444 55 66",
            PassportNo = "TR102938",
            LanguageSchoolName = "Nagoya Language Academy"
        });
        Customers.Add(new Customer
        {
            FirstName = "Ali",
            LastName = "Can",
            Gender = "Erkek",
            Email = "ali.can@ex.com",
            Phone = "+90 534 777 88 99",
            PassportNo = "TR564738",
            LanguageSchoolName = "Sapporo Nihongo Center"
        });
        Customers.Add(new Customer
        {
            FirstName = "Elif",
            LastName = "Çelik",
            Gender = "Kadın",
            Email = "elif.celik@ex.com",
            Phone = "+90 542 111 44 55",
            PassportNo = "TR847362",
            LanguageSchoolName = "Hiroshima Language School"
        });
        Customers.Add(new Customer
        {
            FirstName = "Hilmi",
            LastName = "Susuz",
            Gender = "Erkek",
            Email = "hilmi.s@.com",
            Phone = "+81 70 222 33 44",
            PassportNo = "JP334455",
            LanguageSchoolName = "Kobe Japanese Academy"
        });
    }
}






/*
 using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;
using FmgLib.MauiMarkup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using Edu_mobileapp.Models;

namespace Edu_mobileapp;

public class CustomersPage : FmgLibContentPage
{
    // Tema
    static readonly Color Red = Color.FromArgb("#E50914");
    static readonly Color LightGray = Color.FromArgb("#F3F4F6");

    // Liste veri kaynakları
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Customer> ViewCustomers { get; } = new();

    // Filtreler
    string _searchText = "";
    public string SearchText { get => _searchText; set { _searchText = value; ApplyFilter(); } }

    public ObservableCollection<string> Genders { get; } =
        new(new[] { "Tümü", "Erkek", "Kadın", "Diğer" });

    string _selectedGenderFilter = "Tümü";
    public string SelectedGenderFilter { get => _selectedGenderFilter; set { _selectedGenderFilter = value; ApplyFilter(); } }

    // Sekme: Liste / Yeni
    bool _showForm;
    public bool ShowForm { get => _showForm; set { _showForm = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowList)); } }
    public bool ShowList => !ShowForm;

    // Form alanları
    public string FFirstName { get; set; } = "";
    public string FLastName { get; set; } = "";
    public string FGender { get; set; } = "Diğer";
    public string FAddressTR { get; set; } = "";
    public string FEmail { get; set; } = "";
    public string FPhone { get; set; } = "";
    public string FAddressJP { get; set; } = "";
    public string FLangName { get; set; } = "";
    public string FLangAddr { get; set; } = "";
    public string FPassportNo { get; set; } = "";
    public DateTime? FPassportIssued { get; set; }
    public DateTime? FPassportExpiry { get; set; }
    public string FCoaPath { get; set; } = "";
    public string FCoePath { get; set; } = "";
    public string FEmergencyName { get; set; } = "";
    public string FEmergencyPhone { get; set; } = "";
    public string FEmergencyRel { get; set; } = "";

    // Picker kaynakları
    public ObservableCollection<string> GenderOptions { get; } =
        new(new[] { "Erkek", "Kadın", "Diğer" });

    // Komutlar
    public ICommand SaveCommand { get; }
    public ICommand PickCoaCommand { get; }
    public ICommand PickCoeCommand { get; }
    public ICommand ClearFormCommand { get; }

    public CustomersPage()
    {
        Title = "Müşteri Yönetimi";
        BindingContext = this;

        SaveCommand = new Command(async () => await OnSaveAsync());
        PickCoaCommand = new Command(async () => await PickFileAsync("COA"));
        PickCoeCommand = new Command(async () => await PickFileAsync("COE"));
        ClearFormCommand = new Command(ClearForm);

        Seed();       // örnek veri
        ApplyFilter();

        Build();
    }

    public override void Build()
    {
        // --- Üst başlık ---
        var header = new Frame
        {
            CornerRadius = 16,
            Padding = new Thickness(16, 22),
            BackgroundColor = Red,
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label().Text("Müşteri Yönetimi").FontSize(26).FontAttributes(FontAttributes.Bold)
                               .TextColor(Colors.White).HorizontalTextAlignment(TextAlignment.Center),
                    new Label().Text("Liste / Arama / Ekle").TextColor(Colors.White).Opacity(0.9)
                               .HorizontalTextAlignment(TextAlignment.Center)
                }
            }
        };

        // --- Sekmeler ---
        var tabs = TabsGrid();

        var headerStack = new VerticalStackLayout
        {
            Padding = 10,
            Spacing = 12,
            Children = { header, tabs }
        };
       

        // --- Liste alanı ---
        var listRoot = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        listRoot.SetBinding(IsVisibleProperty, nameof(ShowList));

        listRoot.Children.Add(SearchFilterGrid()); // Arama + Cinsiyet filtresi

        // CollectionView (kart + swipe)
        var cv = new CollectionView
        {
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 },
            ItemTemplate = new DataTemplate(() =>
            {
                var swipe = new SwipeView();

                var edit = new SwipeItem { Text = "Düzenle", BackgroundColor = Colors.Orange };
                edit.Command = new Command<object?>(item => EditCustomer(item as Customer));
                edit.SetBinding(SwipeItem.CommandParameterProperty, new Binding("."));
                swipe.LeftItems = new SwipeItems { edit };

                var del = new SwipeItem { Text = "Sil", BackgroundColor = Colors.Red };
                del.Command = new Command<object?>(item => DeleteCustomer(item as Customer));
                del.SetBinding(SwipeItem.CommandParameterProperty, new Binding("."));
                swipe.RightItems = new SwipeItems { del };

                var frame = new Frame { CornerRadius = 14, Padding = 12, BackgroundColor = Colors.White, BorderColor = LightGray };

                var inner = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{ Width = GridLength.Star },
                        new ColumnDefinition{ Width = GridLength.Auto }
                    }
                };

                var left = new VerticalStackLayout { Spacing = 4 };
                var name = new Label().FontAttributes(FontAttributes.Bold).FontSize(18);
                name.SetBinding(Label.TextProperty, nameof(Customer.FullName));
                var email = new Label().TextColor(Colors.Gray); email.SetBinding(Label.TextProperty, nameof(Customer.Email));
                var phone = new Label().TextColor(Colors.Gray); phone.SetBinding(Label.TextProperty, nameof(Customer.Phone));

                var badges = new HorizontalStackLayout { Spacing = 6 };
                badges.Children.Add(Badge("Cinsiyet:", nameof(Customer.Gender)));
                badges.Children.Add(Badge("Pasaport:", nameof(Customer.PassportNo)));

                left.Children.Add(name);
                left.Children.Add(email);
                left.Children.Add(phone);
                left.Children.Add(badges);

                Grid.SetColumn(left, 0);
                inner.Children.Add(left);

                var detailBtn = new Button
                {
                    Text = "Detay",
                    BackgroundColor = Red,
                    TextColor = Colors.White,
                    CornerRadius = 12,
                    Padding = new Thickness(14, 8)
                };
                detailBtn.Command = new Command<object?>(item => ShowDetail(item as Customer));
                detailBtn.SetBinding(Button.CommandParameterProperty, new Binding("."));
                Grid.SetColumn(detailBtn, 1);
                inner.Children.Add(detailBtn);

                frame.Content = inner;
                swipe.Content = frame;
                return swipe;
            })
        };
        cv.SetBinding(ItemsView.ItemsSourceProperty, nameof(ViewCustomers));
        listRoot.Children.Add(cv);

        // --- Form alanı ---
        var formRoot = new ScrollView();
        formRoot.SetBinding(IsVisibleProperty, nameof(ShowForm));

        var formStack = new VerticalStackLayout { Padding = 16, Spacing = 10 };

        formStack.Children.Add(SectionTitle("Kimlik & İletişim"));
        formStack.Children.Add(TwoCols(
            LabeledEntry("Ad", nameof(FFirstName)),
            LabeledEntry("Soyad", nameof(FLastName))
        ));

        var rowGenderPhone = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star }
            }
        };
        var gender = LabeledPicker("Cinsiyet", nameof(GenderOptions), nameof(FGender));
        Grid.SetColumn(gender, 0);
        var phone = LabeledEntry("Telefon", nameof(FPhone));
        Grid.SetColumn(phone, 1);
        rowGenderPhone.Children.Add(gender);
        rowGenderPhone.Children.Add(phone);
        formStack.Children.Add(rowGenderPhone);

        formStack.Children.Add(LabeledEntry("E-posta", nameof(FEmail), Keyboard.Email));
        formStack.Children.Add(LabeledEditor("Türkiye Adresi", nameof(FAddressTR)));
        formStack.Children.Add(LabeledEditor("Japonya’daki Adres", nameof(FAddressJP)));

        formStack.Children.Add(SectionTitle("Okul Bilgisi"));
        formStack.Children.Add(LabeledEntry("Dil Okulu Adı", nameof(FLangName)));
        formStack.Children.Add(LabeledEditor("Dil Okulu Adresi", nameof(FLangAddr)));

        formStack.Children.Add(SectionTitle("Pasaport Bilgisi"));
        formStack.Children.Add(TwoCols(
            LabeledEntry("Pasaport No", nameof(FPassportNo)),
            LabeledDate("Düzenlenme", nameof(FPassportIssued))
        ));
        formStack.Children.Add(LabeledDate("Geçerlilik", nameof(FPassportExpiry)));

        formStack.Children.Add(SectionTitle("Belgeler"));
        formStack.Children.Add(FilePickRow("COA Belgesi", nameof(FCoaPath), PickCoaCommand));
        formStack.Children.Add(FilePickRow("COE Belgesi", nameof(FCoePath), PickCoeCommand));

        formStack.Children.Add(SectionTitle("Acil Durum"));
        formStack.Children.Add(TwoCols(
            LabeledEntry("Kişi Adı", nameof(FEmergencyName)),
            LabeledEntry("Yakınlık", nameof(FEmergencyRel))
        ));
        formStack.Children.Add(LabeledEntry("Telefon", nameof(FEmergencyPhone)));

        var actions = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Auto }
            },
            Margin = new Thickness(0, 10, 0, 24)
        };
        var clearBtn = new Button().Text("Temizle").CornerRadius(14).BackgroundColor(LightGray).TextColor(Colors.Black);
        clearBtn.SetBinding(Button.CommandProperty, nameof(ClearFormCommand));
        Grid.SetColumn(clearBtn, 0);
        var saveBtn = new Button().Text("Kaydet").CornerRadius(14).BackgroundColor(Red).TextColor(Colors.White);
        saveBtn.SetBinding(Button.CommandProperty, nameof(SaveCommand));
        Grid.SetColumn(saveBtn, 1);
        actions.Children.Add(clearBtn);
        actions.Children.Add(saveBtn);
        formStack.Children.Add(actions);

        formRoot.Content = formStack;

        // --- Kök Grid: 2 satır (header, içerik) ---
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Star }
            }
        };

        Grid.SetRow(headerStack, 0);
        root.Children.Add(headerStack);

        var contentGrid = new Grid(); // iki view'yi üst üste koyuyoruz, IsVisible ile biri açık
        Grid.SetRow(contentGrid, 1);

        contentGrid.Children.Add(listRoot);
        contentGrid.Children.Add(formRoot);
        root.Children.Add(contentGrid);

        this.BackgroundColor(Colors.White).Content(root);
    }

    // ---------- Yardımcı UI parçaları ----------
    private View TabsGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star }
            }
        };

        var btnListe = new Button
        {
            Text = "Liste",
            CornerRadius = 12,
            HeightRequest = 44,
            BorderColor = Red,
            BorderWidth = 1,
            TextColor = Red,
            BackgroundColor = Colors.White,
            Command = new Command(() => ShowForm = false)
        };

        var btnYeni = new Button
        {
            Text = "Yeni",
            CornerRadius = 12,
            HeightRequest = 44,
            BorderColor = Red,
            BorderWidth = 1,
            TextColor = Red,
            BackgroundColor = Colors.White,
            Command = new Command(() => ShowForm = true)
        };

        Grid.SetColumn(btnListe, 0);
        Grid.SetColumn(btnYeni, 1);
        grid.Children.Add(btnListe);
        grid.Children.Add(btnYeni);
        return grid;
    }

    private View SearchFilterGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Star) }
            }
        };

        var txt = new Entry { Placeholder = "İsim / Pasaport / E-posta ara...", BackgroundColor = LightGray, HeightRequest = 44 };
        txt.SetBinding(Entry.TextProperty, nameof(SearchText));
        Grid.SetColumn(txt, 0);

        var pick = new Picker { Title = "Cinsiyet", BackgroundColor = LightGray, HeightRequest = 44 };
        pick.SetBinding(Picker.ItemsSourceProperty, nameof(Genders));
        pick.SetBinding(Picker.SelectedItemProperty, nameof(SelectedGenderFilter));
        Grid.SetColumn(pick, 1);

        grid.Children.Add(txt);
        grid.Children.Add(pick);
        return grid;
    }

    private View Badge(string label, string bindPath) =>
    new Frame
    {
        Padding = new Thickness(10, 4),
        CornerRadius = 10,
        BackgroundColor = LightGray,
        Content = new HorizontalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label().Text(label).FontSize(12).TextColor(Colors.Black),
                new Label()
                    .FontSize(12)
                    .TextColor(Colors.Black)
                    .Text(e => e.Path(bindPath)) // <-- Apply yok
            }
        }
    };


    private View SectionTitle(string text) =>
        new Label().Text(text).FontSize(16).FontAttributes(FontAttributes.Bold).TextColor(Red).Margin(new Thickness(0, 10, 0, 0));

    private Grid TwoCols(View left, View right)
    {
        var g = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star }
            }
        };
        Grid.SetColumn(left, 0); g.Children.Add(left);
        Grid.SetColumn(right, 1); g.Children.Add(right);
        return g;
    }

    private View LabeledEntry(string label, string bindPath, Keyboard? kb = null)
    {
        var entry = new Entry { BackgroundColor = LightGray, HeightRequest = 44 };
        entry.SetBinding(Entry.TextProperty, bindPath);
        if (kb != null) entry.Keyboard = kb;

        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                entry
            }
        };
    }

    private View LabeledEditor(string label, string bindPath)
    {
        var editor = new Editor { BackgroundColor = LightGray, HeightRequest = 80, AutoSize = EditorAutoSizeOption.TextChanges };
        editor.SetBinding(Editor.TextProperty, bindPath);

        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                editor
            }
        };
    }

    private View LabeledPicker(string label, string itemsSourcePath, string selectedItemPath)
    {
        var picker = new Picker { BackgroundColor = LightGray, HeightRequest = 44 };
        picker.SetBinding(Picker.ItemsSourceProperty, itemsSourcePath);
        picker.SetBinding(Picker.SelectedItemProperty, selectedItemPath);

        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                picker
            }
        };
    }

    private View LabeledDate(string label, string bindPath)
    {
        var dp = new DatePicker { BackgroundColor = LightGray, HeightRequest = 44, Format = "dd.MM.yyyy" };
        dp.SetBinding(DatePicker.DateProperty, bindPath);
        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label().Text(label).FontSize(14),
                dp
            }
        };
    }

    private View FilePickRow(string label, string pathBind, ICommand command)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = GridLength.Auto },
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Auto }
            }
        };

        var l1 = new Label { Text = label, VerticalTextAlignment = TextAlignment.Center };
        Grid.SetColumn(l1, 0);

        var l2 = new Label { TextColor = Colors.Gray, LineBreakMode = LineBreakMode.TailTruncation };
        l2.SetBinding(Label.TextProperty, pathBind);
        Grid.SetColumn(l2, 1);

        var btn = new Button { Text = "Seç", BackgroundColor = Red, TextColor = Colors.White, CornerRadius = 10, Command = command };
        Grid.SetColumn(btn, 2);

        grid.Children.Add(l1); grid.Children.Add(l2); grid.Children.Add(btn);
        return grid;
    }

    // ---------- İş mantığı ----------
    private async Task PickFileAsync(string which)
    {
        var pdfTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "application/pdf" } },
            { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
            { DevicePlatform.MacCatalyst, new[] { "com.adobe.pdf" } },
            { DevicePlatform.WinUI, new[] { ".pdf" } },
        });

        var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = $"{which} Belgesi Seç", FileTypes = pdfTypes });
        if (result is null) return;

        if (which == "COA") FCoaPath = result.FullPath;
        else FCoePath = result.FullPath;

        OnPropertyChanged(nameof(FCoaPath));
        OnPropertyChanged(nameof(FCoePath));
    }

    private async Task OnSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FFirstName) || string.IsNullOrWhiteSpace(FEmail))
        { await DisplayAlert("Eksik bilgi", "Ad ve E-posta zorunludur.", "Tamam"); return; }

        var c = new Customer
        {
            FirstName = FFirstName,
            LastName = FLastName,
            Gender = FGender,
            AddressTR = FAddressTR,
            Email = FEmail,
            Phone = FPhone,
            AddressJP = FAddressJP,
            LanguageSchoolName = FLangName,
            LanguageSchoolAddress = FLangAddr,
            PassportNo = FPassportNo,
            PassportIssuedDate = FPassportIssued,
            PassportExpiryDate = FPassportExpiry,
            CoaFilePath = FCoaPath,
            CoeFilePath = FCoePath,
            EmergencyContactName = FEmergencyName,
            EmergencyContactPhone = FEmergencyPhone,
            EmergencyContactRelation = FEmergencyRel
        };

        Customers.Add(c);
        ApplyFilter();
        await DisplayAlert("Kaydedildi", $"{c.FullName} eklendi.", "Tamam");
        ClearForm();
        ShowForm = false;
    }

    private void ClearForm()
    {
        FFirstName = FLastName = FAddressTR = FEmail = FPhone = FAddressJP = "";
        FLangName = FLangAddr = FPassportNo = "";
        FGender = "Diğer";
        FPassportIssued = FPassportExpiry = null;
        FCoaPath = FCoePath = "";
        FEmergencyName = FEmergencyPhone = FEmergencyRel = "";
        OnPropertyChanged(null);
    }

    private void ApplyFilter()
    {
        ViewCustomers.Clear();
        var q = _searchText?.Trim().ToLowerInvariant() ?? "";

        IEnumerable<Customer> src = Customers;

        if (!string.IsNullOrEmpty(q))
        {
            src = src.Where(c =>
                (c.FullName?.ToLowerInvariant().Contains(q) ?? false) ||
                (c.Email?.ToLowerInvariant().Contains(q) ?? false) ||
                (c.PassportNo?.ToLowerInvariant().Contains(q) ?? false) ||
                (c.Phone?.ToLowerInvariant().Contains(q) ?? false));
        }

        if (SelectedGenderFilter != "Tümü")
            src = src.Where(c => string.Equals(c.Gender, SelectedGenderFilter, StringComparison.OrdinalIgnoreCase));

        foreach (var c in src) ViewCustomers.Add(c);
    }

    private void EditCustomer(Customer? c)
    {
        if (c is null) return;

        FFirstName = c.FirstName; FLastName = c.LastName; FGender = c.Gender;
        FAddressTR = c.AddressTR; FEmail = c.Email; FPhone = c.Phone;
        FAddressJP = c.AddressJP; FLangName = c.LanguageSchoolName; FLangAddr = c.LanguageSchoolAddress;
        FPassportNo = c.PassportNo; FPassportIssued = c.PassportIssuedDate; FPassportExpiry = c.PassportExpiryDate;
        FCoaPath = c.CoaFilePath; FCoePath = c.CoeFilePath;
        FEmergencyName = c.EmergencyContactName; FEmergencyPhone = c.EmergencyContactPhone; FEmergencyRel = c.EmergencyContactRelation;

        Customers.Remove(c);
        ApplyFilter();

        ShowForm = true;
        OnPropertyChanged(null);
    }

    private async void DeleteCustomer(Customer? c)
    {
        if (c is null) return;
        var ok = await DisplayAlert("Sil", $"{c.FullName} kaydını silmek istiyor musunuz?", "Evet", "Hayır");
        if (!ok) return;
        Customers.Remove(c);
        ApplyFilter();
    }

    private async void ShowDetail(Customer? c)
    {
        if (c is null) return;
        await DisplayAlert("Müşteri Detayı",
            $"{c.FullName}\nE-posta: {c.Email}\nTelefon: {c.Phone}\nPasaport: {c.PassportNo}\nCinsiyet: {c.Gender}",
            "Kapat");
    }

    private void Seed()
    {
        Customers.Add(new Customer
        {
            FirstName = "Ayako",
            LastName = "Tanaka",
            Gender = "Kadın",
            Email = "ayako.t@ex.com",
            Phone = "+81 90 111 22 33",
            PassportNo = "TR123456",
            LanguageSchoolName = "Shibuya Language School"
        });
        Customers.Add(new Customer
        {
            FirstName = "Mehmet",
            LastName = "Yılmaz",
            Gender = "Erkek",
            Email = "mehmet.y@ex.com",
            Phone = "+90 532 000 00 00",
            PassportNo = "TR789012",
            LanguageSchoolName = "Tokyo Language Academy"
        });
    }
}

 */