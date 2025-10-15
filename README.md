# CSuiteViewWPF

A modern WPF application with a beautiful layered UI design featuring Royal Blue and Gold theme.

## Features

- **4-Layer Architecture**: Gold border → Dark Blue (Layer 1) → Medium Blue (Layer 2) → Light Blue (Layer 3)
- **Custom Window Chrome**: No standard Windows title bar, custom draggable header/footer
- **Golden Accent Lines**: Fine golden separator lines between layers
- **Custom Close Button**: Yellow circle with X in the header
- **3-Panel Resizable Layout**: GridSplitter-based layout with three resizable panels
- **Enhanced TreeView**: Custom styled TreeView with hover effects and selection highlighting
- **Windows 11 Rounded Corners**: Native rounded corners using DWM API

## Technologies

- .NET 8.0
- WPF (Windows Presentation Foundation)
- MahApps.Metro 2.4.11
- Windows 11 DWM API

## Color Scheme

- **Primary**: #2C5AA0 (Royal Blue)
- **Secondary**: #4169E1 (Medium Royal Blue)
- **Accent**: #FFD700 (Gold/Brass)
- **Light Blue**: #8BB9F5

## Building

```powershell
dotnet build
```

## Running

```powershell
dotnet run
```

## Structure

```
Gold Border (8px)
└─ Layer 1: Dark Blue (#2C5AA0)
   ├─ Header (30px) - Draggable, Close Button
   ├─ Golden Line
   ├─ Layer 2: Medium Blue (#4169E1)
   │  ├─ Header (20px)
   │  ├─ Layer 3: Light Blue (#8BB9F5)
   │  │  └─ 3-Panel Splitter Layout
   │  │     ├─ Panel 1 (TreeView)
   │  │     ├─ Panel 2
   │  │     └─ Panel 3
   │  └─ Footer (15px)
   ├─ Golden Line
   └─ Footer (15px) - Draggable
```

## License

MIT
