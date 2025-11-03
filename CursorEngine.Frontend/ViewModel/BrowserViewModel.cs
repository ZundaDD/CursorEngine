using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.Model;
using CursorEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.ViewModel;

public partial class BrowserViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IFileService _fileService;
    private readonly PathService _pathService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<BrowserPageViewModel> Pages { get; } = new();

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(DownloadSchemeCommand))]
    private BrowserSchemeViewModel _selectedScheme = null!;

    [ObservableProperty]
    private BrowserPageViewModel _currentPage = null!;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoNextCommand), nameof(GoPreviousCommand))]
    private int _currentPageIndex = 0;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    private int _nextPageNumberToFetch = 1;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoNextCommand), nameof(GoPreviousCommand))]
    private bool _hasMorePages = true;

    private bool CanDownload => SelectedScheme != null;

    public BrowserViewModel(IServiceProvider serviceProvider, IApiService apiService, IFileService fileService, PathService pathService)
    {
        _serviceProvider = serviceProvider;
        _apiService = apiService;
        _fileService = fileService;
        _pathService = pathService;
    }

    public void ClearTemp()
    {
        var zipPath = Path.Combine(_pathService.UserSchemePath, "temp.zip");
        var zipDir = Path.Combine(_pathService.UserSchemePath, ".temp");

        if (File.Exists(zipPath)) File.Delete(zipPath);
        if (Directory.Exists(zipDir)) Directory.Delete(zipDir, true);
    }

    [RelayCommand(CanExecute = nameof(CanDownload))]
    private async Task DownloadSchemeAsync()
    {
        var general = await DownloadAndUnpackAsync(SelectedScheme.ID);
        general.IsRegistered = false;
        if (general == null) return;

        var localViewModel = _serviceProvider.GetRequiredService<LocalSchemeViewModel>();
        if(localViewModel == null) return;
        localViewModel.ForkScheme(general.FullConvert(), SelectedScheme.Name);

        var unpackPath = Path.Combine(_pathService.UserSchemePath, ".temp");
        if (Directory.Exists(unpackPath)) Directory.Delete(unpackPath, true);
    }

    [RelayCommand]
    private async Task ViewSchemeAsync(BrowserSchemeViewModel browserViewModel)
    {
        var general = await DownloadAndUnpackAsync(browserViewModel.ID);
        if (general == null) return;

        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        if (mainViewModel == null) return;
        mainViewModel.NavigateToSchemeEditorCommand.Execute(general);
    }

    private async Task<SchemeViewModel> DownloadAndUnpackAsync(int schemeId)
    {
        string? tempZipPath = null;
        try
        {
            byte[] zipData = await _apiService.DownloadSchemeAsync(schemeId);
            if (zipData == null || zipData.Length == 0) return null!;

            tempZipPath = Path.Combine(_pathService.UserSchemePath, "temp.zip");
            var unpackPath = Path.Combine(_pathService.UserSchemePath, ".temp");
            await File.WriteAllBytesAsync(tempZipPath, zipData);

            if (Directory.Exists(unpackPath)) Directory.Delete(unpackPath, true);
            Directory.CreateDirectory(unpackPath);

            ZipFile.ExtractToDirectory(tempZipPath, unpackPath);

            var manifestPath = Path.Combine(unpackPath, "scheme.json");
            if (!File.Exists(manifestPath)) return null!;

            var jsonContent = await File.ReadAllTextAsync(manifestPath);
            var pathsWithFileNames = JsonConvert.DeserializeObject<Dictionary<RegistryIndex, string?>>(jsonContent);
            if (pathsWithFileNames == null) return null!;

            Dictionary<RegistryIndex, string?> fullPaths = pathsWithFileNames.ToDictionary(
                kv => kv.Key,
                kv => kv.Value == null ? null : Path.Combine(unpackPath, kv.Value)
            );

            CursorScheme cursorScheme = new CursorScheme(".temp", true);
            cursorScheme.Paths = fullPaths;

            var schemeModel = new SchemeViewModel(cursorScheme);

            return schemeModel;
        }
        catch (Exception)
        {
            if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
            return null!;
        }
        finally
        {
            if (tempZipPath != null && File.Exists(tempZipPath)) File.Delete(tempZipPath);
        }
    }

    [RelayCommand]
    private async Task LoadInitialPageAsync()
    {
        IsLoading = true;

        StatusMessage = "正在连接服务器...";
        Pages.Clear();

        _nextPageNumberToFetch = 1;
        HasMorePages = true;

        var initialSchemes = await _apiService.GetSchemesAsync(_nextPageNumberToFetch);
        if (initialSchemes.Any())
        {
            var firstPageVM = new BrowserPageViewModel(initialSchemes, _pathService, _apiService, _fileService);
            Pages.Add(firstPageVM);
            await firstPageVM.LoadAllPreviews();

            CurrentPage = Pages[0];

            _nextPageNumberToFetch++;
            StatusMessage = null;
        }
        else
        {
            StatusMessage = "未能找到任何在线方案。";
            HasMorePages = false;
        }

        var newSchemes = await _apiService.GetSchemesAsync(_nextPageNumberToFetch);
        if (newSchemes.Any())
        {
            var firstPageVM = new BrowserPageViewModel(newSchemes, _pathService, _apiService, _fileService);
            Pages.Add(firstPageVM);
            await firstPageVM.LoadAllPreviews();

            _nextPageNumberToFetch++;
        }
        else
        {
            HasMorePages = false;
        }

        IsLoading = false;
        CurrentPageIndex = 0;
    }

    private bool CanGoNext() => CurrentPageIndex < Pages.Count - 1 || HasMorePages;

    private bool CanGoPrevious() => CurrentPageIndex > 0;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task GoNextAsync()
    {
        if (CurrentPageIndex == _nextPageNumberToFetch - 3 && HasMorePages)
        {
            IsLoading = true;

            var newSchemes = await _apiService.GetSchemesAsync(_nextPageNumberToFetch);
            if (newSchemes.Any())
            {
                var newPageVM = new BrowserPageViewModel(newSchemes, _pathService, _apiService, _fileService);
                Pages.Add(newPageVM);
                await newPageVM.LoadAllPreviews();

                _nextPageNumberToFetch++;
                CurrentPageIndex++;
            }
            else HasMorePages = false;

            IsLoading = false;
        }
        else CurrentPageIndex++;

        CurrentPage = Pages[CurrentPageIndex];
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void GoPrevious()
    {
        CurrentPageIndex--;
        CurrentPage = Pages[CurrentPageIndex];
    }
}
