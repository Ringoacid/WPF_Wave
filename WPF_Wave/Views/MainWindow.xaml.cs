using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using WPF_Wave.Helpers;
using WPF_Wave.Models;
using WPF_Wave.UserControls;
using WPF_Wave.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Linq;

namespace WPF_Wave;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public MainWindowViewModel ViewModel { get; }

    private bool isFolded = false;

    private string foldButtonArrow = "▼";

    public string FoldButtonArrow
    {
        get => foldButtonArrow;
        set
        {
            if (foldButtonArrow != value)
            {
                foldButtonArrow = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FoldButtonArrow)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainWindowViewModel();
        DataContext = this;
    }

    private Storyboard CreateSlideInStoryboard()
    {
        double contentWidth = FoldableContent.ActualWidth;
        var storyboard = new Storyboard();

        // FoldableContentのマージンアニメーション
        var slideAnimation = new ThicknessAnimation
        {
            To = new Thickness(0),
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            DecelerationRatio = 1.0
        };
        Storyboard.SetTargetName(slideAnimation, "FoldableContent");
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath(FrameworkElement.MarginProperty));

        storyboard.Children.Add(slideAnimation);

        return storyboard;
    }

    private Storyboard CreateSlideOutStoryboard()
    {
        double contentWidth = FoldableContent.ActualWidth;
        var storyboard = new Storyboard();

        // FoldableContentのマージンアニメーション
        var slideAnimation = new ThicknessAnimation
        {
            To = new Thickness(-contentWidth - 1,0,0,0),
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            DecelerationRatio = 1.0
        };
        Storyboard.SetTargetName(slideAnimation, "FoldableContent");
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath(FrameworkElement.MarginProperty));

        storyboard.Children.Add(slideAnimation);

        return storyboard;
    }

    private void OnFoldButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            Storyboard storyboard;
            
            if (!isFolded)
            {
                storyboard = CreateSlideOutStoryboard();
                FoldButtonArrow = "▲";
            }
            else
            {
                storyboard = CreateSlideInStoryboard();
                FoldButtonArrow = "▼";
            }
            
            storyboard.Begin(this);
            isFolded = !isFolded;
        }
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is ModuleTreeNode selectedNode)
        {
            ViewModel.SelectedModule = selectedNode;
        }
    }

    /// <summary>
    /// ListView の選択項目が変更された時の処理
    /// 複数選択された信号をViewModelに通知する
    /// </summary>
    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView)
        {
            // 選択された信号をViewModelに設定
            ViewModel.SelectedSignals = listView.SelectedItems.Cast<VariableDisplayItem>().ToList();
        }
    }
}