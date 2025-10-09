using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.Services;

public class PathService
{   
    /// <summary>
    /// 应用数据根目录
    /// </summary>
    public string AppDataRoot { get; }

    /// <summary>
    /// 系统方案根目录
    /// </summary>
    public string SystemSchemePath { get; }

    /// <summary>
    /// 注册表路径
    /// </summary>
    public string RegistryPath { get; } = @"Control Panel\Cursors";

    /// <summary>
    /// 方案注册表路径
    /// </summary>
    public string SchemeRegistryPath { get; } = @"Control Panel\Cursors\Schemes";

    public PathService(string appName = "CursorEngine")
    {        
        AppDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        Directory.CreateDirectory(AppDataRoot);

        SystemSchemePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Cursors");

        //SchemesDatabaseFile = Path.Combine(AppDataRoot, "cursor_schemes.json");
    }
}
