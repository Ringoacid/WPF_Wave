using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPF_Wave.Models;

namespace WPF_Wave.ViewModels;

/// <summary>
/// モジュールのツリー構造を表現するためのノードクラス
/// VCDファイルから読み込んだモジュール階層をTreeViewで表示するために使用
/// </summary>
public class ModuleTreeNode
{
    /// <summary>
    /// モジュール名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 子モジュールのコレクション
    /// </summary>
    public ObservableCollection<ModuleTreeNode> Children { get; set; } = new();

    /// <summary>
    /// 元のModuleオブジェクトへの参照
    /// このノードが表現するModuleの詳細情報にアクセスするために使用
    /// </summary>
    public Module? ModuleData { get; set; }
}

/// <summary>
/// 変数（信号）の表示用アイテムクラス
/// VCDファイルの変数情報をUIに表示するためのプロパティ変更通知機能付きラッパー
/// </summary>
public class VariableDisplayItem : INotifyPropertyChanged
{
    #region プライベートフィールド

    /// <summary>
    /// 変数名のバッキングフィールド
    /// </summary>
    private string name = string.Empty;

    /// <summary>
    /// 変数タイプのバッキングフィールド
    /// </summary>
    private string type = string.Empty;

    /// <summary>
    /// ビット幅のバッキングフィールド
    /// </summary>
    private int bitWidth;

    #endregion

    #region パブリックプロパティ

    /// <summary>
    /// 変数名（例: "clk", "reset", "data_bus"）
    /// </summary>
    public string Name 
    { 
        get => name;
        set
        {
            if (name != value)
            {
                name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText)); // 表示テキストも更新通知
            }
        }
    }

    /// <summary>
    /// 変数の種類（"Wire", "Reg", "Integer", "Parameter"など）
    /// </summary>
    public string Type 
    { 
        get => type;
        set
        {
            if (type != value)
            {
                type = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText)); // 表示テキストも更新通知
            }
        }
    }

    /// <summary>
    /// 変数のビット幅
    /// </summary>
    public int BitWidth 
    { 
        get => bitWidth;
        set
        {
            if (bitWidth != value)
            {
                bitWidth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText)); // 表示テキストも更新通知
            }
        }
    }

    /// <summary>
    /// 元のVariableオブジェクトへの参照
    /// VCDファイルから読み込んだ詳細な変数情報にアクセスするために使用
    /// </summary>
    public Variable VariableData { get; set; } = null!;
    
    /// <summary>
    /// 表示用フォーマット済みテキスト
    /// "Type: Name (BitWidth bits)" の形式で表示
    /// 例: "Wire: clk (1 bits)", "Reg: counter (8 bits)"
    /// </summary>
    public string DisplayText => $"{Type}: {Name} ({BitWidth} bits)";

    #endregion

    #region INotifyPropertyChanged実装

    /// <summary>
    /// プロパティ変更通知イベント
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// プロパティ変更通知を発生させる
    /// </summary>
    /// <param name="propertyName">変更されたプロパティ名（CallerMemberName属性により自動設定）</param>
    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

