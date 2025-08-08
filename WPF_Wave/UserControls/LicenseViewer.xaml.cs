using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;

namespace WPF_Wave.UserControls;

public class LicenseInformation
{
    public string Name { get; set; } = "???";

    public string Version { get; set; } = "???";

    public string LicenseText { get; set; } = "???";

    public bool IsHasThirdPartyLicense { get; set; } = false;

    public string ThirdPartyLicenseText { get; set; } = "???";

    public bool LoadFromFile(string filepath)
    {
        // ファイル名の形式をチェック
        string filename = Path.GetFileNameWithoutExtension(filepath);
        var parts = filename.Split('-');
        if(parts.Length != 2)
        {
            return false;
        }

        Name = parts[0].Trim();
        Version = parts[1].Trim();

        // サードパーティーライセンスの有無をチェック
        string thirdPartyLicenseFile = Path.Combine(Path.GetDirectoryName(filepath) ?? string.Empty, $"{Name}-{Version}-ThirdPartyNotices.txt");
        if (File.Exists(thirdPartyLicenseFile))
        {
            IsHasThirdPartyLicense = true;
            ThirdPartyLicenseText = File.ReadAllText(thirdPartyLicenseFile);
        }
        else
        {
            IsHasThirdPartyLicense = false;
            ThirdPartyLicenseText = string.Empty;
        }

        // ライセンステキストの読み込み
        LicenseText = File.ReadAllText(filepath);

        return true;
    }

    public static List<LicenseInformation> LoadFromDirectory(string directoryPath = "LICENSES")
    {
        if(Directory.Exists(directoryPath) == false)
        {
            return [];
        }

        var licenseFiles = Directory.GetFiles(directoryPath);
        var licenses = new List<LicenseInformation>();
        foreach (var file in licenseFiles)
        {
            var licenseInfo = new LicenseInformation();
            if (licenseInfo.LoadFromFile(file))
            {
                licenses.Add(licenseInfo);
            }
        }
        return licenses;
    }
}



/// <summary>
/// LicenseViewer.xaml の相互作用ロジック
/// </summary>
public partial class LicenseViewer : UserControl
{
    public LicenseViewer()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        var licenseInfos = LicenseInformation.LoadFromDirectory();

        LicensesControl.ItemsSource = licenseInfos;
    }
}
