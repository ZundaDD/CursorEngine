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

public enum MainPage
{
    LocalSchemes,
    SchemeEditor,
    OnlineBrowser
}

public partial class MainViewModel : ObservableObject
{
    private MainPage _previousPage;
    private readonly IServiceProvider _serviceProvider;

    public LocalSchemeViewModel LocalSchemeVM { get; }

    public SchemeEditViewModel SchemeEditVM { get; }

    public BrowserViewModel OnlineBrowserVM { get; }

    [ObservableProperty] private int _currentPageIndex;

    public MainViewModel(IServiceProvider serviceProvider, LocalSchemeViewModel localSchemeViewModel, BrowserViewModel browserViewModel,SchemeEditViewModel schemeEditViewModel)
    {
        LocalSchemeVM = localSchemeViewModel;
        SchemeEditVM = schemeEditViewModel;
        OnlineBrowserVM = browserViewModel;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    public void ShowWindow()
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        mainWindow.Activate();
    }

    [RelayCommand]
    public void ExitApplication() => Application.Current.Shutdown();

    [RelayCommand]
    private void NavigateToLocalSchemes()
    {
        LocalSchemeVM.SaveScheme();
        CurrentPageIndex = (int)MainPage.LocalSchemes;
    }

    [RelayCommand]
    private void NavigateToOnlineBrowser()
    {
        OnlineBrowserVM.ClearTemp();
        CurrentPageIndex = (int)MainPage.OnlineBrowser;
        if (OnlineBrowserVM.LoadInitialPageCommand.CanExecute(null))
        {
            _ = OnlineBrowserVM.LoadInitialPageCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private void NavigateToSchemeEditor(SchemeViewModel schemeToEdit)
    {
        _previousPage = (MainPage) CurrentPageIndex;

        if (schemeToEdit == null) return;

        SchemeEditVM.LoadScheme(schemeToEdit);

        CurrentPageIndex = (int)MainPage.SchemeEditor;
    }

    [RelayCommand]
    private void NavigateAndSave()
    {
        SchemeEditVM.Save();
        if (_previousPage == MainPage.LocalSchemes) NavigateToLocalSchemesCommand.Execute(null);
        else NavigateToOnlineBrowserCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToPrevious()
    {
        if(_previousPage == MainPage.LocalSchemes) NavigateToLocalSchemesCommand.Execute(null);
        else NavigateToOnlineBrowserCommand.Execute(null);
    }
}
