using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.ViewModel;

public interface IRenameable
{
    string Name { get; set; }
}

public partial class RenameViewModel : ObservableObject
{
    public string NewName { get; private set; }

    [ObservableProperty]
    private string _editText;

    public Action<bool?>? CloseAction { get; set; }

    public RenameViewModel(string currentName)
    {
        _editText = currentName;
        NewName = currentName;
    }

    [RelayCommand]
    private void Confirm()
    {
        if (!string.IsNullOrWhiteSpace(EditText))
        {
            NewName = EditText;
            CloseAction?.Invoke(true);
        }
    }

    [RelayCommand]
    private void Cancel() => CloseAction?.Invoke(false);
}
