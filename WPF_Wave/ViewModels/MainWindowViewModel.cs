using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using WPF_Wave.Models;
using WPF_Wave.Views;

namespace WPF_Wave.ViewModels;


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
        if (App.Current.ThemeMode == ThemeMode.Light)
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
    /// ListView で現在選択されている信号（単一選択用、後方互換性のため残す）
    /// </summary>
    [ObservableProperty]
    VariableDisplayItem? selectedSignal;

    /// <summary>
    /// ListView で現在選択されている複数の信号
    /// 「波形に追加」ボタンで波形表示リストに追加する対象の信号群
    /// </summary>
    [ObservableProperty]
    List<VariableDisplayItem> selectedSignals = new();

    /// <summary>
    /// 波形表示用に選択された信号のコレクション
    /// DragableListで表示され、ユーザーが波形を確認したい信号のリスト
    /// </summary>
    [ObservableProperty]
    ObservableCollection<VariableDisplayItem> selectedSignalsForWaveform = new();

    [ObservableProperty]
    public VariableDisplayItem? firstSelectedSignalsForWaveForm;

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
    [ObservableProperty]
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
    /// 選択された信号を波形表示リストに追加するコマンド
    /// ListViewで選択された複数の信号をDragableListに追加する
    /// </summary>
    [RelayCommand]
    public void AddSelectedSignalToWaveform()
    {
        if (SelectedSignals == null || SelectedSignals.Count == 0)
        {
            MessageBox.Show("追加する信号を選択してください。",
                "選択エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var addedCount = 0;
        var duplicateCount = 0;

        foreach (var selectedSignal in SelectedSignals)
        {
            // 既に追加されているかチェック（重複防止）
            var existingSignal = SelectedSignalsForWaveform.FirstOrDefault(s =>
                s.Name == selectedSignal.Name &&
                s.Type == selectedSignal.Type &&
                s.BitWidth == selectedSignal.BitWidth);

            if (existingSignal == null)
            {
                // 新しいインスタンスを作成して追加（元の信号への影響を防ぐ）
                var newSignal = new VariableDisplayItem
                {
                    Name = selectedSignal.Name,
                    Type = selectedSignal.Type,
                    BitWidth = selectedSignal.BitWidth,
                    VariableData = selectedSignal.VariableData
                };
                SelectedSignalsForWaveform.Add(newSignal);
                addedCount++;
            }
            else
            {
                duplicateCount++;
            }
        }

        // 結果をユーザーに通知
        if (addedCount > 0 && duplicateCount > 0)
        {
            MessageBox.Show($"{addedCount}個の信号を追加しました。\n{duplicateCount}個の信号は既に追加済みのためスキップしました。",
                "一括追加完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (duplicateCount > 0)
        {
            MessageBox.Show($"選択された{duplicateCount}個の信号は全て既に波形表示リストに追加されています。",
                "重複エラー", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        FirstSelectedSignalsForWaveForm = SelectedSignalsForWaveform.FirstOrDefault();
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
            ActiveVcd = new Vcd();
            ActiveVcd.LoadFromFile(filePath);

            // トップモジュール以下のモジュール階層をTreeViewに表示
            ModuleTree.Clear();
            if (ActiveVcd.TopModule != null)
            {
                var topModuleNode = CreateModuleTreeNode(ActiveVcd.TopModule);
                ModuleTree.Add(topModuleNode);

                SignalList.Clear();
                SelectedSignalsForWaveform.Clear();
            }
        }
        catch (Exception ex)
        {
            // ファイル読み込みエラーをユーザーに通知
            MessageBox.Show($"VCDファイルの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region 初期化

    /// <summary>
    /// コンストラクタ
    /// 初期化処理を行う
    /// </summary>
    public MainWindowViewModel()
    {
    }

    #endregion

    #region ライセンス関連
    [RelayCommand]
    void ShowLicense()
    {
        LicenseDisplayWindow licenseWindow = new LicenseDisplayWindow();
        licenseWindow.Owner = Application.Current.MainWindow;
        licenseWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        licenseWindow.ShowDialog();
    }
    #endregion
}

#pragma warning restore WPF0001