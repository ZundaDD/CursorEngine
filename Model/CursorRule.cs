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

    public List<string> UserSchemes { get; set; } = new ();

    public List<string> SystemSchemes { get; set; } = new();

    public int IntervalMinutes { get; set; } = 5;

    public (string,bool) this[int index]
    {
        get
        {
            if(index >= 0 && index < Count)
            {
                if(index < SystemSchemes.Count) return (SystemSchemes[index], true);
                else return (UserSchemes[index - SystemSchemes.Count], false);
            }
            else throw new IndexOutOfRangeException();
        }
    }

    [JsonIgnore] public int Count => UserSchemes.Count + SystemSchemes.Count;
}
