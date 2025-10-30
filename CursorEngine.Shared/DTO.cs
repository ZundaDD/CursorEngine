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
