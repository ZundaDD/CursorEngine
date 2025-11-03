using CursorEngine.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CursorEngine.Model;

/// <summary>
/// 指针方案底层管理器，负责进行实际的文件操作。<br/>
/// 方案分为两种，一种是通过inf文件安装，复制到Windows/Cursors下并注册到注册表中的，称为一等公民，只读<br/>
/// 另一种一种是用户自定义的，位于AppData/Local下统一存放,称为二等公民,可读写。<br/>
/// 用户方案按照名称建立文件夹存储ani和cur文件，并将映射关系登记在cursor_scheme.json中
/// </summary>
public class CursorService
{
    private readonly PathService _pathService;
    private readonly CursorScheme _defaultScheme;
    private readonly IFileService _fileService;
    private readonly IDialogService _dialogService;

    public CursorService(IFileService fileService, PathService pathService, IDialogService dialogService)
    {
        _fileService = fileService;
        _pathService = pathService;
        _dialogService = dialogService;
        _defaultScheme = LoadDefaultScheme();
    }

    /// <summary>
    /// 打包自制方案并生成inf文件->为二等公民设置一等公民晋升证明
    /// </summary>
    /// <param name="scheme">自制方案</param>
    /// <returns>是否成功</returns>
    public bool PackSchemeWithInf(CursorScheme scheme)
    {
        if (scheme.Name == string.Empty) return false;

        try
        {
            var result = _dialogService.ChooseExportPath(scheme.Name);
            
            //如果选择成功且文件路径存在
            if(result != null && Directory.Exists(Path.GetDirectoryName(result)))
            {
                _fileService.ExportZip(_pathService.UserSchemePath, result, scheme, LoadDefaultScheme());
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"打包方案时出错: {ex.Message}");
            return false;
        }
        return true;
    }

    #region 用户方案增删存
    /// <summary>
    /// 删除用户方案
    /// </summary>
    /// <param name="scheme">目标方案</param>
    /// <returns>是否成功</returns>
    public bool DeleteUserSchemes(CursorScheme scheme)
    {
        var destPath = Path.Combine(_pathService.UserSchemePath, scheme.Name);

        if (Directory.Exists(destPath))
        {
            Directory.Delete(destPath, true);
            return true;
        }
        else return false;
    }

