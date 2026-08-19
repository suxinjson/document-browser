# DocShowcase

带动态水印、访问控制、内容加密和访问审计的文档分享 / 展示 Web 应用。基于 .NET 10 Minimal API 构建，单文件可执行，适合把本地目录安全地共享给他人在线浏览。

## ✨ 特性

- 📁 **目录共享** — 通过管理后台把任意本地目录发布成一个共享链接（`/s/{id}`），自动生成文件树
- 🆔 **访问控制** — 每个共享可独立配置访问密码 / 管理员密码，支持匿名、普通用户、管理员三级权限
- 💧 **动态水印** — 在文档上叠加不可见 / 低透明度水印（文字、数量、字号、颜色、旋转均可配），可定时校验防止去除
- 🔐 **内容加密** — 文件树与文件内容在传输层用会话密钥加密，防止直接抓包读取
- 🛡️ **内容保护** — 可禁用复制、防右键等（复制开关 `CopyEnabled`）
- 📝 **访问审计** — 登录成功 / 失败、目录浏览、文件查看等操作均记录日志，可在后台查看
- ⚙️ **图形化配置** — 内置管理后台页面，可视化添加 / 编辑 / 停用共享、浏览磁盘目录、查看日志
- 🔑 **环境变量覆盖** — 敏感口令支持通过环境变量注入，避免硬编码

## 🚀 运行

前置条件：[.NET 10 SDK](https://dotnet.microsoft.com/)

```bash
git clone https://github.com/suxinjson/document-browser.git
cd document-browser
dotnet run
```

启动后控制台会提示：

```
DocShowcase 管理后台已启动
访问 http://localhost:5000 打开配置页面
```

打开 http://localhost:5000 即进入管理后台；也可在启动时附带一个目录参数，自动创建首个共享：

```bash
dotnet run -- "D:\我的文档"
# 已根据启动参数添加共享: 我的文档 -> /s/我的文档
```

## 🔧 配置

口令与服务参数支持三层覆盖（优先级从高到低）：

| 配置项 | 环境变量 | appsettings.json 路径 | 默认值 |
|--------|----------|----------------------|--------|
| 管理后台密码 | `DOC_ADMIN_PASSWORD` | `DocShowcase:AdminPassword` | `194536` |
| 共享访问密码 | `DOC_PASSWORD` | `DocShowcase:AccessPassword` | `123456` |

> ⚠️ 上面的默认口令仅为初始化占位，**生产环境请务必通过环境变量或 appsettings 覆盖**。

其余共享级开关（均可按共享单独配置）：

- `LoginEnabled` — 是否开启访问密码登录
- `WatermarkEnabled` / `Watermark` — 水印总开关与样式
- `EncryptionEnabled` — 是否加密传输文件内容
- `CopyEnabled` — 是否允许复制
- `ProtectionEnabled` — 是否启用内容保护

水印样式在 `watermark-config.json` 中维护（文字、数量、字号、颜色、旋转、栅格列数、校验间隔等）。

## 📂 项目结构

```
├── Program.cs              路由与会话 / 鉴权逻辑（Minimal API）
├── ShowcaseConfig.cs      共享配置存储与数据模型
├── FileService.cs         文件树构建与文件内容读取
├── EncryptionHelper.cs    会话密钥内容加密
├── AccessLogger.cs        访问日志记录
├── HtmlTemplate.cs        文档浏览前端页面
├── AdminTemplate.cs       管理后台前端页面
├── watermark-config.json 水印默认样式
└── appsettings.json       服务与口令配置
```

## 📌 说明

- 本项目当前为中文界面，面向个人 / 团队内部文档的安全分享场景
- `bin/` `obj/` `.vs/` 已加入 `.gitignore`,构建产物与 IDE 缓存不会进入版本库
