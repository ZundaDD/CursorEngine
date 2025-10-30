using System.ComponentModel.DataAnnotations;

namespace CursorEngine.Backend.Models;

public class Scheme
{
    [Key]
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string PreviewPath { get; set; } = string.Empty;
}
