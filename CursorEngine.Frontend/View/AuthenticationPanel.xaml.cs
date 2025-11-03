using CursorEngine.ViewModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CursorEngine.Frontend.View
{
    /// <summary>
    /// AuthenticationPanel.xaml 的交互逻辑
    /// </summary>
    public partial class AuthenticationPanel : Window
    {
        public AuthenticationPanel()
        {
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is AuthenticationViewModel vm)
                {
                    vm.OnLogoutSuccess += () =>
                    {
                        try { this.DialogResult = true; }
                        catch (InvalidOperationException) { this.Close(); }
                    };
                    vm.OnLoginSuccess += (LoginSuccessEventArgs args) =>
                    {
                        try { this.DialogResult = true; }
                        catch (InvalidOperationException) { this.Close(); }
                    };
                }
            };
            InitializeComponent();
        }
    }
}
