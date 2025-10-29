using CursorEngine.Model;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CursorEngine.Services;
public interface IFileService
{
    public bool ExportZip(string schemesPath, string destPath, CursorScheme scheme, CursorScheme defaultScheme);


    BitmapSource? LoadCursorPreview(string filePath);
}

public class FileService : IFileService
{
    public bool ExportZip(string schemesPath, string destPath, CursorScheme scheme, CursorScheme defaultScheme)
    {
        //合并方案，保证无空键
        var merge_scheme = new CursorScheme(scheme.Name, false);
        merge_scheme.Paths = new Dictionary<RegistryIndex, string?>(defaultScheme.Paths);
        foreach(var kv in scheme.Paths) merge_scheme.Paths[kv.Key] = kv.Value;

        //复制ani和cur文件
        var temp_dir = Path.Combine(schemesPath, ".temp");
        Directory.CreateDirectory(temp_dir);
        foreach(var kv in merge_scheme.Paths)
        {
            if (kv.Value == null) continue;

            File.Copy(kv.Value, Path.Combine(temp_dir, Path.GetFileName(kv.Value)));
        }

        //生成inf文件
        GenerateInfFile(Path.Combine(temp_dir, "install.inf"), merge_scheme);

        //生成压缩包
        if (File.Exists(destPath)) File.Delete(destPath);
        ZipFile.CreateFromDirectory(temp_dir, destPath);

        Directory.Delete(temp_dir, true);
        return true;
    }

    private void GenerateInfFile(string destPath,CursorScheme scheme)
    {
        var template_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfTemplate.txt");
        if (!File.Exists(template_path)) return;

        var template = File.ReadAllText(template_path);

        

        StringBuilder sb = new();
        for (int i = 0; i <= (int)RegistryIndex.Person; ++i)
        {
            var fileName = Path.GetFileName(scheme.Paths[(RegistryIndex)i]);
            sb.Append($"\"{fileName}\"\n");
            template = template.Replace($"#{i + 4}#", fileName);
        }

        template = template.Replace("#1#", sb.ToString());
        template = template.Replace("#2#", scheme.Name);
        template = template.Replace("#3#", scheme.Name);

        File.WriteAllText(destPath, template);
    }

    public BitmapSource? LoadCursorPreview(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var hCursor = IntPtr.Zero;
        try
        {
            
            hCursor = PInvokeC.LoadImage(
                (IntPtr)null,
                filePath,
                PInvokeC.IMAGE_CURSOR,
                0,
                0,
                PInvokeC.LR_LOADFROMFILE
            );

            if (hCursor == IntPtr.Zero)
            {
                var errorCode = new Win32Exception(Marshal.GetLastWin32Error());
                return null;
            }

            BitmapSource? bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                hCursor,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            bitmapSource?.Freeze();

            return bitmapSource;
        }
        finally
        {
            if (hCursor != IntPtr.Zero) PInvokeC.DestroyIcon(hCursor);
        }
    }
}
