using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.Services;

public interface IDialogService
{
    string? ChooseCursorFile();
    string? ChooseExportPath();
}
public class DialogService : IDialogService
{
    public string? ChooseCursorFile()
    {
        throw new NotImplementedException();
    }

    public string? ChooseExportPath()
    {
        throw new NotImplementedException();
    }
}
