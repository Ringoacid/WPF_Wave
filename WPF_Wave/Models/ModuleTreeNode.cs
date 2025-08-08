using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

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
