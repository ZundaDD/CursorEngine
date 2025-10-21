using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.Model;

/// <summary>
/// 注册表顺序
/// </summary>
public enum RegistryIndex
{
    Arrow = 0,
    Help = 1,
    AppStarting = 2,
    Wait = 3,
    Crosshair = 4,
    IBeam = 5,
    NWPen = 6,
    No = 7,
    SizeNS = 8,
    SizeWE = 9,
    SizeNWSE = 10,
    SizeNESW = 11,
    SizeAll = 12,
    UpArrow = 13,
    Hand = 14,
    Pin = 15,
    Person = 16,
}

/// <summary>
/// 鼠标指针方案，允许规则覆盖
/// </summary>
public class CursorScheme
{
    public CursorScheme(string name, bool isRegistered = true)
    {
        IsRegistered = isRegistered;
        Name = name;
    }

    public bool IsRegistered { get; set; }
    public string Name { get; set; } = "New Scheme";

    public Dictionary<RegistryIndex, string?> Paths = new();

    public override string ToString()
    {
        StringBuilder sb = new();
        foreach (var value in Enum.GetValues(typeof(RegistryIndex)))
        {
            var v = (RegistryIndex)value;
            sb.Append($"{Paths.GetValueOrDefault(v, "")},");
        }

        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    /// <summary>
    /// 从字符串中解析方案，一定是解析系统方案
    /// </summary>
    /// <param name="name">方案名</param>
    /// <param name="schemeData">方案缩略值</param>
    /// <returns>系统方案</returns>
    public static CursorScheme FromString(string name,string schemeData)
    {
        var scheme = new CursorScheme(name);
        scheme.Name = name;

        string[] paths = schemeData.Split(',');

        foreach (var role in (RegistryIndex[])Enum.GetValues(typeof(RegistryIndex)))
        {
            int index = (int)role;

            if (index < paths.Length)
            {
                string path = paths[index];

                if (!string.IsNullOrEmpty(path)) scheme.Paths[role] = path;
            }
        }

        return scheme;
    }
}