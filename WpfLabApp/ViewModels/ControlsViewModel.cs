using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WpfLabApp.Common;
using WpfLabApp.Models;

namespace WpfLabApp.ViewModels;

/// <summary>
/// Модель представления вкладки «1.3. Элементы управления» (ТЗ 1.3 + Лаб 2).
/// Управляет данными TreeView, DataGrid (основной и целевой), ProgressBar, меню и журналом событий.
/// </summary>
public class ControlsViewModel : ObservableObject
{
    private string _eventLog = "Готово • Выполните действие в меню, таблице или дереве для фиксации события";
    private string _selectedTreeItemText = "WpfLabApp (Решение) — Корневой проект .NET 8";
    private string _selectedDataGridItemText = "Выберите строку в таблице выше для просмотра подробной информации";
    private double _progressValue = 75;
    private bool _isIndeterminateActive = true;
    private ProjectTreeNode? _selectedNode;
    private DataGridSampleItem? _selectedItem;
    private DataGridSampleItem? _selectedTargetItem;
    private DataGridSampleItem? _copiedItem;

    private string _editName = string.Empty;
    private string _editCategory = string.Empty;
    private string _editStatus = "В обработке";
    private StatusCategory _editStatusType = StatusCategory.Info;
    private bool _editIsActive = true;

    public ObservableCollection<ProjectTreeNode> ProjectNodes { get; }
    public ObservableCollection<DataGridSampleItem> DataItems { get; }
    public ObservableCollection<DataGridSampleItem> TargetDataItems { get; } = new();

    public IReadOnlyList<StatusCategory> AvailableStatusTypes { get; } = Enum.GetValues<StatusCategory>();
    public IReadOnlyList<string> AvailableStatuses { get; } = new[]
    {
        "Успешно",
        "В обработке",
        "Основной поток",
        "Акцентная задача",
        "Ожидание подтверждения",
        "Сбой синхронизации"
    };

    public string EventLog
    {
        get => _eventLog;
        set => SetProperty(ref _eventLog, value);
    }

    public string SelectedTreeItemText
    {
        get => _selectedTreeItemText;
        set => SetProperty(ref _selectedTreeItemText, value);
    }

    public string SelectedDataGridItemText
    {
        get => _selectedDataGridItemText;
        set => SetProperty(ref _selectedDataGridItemText, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, Math.Clamp(value, 0, 100));
    }

    public bool IsIndeterminateActive
    {
        get => _isIndeterminateActive;
        set => SetProperty(ref _isIndeterminateActive, value);
    }

