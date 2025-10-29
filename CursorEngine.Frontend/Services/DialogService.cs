using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.Services;

public interface IDialogService
{
    string? ChooseCursorFile();
    string? ChooseExportPath(string defaultName);
}

public class DialogService : IDialogService
{
    public string? ChooseCursorFile()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Cursor Files (*.ani, *.cur)|*.ani;*.cur|All files (*.*)|*.*",
            Title = "选择一个光标文件",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() == true) return openFileDialog.FileName;

        return null;
    }

    public string? ChooseExportPath(string defaultName)
    {
        defaultName += ".zip";
        var dialog = new SaveFileDialog
        {
            Title = "导出为zip",
            FileName = defaultName,
            Filter = "ZIP Archive (*.zip)|*.zip",
            AddExtension = true
        };

        if (dialog.ShowDialog() == true) return dialog.FileName;

        return null;
    }
}
