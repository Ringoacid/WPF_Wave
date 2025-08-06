using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPF_Wave.Models;

namespace WPF_Wave.ViewModels;

public class ModuleTreeNode
{
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<ModuleTreeNode> Children { get; set; } = new();
    public Module? ModuleData { get; set; } // 元のModuleデータへの参照
}

public class VariableDisplayItem
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int BitWidth { get; set; }
    public Variable VariableData { get; set; } = null!;
    
    public string DisplayText => $"{Type}: {Name} ({BitWidth} bits)";
}

#pragma warning disable WPF0001
public partial class MainWindowViewModel : ObservableObject
{
    #region テーマ
    [ObservableProperty]
    bool isDarkMode = true;

    partial void OnIsDarkModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsLightMode));
    }

    public bool IsLightMode => !IsDarkMode;

    [RelayCommand]
    public void ChangeTheme()
    {
        if(App.Current.ThemeMode == ThemeMode.Light)
        {
            App.Current.ThemeMode = ThemeMode.Dark;
            IsDarkMode = true;
        }
        else
        {
            App.Current.ThemeMode = ThemeMode.Light;
            IsDarkMode = false;
        }
    }
    #endregion

    #region モジュールと信号の表示
    [ObservableProperty]
    ObservableCollection<ModuleTreeNode> moduleTree = new();

    [ObservableProperty]
    ModuleTreeNode? selectedModule;

    [ObservableProperty]
    ObservableCollection<VariableDisplayItem> signalList = new();

    partial void OnSelectedModuleChanged(ModuleTreeNode? value)
    {
        UpdateSignalList();
    }

    Vcd? activeVcd;

    [RelayCommand]
    public void OpenVcdFile()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "VCD Files (*.vcd)|*.vcd|All Files (*.*)|*.*",
            Title = "VCDファイルを選択してください"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            ReadVcdFile(openFileDialog.FileName);
        }
    }

    private ModuleTreeNode CreateModuleTreeNode(Module module)
    {
        var node = new ModuleTreeNode
        {
            Name = module.Name,
            ModuleData = module
        };

        // サブモジュールのみを追加（変数は含めない）
        foreach (var subModule in module.SubModules)
        {
            var subModuleNode = CreateModuleTreeNode(subModule);
            node.Children.Add(subModuleNode);
        }

        return node;
    }

    private void UpdateSignalList()
    {
        SignalList.Clear();
        
        if (SelectedModule?.ModuleData != null)
        {
            foreach (var variable in SelectedModule.ModuleData.ID_Variable_Pairs.Values)
            {
                var item = new VariableDisplayItem
                {
                    Name = variable.Name,
                    Type = variable.Type.ToString(),
                    BitWidth = variable.BitWidth,
                    VariableData = variable
                };
                SignalList.Add(item);
            }
        }
    }

    private void ReadVcdFile(string filePath)
    {
        try
        {
            activeVcd = new Vcd();
            activeVcd.LoadFromFile(filePath);

            // activeVcd.TopModule以下のモジュールのみをTreeViewに表示
            ModuleTree.Clear();
            if (activeVcd.TopModule != null)
            {
                var topModuleNode = CreateModuleTreeNode(activeVcd.TopModule);
                ModuleTree.Add(topModuleNode);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"VCDファイルの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    #endregion

    [ObservableProperty]
    ObservableCollection<object> hoges = ["a", "b", "c", "d", "longlonglonglong_e"];



}

#pragma warning restore WPF0001