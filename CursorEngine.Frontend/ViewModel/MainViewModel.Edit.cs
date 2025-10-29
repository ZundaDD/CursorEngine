using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CursorEngine.ViewModel;

public partial class MainViewModel
{
    [ObservableProperty] private SchemeEditViewModel? _currentEditing;

    [ObservableProperty] private int _pageIndex = 0;

    public bool IsEditVisible => CurrentEditing != null;
    public bool IsListVisible => CurrentEditing == null;

    [RelayCommand(CanExecute = nameof(IsListVisible))]
    private void StartEditing(SchemeViewModel? scheme)
    {
        if (scheme == null) return;
        
        CurrentEditing = new SchemeEditViewModel(_pathService, _dialogService, scheme, _fileService);
        PageIndex = 1;
    }

    [RelayCommand(CanExecute = nameof(IsEditVisible))]
    private void SaveEditing()
    {
        CurrentEditing?.Save();
        _cursorControl.SaveUserSchemes(UserSchemes);

        //检查更改写回
        CurrentEditing?.OriginalScheme.LoadPreview(_fileService);
        
        CurrentEditing = null;
        PageIndex = 0;
    }

    [RelayCommand(CanExecute = nameof(IsEditVisible))]
    private void EndEditing()
    {
        CurrentEditing = null;
        PageIndex = 0;
    }
}
