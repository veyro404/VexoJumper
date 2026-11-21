using System.IO;
using System.Text.Json;

namespace VexoJumper.Services;

/// <summary>
/// ذخیره‌سازی محلی تنظیمات برنامه در مسیر LocalApplicationData.
/// نوشتن ابتدا در فایل موقت و سپس جایگزینی در همان پوشه انجام می‌شود.
/// </summary>
public sealed class StorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VexoJumper",
        "settings.json");

    public async Task<Dictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_path))
                return new Dictionary<string, string>(StringComparer.Ordinal);

            await using var stream = new FileStream(
                _path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            return await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
                       stream, JsonOptions, cancellationToken)
                   ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch (UnauthorizedAccessException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    public async Task SaveAsync(
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        var directory = Path.GetDirectoryName(_path)
                        ?? throw new InvalidOperationException("مسیر ذخیره‌سازی معتبر نیست.");
        Directory.CreateDirectory(directory);

        var temporaryPath = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            // فایل موقت در همان پوشه است؛ جایگزینی روی همان volume انجام می‌شود.
            File.Move(temporaryPath, _path, overwrite: true);
        }
        finally
        {
            try
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch (IOException)
            {
                // پاک‌سازی فایل موقت نباید خطای اصلی ذخیره‌سازی را مخفی کند.
            }
            catch (UnauthorizedAccessException)
            {
                // پاک‌سازی فایل موقت نباید خطای اصلی ذخیره‌سازی را مخفی کند.
            }
        }
    }
}
