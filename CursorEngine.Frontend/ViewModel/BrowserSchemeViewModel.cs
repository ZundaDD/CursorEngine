using CommunityToolkit.Mvvm.ComponentModel;
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

public partial class BrowserSchemeViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "New Scheme";
    [ObservableProperty] private ImageSource? _previewImage;
    public Dictionary<RegistryIndex, string?> Paths = new();
    public int ID = -1;

    public BrowserSchemeViewModel(int id, string name)
    {
        ID = id;
        Name = name;
    }

    public async Task LoadPreview(IApiService apiService, IFileService previewService, PathService pathService)
    {
        var tmpPath = Path.Combine(pathService.UserSchemePath, $"tempPreview_{ID}");

        if (PreviewImage != null) return;
        try
        {
            byte[]? imageData = await apiService.GetPreviewAsync(ID);

            if (imageData == null || imageData.Length == 0) return;

            await File.WriteAllBytesAsync(tmpPath ,imageData);
            ImageSource? loadedImage = await Task.Run(() => previewService.LoadCursorPreview(tmpPath));

            if (loadedImage == null) return;

            PreviewImage = loadedImage;
        }
        catch (Exception) { }
        finally
        {
            if(File.Exists(tmpPath)) File.Delete(tmpPath);
        }
    }
}
