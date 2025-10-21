using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.Model;
using CursorEngine.View;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CursorEngine.ViewModel;

public partial class MainViewModel
{
    public ObservableCollection<RuleViewModel> Rules { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyRuleCommand),
        nameof(CancelRuleCommand),
        nameof(RenameRuleCommand),
        nameof(DeleteRuleCommand))]
    private RuleViewModel _selectedRule = null!;

    [ObservableProperty]
    private string _appliedRuleName = null!;

    private bool IsRuleEnough => SelectedRule != null && Rules.Count > 1;
    private bool IsRuleNotNull => SelectedRule != null;

    public List<CursorRule> RuleModel => Rules.Select(rvm => rvm.Convert()).ToList();

    private void NotifyCountChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => DeleteRuleCommand.NotifyCanExecuteChanged();

    protected override void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        base.OnPropertyChanging(e);

        //在修改选择之前，需要将目前的选择情况持久化
        if (e.PropertyName == nameof(SelectedRule)) SaveSelectionToRule();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //在修改选择之后，就需要重新加载方案的勾选
        if (e.PropertyName == nameof(SelectedRule)) LoadSelectionFromRule();
    }

    private void SaveSelectionToRule()
    {
        if (SelectedRule == null) return;

        SelectedRule.SystemSchemes = Schemes
                .Where(s => s.IsRegistered && s.IsSelected)
                .Select(s => s.Name).ToList();

        SelectedRule.UserSchemes = Schemes
            .Where(s => !s.IsRegistered && s.IsSelected)
            .Select(s => s.Name).ToList();

        _ruleControl.SaveRules(RuleModel);
    }

    private void LoadSelectionFromRule()
    {
        if (SelectedRule == null) return;

        foreach (var item in Schemes) item.IsSelected = false;

        foreach (var item in Schemes
            .Where(s => s.IsRegistered
            && SelectedRule.SystemSchemes.Any(ss => ss == s.Name)))
            item.IsSelected = true;

        foreach (var item in Schemes
            .Where(s => !s.IsRegistered
            && SelectedRule.UserSchemes.Any(ss => ss == s.Name)))
            item.IsSelected = true;
    }


    [RelayCommand(CanExecute = nameof(IsRuleNotNull))]
    public void ApplyRule()
    {
        _ruleControl.ApplyNewRule(SelectedRule);
        _ruleControl.SaveRules(RuleModel);
        AppliedRuleName = SelectedRule.Name;
    }

    [RelayCommand(CanExecute = nameof(IsRuleNotNull))]
    public void CancelRule()
    {
        _ruleControl.CancelRule();
        _ruleControl.SaveRules(RuleModel);
        AppliedRuleName = null!;
    }

    [RelayCommand]
    public void SaveRule()
    {
        SaveSelectionToRule();

        _ruleControl.SaveRules(RuleModel);
    }

    [RelayCommand]
    public void AddRule()
    {
        RuleViewModel newRule = new();
        newRule.Name = GetUniqueName("New Rule", Rules.OfType<IRenameable>());

        Rules.Add(newRule);
        _ruleControl.SaveRules(RuleModel);
    }

    [RelayCommand(CanExecute = nameof(IsRuleEnough))]
    public void DeleteRule()
    {
        Rules.Remove(SelectedRule);
        _ruleControl.SaveRules(RuleModel);

        SelectedRule = Rules.First();
    }

    [RelayCommand(CanExecute = nameof(IsRuleNotNull))]
    public void RenameRule()
    {
        var renameViewModel = new RenameViewModel(SelectedRule.Name);

        //RenamePanel只是对于RenameViewModel的修改者
        var renamePanel = _serviceProvider.GetRequiredService<RenamePanel>();
        renamePanel.DataContext = renameViewModel;
        renamePanel.Owner = Application.Current.MainWindow;

        var dialogResult = renamePanel.ShowDialog();

        if (dialogResult == true)
        {
            var newName = renameViewModel.NewName;
            if (!IsNameExisted(newName, Schemes))
            {
                SelectedRule.Name = newName;

                _ruleControl.SaveRules(RuleModel);
            }
        }
        
    }
}
