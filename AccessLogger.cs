using System.Text.Json;
using System.Text.RegularExpressions;

static class AccessLogger
{
    private static readonly string LogFile = Path.Combine(AppContext.BaseDirectory, "access_log.json");
    private static readonly object LockObj = new();

    /// <summary>
    /// 记录访问日志
    /// </summary>
    public static void Log(string eventType, HttpContext ctx, string? details = null)
    {
        var userAgent = ctx.Request.Headers.UserAgent.ToString();
        var entry = new AccessLogEntry
        {
            Timestamp = DateTime.Now,
            EventType = eventType,
            IpAddress = GetClientIp(ctx),
            UserAgent = userAgent,
            Browser = GetBrowser(userAgent),
            Device = GetDevice(userAgent),
            OperatingSystem = GetOperatingSystem(userAgent),
            Referer = ctx.Request.Headers.Referer.ToString(),
            RequestPath = ctx.Request.Path.ToString(),
            QueryString = ctx.Request.QueryString.ToString(),
            RemotePort = ctx.Connection.RemotePort,
            Method = ctx.Request.Method,
            Details = details
        };

        lock (LockObj)
        {
            var logs = LoadLogs();
            logs.Add(entry);

            // 只保留最近 1000 条记录
            if (logs.Count > 1000)
                logs = logs.Skip(logs.Count - 1000).ToList();

            File.WriteAllText(LogFile, JsonSerializer.Serialize(logs, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }

    /// <summary>
    /// 获取所有访问日志
    /// </summary>
    public static List<AccessLogEntry> GetLogs()
    {
        lock (LockObj)
        {
            return LoadLogs();
        }
    }

    private static List<AccessLogEntry> LoadLogs()
    {
        if (!File.Exists(LogFile))
            return new List<AccessLogEntry>();

        try
        {
            var json = File.ReadAllText(LogFile);
            return JsonSerializer.Deserialize<List<AccessLogEntry>>(json) ?? new List<AccessLogEntry>();
        }
        catch
        {
            return new List<AccessLogEntry>();
        }
    }

    /// <summary>
    /// 获取客户端真实 IP
    /// </summary>
    private static string GetClientIp(HttpContext ctx)
    {
        var forwardedFor = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        var realIp = ctx.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GetBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "未知浏览器";
        if (userAgent.Contains("Edg/")) return "Microsoft Edge";
        if (userAgent.Contains("Chrome/") && !userAgent.Contains("Edg/")) return "Google Chrome";
        if (userAgent.Contains("Firefox/")) return "Mozilla Firefox";
        if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome/")) return "Safari";
        if (userAgent.Contains("Opera/") || userAgent.Contains("OPR/")) return "Opera";
        if (userAgent.Contains("MSIE") || userAgent.Contains("Trident/")) return "Internet Explorer";
        return "其他浏览器";
    }

    private static string GetDevice(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "未知设备";
        if (Regex.IsMatch(userAgent, @"Mobile|Android|iPhone|iPad|iPod", RegexOptions.IgnoreCase))
        {
            if (userAgent.Contains("iPhone")) return "iPhone";
            if (userAgent.Contains("iPad")) return "iPad";
            if (userAgent.Contains("Android")) return "Android设备";
            return "移动设备";
        }
        return "桌面设备";
    }

    private static string GetOperatingSystem(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "未知系统";
        if (userAgent.Contains("Windows NT 10.0")) return "Windows 10/11";
        if (userAgent.Contains("Windows NT 6.3")) return "Windows 8.1";
        if (userAgent.Contains("Windows NT 6.1")) return "Windows 7";
        if (userAgent.Contains("Mac OS X")) return "macOS";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
        if (userAgent.Contains("Linux")) return "Linux";
        return "其他系统";
    }
}

class AccessLogEntry
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public string Browser { get; set; } = "";
    public string Device { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public string Referer { get; set; } = "";
    public string RequestPath { get; set; } = "";
    public string QueryString { get; set; } = "";
    public int RemotePort { get; set; }
    public string Method { get; set; } = "";
    public string? Details { get; set; }
}
