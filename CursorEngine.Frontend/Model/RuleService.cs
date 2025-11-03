using CursorEngine.Services;
using CursorEngine.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CursorEngine.Model;

public class RuleService
{
    private readonly CursorService _cursorControl;
    private readonly IServiceProvider _serviceProvider;
    private readonly PathService _pathService;
    private readonly Random _random = new();

    private Timer _timer;
    private RuleViewModel _activeRule = null!;
    private int _ticksUntilChange = 0;
     
    public RuleService(CursorService cursorControl, IServiceProvider serviceProvider, PathService pathService)
    {
        _cursorControl = cursorControl;
        _pathService = pathService;
        _serviceProvider = serviceProvider;
        _timer = new Timer(OnTimerTick, null, Timeout.Infinite, Timeout.Infinite);
    }

    #region 规则IO
    /// <summary>
    /// 存储所有规则
    /// </summary>
    /// <param name="schemes">规则列表</param>
    /// <returns>是否成功</returns>
    public bool SaveRules(List<CursorRule> rules)
    {
        try
        {
            var json = JsonConvert.SerializeObject(rules);
            File.WriteAllText(_pathService.UserRuleFile, json);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"存储规则时出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 加载所有规则
    /// </summary>
    /// <returns>生成的用户方案</returns>
    public List<CursorRule> LoadRules()
    {
        try
        {
            //文件不存在就先创建
            if (!Path.Exists(_pathService.UserRuleFile))
            {
                var defaultRule = new List<CursorRule>() { new CursorRule() { Name = "Default" } };
                SaveRules(defaultRule);
                return defaultRule;
            }

            var json = File.ReadAllText(_pathService.UserRuleFile);
            var ls = JsonConvert.DeserializeObject<List<CursorRule>>(json);
            return ls ?? new();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载规则时出错: {ex.Message}");
            return new();
        }
    }
    #endregion

    
    public void ApplyNewRule(RuleViewModel rule)
    {
        TimerStop();
        _activeRule = rule;
        ApplyRandomScheme();
        ResetCounter();
        TimerStart();
    }

    public void CancelRule()
    {
        TimerStop();
        _activeRule = null!;
    }

    private void TimerStart() => _timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

    private void TimerStop() => _timer.Change(Timeout.Infinite, Timeout.Infinite);

    private void ResetCounter() => _ticksUntilChange = _activeRule == null ? 0 : _activeRule.IntervalMinutes;

    private void OnTimerTick(object? state)
    {
        _ticksUntilChange--;
        if (_ticksUntilChange <= 0)
        {
            ApplyRandomScheme();
            ResetCounter();
        }
    }

    private void ApplyRandomScheme()
    {
        if (_activeRule == null) return;

        int index = _random.Next(_activeRule.Count);
        (string, bool) schemeInfo = _activeRule[index];

        var localViewModel = _serviceProvider.GetService<LocalSchemeViewModel>();
        if (localViewModel == null) return;

        CursorScheme? scheme = localViewModel.Schemes.Where(sq => sq.IsRegistered == schemeInfo.Item2 && sq.Name == schemeInfo.Item1).FirstOrDefault()?.FullConvert();
        if (scheme != null) _cursorControl.ApplyScheme(scheme);
    }
}
