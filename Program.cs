var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});
builder.Services.AddSingleton<ShowcaseConfigStore>();

var app = builder.Build();
app.UseSession();

var initialPath = args.FirstOrDefault(a => !a.StartsWith("-"));
var configStore = app.Services.GetRequiredService<ShowcaseConfigStore>();
var initialShare = configStore.EnsureInitialShare(initialPath);
var adminPassword = Environment.GetEnvironmentVariable("DOC_ADMIN_PASSWORD")
    ?? builder.Configuration["DocShowcase:AdminPassword"]
    ?? "194536";

Console.WriteLine("DocShowcase 管理后台已启动");
Console.WriteLine("访问 http://localhost:5000 打开配置页面");
if (initialShare is not null)
{
    Console.WriteLine($"已根据启动参数添加共享: {initialShare.Name} -> /s/{initialShare.Id}");
}

bool IsAuthenticated(HttpContext ctx, ShareItem share) =>
    !share.Settings.LoginEnabled || ctx.Session.GetString(SessionKey(share.Id, "authenticated")) == "true";

bool IsAdminAuthenticated(HttpContext ctx) => ctx.Session.GetString("admin_authenticated") == "true";

string CreateSession(HttpContext ctx, ShareItem share, bool isAdmin)
{
    var sessionKey = Guid.NewGuid().ToString("N");
    ctx.Session.SetString(SessionKey(share.Id, "authenticated"), "true");
    ctx.Session.SetString(SessionKey(share.Id, "session_key"), sessionKey);
    ctx.Session.SetString(SessionKey(share.Id, "is_admin"), isAdmin.ToString());
    return sessionKey;
}

string? GetSessionKey(HttpContext ctx, ShareItem share)
{
    var sessionKey = ctx.Session.GetString(SessionKey(share.Id, "session_key"));
    if (!string.IsNullOrEmpty(sessionKey)) return sessionKey;
    return share.Settings.LoginEnabled ? null : CreateSession(ctx, share, isAdmin: true);
}

static string SessionKey(string shareId, string name) => $"share:{shareId}:{name}";

ShareItem? GetEnabledShare(ShowcaseConfigStore store, string shareId)
{
    var share = store.GetShare(shareId);
    return share is not null && share.Enabled && Directory.Exists(share.Path) ? share : null;
}

WatermarkSettings EffectiveWatermark(ShareItem share)
{
    var watermark = Clone(share.Settings.Watermark);
    watermark.Enabled = share.Settings.WatermarkEnabled && share.Settings.Watermark.Enabled;
    return watermark;
}

static T Clone<T>(T value)
{
    var json = System.Text.Json.JsonSerializer.Serialize(value);
    return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
}

app.MapGet("/", () => Results.Text(AdminTemplate.Page, "text/html; charset=utf-8"));

app.MapGet("/s/{shareId}", (string shareId, ShowcaseConfigStore store) =>
{
    var share = store.GetShare(shareId);
    if (share is null) return Results.NotFound("共享不存在");
    return Results.Text(HtmlTemplate.Page, "text/html; charset=utf-8");
});

app.MapPost("/api/admin/auth", (HttpContext ctx, AuthRequest req) =>
{
    if (req.Password != adminPassword)
    {
        return Results.Json(new { success = false, message = "管理员密码错误" }, statusCode: 401);
    }

    ctx.Session.SetString("admin_authenticated", "true");
    return Results.Ok(new { success = true });
});

app.MapGet("/api/admin/config", (HttpContext ctx, ShowcaseConfigStore store) =>
{
    if (!IsAdminAuthenticated(ctx)) return Results.Unauthorized();
    return Results.Json(store.GetState());
});

