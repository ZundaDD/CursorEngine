using CommunityToolkit.Mvvm.ComponentModel;
using CursorEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.ViewModel;

public partial class RuleViewModel : ObservableObject, IRenameable
{
    [ObservableProperty]
    private string _name = "New Rule";

    [ObservableProperty]
    private List<string> _userSchemes = new();

    [ObservableProperty]
    private List<string> _systemSchemes = new();

    [ObservableProperty]
    private int _intervalMinutes = 5;

    public RuleViewModel(CursorRule rule)
    {
        Name = rule.Name;
        UserSchemes = rule.UserSchemes;
        SystemSchemes = rule.SystemSchemes;
        IntervalMinutes = rule.IntervalMinutes;
    }

    public RuleViewModel() { }

    public CursorRule Convert() => new CursorRule
    {
        Name = Name,
        UserSchemes = UserSchemes,
        SystemSchemes = SystemSchemes,
        IntervalMinutes = IntervalMinutes
    };
    public (string, bool) this[int index]
    {
        get
        {
            if (index >= 0 && index < Count)
            {
                if (index < SystemSchemes.Count) return (SystemSchemes[index], true);
                else return (UserSchemes[index - SystemSchemes.Count], false);
            }
            else throw new IndexOutOfRangeException();
        }
    }

    public int Count => UserSchemes.Count + SystemSchemes.Count;

}
