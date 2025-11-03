using System.ComponentModel.DataAnnotations;

namespace CursorEngine.Shared;

public class RegisterModel
{
    [Required] public string Username { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
}

public class LoginModel
{
    [Required] public string Username { get; set; }
    [Required] public string Password { get; set; }
}

public class SchemeInfoViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ServiceResponse
{
    public bool IsSuccess { get; set; } = false;
    public string Message { get; set; } = string.Empty;

    public ServiceResponse(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
}

public class LoginResponse : ServiceResponse
{
    public string Token { get; set; }

    public LoginResponse(bool isSuccess, string message, string token) : base(isSuccess, message)
    {
        IsSuccess = isSuccess;
        Message = message;
        Token = token; 
    }
}