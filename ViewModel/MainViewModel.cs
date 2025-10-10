using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.View;
using CursorEngine.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CursorEngine.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly CursorControl _cursorControl;
    private readonly IServiceProvider _serviceProvider;

    public MainViewModel(CursorControl cursorControl, IServiceProvider serviceProvider)
    {
        _cursorControl = cursorControl;
        _serviceProvider = serviceProvider;
        Schemes = new ObservableCollection<CursorScheme>(_cursorControl.LoadAllSchemes());

        ShowWindowCommand = new RelayCommand(_ => ShowWindow());
        ExitApplicationCommand = new RelayCommand(_ => ExitApplication());
        DeleteSchemeCommand = new RelayCommand(_ => DeleteScheme(), _ => IsNotRegistered());
        RegisterSchemeCommand = new RelayCommand(_ => RegisterScheme(), _ => IsNotRegistered());
        AddSchemeCommand = new RelayCommand(_ => AddSheme());
        TrySelectedSchemeCommand = new RelayCommand(_ => TrySelectedScheme());
    }

    public ObservableCollection<CursorScheme> Schemes { get; set; }
    
    [ObservableProperty] private CursorScheme _selectedScheme = null!;
    
    public ICommand ShowWindowCommand { get; }
    public ICommand ExitApplicationCommand { get; }
    public ICommand DeleteSchemeCommand { get; }
    public ICommand RegisterSchemeCommand { get; }
    public ICommand AddSchemeCommand { get; }
    public ICommand TrySelectedSchemeCommand { get; }

    public bool IsRegistered() => SelectedScheme == null ? false : SelectedScheme.IsRegistered;
    
    public bool IsNotRegistered() => SelectedScheme == null ? false : !SelectedScheme.IsRegistered;

    public void TrySelectedScheme()
    {
        if (SelectedScheme == null) return;

        _cursorControl.ApplyScheme(SelectedScheme);
        //只是应用，不会触发文件修改
    }

    public void AddSheme()
    {
        var scheme = _cursorControl.AddRawUserScheme(GetUniqueName("New Scheme"));
        
        if(scheme != null)
        {
            Schemes.Add(scheme);
            _cursorControl.SaveUserSchemes(UserSchemes);
        }
    }

    public void DeleteScheme()
    {
        if (SelectedScheme == null) return;
        
        if(_cursorControl.DeleteUserSchemes(SelectedScheme))
        {
            Schemes.Remove(SelectedScheme);
            _cursorControl.SaveUserSchemes(UserSchemes);
        }
    }

    public void RegisterScheme()
    {
        if(SelectedScheme == null) return;
        var scheme = _cursorControl.SaveSchemeToRegistry(SelectedScheme, GetUniqueName(SelectedScheme.Name));
        if(scheme != null) Schemes.Add(scheme);
        //由于是注册方案，所以不需要额外写入json文件
    }

    public void ShowWindow()
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        mainWindow.Activate();
    }

    public void ExitApplication() => Application.Current.Shutdown();

    #region 辅助函数
    public bool IsNameExisted(string name) => Schemes.Any(cs => cs.Name == name);

    public List<CursorScheme> UserSchemes => Schemes.Where(cs => !cs.IsRegistered).ToList();

    public string GetUniqueName(string baseName)
    {
        int suffix = 0;
        string uniqueName = baseName;
        //如果重复就一直刷新
        while (IsNameExisted(uniqueName))
        {
            uniqueName = $"{baseName} {suffix}";
            suffix++;
        }
        return uniqueName;
    }
    #endregion
}
