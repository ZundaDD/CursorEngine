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

    public SchemeViewModel OriginalScheme { get; }

    public bool Editable => !OriginalScheme.IsRegistered;

    public string SchemeName => OriginalScheme.Name;

    public ObservableCollection<SchemeSlotViewModel> Slots { get; } = new();

    public SchemeEditViewModel(PathService pathService,IDialogService dialogService,SchemeViewModel scheme, IFileService fileService)
    {
        OriginalScheme = scheme;

        //这里是从原始数据中加载复制的ViewModel
        foreach(var key in Enum.GetValues<RegistryIndex>())
        {
            var slotVM = new SchemeSlotViewModel(OriginalScheme.Name, key, OriginalScheme.Paths.GetValueOrDefault(key, ""), dialogService, fileService, pathService, Editable);
            Slots.Add(slotVM);
        }
    }

    public void Save()
    {
        foreach (var slotVM in Slots)
        {
            //路径有效才写回
            if(File.Exists(slotVM.FilePath)) OriginalScheme.Paths[slotVM.SlotKey] = slotVM.FilePath;
        }
    }
}