    public ProjectTreeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (SetProperty(ref _selectedNode, value) && value != null)
            {
                SelectedTreeItemText = $"{value.Icon} {value.Name} — {value.Info} ({value.NodeType})";
                LogEvent($"Выбран узел дерева: '{value.Name}' ({value.NodeType})");
            }
        }
    }

    public DataGridSampleItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                if (value != null)
                {
                    EditName = value.Name;
                    EditCategory = value.Category;
                    EditStatus = value.Status;
                    EditStatusType = value.StatusType;
                    EditIsActive = value.IsActive;
                    UpdateSelectedItemInspector(value);
                    LogEvent($"Выбрана строка таблицы: #{value.Id} {value.Name} [{value.Status}]");
                }
                else
                {
                    SelectedDataGridItemText = "Выберите строку в таблице выше для просмотра подробной информации";
                }
            }
        }
    }

    public DataGridSampleItem? SelectedTargetItem
    {
        get => _selectedTargetItem;
        set => SetProperty(ref _selectedTargetItem, value);
    }

    public string EditName
    {
        get => _editName;
        set => SetProperty(ref _editName, value);
    }

    public string EditCategory
    {
        get => _editCategory;
        set => SetProperty(ref _editCategory, value);
    }

    public string EditStatus
    {
        get => _editStatus;
        set => SetProperty(ref _editStatus, value);
    }

    public StatusCategory EditStatusType
    {
        get => _editStatusType;
        set => SetProperty(ref _editStatusType, value);
    }

    public bool EditIsActive
    {
        get => _editIsActive;
        set => SetProperty(ref _editIsActive, value);
    }

    public RelayCommand AddRowCommand { get; }
    public RelayCommand UpdateRowCommand { get; }
    public RelayCommand DeleteRowCommand { get; }
    public RelayCommand ResetRowsCommand { get; }
    public RelayCommand CopyRowCommand { get; }
    public RelayCommand PasteRowCommand { get; }
    public RelayCommand DeleteTargetRowCommand { get; }
    public RelayCommand<object?> ToggleActiveCommand { get; }

    public RelayCommand ExpandAllTreeCommand { get; }
    public RelayCommand CollapseAllTreeCommand { get; }
    public RelayCommand<object?> ExpandSelectedNodeCommand { get; }
    public RelayCommand<object?> CollapseSelectedNodeCommand { get; }
    public RelayCommand<object?> CopyNodeNameCommand { get; }

    public RelayCommand<object?> SetProgressCommand { get; }
    public RelayCommand<object?> MenuActionCommand { get; }

    public ControlsViewModel()
    {
        ProjectNodes = ProjectTreeNode.GetSampleProjectStructure();
        AttachTreeNodeListeners(ProjectNodes);

        DataItems = DataGridSampleItem.GetSampleItems();

        AddRowCommand = new RelayCommand(AddNewItem);
        UpdateRowCommand = new RelayCommand(UpdateSelectedItem, () => SelectedItem != null);
        DeleteRowCommand = new RelayCommand(DeleteSelectedItem, () => SelectedItem != null);
        ResetRowsCommand = new RelayCommand(ResetItems);
        CopyRowCommand = new RelayCommand(CopySelectedItem, () => SelectedItem != null);
        PasteRowCommand = new RelayCommand(PasteCopiedItem, () => _copiedItem != null);
        DeleteTargetRowCommand = new RelayCommand(DeleteSelectedTargetItem, () => SelectedTargetItem != null);
        ToggleActiveCommand = new RelayCommand<object?>(ToggleItemActive, p => (p as DataGridSampleItem ?? SelectedItem) != null);

        ExpandAllTreeCommand = new RelayCommand(() => SetTreeExpansion(true));
        CollapseAllTreeCommand = new RelayCommand(() => SetTreeExpansion(false));
        ExpandSelectedNodeCommand = new RelayCommand<object?>(p => SetSingleNodeExpansion(p as ProjectTreeNode ?? SelectedNode, true), p => (p as ProjectTreeNode ?? SelectedNode) != null);
        CollapseSelectedNodeCommand = new RelayCommand<object?>(p => SetSingleNodeExpansion(p as ProjectTreeNode ?? SelectedNode, false), p => (p as ProjectTreeNode ?? SelectedNode) != null);
        CopyNodeNameCommand = new RelayCommand<object?>(CopyNodeName, p => (p as ProjectTreeNode ?? SelectedNode) != null);

        SetProgressCommand = new RelayCommand<object?>(SetProgress);
        MenuActionCommand = new RelayCommand<object?>(p => LogEvent($"Выполнено действие меню: '{p}'"));
    }

    public void LogEvent(string action)
    {
        EventLog = $"[{DateTime.Now:HH:mm:ss}] {action}";
    }

    private void UpdateSelectedItemInspector(DataGridSampleItem item)
    {
        SelectedDataGridItemText = $"[#{item.Id}] {item.Name} | Категория: {item.Category} | Статус: {item.Status} | Активен: {(item.IsActive ? "Да" : "Нет")}";
    }

    private void AddNewItem()
    {
        int nextId = DataItems.Count > 0 ? DataItems.Max(i => i.Id) + 1 : 101;
        string name = string.IsNullOrWhiteSpace(EditName) ? $"Новая задача службы #{nextId}" : EditName.Trim();
        string category = string.IsNullOrWhiteSpace(EditCategory) ? "Очередь обработки" : EditCategory.Trim();
        string status = string.IsNullOrWhiteSpace(EditStatus) ? GetDefaultStatusText(EditStatusType) : EditStatus.Trim();

        var newItem = new DataGridSampleItem
        {
            Id = nextId,
            Name = name,
            Category = category,
            Status = status,
            StatusType = EditStatusType,
            Date = DateTime.Now,
            IsActive = EditIsActive
        };

        DataItems.Add(newItem);
        SelectedItem = newItem;
        LogEvent($"Добавлена новая запись #{newItem.Id}: {newItem.Name}");
    }

    private void UpdateSelectedItem()
    {
        if (SelectedItem == null) return;

        SelectedItem.Name = string.IsNullOrWhiteSpace(EditName) ? SelectedItem.Name : EditName.Trim();
        SelectedItem.Category = string.IsNullOrWhiteSpace(EditCategory) ? SelectedItem.Category : EditCategory.Trim();
        SelectedItem.Status = string.IsNullOrWhiteSpace(EditStatus) ? GetDefaultStatusText(EditStatusType) : EditStatus.Trim();
        SelectedItem.StatusType = EditStatusType;
        SelectedItem.IsActive = EditIsActive;
        SelectedItem.Date = DateTime.Now;

        UpdateSelectedItemInspector(SelectedItem);
        LogEvent($"Обновлена запись #{SelectedItem.Id}: {SelectedItem.Name}");
    }

    private void DeleteSelectedItem()
    {
        if (SelectedItem == null) return;

        var removed = SelectedItem;
        DataItems.Remove(removed);
        SelectedItem = DataItems.FirstOrDefault();
        SelectedDataGridItemText = $"Запись #{removed.Id} успешно удалена из таблицы";
        LogEvent($"Удалена запись #{removed.Id} - '{removed.Name}'");
    }

    private void ResetItems()
    {
        DataItems.Clear();
        foreach (var item in DataGridSampleItem.GetSampleItems())
        {
            DataItems.Add(item);
        }

        TargetDataItems.Clear();
        SelectedItem = null;
        SelectedTargetItem = null;
        SelectedDataGridItemText = "Таблица сброшена к начальным 8 демонстрационным строкам";
        LogEvent("Основная таблица сброшена к 8 строкам, целевая таблица очищена");
    }

    private void CopySelectedItem()
    {
        if (SelectedItem == null) return;

        _copiedItem = SelectedItem.Clone();
        try
        {
            Clipboard.SetText($"#{_copiedItem.Id} {_copiedItem.Name} [{_copiedItem.Category}] - {_copiedItem.Status}");
        }
        catch
        {
            // Игнорируем возможную блокировку буфера обмена Windows
        }

        CommandManager.InvalidateRequerySuggested();
        LogEvent($"Скопирована запись #{_copiedItem.Id}: '{_copiedItem.Name}'");
    }

    private void PasteCopiedItem()
    {
        if (_copiedItem == null) return;

        var pasted = _copiedItem.Clone();
        pasted.Id = TargetDataItems.Count > 0 ? TargetDataItems.Max(i => i.Id) + 1 : 201;
        pasted.Date = DateTime.Now;

        TargetDataItems.Add(pasted);
        SelectedTargetItem = pasted;
        LogEvent($"Вставлена запись в целевую таблицу: #{pasted.Id} '{pasted.Name}'");
    }

    private void DeleteSelectedTargetItem()
    {
        if (SelectedTargetItem == null) return;

        var removed = SelectedTargetItem;
        TargetDataItems.Remove(removed);
        SelectedTargetItem = TargetDataItems.FirstOrDefault();
        LogEvent($"Удалена запись из целевой таблицы: #{removed.Id} '{removed.Name}'");
    }

    private void ToggleItemActive(object? parameter)
    {
        var item = parameter as DataGridSampleItem ?? SelectedItem;
        if (item == null) return;

        item.IsActive = !item.IsActive;
        if (item == SelectedItem)
        {
            EditIsActive = item.IsActive;
            UpdateSelectedItemInspector(item);
        }

        LogEvent($"Статус активности записи #{item.Id}: {(item.IsActive ? "Активен" : "Неактивен")}");
    }

    private void SetProgress(object? parameter)
    {
        string raw = parameter?.ToString()?.Trim() ?? string.Empty;
        if (!double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
        {
            return;
        }

        if (raw.StartsWith('+') || raw.StartsWith('-'))
        {
            ProgressValue += value;
        }
        else
        {
            ProgressValue = value;
        }

        LogEvent($"Установлено значение прогресса: {ProgressValue:F0}%");
    }

    private void SetTreeExpansion(bool expand)
    {
        void Recurse(IEnumerable<ProjectTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.IsExpanded = expand;
                if (node.Children.Count > 0)
                {
                    Recurse(node.Children);
                }
            }
        }

        Recurse(ProjectNodes);
        if (!expand && ProjectNodes.Count > 0)
        {
            ProjectNodes[0].IsExpanded = true;
        }

        LogEvent(expand ? "Все узлы дерева развернуты" : "Все вложенные ветки дерева свернуты");
    }

    private void SetSingleNodeExpansion(ProjectTreeNode? node, bool expand)
    {
        if (node == null) return;
        node.IsExpanded = expand;
        LogEvent($"{(expand ? "Развернут" : "Свернут")} узел: '{node.Name}'");
    }

    private void CopyNodeName(object? parameter)
    {
        var node = parameter as ProjectTreeNode ?? SelectedNode;
        if (node == null) return;

        try
        {
            Clipboard.SetText(node.Name);
        }
        catch
        {
            // Игнорируем блокировку буфера обмена
        }

        LogEvent($"Скопировано имя узла: '{node.Name}'");
    }

    private void AttachTreeNodeListeners(IEnumerable<ProjectTreeNode> nodes)
    {
        foreach (var node in nodes)
        {
            node.PropertyChanged += OnTreeNodePropertyChanged;
            if (node.Children.Count > 0)
            {
                AttachTreeNodeListeners(node.Children);
            }
        }
    }

    private void OnTreeNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectTreeNode.IsSelected) && sender is ProjectTreeNode node && node.IsSelected)
        {
            SelectedNode = node;
        }
    }

    private static string GetDefaultStatusText(StatusCategory category) => category switch
    {
        StatusCategory.Success => "Успешно",
        StatusCategory.Error => "Сбой синхронизации",
        StatusCategory.Accent => "Акцентная задача",
        StatusCategory.Info => "В обработке",
        StatusCategory.Primary => "Основной поток",
        StatusCategory.Warning => "Ожидание подтверждения",
        _ => "В обработке"
    };
}
