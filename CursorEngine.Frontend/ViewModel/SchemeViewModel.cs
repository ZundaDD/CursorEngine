using CommunityToolkit.Mvvm.ComponentModel;
using CursorEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.ViewModel;

public partial class SchemeViewModel : ObservableObject, IRenameable
{
    [ObservableProperty] private bool _isRegistered;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _name = "New Scheme";
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

    public CursorScheme MinConvert() => new CursorScheme(Name, IsRegistered);
}
