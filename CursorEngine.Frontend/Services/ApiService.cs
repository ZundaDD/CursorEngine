using CursorEngine.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CursorEngine.Services;

public interface IApiService
{  
    /// <summary>
    /// 用户注册
    /// </summary>
    Task<ServiceResponse> RegisterAsync(string username, string email, string password);

    /// <summary>
    /// 用户登录，成功后返回包含JWT的响应
    /// </summary>
    Task<LoginResponse> LoginAsync(string username, string password);

    /// <summary>
    /// 分页获取在线方案列表
    /// </summary>
    Task<IEnumerable<SchemeInfoViewModel>> GetSchemesAsync(int pageNumber);

    /// <summary>
    /// 根据ID获取方案的预览图（.cur或.ani）的原始字节数据
    /// </summary>
    Task<byte[]> GetPreviewAsync(int schemeId);

    /// <summary>
    /// 根据ID下载完整的方案ZIP包的原始字节数据
    /// </summary>
    Task<byte[]> DownloadSchemeAsync(int schemeId);

    /// <summary>
    /// 上传一个新的光标方案
    /// </summary>
    Task<ServiceResponse> UploadSchemeAsync(
        string schemeName,
        byte[] previewImageData,
        string previewImageFileName,
        byte[] zipData,
        string zipFileName,
        string token);
}


public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<byte[]> DownloadSchemeAsync(int schemeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/scheme/download/{schemeId}");
            if (response.IsSuccessStatusCode) return await response.Content.ReadAsByteArrayAsync();
            return Array.Empty<byte>();
        }
        catch (Exception) { return Array.Empty<byte>(); }
    }

    public async Task<byte[]> GetPreviewAsync(int schemeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/scheme/preview/{schemeId}");
            if (response.IsSuccessStatusCode) return await response.Content.ReadAsByteArrayAsync();
            return Array.Empty<byte>();
        }
        catch (Exception) { return Array.Empty<byte>(); }
    }

    public async Task<IEnumerable<SchemeInfoViewModel>> GetSchemesAsync(int pageNumber)
    {
        try
        {
            var schemes = await _httpClient.GetFromJsonAsync<IEnumerable<SchemeInfoViewModel>>($"api/scheme/{pageNumber}");
            return schemes ?? Enumerable.Empty<SchemeInfoViewModel>();
        }
        catch (Exception) { return Enumerable.Empty<SchemeInfoViewModel>(); }
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        try
        {
            var loginModel = new { username, password };
            var response = await _httpClient.PostAsJsonAsync("identity/login", loginModel);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>()
                       ?? new LoginResponse(false, "Failed to parse successful login response.", null!);
            }

            return new LoginResponse(false, $"Login failed with status {response.StatusCode}.", null!);
        }
        catch (Exception ex)
        {
            return new LoginResponse(false, $"An unexpected error occurred: {ex.Message}", null!);
        }
    }

    public async Task<ServiceResponse> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var registerModel = new { username, email, password };
            var response = await _httpClient.PostAsJsonAsync("identity/register", registerModel);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ServiceResponse>()
                       ?? new ServiceResponse(false, "Failed to parse successful response.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ServiceResponse(false, $"Registration failed with status {response.StatusCode}. Details: {errorContent}");
        }
        catch (Exception ex)
        {
            return new ServiceResponse(false, $"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<ServiceResponse> UploadSchemeAsync(string schemeName, byte[] previewImageData, string previewImageFileName, byte[] zipData, string zipFileName, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/scheme/upload");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(schemeName), "schemeName");

        var previewContent = new ByteArrayContent(previewImageData);
        previewContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream"); // 通用二进制类型
        content.Add(previewContent, "previewImage", previewImageFileName); 

        var zipContent = new ByteArrayContent(zipData);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(zipContent, "zipFile", zipFileName); 
        
        request.Content = content;

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ServiceResponse>()
                       ?? new ServiceResponse(false, "Failed to parse successful upload response.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ServiceResponse(false, $"Upload failed with status {response.StatusCode}. Details: {errorContent}");
        }
        catch (Exception ex)
        {
            return new ServiceResponse(false, $"An unexpected error occurred during upload: {ex.Message}");
        }
    }
}