#pragma warning disable WPF0001
/// <summary>
/// メインウィンドウのビューモデル
/// VCD波形ビューアアプリケーションの主要なビジネスロジックとUI状態を管理
/// MVVMパターンに基づいてViewとModelを仲介する役割を持つ
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    #region テーマ関連

    /// <summary>
    /// ダークモードが有効かどうかのフラグ
    /// </summary>
    [ObservableProperty]
    bool isDarkMode = true;

    /// <summary>
    /// ダークモード設定変更時の処理
    /// ライトモードプロパティの変更通知も併せて発生させる
    /// </summary>
    /// <param name="value">新しいダークモード設定値</param>
    partial void OnIsDarkModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsLightMode));
    }

    /// <summary>
    /// ライトモードが有効かどうか（ダークモードの逆）
    /// </summary>
    public bool IsLightMode => !IsDarkMode;

    /// <summary>
    /// テーマ切り替えコマンド
    /// ダークモードとライトモードを切り替える
    /// </summary>
    [RelayCommand]
    public void ChangeTheme()
    {
        if(App.Current.ThemeMode == ThemeMode.Light)
        {
            // ライトモードからダークモードに切り替え
            App.Current.ThemeMode = ThemeMode.Dark;
            IsDarkMode = true;
        }
        else
        {
            // ダークモードからライトモードに切り替え
            App.Current.ThemeMode = ThemeMode.Light;
            IsDarkMode = false;
        }
    }

    #endregion

    #region VCDファイルとモジュール・信号管理

    /// <summary>
    /// モジュールのツリー構造
    /// VCDファイルから読み込んだモジュール階層をTreeViewで表示するために使用
    /// </summary>
    [ObservableProperty]
    ObservableCollection<ModuleTreeNode> moduleTree = new();

    /// <summary>
    /// 現在選択されているモジュール
    /// TreeViewで選択されたモジュールの情報を保持
    /// </summary>
    [ObservableProperty]
    ModuleTreeNode? selectedModule;

    /// <summary>
    /// 選択されたモジュール内の信号リスト
    /// 現在選択されているモジュールに含まれる変数（信号）を表示するために使用
    /// </summary>
    [ObservableProperty]
    ObservableCollection<VariableDisplayItem> signalList = new();

    /// <summary>
    /// 選択モジュール変更時の処理
    /// 新しく選択されたモジュールの信号リストを更新する
    /// </summary>
    /// <param name="value">新しく選択されたモジュール</param>
    partial void OnSelectedModuleChanged(ModuleTreeNode? value)
    {
        UpdateSignalList();
    }

    /// <summary>
    /// 現在読み込まれているVCDファイルのデータ
    /// nullの場合はVCDファイルが読み込まれていない状態を示す
    /// </summary>
    Vcd? activeVcd;

    /// <summary>
    /// VCDファイル読み込みコマンド
    /// ファイルダイアログを表示してVCDファイルを選択・読み込みを行う
    /// </summary>
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

    /// <summary>
    /// ModuleオブジェクトからModuleTreeNodeを再帰的に作成
    /// VCDファイルから読み込んだモジュール階層をTreeView表示用に変換する
    /// </summary>
    /// <param name="module">変換対象のModuleオブジェクト</param>
    /// <returns>作成されたModuleTreeNode</returns>
    private ModuleTreeNode CreateModuleTreeNode(Module module)
    {
        var node = new ModuleTreeNode
        {
            Name = module.Name,
            ModuleData = module
        };

        // サブモジュールのみを再帰的に追加（変数は含めない）
        // 変数は選択されたモジュールの SignalList で別途表示
        foreach (var subModule in module.SubModules)
        {
            var subModuleNode = CreateModuleTreeNode(subModule);
            node.Children.Add(subModuleNode);
        }

        return node;
    }

    /// <summary>
    /// 選択されたモジュールの信号リストを更新
    /// 現在選択されているモジュールに含まれる全ての変数をSignalListに反映する
    /// </summary>
    private void UpdateSignalList()
    {
        SignalList.Clear();
        
        if (SelectedModule?.ModuleData != null)
        {
            // 選択されたモジュールの全変数をVariableDisplayItemに変換して追加
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

    /// <summary>
    /// VCDファイルを読み込んでモジュールツリーを構築
    /// </summary>
    /// <param name="filePath">読み込むVCDファイルのパス</param>
    private void ReadVcdFile(string filePath)
    {
        try
        {
            // 新しいVCDオブジェクトを作成してファイルを読み込み
            activeVcd = new Vcd();
            activeVcd.LoadFromFile(filePath);

            // トップモジュール以下のモジュール階層をTreeViewに表示
            ModuleTree.Clear();
            if (activeVcd.TopModule != null)
            {
                var topModuleNode = CreateModuleTreeNode(activeVcd.TopModule);
                ModuleTree.Add(topModuleNode);
            }
        }
        catch (Exception ex)
        {
            // ファイル読み込みエラーをユーザーに通知
            MessageBox.Show($"VCDファイルの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region テスト・デモ用データ

    /// <summary>
    /// テスト用の単純な文字列コレクション
    /// DragableListの基本動作確認用
    /// </summary>
    [ObservableProperty]
    ObservableCollection<object> hoges = ["a", "b", "c", "d", "longlonglonglong_e"];

    /// <summary>
    /// テスト用のサンプル信号コレクション
    /// DragableListのDisplayMemberPath機能とプロパティ変更通知のテスト用
    /// </summary>
    [ObservableProperty]
    ObservableCollection<object> sampleSignals =
    [
        new VariableDisplayItem 
        { 
            Name = "clk", 
            Type = "Wire", 
            BitWidth = 1,
            VariableData = new Variable(Variable.VariableType.Wire, 1, "clk_id", "clk")
        },
        new VariableDisplayItem 
        { 
            Name = "reset", 
            Type = "Wire", 
            BitWidth = 1,
            VariableData = new Variable(Variable.VariableType.Wire, 1, "reset_id", "reset")
        },
        new VariableDisplayItem 
        { 
            Name = "data_bus", 
            Type = "Wire", 
            BitWidth = 32,
            VariableData = new Variable(Variable.VariableType.Wire, 32, "data_bus_id", "data_bus")
        },
        new VariableDisplayItem 
        { 
            Name = "counter", 
            Type = "Reg", 
            BitWidth = 8,
            VariableData = new Variable(Variable.VariableType.Reg, 8, "counter_id", "counter")
        }
    ];

    #endregion

    #region テスト用コマンド

    /// <summary>
    /// 最初の信号のプロパティを変更するテストコマンド
    /// プロパティ変更通知の動作確認用
    /// BitWidthとTypeを交互に変更してDragableListの自動更新をテスト
    /// </summary>
    [RelayCommand]
    public void ModifyFirstSignal()
    {
        if (SampleSignals.Count > 0)
        {
            var sample = SampleSignals[0];
            if (sample is not VariableDisplayItem signal) return;
            
            // BitWidthとTypeを変更してプロパティ変更通知をテスト
            signal.BitWidth = signal.BitWidth == 1 ? 8 : 1;
            signal.Type = signal.Type == "Wire" ? "Reg" : "Wire";
        }
    }

    /// <summary>
    /// ランダムなテスト信号を追加するコマンド
    /// コレクション変更の動作確認用
    /// ランダムな名前、タイプ、ビット幅でダミー信号を生成・追加
    /// </summary>
    [RelayCommand]
    public void AddTestSignal()
    {
        var random = new Random();
        var signalNumber = SampleSignals.Count + 1;
        SampleSignals.Add(new VariableDisplayItem
        {
            Name = $"test_signal_{signalNumber}",
            Type = random.Next(2) == 0 ? "Wire" : "Reg",
            BitWidth = random.Next(1, 33), // 1-32ビットのランダムな幅
            VariableData = new Variable(Variable.VariableType.Wire, 1, $"test_{signalNumber}_id", $"test_signal_{signalNumber}")
        });
    }

    /// <summary>
    /// 最後の信号を削除するコマンド
    /// コレクションからのアイテム削除の動作確認用
    /// </summary>
    [RelayCommand]
    public void RemoveLastSignal()
    {
        if (SampleSignals.Count > 0)
        {
            SampleSignals.RemoveAt(SampleSignals.Count - 1);
        }
    }

    #endregion
}

#pragma warning restore WPF0001