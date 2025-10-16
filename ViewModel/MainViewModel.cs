using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.Model;
using CursorEngine.Services;
using CursorEngine.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace CursorEngine.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly CursorControl _cursorControl;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;
    private readonly RuleControl _ruleControl;

    public MainViewModel(CursorControl cursorControl, RuleControl ruleControl,IServiceProvider serviceProvider, IDialogService dialogService)
    {
        _cursorControl = cursorControl;
        _ruleControl = ruleControl;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;

        Schemes = new ObservableCollection<SchemeViewModel>(_cursorControl.LoadAllSchemes().Select(cs => new SchemeViewModel(cs)).ToList());
        Rules = new ObservableCollection<CursorRule>(_ruleControl.LoadRules());
        SelectedRule = Rules.Count == 0 ? null! : Rules[0];
    }

    #region 框体指令

    [RelayCommand]
    public void ShowWindow()
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        mainWindow.Activate();
    }

    [RelayCommand]
    public void ExitApplication() => Application.Current.Shutdown();
    #endregion

    #region 辅助函数
    public List<CursorScheme> UserSchemes => Schemes.Where(svm => !svm.IsRegistered).Select(svm => svm.FullConvert()).ToList();

    public bool IsNameExisted(string name, IEnumerable<IRenameable> origins) => origins.Any(cs => cs.Name == name);

    
    public string GetUniqueName(string baseName, IEnumerable<IRenameable> origins)
    {
        int suffix = 0;
        string uniqueName = baseName;
        //如果重复就一直刷新
        while (IsNameExisted(uniqueName, origins))
        {
            uniqueName = $"{baseName} {suffix}";
            suffix++;
        }
        return uniqueName;
    }
    #endregion
}
