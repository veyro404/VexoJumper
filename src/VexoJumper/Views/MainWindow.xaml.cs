using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VexoJumper.Models;
using VexoJumper.Services;

namespace VexoJumper.Views;

public partial class MainWindow : Window
{
    private readonly NetworkService _network = new();
    private readonly DnsService _dns = new();
    private readonly TrafficService _traffic = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private CancellationTokenSource? _operation;

    private sealed record DnsPreset(string Name, string Primary, string Secondary, string Description);
    private static readonly DnsPreset[] DnsPresets =
    {
        new("الکترو (Electro)", "78.157.42.100", "78.157.42.101", "گزینه پیشنهادی کاربر"),
        new("شکن (Shecan)", "178.22.122.100", "185.51.200.2", "DNS مناسب دسترسی به برخی دامنه‌ها"),
        new("بگذر (Begzar)", "185.55.226.26", "185.55.225.25", "سرویس DNS ایرانی"),
        new("گوگل (Google Public DNS)", "8.8.8.8", "8.8.4.4", "پایداری و دانلود"),
        new("کلودفلر (Cloudflare)", "1.1.1.1", "1.0.0.1", "سرعت و حریم خصوصی"),
        new("Quad9", "9.9.9.9", "149.112.112.112", "امنیت بالاتر؛ مسدودسازی دامنه‌های مخرب"),
        new("OpenDNS (سیسکو)", "208.67.222.222", "208.67.220.220", "پایداری؛ برخی کاربران برای PSN امتحان می‌کنند"),
        new("AdGuard DNS", "94.140.14.14", "94.140.15.15", "مسدودسازی تبلیغات و ردیاب‌ها"),
        new("Safe DNS", "195.46.39.39", "195.46.39.40", "اتصال پایدار"),
        new("NTT", "129.250.35.250", "129.250.35.251", "گزینه جایگزین برای دانلود")
    };

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += (_, _) =>
        {
            try { UpdateTraffic(); }
            catch (Exception ex) { App.LogException("Traffic monitor tick failed", ex); }
        };
        Navigate("Dashboard");
        RefreshConnectionBadge();
        _timer.Start();
        Closed += (_, _) => { _timer.Stop(); _operation?.Cancel(); _operation?.Dispose(); };
    }

    private NetworkAdapterInfo? ActiveAdapter => _network.GetActiveAdapter();

    private void Navigate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string page) Navigate(page);
    }

    private void Navigate(string page)
    {
        PageHost.Content = page switch
        {
            "Dashboard" => Dashboard(),
            "Network" => NetworkPage(),
            "Ping" => PingPage(),
            "Lookup" => LookupPage(),
            "Monitor" => MonitorPage(),
            _ => AboutPage()
        };
        PageHost.Opacity = 0;
        PageHost.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(260))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        PageTitle.Text = page switch
        {
            "Dashboard" => "داشبورد",
            "Network" => "شبکه و DNS خودکار",
            "Ping" => "آزمایش Ping",
            "Lookup" => "جست‌وجوی DNS",
            "Monitor" => "مانیتورینگ مصرف",
            _ => "درباره Vexo Jumper"
        };
    }

    private static Border Card(UIElement child) => new()
    {
        Background = (Brush)Application.Current.Resources["CardGradient"],
        BorderBrush = (Brush)Application.Current.Resources["Line"],
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(18),
        Padding = new Thickness(20),
        Margin = new Thickness(7),
        Effect = (System.Windows.Media.Effects.Effect)Application.Current.Resources["SoftShadow"],
        Child = child
    };

    private static TextBlock Heading(string text) => new()
    {
        Text = text, FontSize = 17, FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 0, 0, 12), TextAlignment = TextAlignment.Center
    };

    // Results are intentionally large, centered and high-contrast so they are not missed.
    private static Border ResultBox(string title, string message = "نتیجه پس از اجرای عملیات اینجا نمایش داده می‌شود.")
    {
        var panel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock
        {
            Text = title, FontSize = 13, Foreground = (Brush)Application.Current.Resources["Cyan"],
            FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        });
        var resultText = new TextBlock
        {
            Text = message, FontSize = 20, FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch
        };
        if (title == "پایش زنده رابط شبکه فعال") resultText.Name = "MonitorResultText";
        panel.Children.Add(resultText);
        return new Border
        {
            MinHeight = 112, Margin = new Thickness(6, 14, 6, 8), Padding = new Thickness(22),
            CornerRadius = new CornerRadius(18), BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(62, 139, 210)),
            Background = new LinearGradientBrush(Color.FromRgb(25, 52, 82), Color.FromRgb(13, 25, 43), 35),
            Effect = (System.Windows.Media.Effects.Effect)Application.Current.Resources["GlowShadow"],
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(1, 1),
            Child = panel
        };
    }

    private static void SetResult(Border box, string message, bool? success = null)
    {
        if (box.Child is not StackPanel panel || panel.Children.Count < 2 || panel.Children[1] is not TextBlock text) return;
        text.Text = message;
        text.Foreground = success switch
        {
            true => new SolidColorBrush(Color.FromRgb(126, 244, 190)),
            false => new SolidColorBrush(Color.FromRgb(255, 166, 166)),
            _ => (Brush)Application.Current.Resources["TextBrush"]
        };
        box.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(0.65, 1, TimeSpan.FromMilliseconds(260))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        if (box.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.97, 1, TimeSpan.FromMilliseconds(300))
                { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.18 } });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.97, 1, TimeSpan.FromMilliseconds(300))
                { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.18 } });
        }
    }

    private UIElement Dashboard()
    {
        var adapter = ActiveAdapter;
        var cards = new UniformGrid { Columns = 2 };
        cards.Children.Add(Card(new StackPanel
        {
            Children =
            {
                Heading("وضعیت اتصال خودکار"),
                new TextBlock
                {
                    Text = adapter is null ? "شبکه فعال پیدا نشد" : "متصل به شبکه",
                    FontSize = 24, FontWeight = FontWeights.Bold,
                    Foreground = adapter is null ? Brushes.Orange : new SolidColorBrush(Color.FromRgb(116, 239, 183)),
                    TextAlignment = TextAlignment.Center
                },
                new TextBlock
                {
                    Text = adapter is null ? "اتصال Wi-Fi یا Ethernet را بررسی کنید." : $"{adapter.Name}  •  {adapter.Type}",
                    FontSize = 15, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)Application.Current.Resources["Muted"]
                }
            }
        }));
        cards.Children.Add(Card(new StackPanel
        {
            Children =
            {
                Heading("سرعت لحظه‌ای شبکه فعال"),
                new TextBlock { Name = "DashDown", Text = "دریافت: در حال نمونه‌برداری…", FontSize = 21, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center },
                new TextBlock { Name = "DashUp", Text = "ارسال: در حال نمونه‌برداری…", FontSize = 21, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 8, 0, 0) }
            }
        }));
        cards.Children.Add(Card(new StackPanel
        {
            Children =
            {
                Heading("IP و درگاه پیش‌فرض"),
                new TextBlock { Text = adapter?.IPv4 ?? "—", FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center },
                new TextBlock { Text = $"Gateway: {adapter?.Gateway ?? "—"}", FontSize = 15, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"] }
            }
        }));
        cards.Children.Add(Card(new StackPanel
        {
            Children =
            {
                Heading("DNS فعال سیستم"),
                new TextBlock { Text = adapter is null ? "—" : string.Join("   •   ", adapter.DnsServers), FontSize = 17, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap },
                new TextBlock { Text = "انتخاب رابط شبکه به‌صورت خودکار انجام می‌شود.", FontSize = 12, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"] }
            }
        }));
        var stack = new StackPanel();
        stack.Children.Add(cards);
        stack.Children.Add(Card(new StackPanel
        {
            Children =
            {
                Heading("اندازه‌گیری شفاف"),
                new TextBlock
                {
                    Text = "همه ابزارها از رابط فعال دارای درگاه پیش‌فرض استفاده می‌کنند. نرخ ترافیک از شمارنده‌های ویندوز خوانده می‌شود و ممکن است شامل ترافیک شبکه محلی باشد؛ این عدد معادل تست سرعت اینترنت نیست.",
                    FontSize = 14, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center
                }
            }
        }));
        return new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private UIElement NetworkPage()
    {
        var root = new StackPanel();
        var adapter = ActiveAdapter;
        var adapterResult = ResultBox("رابط شبکه انتخاب‌شده به‌صورت خودکار",
            adapter is null
                ? "هیچ رابط فعال دارای درگاه IPv4 پیدا نشد."
                : $"{adapter.Name}\n{adapter.Type}  •  IPv4: {adapter.IPv4}\nGateway: {adapter.Gateway}\nDNS فعلی: {string.Join("  |  ", adapter.DnsServers)}");
        root.Children.Add(Card(new StackPanel
        {
            Children =
            {
                Heading("اتصال فعلی"),
                adapterResult,
                new TextBlock
                {
                    Text = "نیازی به انتخاب دستی نیست؛ Vexo Jumper رابط فعال دارای Default Gateway را تشخیص می‌دهد و در تمام ابزارها از همان استفاده می‌کند.",
                    FontSize = 13, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center,
                    Foreground = (Brush)Application.Current.Resources["Muted"]
                }
            }
        }));

        var primary = new TextBox { Text = "1.1.1.1", Margin = new Thickness(4), FontSize = 17, TextAlignment = TextAlignment.Center };
        var secondary = new TextBox { Text = "1.0.0.1", Margin = new Thickness(4), FontSize = 17, TextAlignment = TextAlignment.Center };
        var result = ResultBox("نتیجه تغییر DNS");
        var presetGrid = new UniformGrid { Columns = 2 };
        foreach (var preset in DnsPresets)
        {
            var select = new Button
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = preset.Name, FontSize = 15, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center },
                        new TextBlock { Text = $"{preset.Primary}   •   {preset.Secondary}", FontSize = 13, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Cyan"], Margin = new Thickness(0, 5, 0, 4) },
                        new TextBlock { Text = preset.Description, FontSize = 11, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"] },
                        new TextBlock { Text = "انتخاب این DNS", FontSize = 12, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 7, 0, 0) }
                    }
                },
                Padding = new Thickness(12), Margin = new Thickness(5), MinHeight = 112,
                Background = new LinearGradientBrush(Color.FromRgb(27, 48, 73), Color.FromRgb(15, 27, 44), 45),
                BorderBrush = new SolidColorBrush(Color.FromRgb(47, 91, 133))
            };
            select.Click += (_, _) =>
            {
                primary.Text = preset.Primary;
                secondary.Text = preset.Secondary;
                SetResult(result, $"{preset.Name}\n{preset.Primary}  •  {preset.Secondary}\nبرای اعمال، دکمه «اعمال DNS» را بزنید.", true);
            };
            presetGrid.Children.Add(Card(select));
        }

        var apply = new Button { Content = "اعمال DNS روی شبکه فعال", Style = (Style)Application.Current.Resources["PrimaryButton"], FontSize = 15, Padding = new Thickness(22, 13, 22, 13) };
        apply.Click += async (_, _) =>
        {
            var current = ActiveAdapter;
            if (current is null) { SetResult(result, "رابط شبکه فعال پیدا نشد. اتصال اینترنت را بررسی کنید.", false); return; }
            if (MessageBox.Show($"DNS رابط فعال «{current.Name}» تغییر کند؟\n\n{primary.Text}\n{secondary.Text}",
                "تأیید تغییر DNS", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            apply.IsEnabled = false;
            try
            {
                var r = await _dns.SetDnsAsync(current.Name, primary.Text, secondary.Text);
                SetResult(result, r.Message, r.Success);
            }
            catch (Exception ex) { SetResult(result, "خطا در اعمال DNS: " + ex.Message, false); }
            finally { apply.IsEnabled = true; }
        };
        var dhcp = new Button { Content = "بازگردانی DNS خودکار (DHCP)", FontSize = 14 };
        dhcp.Click += async (_, _) =>
        {
            var current = ActiveAdapter;
            if (current is null) { SetResult(result, "رابط شبکه فعال پیدا نشد.", false); return; }
            if (MessageBox.Show($"DNS رابط فعال «{current.Name}» به حالت خودکار بازگردد؟",
                "تأیید بازگردانی", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            dhcp.IsEnabled = false;
            try
            {
                var r = await _dns.RestoreDhcpAsync(current.Name);
                SetResult(result, r.Message, r.Success);
            }
            catch (Exception ex) { SetResult(result, "خطا در بازگردانی DNS: " + ex.Message, false); }
            finally { dhcp.IsEnabled = true; }
        };

        root.Children.Add(Card(new StackPanel
        {
            Children =
            {
                Heading("DNSهای آماده"),
                new TextBlock { Text = "یک سرویس را انتخاب کن؛ آدرس‌ها خودکار در فیلدها قرار می‌گیرند.", FontSize = 13, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"], Margin = new Thickness(0, 0, 0, 8) },
                presetGrid,
                Heading("DNS انتخاب‌شده"),
                new TextBlock { Text = "DNS اصلی", TextAlignment = TextAlignment.Center },
                primary,
                new TextBlock { Text = "DNS جایگزین", TextAlignment = TextAlignment.Center },
                secondary,
                new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center, Children = { apply, dhcp } },
                result,
                new TextBlock { Text = "تغییر DNS فقط پس از تأیید شما انجام می‌شود و ممکن است به دسترسی مدیر نیاز داشته باشد.", FontSize = 12, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"] }
            }
        }));
        return new ScrollViewer { Content = root, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private UIElement PingPage()
    {
        var host = new TextBox { Text = "1.1.1.1", Margin = new Thickness(4), FontSize = 18, TextAlignment = TextAlignment.Center };
        var count = new TextBox { Text = "4", Margin = new Thickness(4), FontSize = 18, TextAlignment = TextAlignment.Center };
        var result = ResultBox("نتیجه آزمایش Ping");
        var button = new Button { Content = "شروع آزمایش", Style = (Style)Application.Current.Resources["PrimaryButton"], FontSize = 15 };
        button.Click += async (_, _) =>
        {
            if (!int.TryParse(count.Text, out var n)) n = 4;
            button.IsEnabled = false;
            _operation?.Cancel(); _operation?.Dispose(); _operation = new CancellationTokenSource();
            try
            {
                var r = await _network.PingAsync(host.Text, n, _operation.Token);
                SetResult(result, $"{r.Message}\nمیانگین زمان پاسخ: {(r.RoundtripMs.HasValue ? r.RoundtripMs + " ms" : "—")}", r.Success);
            }
            catch (Exception ex) { SetResult(result, "آزمایش ناموفق بود: " + ex.Message, false); }
            finally { button.IsEnabled = true; }
        };
        return Card(new StackPanel
        {
            Children =
            {
                Heading("آزمایش واقعی ICMP Ping"),
                new TextBlock { Text = "نشانی IP یا دامنه مقصد", TextAlignment = TextAlignment.Center },
                host,
                new TextBlock { Text = "تعداد درخواست (۱ تا ۲۰)", TextAlignment = TextAlignment.Center },
                count,
                button,
                result,
                new TextBlock { Text = $"رابط مورد استفاده ویندوز: {ActiveAdapter?.Name ?? "تشخیص خودکار مسیر توسط ویندوز"}\nبرخی سرورها پاسخ ICMP را مسدود می‌کنند؛ عدم پاسخ الزاماً به معنی قطع اینترنت نیست.", FontSize = 12, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"] }
            }
        });
    }

    private UIElement LookupPage()
    {
        var host = new TextBox { Text = "example.com", Margin = new Thickness(4), FontSize = 18, TextAlignment = TextAlignment.Center };
        var result = ResultBox("نتیجه DNS Lookup");
        var button = new Button { Content = "جست‌وجوی رکوردهای DNS", Style = (Style)Application.Current.Resources["PrimaryButton"], FontSize = 15 };
        button.Click += async (_, _) =>
        {
            button.IsEnabled = false;
            try
            {
                var r = await _network.LookupAsync(host.Text, CancellationToken.None);
                SetResult(result, $"{r.Message}\nزمان کل درخواست: {r.ElapsedMs} میلی‌ثانیه\n{r.Addresses}", r.Success);
            }
            catch (Exception ex) { SetResult(result, "جست‌وجو ناموفق بود: " + ex.Message, false); }
            finally { button.IsEnabled = true; }
        };
        return Card(new StackPanel
        {
            Children =
            {
                Heading("DNS Lookup"),
                new TextBlock { Text = "نام دامنه", TextAlignment = TextAlignment.Center },
                host, button, result,
                new TextBlock { Text = $"رابط فعال: {ActiveAdapter?.Name ?? "مسیر پیش‌فرض ویندوز"}\nاین ابزار از DNS تنظیم‌شده در ویندوز استفاده می‌کند؛ زمان نمایش‌داده‌شده زمان کل درخواست است، نه تأخیر خالص یک سرور DNS خاص.", FontSize = 12, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"] }
            }
        });
    }

    private UIElement MonitorPage()
    {
        var result = ResultBox("پایش زنده رابط شبکه فعال", "در حال دریافت نمونه…");
        result.Name = "MonitorResult";
        var adapter = ActiveAdapter;
        var info = new TextBlock
        {
            Text = $"رابط پایش‌شده: {adapter?.Name ?? "هیچ رابط فعالی شناسایی نشد"}",
            FontSize = 15, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"]
        };
        return Card(new StackPanel
        {
            Children =
            {
                Heading("پایش زنده مصرف اینترنت"),
                info, result,
                new TextBlock { Text = "نمونه‌برداری یک‌ثانیه‌ای از شمارنده‌های رابط فعال ویندوز. آمار ممکن است شامل ترافیک داخلی شبکه باشد و لزوماً با حجم محاسبه‌شده توسط اپراتور برابر نیست.", FontSize = 12, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Foreground = (Brush)Application.Current.Resources["Muted"] }
            }
        });
    }

    private UIElement AboutPage() => Card(new StackPanel
    {
        Children =
        {
            Heading("Vexo Jumper"),
            new TextBlock
            {
                Text = "ابزار مدیریت و عیب‌یابی اتصال شبکه برای ویندوز\nنسخه ۱.۲.۰\n\nانتخاب رابط شبکه به‌صورت خودکار انجام می‌شود. برای تغییر DNS، انتخاب سرویس و تأیید کاربر لازم است.",
                FontSize = 16, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center
            }
        }
    });

    private void UpdateTraffic()
    {
        var adapter = ActiveAdapter;
        var sample = _traffic.Sample(adapter?.Name);
        var down = Format(sample.DownloadBytesPerSecond);
        var up = Format(sample.UploadBytesPerSecond);

        if (PageHost.Content is ScrollViewer sv)
        {
            foreach (var t in FindTextBlocks(sv))
            {
                if (t.Name == "DashDown") t.Text = $"دریافت: {down}/s";
                else if (t.Name == "DashUp") t.Text = $"ارسال: {up}/s";
            }
        }
        if (PageHost.Content is Border border)
        {
            foreach (var t in FindTextBlocks(border))
            {
                if (t.Name == "MonitorResultText")
                    t.Text = $"دریافت لحظه‌ای\n{down}/s\n\nارسال لحظه‌ای\n{up}/s\n\nکل دریافت: {Format(sample.TotalReceived)}\nکل ارسال: {Format(sample.TotalSent)}";
            }
        }
        RefreshConnectionBadge();
    }

    private void RefreshConnectionBadge()
    {
        var adapter = ActiveAdapter;
        ConnectionBadge.Text = adapter is null ? "بدون اتصال فعال" : $"متصل • {adapter.Name}";
        StatusDot.Fill = adapter is null ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(82, 224, 166));
    }

    private static IEnumerable<TextBlock> FindTextBlocks(DependencyObject root)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is TextBlock text) yield return text;
            foreach (var nested in FindTextBlocks(child)) yield return nested;
        }
    }

    private static string Format(double bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes; int i = 0;
        while (value >= 1024 && i < units.Length - 1) { value /= 1024; i++; }
        return $"{value:0.##} {units[i]}";
    }

    private void Github_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("https://github.com/veyro404") { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show("بازکردن پیوند ناموفق بود: " + ex.Message, "Vexo Jumper", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