app.MapPost("/api/admin/shares", (HttpContext ctx, ShowcaseConfigStore store, CreateShareRequest request) =>
{
    if (!IsAdminAuthenticated(ctx)) return Results.Unauthorized();
    try
    {
        return Results.Ok(store.AddShare(request));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/admin/shares/{shareId}", (HttpContext ctx, ShowcaseConfigStore store, string shareId, UpdateShareRequest request) =>
{
    if (!IsAdminAuthenticated(ctx)) return Results.Unauthorized();
    try
    {
        var share = store.UpdateShare(shareId, request);
        return share is null ? Results.NotFound(new { message = "共享不存在" }) : Results.Ok(share);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapDelete("/api/admin/shares/{shareId}", (HttpContext ctx, ShowcaseConfigStore store, string shareId) =>
{
    if (!IsAdminAuthenticated(ctx)) return Results.Unauthorized();
    return store.DeleteShare(shareId)
        ? Results.Ok(new { success = true })
        : Results.NotFound(new { message = "共享不存在" });
});

app.MapGet("/api/admin/fs", (HttpContext ctx, string? path) =>
{
    if (!IsAdminAuthenticated(ctx)) return Results.Unauthorized();
    try
    {
        return Results.Ok(BrowseDirectories(path));
    }
    catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or ArgumentException)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/admin/logs", (HttpContext ctx) =>
{
    if (!IsAdminAuthenticated(ctx)) return Results.Unauthorized();
    return Results.Json(AccessLogger.GetLogs());
});

app.MapGet("/api/share/{shareId}/app-config", (ShowcaseConfigStore store, string shareId) =>
{
    var share = store.GetShare(shareId);
    if (share is null) return Results.NotFound(new { message = "共享不存在" });

    return Results.Json(new
    {
        share.Id,
        share.Name,
        share.Enabled,
        share.Settings.LoginEnabled,
        share.Settings.EncryptionEnabled,
        share.Settings.CopyEnabled,
        share.Settings.ProtectionEnabled,
        WatermarkEnabled = EffectiveWatermark(share).Enabled
    });
});

app.MapPost("/api/share/{shareId}/auth", (HttpContext ctx, ShowcaseConfigStore store, string shareId, AuthRequest req) =>
{
    var share = GetEnabledShare(store, shareId);
    if (share is null) return Results.NotFound(new { success = false, message = "共享不存在或已停用" });

    if (!share.Settings.LoginEnabled)
    {
        var anonymousSessionKey = CreateSession(ctx, share, isAdmin: true);
        return Results.Ok(new { success = true, key = anonymousSessionKey, isAdmin = true });
    }

    var accessPassword = Environment.GetEnvironmentVariable("DOC_PASSWORD") ?? share.Settings.AccessPassword;
    var isAdmin = req.Password == share.Settings.AdminPassword;
    if (!isAdmin && req.Password != accessPassword)
    {
        AccessLogger.Log("login_failed", ctx, $"共享: {share.Name}, 目录: {share.Path}, 密码错误: {req.Password}");
        return Results.Json(new { success = false, message = "密码错误" }, statusCode: 401);
    }

    var sessionKey = CreateSession(ctx, share, isAdmin);
    AccessLogger.Log("login_success", ctx, isAdmin ? $"管理员登录: {share.Name}, 目录: {share.Path}" : $"普通用户登录: {share.Name}, 目录: {share.Path}");
    return Results.Ok(new { success = true, key = sessionKey, isAdmin });
});

app.MapGet("/api/share/{shareId}/watermark-config", (ShowcaseConfigStore store, string shareId) =>
{
    var share = store.GetShare(shareId);
    if (share is null) return Results.NotFound(new { message = "共享不存在" });
    return Results.Json(EffectiveWatermark(share));
});

app.MapGet("/api/share/{shareId}/tree", (HttpContext ctx, ShowcaseConfigStore store, string shareId) =>
{
    var share = GetEnabledShare(store, shareId);
    if (share is null) return Results.NotFound(new { message = "共享不存在或已停用" });
    if (!IsAuthenticated(ctx, share)) return Results.Unauthorized();

    var tree = FileService.BuildTree(share.Path, share.Path);
    AccessLogger.Log("view_tree", ctx, $"共享: {share.Name}, 目录: {share.Path}");

    var options = new System.Text.Json.JsonSerializerOptions
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    if (!share.Settings.EncryptionEnabled)
    {
        return Results.Json(new { encrypted = false, data = tree.Children }, options);
    }

    var sessionKey = GetSessionKey(ctx, share);
    if (string.IsNullOrEmpty(sessionKey)) return Results.Unauthorized();

    var json = System.Text.Json.JsonSerializer.Serialize(tree.Children, options);
    var encrypted = EncryptionHelper.Encrypt(json, sessionKey);
    return Results.Json(new { encrypted = true, data = encrypted });
});

app.MapGet("/api/share/{shareId}/file", (HttpContext ctx, ShowcaseConfigStore store, string shareId, string id) =>
{
    var share = GetEnabledShare(store, shareId);
    if (share is null) return Results.NotFound(new { message = "共享不存在或已停用" });
    if (!IsAuthenticated(ctx, share)) return Results.Unauthorized();

    var content = FileService.GetFileContent(share.Path, id);
    if (content is null) return Results.NotFound();

    AccessLogger.Log("view_file", ctx, $"共享: {share.Name}, 目录: {share.Path}, 文件: {content.Name}");

    var options = new System.Text.Json.JsonSerializerOptions
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    if (!share.Settings.EncryptionEnabled)
    {
        return Results.Json(new { encrypted = false, data = content }, options);
    }

    var sessionKey = GetSessionKey(ctx, share);
    if (string.IsNullOrEmpty(sessionKey)) return Results.Unauthorized();

    var json = System.Text.Json.JsonSerializer.Serialize(content, options);
    var encrypted = EncryptionHelper.Encrypt(json, sessionKey);
    return Results.Json(new { encrypted = true, data = encrypted });
});

app.MapGet("/api/share/{shareId}/image", (HttpContext ctx, ShowcaseConfigStore store, string shareId, string? cur, string? link) =>
{
    var share = GetEnabledShare(store, shareId);
    if (share is null) return Results.NotFound(new { message = "共享不存在或已停用" });
    if (!IsAuthenticated(ctx, share)) return Results.Unauthorized();

    if (share.Settings.EncryptionEnabled)
    {
        var sessionKey = GetSessionKey(ctx, share);
        if (string.IsNullOrEmpty(sessionKey)) return Results.Unauthorized();
    }

    if (string.IsNullOrEmpty(cur) || string.IsNullOrEmpty(link)) return Results.BadRequest();

    var currentAbs = Path.GetFullPath(Path.Combine(share.Path, cur));
    var rootFull = Path.GetFullPath(share.Path);
    if (!currentAbs.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)) return Results.BadRequest();

    var img = FileService.GetImageBytes(share.Path, currentAbs, link);
    if (img is null) return Results.NotFound();
    return Results.File(img.Value.Bytes, img.Value.ContentType);
});

app.MapGet("/api/share/{shareId}/logs", (HttpContext ctx, ShowcaseConfigStore store, string shareId) =>
{
    var share = GetEnabledShare(store, shareId);
    if (share is null) return Results.NotFound(new { message = "共享不存在或已停用" });
    if (!IsAuthenticated(ctx, share)) return Results.Unauthorized();

    return Results.Json(AccessLogger.GetLogs());
});

app.Run();

static DirectoryBrowserResult BrowseDirectories(string? requestedPath)
{
    if (string.IsNullOrWhiteSpace(requestedPath))
    {
        var drives = DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => new DirectoryEntry(d.Name, d.Name))
            .ToList();
        return new DirectoryBrowserResult("", null, drives);
    }

    var current = Path.GetFullPath(requestedPath);
    if (!Directory.Exists(current)) throw new ArgumentException($"目录不存在: {current}");

    var parent = Directory.GetParent(current)?.FullName;
    var directories = Directory.GetDirectories(current)
        .Select(d => new DirectoryInfo(d))
        .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
        .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
        .Select(d => new DirectoryEntry(d.Name, d.FullName))
        .ToList();

    return new DirectoryBrowserResult(current, parent, directories);
}

record AuthRequest(string Password);
record DirectoryBrowserResult(string CurrentPath, string? ParentPath, List<DirectoryEntry> Directories);
record DirectoryEntry(string Name, string Path);
