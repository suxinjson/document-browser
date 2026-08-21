using System.Security.Cryptography;
using System.Text;

static class FileService
{
    static readonly HashSet<string> SkipDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        "DocShowcase", "bin", "obj", "node_modules", "__pycache__", ".git", ".vs", ".claude", ".spec-workflow"
    };

    /// <summary>
    /// 构建目录树
    /// </summary>
    public static TreeNode BuildTree(string root, string current)
    {
        var rel = Path.GetRelativePath(root, current).Replace('\\', '/');
        var name = new DirectoryInfo(current).Name;
        var node = new TreeNode
        {
            Name = name,
            Id = MakeId(rel),
            RelPath = rel == "." ? "" : rel,
            Type = "dir",
            Children = []
        };

        if (!Directory.Exists(current)) return node;

        // 子目录
        foreach (var d in Directory.GetDirectories(current)
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            var dirName = Path.GetFileName(d);
            if (dirName.StartsWith('.') || SkipDirs.Contains(dirName)) continue;
            node.Children.Add(BuildTree(root, d));
        }

        // 文件
        foreach (var f in Directory.GetFiles(current)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            var fileName = Path.GetFileName(f);
            var relPath = Path.GetRelativePath(root, f).Replace('\\', '/');
            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            node.Children.Add(new TreeNode
            {
                Name = fileName,
                DisplayName = Path.GetFileNameWithoutExtension(fileName), // 显示名称（无后缀）
                Id = MakeId(relPath),
                RelPath = relPath,
                Type = "file",
                IsMd = ext == ".md"
            });
        }

        return node;
    }

    static readonly HashSet<string> ImageExts = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".bmp"
    };

    static readonly Dictionary<string, string> ImageMime = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        [".bmp"] = "image/bmp"
    };

    /// <summary>
    /// 解析相对路径，校验不越过 root，返回规范化绝对路径（文件或目录），不存在返回 null
    /// </summary>
    static string? Resolve(string root, string currentFilePath, string link)
    {
        try
        {
            var baseDir = Path.GetDirectoryName(currentFilePath) ?? currentFilePath;
            var abs = Path.GetFullPath(Path.Combine(baseDir, link));
            var rootFull = Path.GetFullPath(root);
            if (!abs.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)) return null;
            if (!File.Exists(abs) && !Directory.Exists(abs)) return null;
            return abs;
        }
        catch { return null; }
    }

    /// <summary>
    /// 读取图片字节（相对当前文件解析路径，校验白名单与 root 边界）
    /// </summary>
    public static (byte[] Bytes, string ContentType)? GetImageBytes(string root, string currentFilePath, string link)
    {
        var abs = Resolve(root, currentFilePath, link);
        if (abs is null || !File.Exists(abs)) return null;
        var ext = Path.GetExtension(abs).ToLowerInvariant();
        if (!ImageExts.Contains(ext)) return null;
        try
        {
            var bytes = File.ReadAllBytes(abs);
            return (bytes, ImageMime[ext]);
        }
        catch { return null; }
    }

    /// <summary>
    /// 获取文件内容
    /// </summary>
    public static FileContent? GetFileContent(string root, string id)
    {
        // 通过遍历找到对应文件
        var (filePath, entry) = FindById(root, root, id);
        if (filePath is null) return null;

        var ext = Path.GetExtension(entry!.Name).ToLowerInvariant();
        var content = "";
        if (ext == ".md")
        {
            try { content = File.ReadAllText(filePath, System.Text.Encoding.UTF8); }
            catch { content = "*无法读取此文件*"; }
        }

        return new FileContent
        {
            Name = entry.Name,
            Path = Path.GetRelativePath(root, filePath).Replace('\\', '/'),
            IsMd = ext == ".md",
            Content = content
        };
    }

    static (string? Path, TreeNode? Node) FindById(string root, string current, string targetId)
    {
        if (!Directory.Exists(current)) return (null, null);

        foreach (var f in Directory.GetFiles(current))
        {
            var rel = Path.GetRelativePath(root, f);
            var id = MakeId(rel);
            if (id == targetId)
                return (f, new TreeNode { Name = Path.GetFileName(f), Id = id });
        }

        foreach (var d in Directory.GetDirectories(current))
        {
            var dirName = Path.GetFileName(d);
            if (dirName.StartsWith('.') || SkipDirs.Contains(dirName)) continue;
            var result = FindById(root, d, targetId);
            if (result.Path is not null) return result;
        }

        return (null, null);
    }

    // 生成节点 id：原路径的 SHA256 哈希前 8 位。避免中文文件名被替换成下划线串、保证唯一且跨重启稳定
    static string MakeId(string path)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path))).ToLowerInvariant();
        return hash[..8];
    }
}

class TreeNode
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = ""; // 显示名称（无后缀）
    public string Id { get; set; } = "";
    public string RelPath { get; set; } = ""; // 相对根目录的路径（/分隔）
    public string Type { get; set; } = ""; // "dir" | "file"
    public bool IsMd { get; set; }
    public List<TreeNode> Children { get; set; } = [];
}

class FileContent
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public bool IsMd { get; set; }
    public string Content { get; set; } = "";
}
