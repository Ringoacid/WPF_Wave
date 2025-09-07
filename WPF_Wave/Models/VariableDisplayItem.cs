using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

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

                // 表示テキストも更新通知
                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(DescriptionText));
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

                // 表示テキストも更新通知
                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(DescriptionText));
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
    public string DisplayText => (BitWidth == 1) ? $"{Type}: {Name} ({BitWidth} bit)" : $"{Type}: {Name} ({BitWidth} bits)";

    public string DescriptionText => (BitWidth == 1) ? $"{Type}: ({BitWidth} bit)" : $"{Type}: ({BitWidth} bits)";

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

