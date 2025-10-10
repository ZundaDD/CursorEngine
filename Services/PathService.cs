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
    /// 用户方案根目录
    /// </summary>
    public string UserSchemePath { get; }

    /// <summary>
    /// 用户方案json文件路径
    /// </summary>
    public string UserSchemeFile { get; }

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
        AppDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
        Directory.CreateDirectory(AppDataRoot);

        UserSchemePath = Path.Combine(AppDataRoot, "Schemes");
        Directory.CreateDirectory(UserSchemePath);

        UserSchemeFile = Path.Combine(UserSchemePath, "user_schemes.json");

        SystemSchemePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Cursors");
    }
}
