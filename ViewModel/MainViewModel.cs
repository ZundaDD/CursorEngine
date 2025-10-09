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
    }

    public ObservableCollection<CursorScheme> Schemes { get; set; }

    [RelayCommand]
    public void ShowWindow()
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        mainWindow.Activate();
    }


    [RelayCommand]
    public void ExitApplication()
    {
        Application.Current.Shutdown();
    }
}
