using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.Model;
using CursorEngine.Services;
using CursorEngine.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.ViewModel;

public partial class SchemeEditViewModel : ObservableObject
{
    private readonly PathService _pathService;
    private readonly IDialogService _dialogService;
    private readonly IFileService _fileService;

    [ObservableProperty,NotifyPropertyChangedFor(nameof(Editable),nameof(SchemeName))]
    private SchemeViewModel _originalScheme = null!;

    public ObservableCollection<SchemeSlotViewModel> Slots { get; } = new();

    public bool Editable => OriginalScheme == null ? false : !OriginalScheme.IsRegistered;

    public string SchemeName => OriginalScheme == null ? "" : OriginalScheme.Name;

    public SchemeEditViewModel(PathService pathService, IDialogService dialogService, IFileService fileService)
    {
        _pathService = pathService;
        _dialogService = dialogService;
        _fileService = fileService;
    }

    public void Save()
    {
        foreach (var slotVM in Slots)
        {
            //路径有效才写回
            if(File.Exists(slotVM.FilePath)) OriginalScheme.Paths[slotVM.SlotKey] = slotVM.FilePath;
        }
    }

    public void LoadScheme(SchemeViewModel scheme)
    {
        OriginalScheme = scheme;

        Slots.Clear();

        //这里是从原始数据中加载复制的ViewModel
        foreach (var key in Enum.GetValues<RegistryIndex>())
        {
            var slotVM = new SchemeSlotViewModel(OriginalScheme.Name, key, OriginalScheme.Paths.GetValueOrDefault(key, ""), _dialogService, _fileService, _pathService, Editable);
            Slots.Add(slotVM);
        }
    }
}
