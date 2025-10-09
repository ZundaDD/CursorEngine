using CursorEngine.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CursorEngine.Model;

/// <summary>
/// WindowsAPI调用器
/// </summary>
public class CursorControl
{
    private readonly PathService _pathService;

    public CursorControl(PathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// 打包自制方案并生成inf文件->为二等公民设置一等公民晋升证明
    /// </summary>
    /// <param name="scheme">自制方案</param>
    /// <returns>是否成功</returns>
    public unsafe bool PackSchemeWithInf(CursorScheme scheme)
    {
        if (scheme.Name == string.Empty) return false;

        try
        {
            
        }
        catch (Exception ex)
        {
            MessageBox.Show($"打包方案时出错: {ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 从注册表中复制一个计划->获取一个一等公民的克隆，并降级
    /// </summary>
    /// <param name="scheme">只读方案</param>
    /// <returns>复制得到的方案</returns>
    public unsafe CursorScheme ForkFromRegistry(CursorScheme scheme)
    {
        if(!scheme.IsRegistered) return null!;
        return null!;

    }

    /// <summary>
    /// 持久化一个自制方案到注册表->将二等公民提升为一等公民
    /// </summary>
    /// <param name="scheme">自制方案</param>
    /// <returns>新的系统方案</returns>
    public unsafe CursorScheme SaveSchemeToRegistry(CursorScheme scheme, string name)
    {
        if (scheme.Name == string.Empty) return null!;
        if (scheme.Name == name) return null!;
        if (scheme.IsRegistered) return null!;

        try
        {
            //采用覆盖式导出
            var destPath = Path.Combine(_pathService.SystemSchemePath, scheme.Name);
            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
            Directory.CreateDirectory(destPath);

            //复制以防止删除丢失
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

                foreach (var kv in scheme.Paths)
                {
                    if (!string.IsNullOrEmpty(kv.Value)) cursorKey.SetValue(kv.Key.ToString(), kv.Value);
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
