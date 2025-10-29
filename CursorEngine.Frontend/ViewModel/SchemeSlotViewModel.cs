using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.Model;
using CursorEngine.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CursorEngine.ViewModel;

public partial class SchemeSlotViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly PathService _pathService;
    private readonly string _schemeName;

    public RegistryIndex SlotKey { get; }

    public string SlotName => SlotKey.ToString();

    [ObservableProperty] private string _filePath;
    [ObservableProperty] private ImageSource? _previewImage;
    [ObservableProperty] private bool _editable;

    public SchemeSlotViewModel(string schemeName, RegistryIndex slotKey, string initialPath, IDialogService dialogSerice, IFileService fileService, PathService pathService, bool editable)
    {
        SlotKey = slotKey;
        Editable = editable;
        _schemeName = schemeName;
        _filePath = initialPath;
        _fileService = fileService;
        _dialogService = dialogSerice;
        _pathService = pathService;
        _ = LoadPreviewAsync();
    }

    partial void OnFilePathChanged(string value) => _ = LoadPreviewAsync();

    private async Task LoadPreviewAsync()
    {
        try
        {
            var image = await Task.Run(() => _fileService.LoadCursorPreview(FilePath));
            PreviewImage = image;
        }
        catch (System.Exception) { }
    }

    [RelayCommand(CanExecute = nameof(Editable))]
    private void BrowseFile()
    {
        var result = _dialogService.ChooseCursorFile();
        if (result != null && File.Exists(result))
        {
            string srcDir = result;
            string destDir = Path.Combine(_pathService.UserSchemePath,_schemeName, Path.GetFileName(result));
            
            File.Copy(srcDir, destDir);

            FilePath = destDir;
        }
    }
}
