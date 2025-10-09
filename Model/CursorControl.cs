using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CursorEngine.Model;

/// <summary>
/// WindowsAPI调用器
/// </summary>
internal class CursorControl
{
    private const string RegistryPath = @"Control Panel\Cursors";
    private const string SchemePath = @"Control Panel\Cursors\Schemes";

    public unsafe bool SetPermanentChange(CursorScheme cursor)
    {
        try
        {
            using (RegistryKey? cursorKey = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
            {
                if (cursorKey == null) return false;

                cursorKey.SetValue("Arrow", @"D:\Donwload\偶像玛丽+鼠标指针+ani\Normal.ani");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"应用方案时出错: {ex.Message}");
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
