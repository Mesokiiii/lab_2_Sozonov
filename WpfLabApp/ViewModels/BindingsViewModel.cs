using System;
using System.Windows.Input;
using WpfLabApp.Common;

namespace WpfLabApp.ViewModels;

/// <summary>
/// Модель представления вкладки «2.1. Типы привязки и UpdateSourceTrigger».
/// Демонстрирует все режимы <see cref="System.Windows.Data.BindingMode"/> и <see cref="System.Windows.Data.UpdateSourceTrigger"/>.
/// </summary>
public class BindingsViewModel : ObservableObject
{
    private const string InitialModeText = "Начальное значение";

    private int _changeCounter;
    private string _modeSourceText = InitialModeText;
    private string _oneWayToSourceTargetText = string.Empty;

    private string _propertyChangedText = "Печатайте здесь...";
    private string _lostFocusText = "Нажмите Tab после ввода";
    private string _explicitText = "Нажмите кнопку обновления";
    private string _defaultTriggerText = "Поведение по умолчанию";

    private string _lastUpdatedInfo = "Ожидание изменений...";

    public BindingsViewModel()
    {
        ChangeModeSourceCommand = new RelayCommand(ChangeModeSource);
        ResetModeSourceCommand = new RelayCommand(ResetModeSource);
    }

    public ICommand ChangeModeSourceCommand { get; }
    public ICommand ResetModeSourceCommand { get; }

    public string ModeSourceText
    {
        get => _modeSourceText;
        set
        {
            if (SetProperty(ref _modeSourceText, value))
            {
                RecordUpdate(nameof(ModeSourceText), value);
            }
        }
    }

    public string OneWayToSourceTargetText
    {
        get => _oneWayToSourceTargetText;
        set
        {
            if (SetProperty(ref _oneWayToSourceTargetText, value))
            {
                RecordUpdate(nameof(OneWayToSourceTargetText), value);
            }
        }
    }

    public string PropertyChangedText
    {
        get => _propertyChangedText;
        set
        {
            if (SetProperty(ref _propertyChangedText, value))
            {
                RecordUpdate(nameof(PropertyChangedText), value);
            }
        }
    }

    public string LostFocusText
    {
        get => _lostFocusText;
        set
        {
            if (SetProperty(ref _lostFocusText, value))
            {
                RecordUpdate(nameof(LostFocusText), value);
            }
        }
    }

    public string ExplicitText
    {
        get => _explicitText;
        set
        {
            if (SetProperty(ref _explicitText, value))
            {
                RecordUpdate(nameof(ExplicitText), value);
            }
        }
    }

    public string DefaultTriggerText
    {
        get => _defaultTriggerText;
        set
        {
            if (SetProperty(ref _defaultTriggerText, value))
            {
                RecordUpdate(nameof(DefaultTriggerText), value);
            }
        }
    }

    public string LastUpdatedInfo
    {
        get => _lastUpdatedInfo;
        private set => SetProperty(ref _lastUpdatedInfo, value);
    }

    private void ChangeModeSource()
    {
        _changeCounter++;
        ModeSourceText = $"Изменено из ViewModel #{_changeCounter} ({DateTime.Now:HH:mm:ss})";
    }

    private void ResetModeSource()
    {
        _changeCounter = 0;
        ModeSourceText = InitialModeText;
    }

    private void RecordUpdate(string propertyName, string value)
    {
        LastUpdatedInfo = $"[{DateTime.Now:HH:mm:ss.fff}] Обновлено свойство {propertyName} → \"{value}\"";
    }
}
