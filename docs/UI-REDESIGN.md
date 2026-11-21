# Vexo Jumper UI redesign — 1.1.0

## Visual direction

A minimal dark network console with a restrained electric-blue/cyan accent. The main window uses a compact navigation rail, a quiet page header, and one content surface. Visual depth comes from layered navy surfaces, subtle gradients, thin borders, and soft shadows rather than dense decoration.

## Motion

- Button hover: short scale and border/background transition.
- Button press: slight compression and opacity feedback.
- Page navigation: 260 ms ease-out fade.
- Brand tile: low-amplitude 2.4-second pulse.
- Connection indicator: subtle 1.15-second opacity pulse.

Animations are deliberately low-amplitude to keep the app readable and avoid excessive CPU/GPU use.

## Typography

Vazirmatn 33.003 is downloaded by `build-and-publish.ps1` into `src/VexoJumper/Assets/Fonts` and embedded as WPF resources. If the font download is unavailable, Windows Segoe UI remains the fallback. The upstream project states that Vazirmatn is licensed under SIL Open Font License 1.1: https://github.com/rastikerdar/vazirmatn

## Implementation note

This is a WPF application, so styling is implemented with XAML resource dictionaries, control templates, triggers, and storyboards rather than CSS.
