using System.Text.Json;
using System.Text.RegularExpressions;

sealed class ShowcaseConfigStore
{
    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "docshowcase.config.json");
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private ShowcaseState _state;

    public ShowcaseConfigStore(IConfiguration configuration)
    {
        _state = LoadOrCreate(configuration);
    }

    public ShowcaseState GetState()
    {
        lock (_lock)
        {
            return Clone(_state);
        }
    }

    public ShareItem? GetShare(string id)
    {
        lock (_lock)
        {
            var share = _state.Shares.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return share is null ? null : Clone(share);
        }
    }

    public ShareItem AddShare(CreateShareRequest request)
    {
        lock (_lock)
        {
            var path = NormalizeExistingDirectory(request.Path);
            var name = string.IsNullOrWhiteSpace(request.Name) ? new DirectoryInfo(path).Name : request.Name.Trim();
            var share = new ShareItem
            {
                Id = CreateUniqueId(name),
                Name = name,
                Path = path,
                Enabled = true,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now,
                Settings = request.Settings ?? Clone(_state.Defaults)
            };

            _state.Shares.Add(share);
            SaveLocked();
            return Clone(share);
        }
    }

    public ShareItem? UpdateShare(string id, UpdateShareRequest request)
    {
        lock (_lock)
        {
            var share = _state.Shares.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (share is null) return null;

            if (request.Name is not null)
            {
                share.Name = string.IsNullOrWhiteSpace(request.Name) ? share.Name : request.Name.Trim();
            }

            if (request.Path is not null)
            {
                share.Path = NormalizeExistingDirectory(request.Path);
            }

            if (request.Enabled.HasValue)
            {
                share.Enabled = request.Enabled.Value;
            }

            if (request.Settings is not null)
            {
                share.Settings = request.Settings;
            }

            share.UpdatedAt = DateTimeOffset.Now;
            SaveLocked();
            return Clone(share);
        }
    }

    public bool DeleteShare(string id)
    {
        lock (_lock)
        {
            var removed = _state.Shares.RemoveAll(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed) SaveLocked();
            return removed;
        }
    }

    public ShareItem? EnsureInitialShare(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return null;

        lock (_lock)
        {
            var fullPath = Path.GetFullPath(path);
            var existing = _state.Shares.FirstOrDefault(s => s.Path.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
            if (existing is not null) return Clone(existing);

            var name = new DirectoryInfo(fullPath).Name;
            var share = new ShareItem
            {
                Id = CreateUniqueId(name),
                Name = name,
                Path = fullPath,
                Enabled = true,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now,
                Settings = Clone(_state.Defaults)
            };
            _state.Shares.Add(share);
            SaveLocked();
            return Clone(share);
        }
    }

    private ShowcaseState LoadOrCreate(IConfiguration configuration)
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var state = JsonSerializer.Deserialize<ShowcaseState>(File.ReadAllText(_configPath), _jsonOptions);
                if (state is not null)
                {
                    state.Defaults ??= BuildDefaultSettings(configuration);
                    state.Shares ??= [];
                    foreach (var share in state.Shares)
                    {
                        share.Settings ??= Clone(state.Defaults);
                        share.Settings.Watermark ??= new WatermarkSettings();
                    }
                    return state;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"配置文件解析失败, 将使用默认配置: {ex.Message}");
            }
        }

        var created = new ShowcaseState
        {
            Defaults = BuildDefaultSettings(configuration),
            Shares = []
        };
        File.WriteAllText(_configPath, JsonSerializer.Serialize(created, _jsonOptions));
        return created;
    }

    private ShareSettings BuildDefaultSettings(IConfiguration configuration)
    {
        return new ShareSettings
        {
            LoginEnabled = configuration.GetValue("DocShowcase:LoginEnabled", true),
            WatermarkEnabled = configuration.GetValue("DocShowcase:WatermarkEnabled", true),
            EncryptionEnabled = configuration.GetValue("DocShowcase:EncryptionEnabled", true),
            CopyEnabled = configuration.GetValue("DocShowcase:CopyEnabled", false),
            ProtectionEnabled = configuration.GetValue("DocShowcase:ProtectionEnabled", true),
            AccessPassword = Environment.GetEnvironmentVariable("DOC_PASSWORD")
                ?? configuration["DocShowcase:AccessPassword"]
                ?? "123456",
            AdminPassword = configuration["DocShowcase:AdminPassword"] ?? "194536",
            Watermark = LoadWatermarkDefaults()
        };
    }

    private WatermarkSettings LoadWatermarkDefaults()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "watermark-config.json");
        if (File.Exists(configPath))
        {
            try
            {
                var watermark = JsonSerializer.Deserialize<WatermarkSettings>(File.ReadAllText(configPath), _jsonOptions);
                if (watermark is not null) return watermark;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"水印配置解析失败, 使用默认水印配置: {ex.Message}");
            }
        }

        return new WatermarkSettings();
    }

    private string CreateUniqueId(string name)
    {
        var slug = Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(slug)) slug = "share";
        var id = slug;
        var index = 2;
        while (_state.Shares.Any(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
        {
            id = $"{slug}-{index++}";
        }
        return id;
    }

    private static string NormalizeExistingDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("目录路径不能为空");
        }

        var fullPath = Path.GetFullPath(path.Trim());
        if (!Directory.Exists(fullPath))
        {
            throw new ArgumentException($"目录不存在: {fullPath}");
        }

        return fullPath;
    }

    private void SaveLocked()
    {
        File.WriteAllText(_configPath, JsonSerializer.Serialize(_state, _jsonOptions));
    }

    private T Clone<T>(T value)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, _jsonOptions), _jsonOptions)!;
    }
}

sealed class ShowcaseState
{
    public ShareSettings Defaults { get; set; } = new();
    public List<ShareItem> Shares { get; set; } = [];
}

sealed class ShareItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ShareSettings Settings { get; set; } = new();
}

sealed class ShareSettings
{
    public bool LoginEnabled { get; set; } = true;
    public bool WatermarkEnabled { get; set; } = true;
    public bool EncryptionEnabled { get; set; } = true;
    public bool CopyEnabled { get; set; }
    public bool ProtectionEnabled { get; set; } = true;
    public string AccessPassword { get; set; } = "123456";
    public string AdminPassword { get; set; } = "194536";
    public WatermarkSettings Watermark { get; set; } = new();
}

sealed class WatermarkSettings
{
    public bool Enabled { get; set; } = true;
    public string Text { get; set; } = "机密文档 严禁传播 违者必究";
    public int Count { get; set; } = 100;
    public int FontSize { get; set; } = 12;
    public string FontFamily { get; set; } = "Arial";
    public int LetterSpacing { get; set; } = 2;
    public List<string> Colors { get; set; } = ["rgba(0,0,0,0.01)", "rgba(255,0,0,0.01)", "rgba(0,0,255,0.01)"];
    public List<int> Rotations { get; set; } = [-45, -40, -35];
    public int GridColumns { get; set; } = 20;
    public int CheckInterval { get; set; } = 2000;
}

sealed class CreateShareRequest
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public ShareSettings? Settings { get; set; }
}

sealed class UpdateShareRequest
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public bool? Enabled { get; set; }
    public ShareSettings? Settings { get; set; }
}