    /// <summary>
    /// 存储所有用户方案
    /// </summary>
    /// <param name="schemes">用户方案列表</param>
    /// <returns>是否成功</returns>
    public bool SaveUserSchemes(List<CursorScheme> schemes)
    {
        try
        {
            var json = JsonConvert.SerializeObject(schemes, Formatting.Indented);
            File.WriteAllText(_pathService.UserSchemeFile, json);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"存储方案时出错: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 添加一个原始用户方案
    /// </summary>
    /// <param name="name">方案名</param>
    /// <returns>生成的用户方案</returns>
    public CursorScheme AddRawUserScheme(string name)
    {
        if (name == string.Empty) return null!;

        var destPath = Path.Combine(_pathService.UserSchemePath, name);

        //以防万一，就当无事发生
        if (Directory.Exists(destPath)) return null!;

        Directory.CreateDirectory(destPath);
        return new(name, false);
    }
    #endregion

    #region 方案加载
    /// <summary>
    /// 加载默认方案
    /// </summary>
    /// <returns>默认方案</returns>
    public CursorScheme LoadDefaultScheme()
    {
        var defaultScheme = new CursorScheme("(Default)");
        //硬编码的意义是，以免注册表被改
        defaultScheme.Paths = new()
        {
            { RegistryIndex.Arrow,      Path.Combine(_pathService.SystemSchemePath, "aero_arrow.cur") },
            { RegistryIndex.Help,       Path.Combine(_pathService.SystemSchemePath, "aero_helpsel.cur") },
            { RegistryIndex.AppStarting,Path.Combine(_pathService.SystemSchemePath, "aero_working.ani") },
            { RegistryIndex.Wait,       Path.Combine(_pathService.SystemSchemePath, "aero_busy.ani") },
            { RegistryIndex.Crosshair,  Path.Combine(_pathService.SystemSchemePath, "cross_rl.cur") },
            { RegistryIndex.IBeam,      Path.Combine(_pathService.SystemSchemePath, "beam_r.cur") },
            { RegistryIndex.NWPen,      Path.Combine(_pathService.SystemSchemePath, "aero_pen.cur") },
            { RegistryIndex.No,         Path.Combine(_pathService.SystemSchemePath, "aero_unavail.cur") },
            { RegistryIndex.SizeNS,     Path.Combine(_pathService.SystemSchemePath, "aero_ns.cur") },
            { RegistryIndex.SizeWE,     Path.Combine(_pathService.SystemSchemePath, "aero_ew.cur") },
            { RegistryIndex.SizeNWSE,   Path.Combine(_pathService.SystemSchemePath, "aero_nwse.cur") },
            { RegistryIndex.SizeNESW,   Path.Combine(_pathService.SystemSchemePath, "aero_nesw.cur") },
            { RegistryIndex.SizeAll,    Path.Combine(_pathService.SystemSchemePath, "aero_move.cur") },
            { RegistryIndex.UpArrow,    Path.Combine(_pathService.SystemSchemePath, "aero_up.cur") },
            { RegistryIndex.Hand,       Path.Combine(_pathService.SystemSchemePath, "aero_link.cur") },
            { RegistryIndex.Pin,        Path.Combine(_pathService.SystemSchemePath, "pin_l.cur") },
            { RegistryIndex.Person,     Path.Combine(_pathService.SystemSchemePath, "person_l.cur") }
        };
        
            
        return defaultScheme;
        
    }

    public void RenameScheme(CursorScheme scheme, string newName)
    {
        var srcPath = Path.Combine(_pathService.UserSchemePath, scheme.Name);
        var destPath = Path.Combine(_pathService.UserSchemePath, newName);

        if (!Directory.Exists(srcPath) || Directory.Exists(destPath)) return;

        Directory.Move(srcPath, destPath);
    }

    /// <summary>
    /// 加载所有用户方案列表
    /// </summary>
    /// <returns>用户方案列表</returns>
    public List<CursorScheme> LoadUsersSchemes()
    {
        try
        {
            //文件不存在就先创建
            if(!Path.Exists(_pathService.UserSchemeFile))
            {
                SaveUserSchemes(new());
                return new();
            }

            var json = File.ReadAllText(_pathService.UserSchemeFile);
            var ls = JsonConvert.DeserializeObject<List<CursorScheme>>(json);
            return ls ?? new();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载方案时出错: {ex.Message}");
            return new();
        }

    }

    /// <summary>
    /// 加载所有系统方案
    /// </summary>
    /// <returns>系统方案列表</returns>
    public unsafe List<CursorScheme> LoadSystemSchemes()
    {
        var ls = new List<CursorScheme>();
        try
        {
            using (RegistryKey? cursorKey = Registry.CurrentUser.OpenSubKey(_pathService.SchemeRegistryPath, true))
            {
                if (cursorKey == null) return null!;

                foreach(var key in cursorKey.GetValueNames())
                {
                    var value = cursorKey.GetValue(key) as string;
                    if (string.IsNullOrEmpty(value)) continue;

                    ls.Add(CursorScheme.FromString(key, value));
                }
                return ls;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"获取系统方案时出错: {ex.Message}");
            return new();
        }
    }

    /// <summary>
    /// 加载所有方案
    /// </summary>
    /// <returns>方案列表</returns>
    public unsafe List<CursorScheme> LoadAllSchemes()
    {
        var system = new List<CursorScheme>() { _defaultScheme};
        system.AddRange(LoadSystemSchemes());
        system.AddRange(LoadUsersSchemes());
        
        return system;
    }
    #endregion

    /// <summary>
    /// 复制方案
    /// </summary>
    /// <param name="scheme">复制源方案</param>
    /// <param name="name">新方案名称</param>
    /// <returns>新用户方案</returns>
    public unsafe CursorScheme ForkScheme(CursorScheme scheme, string name)
    {
        if (scheme.Name == string.Empty) return null!;

        var srcPath = Path.Combine(scheme.IsRegistered ? 
            _pathService.SystemSchemePath : _pathService.UserSchemePath, scheme.Name);
        var destPath = Path.Combine(_pathService.UserSchemePath, name);

        //以防万一，就当无事发生
        if (!Directory.Exists(srcPath)) return null!;
        if (Directory.Exists(destPath)) return null!;

        //复制源文件并构建数据对象
        Directory.CreateDirectory(destPath);
        var newScheme = new CursorScheme(name, false);
        foreach (var kv in scheme.Paths)
        {
            if (kv.Value == null || !File.Exists(kv.Value)) continue;

            var destFilePath = Path.Combine(destPath, Path.GetFileName(kv.Value));
            File.Copy(kv.Value, destFilePath, true);
            newScheme.Paths[kv.Key] = destFilePath;
        }

        return newScheme;
    }

    /// <summary>
    /// 持久化一个自制方案到注册表->将二等公民提升为一等公民
    /// </summary>
    /// <param name="scheme">自制方案</param>
    /// <returns>新的系统方案</returns>
    public unsafe CursorScheme SaveSchemeToRegistry(CursorScheme scheme, string name)
    {
        if (scheme.Name == string.Empty) return null!;
        if (scheme.IsRegistered) return null!;

        try
        {
            //采用覆盖式导出
            var destPath = Path.Combine(_pathService.SystemSchemePath, scheme.Name);
            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
            Directory.CreateDirectory(destPath);

            //复制源文件
            var newScheme = new CursorScheme(name);
            foreach(var kv in scheme.Paths)
            {
                if (kv.Value == null || !File.Exists(kv.Value)) continue;

                var destFilePath = Path.Combine(destPath, Path.GetFileName(kv.Value));
                File.Copy(kv.Value, destFilePath, true);
                newScheme.Paths[kv.Key] = destFilePath;
            }
            
            //写入注册表
            using (RegistryKey? cursorKey = Registry.CurrentUser.OpenSubKey(_pathService.SchemeRegistryPath, true))
            {
                if(cursorKey == null) return null!;

                cursorKey.SetValue(newScheme.Name, newScheme.ToString());
            }

            return newScheme;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"存储方案时出错: {ex.Message}");
            return null!;
        }
    }

    /// <summary>
    /// 应用一个方案->将公民设置为系统代表
    /// </summary>
    /// <param name="scheme">经过规则合并后的方案</param>
    /// <returns>是否成功</returns>
    public unsafe bool ApplyScheme(CursorScheme scheme)
    {
        try
        {
            using (RegistryKey? cursorKey = Registry.CurrentUser.OpenSubKey(_pathService.RegistryPath, true))
            {
                if (cursorKey == null) return false;

                foreach (RegistryIndex role in Enum.GetValues(typeof(RegistryIndex)))
                {
                    if(scheme.Paths.TryGetValue(role, out var value))
                    {
                        if (!string.IsNullOrEmpty(value)) cursorKey.SetValue(role.ToString(), value); 
                    }
                    else if(_defaultScheme.Paths.TryGetValue(role, out var dvalue))
                    {
                        if(!string.IsNullOrEmpty(dvalue)) cursorKey.SetValue(role.ToString(), dvalue);
                    }
                }
                    
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用方案时出错: {ex.Message}");
            return false;
        }

        PInvoke.SystemParametersInfo(
                SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETCURSORS,
                0,
                null,
                SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_UPDATEINIFILE | SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_SENDCHANGE
            );
        return true;
    }
}
