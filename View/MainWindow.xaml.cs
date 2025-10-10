using CursorEngine.Model;
using CursorEngine.ViewModel;
using H.NotifyIcon;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CursorEngine.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public MainWindow(MainViewModel mainViewModel, IHostApplicationLifetime hostLifetime)
    {
        InitializeComponent();
        this.DataContext = mainViewModel;

        hostLifetime.ApplicationStopping.Register(OnApplicationExit);
    }

    /// <summary>
    /// 骗你的，没有真的关闭
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        base.OnClosing(e);
    }

    public void OnApplicationExit()
    {
        Dispatcher.Invoke(() => TaskbarIcon.Dispose());
    }
}