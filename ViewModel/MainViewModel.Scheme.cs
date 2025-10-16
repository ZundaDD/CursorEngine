using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.View;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CursorEngine.ViewModel;

public partial class MainViewModel
{
    public ObservableCollection<SchemeViewModel> Schemes { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameSchemeCommand))]
    private SchemeViewModel _selectedScheme = null!;
    
    private bool IsNotNull() => SelectedScheme != null;
    
    private bool IsRegistered() => SelectedScheme == null ? false : SelectedScheme.IsRegistered;

    private bool IsNotRegistered() => SelectedScheme != null && !SelectedScheme.IsRegistered;

    [RelayCommand(CanExecute = nameof(IsNotRegistered))]
    private void RenameScheme()
    {
        var renameViewModel = new RenameViewModel(SelectedScheme.Name);
        
        //RenamePanel只是对于RenameViewModel的修改者
        var renamePanel = _serviceProvider.GetRequiredService<RenamePanel>();
        renamePanel.DataContext = renameViewModel;
        renamePanel.Owner = Application.Current.MainWindow;
        
        var dialogResult = renamePanel.ShowDialog();

        if (dialogResult == true)
        {
            var newName = renameViewModel.NewName;
            if (!IsNameExisted(newName, Schemes))
            {
                _cursorControl.RenameScheme(SelectedScheme.MinConvert(), newName);
                SelectedScheme.Name = newName;

                _cursorControl.SaveUserSchemes(UserSchemes);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotNull))]
    public void TrySelectedScheme() => _cursorControl.ApplyScheme(SelectedScheme.FullConvert());

    [RelayCommand]
    public void AddScheme()
    {
        var scheme = _cursorControl.AddRawUserScheme(GetUniqueName("New Scheme", Schemes.OfType<IRenameable>()));

        if (scheme != null)
        {
            Schemes.Add(new(scheme));
            _cursorControl.SaveUserSchemes(UserSchemes);
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotNull))]
    public void ForkScheme()
    {
        var newScheme = _cursorControl.ForkScheme(SelectedScheme.FullConvert(), GetUniqueName(SelectedScheme.Name, Schemes.OfType<IRenameable>()));
        if (newScheme != null)
        {
            Schemes.Add(new(newScheme));
            _cursorControl.SaveUserSchemes(UserSchemes);
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotRegistered))]
    public void DeleteScheme()
    {
        if (_cursorControl.DeleteUserSchemes(SelectedScheme.MinConvert()))
        {
            Schemes.Remove(SelectedScheme);
            _cursorControl.SaveUserSchemes(UserSchemes);
        }

        SelectedScheme = null!;
    }
}
