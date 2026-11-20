using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using WpfLabApp.Common;
using WpfLabApp.Models;
using WpfLabApp.Services.Implementations;

namespace WpfLabApp.ViewModels;

/// <summary>
/// Модель представления вкладки «1.2. Цветовая палитра» (ТЗ 1.2).
/// Предоставляет коллекции базовых цветов и 50 математически выверенных оттенков.
/// </summary>
public class ColorPaletteViewModel : ObservableObject
{
    private ColorItem? _selectedColor;
    private string _statusNotification = "Нажмите на любой оттенок в таблице или карточке для инспекции и копирования";
    private bool _isNotificationActive;
    private DispatcherTimer? _notificationTimer;

    public string CurrentThemeName => ColorPaletteData.CurrentThemeName;
    public string ThemeDescription => ColorPaletteData.ThemeDescription;

    public ColorGroup PrimaryGroup { get; } = ColorPaletteData.GetPrimaryGroup();
    public ColorGroup AccentGroup { get; } = ColorPaletteData.GetAccentGroup();
    public ColorGroup SuccessGroup { get; } = ColorPaletteData.GetSuccessGroup();
    public ColorGroup ErrorGroup { get; } = ColorPaletteData.GetErrorGroup();
    public ColorGroup InfoGroup { get; } = ColorPaletteData.GetInfoGroup();

    public ObservableCollection<ColorGroup> AllGroups { get; }

    public ColorItem? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (SetProperty(ref _selectedColor, value))
            {
                OnPropertyChanged(nameof(SelectedShade));
                OnPropertyChanged(nameof(HasSelectedColor));
                if (value != null)
                {
                    ShowNotification($"Выбран оттенок: {value.Name} ({value.HexCode})");
                }
            }
        }
    }

    public ColorItem? SelectedShade
    {
        get => SelectedColor;
        set => SelectedColor = value;
    }

    public bool HasSelectedColor => _selectedColor != null;

    public string StatusNotification
    {
        get => _statusNotification;
        set => SetProperty(ref _statusNotification, value);
    }

    public bool IsNotificationActive
    {
        get => _isNotificationActive;
        set => SetProperty(ref _isNotificationActive, value);
    }

    public RelayCommand<string> CopyHexCommand { get; }
    public RelayCommand<string> CopyRgbCommand { get; }
    public RelayCommand<ColorItem> SelectShadeCommand { get; }

    public ColorPaletteViewModel()
    {
        AllGroups = new ObservableCollection<ColorGroup>
        {
            PrimaryGroup,
            AccentGroup,
            SuccessGroup,
            ErrorGroup,
            InfoGroup
        };

        _selectedColor = PrimaryGroup.Shades.Find(s => s.Shade == "500") ?? PrimaryGroup.Shades[0];

        CopyHexCommand = new RelayCommand<string>(paramHex =>
        {
            var hex = !string.IsNullOrEmpty(paramHex) ? paramHex : SelectedColor?.HexCode;
            if (string.IsNullOrEmpty(hex)) return;
            NotificationService.Instance.CopyToClipboard(hex, "HEX-код");
            ShowNotification($"HEX-код {hex} скопирован в буфер обмена!");
        });

        CopyRgbCommand = new RelayCommand<string>(paramRgb =>
        {
            var rgb = !string.IsNullOrEmpty(paramRgb) ? paramRgb : SelectedColor?.RgbCode;
            if (string.IsNullOrEmpty(rgb)) return;
            NotificationService.Instance.CopyToClipboard(rgb, "RGB-код");
            ShowNotification($"RGB-код {rgb} скопирован в буфер обмена!");
        });

        SelectShadeCommand = new RelayCommand<ColorItem>(shade =>
        {
            if (shade != null)
            {
                SelectedColor = shade;
            }
        });
    }

    private void ShowNotification(string message)
    {
        StatusNotification = message;
        IsNotificationActive = true;

        _notificationTimer?.Stop();
        _notificationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _notificationTimer.Tick += (_, _) =>
        {
            IsNotificationActive = false;
            _notificationTimer?.Stop();
        };
        _notificationTimer.Start();
    }
}
