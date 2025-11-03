using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.Frontend.View;
using CursorEngine.Model;
using CursorEngine.Services;
using CursorEngine.View;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CursorEngine.ViewModel;

public partial class LocalSchemeViewModel : ObservableObject
{
    private readonly PathService _pathService;
    private readonly IFileService _fileService;
    private readonly IApiService _apiService;
    private readonly CursorService _cursorService;
    private readonly RuleService _ruleService;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthenticationViewModel _authenticationViewModel;

    #region 绑定字段
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoginStatusText))]
    [NotifyCanExecuteChangedFor(nameof(UploadSchemeCommand))]
    private bool _isLoggedIn;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoginStatusText))]
    private string? _currentUsername;

    private string? _jwtToken;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadSchemeCommand))]
    private bool _isBusy = false;

    public string LoginStatusText => IsLoggedIn ? $"{CurrentUsername}" : "未登录";

    public ObservableCollection<SchemeViewModel> Schemes { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameSchemeCommand)
        , nameof(DeleteSchemeCommand)
        , nameof(TrySelectedSchemeCommand)
        , nameof(ForkSchemeCommand)
        , nameof(ExportSchemeCommand)
        , nameof(UploadSchemeCommand))]
    private SchemeViewModel _selectedScheme = null!;

    public ObservableCollection<RuleViewModel> Rules { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyRuleCommand),
        nameof(CancelRuleCommand),
        nameof(RenameRuleCommand),
        nameof(DeleteRuleCommand))]
    private RuleViewModel _selectedRule = null!;

    [ObservableProperty]
    private string _appliedRuleName = null!;
    #endregion

    public LocalSchemeViewModel(AuthenticationViewModel authenticationViewModel, IApiService apiService, PathService pathService, CursorService cursorService, IFileService fileService, RuleService ruleService, IServiceProvider serviceProvider)
    {
        _apiService = apiService;
        _cursorService = cursorService;
        _ruleService = ruleService;
        _serviceProvider = serviceProvider;
        _fileService = fileService;
        _pathService = pathService;
        _authenticationViewModel = authenticationViewModel;

        authenticationViewModel.OnLoginSuccess += LoginSuccess;
        authenticationViewModel.OnLogoutSuccess += LogoutSuccess;

        Schemes = new ObservableCollection<SchemeViewModel>(_cursorService.LoadAllSchemes().Select(cs => new SchemeViewModel(cs)).ToList());
        foreach (var scheme in Schemes) scheme.LoadPreview(_fileService);

        Rules = new ObservableCollection<RuleViewModel>(_ruleService.LoadRules().Select(cr => new RuleViewModel(cr)).ToList());
        SelectedRule = Rules.Count == 0 ? null! : Rules[0];
        Rules.CollectionChanged += NotifyCountChanged;
    }

    #region 谓词&通知
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

    public List<CursorScheme> UserSchemes => Schemes.Where(svm => !svm.IsRegistered).Select(svm => svm.FullConvert()).ToList();

    public List<CursorRule> RuleModel => Rules.Select(rvm => rvm.Convert()).ToList();

    public bool CanUpload => IsLoggedIn && !IsBusy && SelectedScheme != null;

    private bool IsNotNull() => SelectedScheme != null;

    private bool IsRegistered() => SelectedScheme == null ? false : SelectedScheme.IsRegistered;

    private bool IsNotRegistered() => SelectedScheme != null && !SelectedScheme.IsRegistered;

    private bool IsRuleEnough => SelectedRule != null && Rules.Count > 1;

    private bool IsRuleNotNull => SelectedRule != null;
    

    private void SaveSelectionToRule()
    {
        if (SelectedRule == null) return;

        SelectedRule.SystemSchemes = Schemes
                .Where(s => s.IsRegistered && s.IsSelected)
                .Select(s => s.Name).ToList();

        SelectedRule.UserSchemes = Schemes
            .Where(s => !s.IsRegistered && s.IsSelected)
            .Select(s => s.Name).ToList();

        _ruleService.SaveRules(RuleModel);
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
    #endregion

    #region RuleCommand
    [RelayCommand(CanExecute = nameof(IsRuleNotNull))]
    public void ApplyRule()
    {
        _ruleService.ApplyNewRule(SelectedRule);
        _ruleService.SaveRules(RuleModel);
        AppliedRuleName = SelectedRule.Name;
    }

    [RelayCommand(CanExecute = nameof(IsRuleNotNull))]
    public void CancelRule()
    {
        _ruleService.CancelRule();
        _ruleService.SaveRules(RuleModel);
        AppliedRuleName = null!;
    }

    [RelayCommand]
    public void SaveRule()
    {
        SaveSelectionToRule();

        _ruleService.SaveRules(RuleModel);
    }

    [RelayCommand]
    public void AddRule()
    {
        RuleViewModel newRule = new();
        newRule.Name = Utils.GetUniqueName("New Rule", Rules.OfType<IRenameable>());

        Rules.Add(newRule);
        _ruleService.SaveRules(RuleModel);
    }

    [RelayCommand(CanExecute = nameof(IsRuleEnough))]
    public void DeleteRule()
    {
        Rules.Remove(SelectedRule);
        _ruleService.SaveRules(RuleModel);

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
            if (!Utils.IsNameExisted(newName, Schemes))
            {
                SelectedRule.Name = newName;

                _ruleService.SaveRules(RuleModel);
            }
        }

    }
    #endregion

    #region AuthCommand
    private void LoginSuccess(LoginSuccessEventArgs args)
    {
        IsLoggedIn = true;
        CurrentUsername = args.Username;
        _jwtToken = args.Token;
    }

    private void LogoutSuccess()
    {
        IsLoggedIn = false;
        CurrentUsername = null;
        _jwtToken = null;
    }

    [RelayCommand]
    private void OpenLoginPanel()
    {
        _authenticationViewModel.Reset();

        var authPanel = _serviceProvider.GetRequiredService<AuthenticationPanel>();
        authPanel.DataContext = _authenticationViewModel;
        authPanel.Owner = Application.Current.MainWindow;

        authPanel.ShowDialog();
    }
    #endregion

    #region SchemeCommand
    [RelayCommand(CanExecute = nameof(IsNotRegistered))]
    private void RenameScheme()
    {
        var renameViewModel = new RenameViewModel(SelectedScheme.Name);

        //RenamePanel只是对于RenameViewModel的修改者
        var renamePanel = _serviceProvider.GetRequiredService<RenamePanel>();
        renamePanel.DataContext = renameViewModel;
        renamePanel.Owner = Application.Current.MainWindow;

        var dialogResult = renamePanel.ShowDialog();

        if (dialogResult == true)
        {
            var newName = renameViewModel.NewName;
            if (!Utils.IsNameExisted(newName, Schemes))
            {
                _cursorService.RenameScheme(SelectedScheme.MinConvert(), newName);
                SelectedScheme.Name = newName;

                SaveScheme();
            }
        }
    }

    public void SaveScheme()
    {
        _cursorService.SaveUserSchemes(UserSchemes);
        //每次触发保存后，都重新加载预览图
        foreach (var scheme in Schemes) scheme.LoadPreview(_fileService);
    }

    [RelayCommand(CanExecute = nameof(IsNotNull))]
    public void TrySelectedScheme() => _cursorService.ApplyScheme(SelectedScheme.FullConvert());

    [RelayCommand]
    public void AddScheme()
    {
        var scheme = _cursorService.AddRawUserScheme(Utils.GetUniqueName("New Scheme", Schemes.OfType<IRenameable>()));

        if (scheme != null)
        {
            Schemes.Add(new(scheme));
            SaveScheme();
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpload))]
    public async Task UploadSchemeAsync()
    {
        if (_jwtToken == null) return;

        var zipPath = Path.Combine(_pathService.UserSchemePath, "temp.zip");
        var previewPath = Path.Combine(_pathService.UserSchemePath, "tempPreview");

        IsBusy = true;

        try
        {
            var scheme = SelectedScheme!.FullConvert();
            var defaultScheme = _cursorService.LoadDefaultScheme();

            await Task.Run(() =>
            {
                _fileService.ExportZip(_pathService.UserSchemePath, zipPath, scheme);
                _fileService.ExportPreview(_pathService.UserSchemePath, previewPath, scheme, defaultScheme);
            });

            byte[] zipData = await File.ReadAllBytesAsync(zipPath);
            byte[] previewData = await File.ReadAllBytesAsync(previewPath);
            string previewFileName = Path.GetFileName(previewPath);

            var response = await _apiService.UploadSchemeAsync(
               scheme.Name,
               previewData,
               previewFileName,
               zipData,
               "scheme.zip",
               _jwtToken
            );

            if (!response.IsSuccess) MessageBox.Show($"上传失败：{response.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"上传时发生错误: {ex.Message}");
        }
        finally
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (File.Exists(previewPath)) File.Delete(previewPath);

            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotNull))]
    public void ForkScheme()
    {
        var newScheme = _cursorService.ForkScheme(SelectedScheme.FullConvert(), Utils.GetUniqueName(SelectedScheme.Name, Schemes.OfType<IRenameable>()));
        if (newScheme != null)
        {
            Schemes.Add(new(newScheme));
            SaveScheme();
        }
    }

    public void ForkScheme(CursorScheme scheme, string name)
    {
        var newScheme = _cursorService.ForkScheme(scheme, Utils.GetUniqueName(name, Schemes.OfType<IRenameable>()));
        if (newScheme != null)
        {
            Schemes.Add(new(newScheme));
            SaveScheme();
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotRegistered))]
    public void DeleteScheme()
    {
        if (_cursorService.DeleteUserSchemes(SelectedScheme.MinConvert()))
        {
            Schemes.Remove(SelectedScheme);
            SaveScheme();
        }

        SelectedScheme = null!;
    }

    [RelayCommand(CanExecute = nameof(IsNotNull))]
    public void ExportScheme() => _cursorService.PackSchemeWithInf(SelectedScheme.FullConvert());
    #endregion
}
