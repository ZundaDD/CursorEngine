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

public partial class SchemeViewModel : ObservableObject, IRenameable
{
    [ObservableProperty] private bool _isRegistered;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _name = "New Scheme";
    [ObservableProperty] private ImageSource? _previewImage;
    public Dictionary<RegistryIndex, string?> Paths = new();

    public SchemeViewModel(CursorScheme scheme)
    {
        IsRegistered = scheme.IsRegistered;
        Name = scheme.Name;
        Paths = scheme.Paths;
    }

    public CursorScheme FullConvert()
    {
        var raw = new CursorScheme(Name, IsRegistered);
        raw.Paths = Paths;
        return raw;
    }

    public void LoadPreview(IFileService previewService)
    {
        if (Paths.TryGetValue(RegistryIndex.Arrow,out var value) && value != null) 
            PreviewImage = previewService.LoadCursorPreview(value);
    }

    public CursorScheme MinConvert() => new CursorScheme(Name, IsRegistered);
}
