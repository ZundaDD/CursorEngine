using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.Model;

/// <summary>
/// 鼠标指针方案，允许规则覆盖
/// </summary>
internal class CursorScheme
{
    public string Name { get; set; } = "New Scheme";

    public string? Arrow { get; set; }               // 正常选择
    public string? Help { get; set; }                // 帮助选择
    public string? AppStarting { get; set; }         // 在后台工作
    public string? Wait { get; set; }                // 忙
    public string? Crosshair { get; set; }           // 精确定位
    public string? IBeam { get; set; }               // 文本选择
    public string? NWPen { get; set; }               // 手写
    public string? No { get; set; }                  // 不可用
    public string? SizeNS { get; set; }              // 垂直调整大小
    public string? SizeWE { get; set; }              // 水平调整大小
    public string? SizeNWSE { get; set; }            // 沿对角线调整大小 1
    public string? SizeNESW { get; set; }            // 沿对角线调整大小 2
    public string? SizeAll { get; set; }             // 移动
    public string? UpArrow { get; set; }             // 备用选择
    public string? Hand { get; set; }                // 链接选择
}
