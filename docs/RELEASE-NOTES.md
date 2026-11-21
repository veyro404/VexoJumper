# یادداشت انتشار 1.0.0 (نسخه اولیه منبع)

این خروجی برای آغاز توسعه و بازبینی فنی است، نه انتشار عمومی نهایی. عملیات اصلی شبکه‌ای موجود هستند اما دامنه محصول هنوز با سند نیازمندی‌ها برابر نیست. هیچ ادعایی درباره Build موفق، تست Windows یا آمادگی انتشار عمومی مطرح نمی‌شود.


## UI build hotfix

- Removed unsupported WPF `TextBlock.CharacterSpacing` setter.
- Replaced `ScaleTransform` trigger setters on the non-FrameworkElement transform with supported visual feedback on the button template root.
- Retained the hover scale storyboard animation.


## Hotfix 2
- Corrected the WPF effect type reference to `System.Windows.Media.Effects.Effect` in `MainWindow.xaml.cs`.
