using CommunityToolkit.Mvvm.ComponentModel;
using CursorEngine.Model;
using CursorEngine.Services;
using CursorEngine.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.ViewModel;

public partial class BrowserPageViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IFileService _fileService;
    private readonly PathService _pathService;

    public ObservableCollection<BrowserSchemeViewModel> Schemes { get; }

    public BrowserPageViewModel(IEnumerable<SchemeInfoViewModel> schemes, PathService pathService, IApiService apiService, IFileService fileService)
    {
        _apiService = apiService;
        _fileService = fileService;
        _pathService = pathService;

        Schemes = new ObservableCollection<BrowserSchemeViewModel>(
            schemes.Select(dto =>  new BrowserSchemeViewModel(dto.Id, dto.Name)));
    }

    public async Task LoadAllPreviews()
    {
        var previewTasks = Schemes.Select(vm => vm.LoadPreview(_apiService, _fileService, _pathService));
        await Task.WhenAll(previewTasks);
    }
}
