using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.Model;

/// <summary>
/// 光标规则
/// </summary>
public class CursorRule
{
    public string Name { get; set; } = "New Rule";

    public List<string> UserSchemes { get; set; } = new();

    public List<string> SystemSchemes { get; set; } = new();

    public int IntervalMinutes { get; set; } = 5;

}
