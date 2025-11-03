using CursorEngine.Backend.Models;
using CursorEngine.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CursorEngine.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SchemeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApiUser> _userManager;
    private readonly string _storagePath;

    public SchemeController(ApplicationDbContext context, UserManager<ApiUser> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _storagePath = Path.Combine(env.WebRootPath, "schemes");
        if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadScheme([FromForm] IFormFile zipFile, [FromForm] IFormFile previewImage, [FromForm] string schemeName)
    {
        if (zipFile == null || zipFile.Length == 0) return BadRequest("No file uploaded.");
        if (previewImage == null || previewImage.Length == 0) return BadRequest("Preview image is required.");

        if (Path.GetExtension(zipFile.FileName).ToLower() != ".zip") return BadRequest("Only .zip files are allowed.");

        // 身份验证
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        var previewsStoragePath = Path.Combine(_storagePath, "previews");
        if (!Directory.Exists(previewsStoragePath)) Directory.CreateDirectory(previewsStoragePath);

        var zipsStoragePath = Path.Combine(_storagePath, "zips");
        if (!Directory.Exists(zipsStoragePath)) Directory.CreateDirectory(zipsStoragePath);

        // 保存预览图
        var previewFileName = $"{Guid.NewGuid()}{Path.GetExtension(previewImage.FileName)}";
        var previewFilePath = Path.Combine(previewsStoragePath, previewFileName);
        using (var stream = new FileStream(previewFilePath, FileMode.Create))
        {
            await previewImage.CopyToAsync(stream);
        }

        // 保存ZIP包
        var zipFileName = $"{Guid.NewGuid()}.zip";
        var zipFilePath = Path.Combine(zipsStoragePath, zipFileName);
        using (var stream = new FileStream(zipFilePath, FileMode.Create))
        {
            await zipFile.CopyToAsync(stream);
        }

        var scheme = new Scheme
        {
            Name = schemeName,
            FilePath = zipFileName,
            PreviewPath = previewFileName, 
        };

        _context.Schemes.Add(scheme);
        await _context.SaveChangesAsync();

        return Ok(new { IsSuccess = true, Message = "Scheme uploaded successfully", SchemeId = scheme.Id });
    }

    [HttpGet("download/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadScheme(int id)
    {
        var scheme = await _context.Schemes.FindAsync(id);
        if (scheme == null) return NotFound();

        var filePath = Path.Combine(_storagePath, "zips", scheme.FilePath);
        if (!System.IO.File.Exists(filePath)) return NotFound("File not found on server.");

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, "application/zip", $"{scheme.Name}.zip");
    }

    [HttpGet("preview/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadPreview(int id)
    {
        var scheme = await _context.Schemes.FindAsync(id);
        if (scheme == null || string.IsNullOrEmpty(scheme.PreviewPath))
        {
            return NotFound();
        }

        var previewsStoragePath = Path.Combine(_storagePath, "previews");
        var filePath = Path.Combine(previewsStoragePath, scheme.PreviewPath);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Preview file not found on server.");
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var mimeType = GetMimeType(filePath);

        return File(fileStream, mimeType, $"{scheme.Name}{Path.GetExtension(filePath).ToLowerInvariant()}");
    }

    [HttpGet("{cnt}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SchemeInfoViewModel>>> GetSchemes(int cnt)
    {
        if (cnt < 1) cnt = 1;
        const int pageSize = 10;

        var schemes = await _context.Schemes
            .OrderByDescending(s => s.Id)
            .Skip((cnt - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SchemeInfoViewModel
            {
                Id = s.Id,
                Name = s.Name,
            })
            .ToListAsync();

        return Ok(schemes);
    }

    private string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".cur" => "image/vnd.microsoft.icon",
            ".ani" => "application/x-navi-animation",
            _ => "application/octet-stream",
        };
    }
}
