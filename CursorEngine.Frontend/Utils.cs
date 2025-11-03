using CursorEngine.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine;

public static class Utils
{
    public static bool IsNameExisted(string name, IEnumerable<IRenameable> origins) => origins.Any(cs => cs.Name == name);

    public static string GetUniqueName(string baseName, IEnumerable<IRenameable> origins)
    {
        int suffix = 0;
        string uniqueName = baseName;
        //如果重复就一直刷新
        while (IsNameExisted(uniqueName, origins))
        {
            uniqueName = $"{baseName} {suffix}";
            suffix++;
        }
        return uniqueName;
    }
}
