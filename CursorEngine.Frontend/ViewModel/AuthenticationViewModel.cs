using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursorEngine.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.ViewModel;

public partial class AuthenticationViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    public event Action<LoginSuccessEventArgs>? OnLoginSuccess;
    public event Action? OnLogoutSuccess;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty, NotifyCanExecuteChangedFor(
        nameof(LoginCommand),
        nameof(LogoutCommand),
        nameof(RegisterCommand))] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    private bool IsFree => !IsBusy;

    public AuthenticationViewModel(IApiService apiService) => _apiService = apiService;
    

    [RelayCommand(CanExecute = nameof(IsFree))]
    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        var response = await _apiService.LoginAsync(Username, Password);
        
        if (response.IsSuccess) OnLoginSuccess?.Invoke(new (Username, response.Token));
        else ErrorMessage = response.Message;

        IsBusy = false;
    }

    [RelayCommand(CanExecute = nameof(IsFree))]
    private async Task RegisterAsync()
    {
        IsBusy = true;

        ErrorMessage = null;
        var response = await _apiService.RegisterAsync(Username, Username, Password);
        ErrorMessage = response.Message;
        
        IsBusy = false;
    }

    [RelayCommand(CanExecute = nameof(IsFree))]
    private void Logout() => OnLogoutSuccess?.Invoke();

    public void Reset()
    {
        Username = "";
        Password = "";
        ErrorMessage = null;
        IsBusy = false;
    }
}

public class LoginSuccessEventArgs : EventArgs
{
    public string Username { get; }
    public string Token { get; }
    public LoginSuccessEventArgs(string username, string token)
    {
        Username = username;
        Token = token;
    }
}