# معماری فنی

## انتخاب فناوری

WPF روی `net10.0-windows` انتخاب شده است: فناوری دسکتاپ بومی ویندوز با دسترسی مستقیم به APIهای .NET شبکه و پشتیبانی از RTL. وابستگی UI خارجی اضافه نشده است تا سطح زنجیره تأمین کوچک بماند.

## لایه‌ها

- Presentation: صفحات WPF در `Views`
- Services: سرویس‌های شبکه، DNS، شمارنده ترافیک و ذخیره‌سازی
- Models: مدل‌های انتقال داده رابط شبکه

این نسخه هنوز جداسازی کامل Application/Domain/Infrastructure و DI container ندارد؛ این مورد پیش از توسعه کامل باید انجام شود.

## روش اندازه‌گیری

- Ping: `System.Net.NetworkInformation.Ping` و ICMP؛ مستقل از زمان DNS.
- DNS Lookup: `Dns.GetHostAddressesAsync`؛ زمان کل resolve با resolver سیستم، نه RTT اختصاصی DNS.
- Throughput: اختلاف BytesReceived/BytesSent بین دو نمونه یک‌ثانیه‌ای برای رابط‌های Up. نمونه اول نرخ معتبر ندارد و نرخ در صورت تأخیر UI تقریبی است.
- Traffic totals: جمع شمارنده‌های رابط‌های فعال؛ ممکن است شامل LAN و ترافیک داخلی باشد.

## تغییر DNS

`netsh.exe` با `ProcessStartInfo.ArgumentList` و بدون shell اجرا می‌شود. ورودی IP قبل از اجرا parse می‌شود و نام رابط باید از فهرست رابط‌های سیستم انتخاب شده باشد. موفقیت اجرای netsh به معنی تأیید مستقل پیکربندی resolver نیست؛ پس از عملیات باید رابط مجدداً خوانده شود.
