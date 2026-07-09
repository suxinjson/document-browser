using System.Text.RegularExpressions;

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
        var name = new DirectoryInfo(current).Name;
        var node = new TreeNode
        {
            Name = name,
            Id = MakeId(Path.GetRelativePath(root, current)),
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
            var rel = Path.GetRelativePath(root, f);
            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            node.Children.Add(new TreeNode
            {
                Name = fileName,
                DisplayName = Path.GetFileNameWithoutExtension(fileName), // 显示名称（无后缀）
                Id = MakeId(rel),
                Type = "file",
                IsMd = ext == ".md"
            });
        }

        return node;
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

    static string MakeId(string path) =>
        Regex.Replace(path, @"[^a-zA-Z0-9_-]", "_");
}

class TreeNode
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = ""; // 显示名称（无后缀）
    public string Id { get; set; } = "";
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
