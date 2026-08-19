static class HtmlTemplate
{
    public const string Page = """
<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>文档浏览器</title>
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/github-markdown-css@5/github-markdown-light.min.css">
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/highlight.js@11/styles/github.min.css">
<script src="https://cdn.jsdelivr.net/npm/marked@4.3.0/marked.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/@highlightjs/cdn-assets@11.11.1/highlight.min.js"></script>
<!-- crypto-js：局域网 HTTP 下 crypto.subtle 不可用（仅 HTTPS / localhost 安全上下文可用），用它作 AES 解密回退 -->
<script src="https://cdn.jsdelivr.net/npm/crypto-js@4.2.0/crypto-js.min.js"></script>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  :root {
    --sidebar-bg: #f8f9fb;
    --sidebar-w: 300px;
    --accent: #4f6ef7;
    --accent-light: #eef1fe;
    --text: #1a1a2e;
    --text2: #555;
    --border: #e2e6ef;
    --hover-bg: #eaeef8;
  }
  body { font-family: -apple-system, "PingFang SC", "Microsoft YaHei", sans-serif; color: var(--text); display: flex; height: 100vh; overflow: hidden; }

  /* 登录界面样式 */
  .login-container {
    display: none; align-items: center; justify-content: center; width: 100%; height: 100vh;
    background: linear-gradient(135deg, var(--accent-light) 0%, #fff 100%);
  }
  .login-container.show { display: flex; }
  .login-box {
    background: #fff; padding: 40px; border-radius: 16px; box-shadow: 0 8px 32px rgba(0,0,0,0.1);
    width: 100%; max-width: 400px; text-align: center;
  }
  .login-box h1 { font-size: 24px; margin-bottom: 8px; color: var(--accent); }
  .login-box p { color: var(--text2); margin-bottom: 32px; font-size: 14px; }
  .login-box input {
    width: 100%; padding: 12px 16px; border: 1px solid var(--border); border-radius: 8px;
    font-size: 14px; margin-bottom: 16px; outline: none; transition: border .2s;
  }
  .login-box input:focus { border-color: var(--accent); }
  .login-box button {
    width: 100%; padding: 12px; background: var(--accent); color: #fff; border: none;
    border-radius: 8px; font-size: 15px; font-weight: 600; cursor: pointer; transition: opacity .2s;
  }
  .login-box button:hover { opacity: 0.9; }
  .login-box button:disabled { opacity: 0.5; cursor: not-allowed; }
  .error-msg { color: #e74c3c; font-size: 13px; margin-top: -8px; margin-bottom: 16px; }

  /* 主界面样式 */
  .app-container { display: none; width: 100%; height: 100vh; }
  .app-container.show { display: flex; }
  .sidebar {
    width: var(--sidebar-w); min-width: var(--sidebar-w); background: var(--sidebar-bg);
    border-right: 1px solid var(--border); display: flex; flex-direction: column; overflow: hidden;
  }
  .sidebar-header {
    padding: 20px 16px 12px; border-bottom: 1px solid var(--border);
    font-size: 16px; font-weight: 700; color: var(--accent); display: flex; align-items: center; gap: 8px;
  }
  .sidebar-header span { font-size: 20px; }
  .sidebar-search { padding: 10px 12px; }
  .sidebar-search input {
    width: 100%; padding: 8px 12px; border: 1px solid var(--border); border-radius: 8px;
    font-size: 13px; outline: none; background: #fff; transition: border .2s;
  }
  .sidebar-search input:focus { border-color: var(--accent); }
  .sidebar-tree { flex: 1; overflow-y: auto; padding: 8px 0; }
  .dir-group {}
  .dir-header {
    padding: 6px 12px; cursor: pointer; font-size: 14px; font-weight: 600;
    display: flex; align-items: center; gap: 4px; user-select: none; transition: background .15s;
  }
  .dir-header:hover { background: var(--hover-bg); }
  .arrow { font-size: 10px; width: 16px; text-align: center; transition: transform .2s; display: inline-block; }
  .arrow.open { transform: rotate(90deg); }
  .dname { color: var(--text); }
  .dir-content { padding-left: 16px; overflow: hidden; }
  .file-item {
    padding: 5px 12px; cursor: pointer; font-size: 13px; display: flex; align-items: center; gap: 6px;
    border-radius: 4px; margin: 1px 4px; transition: background .15s, color .15s;
  }
  .file-item:hover { background: var(--hover-bg); }
  .file-item.active { background: var(--accent); color: #fff; }
  .file-item.active .fname { color: #fff; }
  .fname { color: var(--text2); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
  .icon { font-size: 14px; flex-shrink: 0; }
  .main { flex: 1; overflow-y: auto; background: #fff; }
  .welcome {
    display: flex; flex-direction: column; align-items: center; justify-content: center;
    height: 100%; color: #aaa; gap: 16px;
  }
  .welcome .big-icon { font-size: 64px; }
  .welcome p { font-size: 16px; }
  .content-wrap { max-width: 900px; margin: 0 auto; padding: 40px 32px 80px; }
  .breadcrumb {
    font-size: 13px; color: #999; margin-bottom: 16px; display: flex; align-items: center; gap: 4px; flex-wrap: wrap;
  }
  .breadcrumb span { cursor: pointer; }
  .breadcrumb span:hover { color: var(--accent); }
  .markdown-body { font-size: 15px; line-height: 1.75; }
  .markdown-body h1 { font-size: 26px; border-bottom: 2px solid var(--border); padding-bottom: 10px; margin: 0 0 20px; }
  .markdown-body h2 { font-size: 22px; margin: 28px 0 14px; }
  .markdown-body h3 { font-size: 18px; margin: 22px 0 10px; }
  .markdown-body h4 { font-size: 16px; margin: 18px 0 8px; }
  .markdown-body p { margin: 10px 0; }
  .markdown-body ul, .markdown-body ol { padding-left: 24px; margin: 10px 0; }
  .markdown-body li { margin: 4px 0; }
  .markdown-body code {
    background: var(--accent-light); color: var(--accent); padding: 2px 6px; border-radius: 4px; font-size: 0.9em;
  }
  .markdown-body pre {
    background: #1e1e2e; border-radius: 8px; padding: 16px; overflow-x: auto; margin: 14px 0;
  }
  .markdown-body pre code { background: none; color: #cdd6f4; padding: 0; font-size: 13px; }
  .markdown-body blockquote {
    border-left: 4px solid var(--accent); padding: 8px 16px; background: var(--accent-light);
    border-radius: 0 8px 8px 0; margin: 14px 0; color: var(--text2);
  }
  .markdown-body table { border-collapse: collapse; width: 100%; margin: 14px 0; }
  .markdown-body th, .markdown-body td { border: 1px solid var(--border); padding: 8px 12px; text-align: left; }
  .markdown-body th { background: var(--sidebar-bg); font-weight: 600; }
  .markdown-body a { color: var(--accent); text-decoration: none; }
  .markdown-body a:hover { text-decoration: underline; }
  .markdown-body hr { border: none; border-top: 1px solid var(--border); margin: 24px 0; }
  .markdown-body img { max-width: 100%; border-radius: 8px; }
  .raw-file { padding: 20px; background: var(--sidebar-bg); border-radius: 8px; color: var(--text2); }
  .raw-file .filename { font-weight: 700; font-size: 16px; margin-bottom: 8px; }
  .toast {
    position: fixed; top: 20px; left: 50%; transform: translateX(-50%) translateY(-20px);
    background: rgba(20,20,30,0.9); color: #fff; padding: 10px 18px; border-radius: 8px;
    font-size: 14px; z-index: 9999; opacity: 0; pointer-events: none;
    transition: opacity 0.2s, transform 0.2s;
  }
  .toast.show { opacity: 1; transform: translateX(-50%) translateY(0); }

  /* 防复制和防截屏保护 */
  body.protect-copy * { user-select: none; -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; }
  body.copy-enabled .markdown-body, body.copy-enabled .markdown-body * { user-select: text; -webkit-user-select: text; -moz-user-select: text; -ms-user-select: text; }
  .content-wrap { pointer-events: auto; }
  .markdown-body::before { content: ''; position: absolute; top: 0; left: 0; right: 0; bottom: 0; pointer-events: none; }
  @media print { body.protect-copy { display: none !important; } }

  .menu-btn {
    display: none; position: fixed; top: 12px; left: 12px; z-index: 100;
    background: var(--accent); color: #fff; border: none; border-radius: 8px;
    width: 40px; height: 40px; font-size: 20px; cursor: pointer;
  }
  .loading { display: flex; align-items: center; justify-content: center; height: 100%; gap: 12px; color: var(--accent); }
  .logs-modal {
    display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%;
    background: rgba(0,0,0,0.5); z-index: 2000; align-items: center; justify-content: center;
  }
  .logs-modal.show { display: flex; }
  .logs-content {
    background: #fff; border-radius: 12px; box-shadow: 0 8px 32px rgba(0,0,0,0.2);
    width: 90%; max-width: 900px; max-height: 80vh; display: flex; flex-direction: column;
  }
  .logs-header {
    padding: 20px; border-bottom: 1px solid var(--border); display: flex;
    align-items: center; justify-content: space-between; font-weight: 700; font-size: 18px;
  }
  .logs-close { cursor: pointer; font-size: 24px; color: var(--text2); padding: 0 8px; }
  .logs-close:hover { color: var(--accent); }
  .logs-body { padding: 20px; overflow-y: auto; flex: 1; }
  .log-entry {
    padding: 12px; border: 1px solid var(--border); border-radius: 8px;
    margin-bottom: 12px; font-size: 13px; background: #fafafa;
  }
  .log-entry:last-child { margin-bottom: 0; }
  .log-time { color: var(--text2); font-size: 11px; margin-bottom: 4px; }
  .log-event { font-weight: 600; color: var(--accent); margin-bottom: 4px; }
  .log-ip { color: var(--text2); font-size: 11px; margin-bottom: 4px; }
  .log-details { color: var(--text); font-size: 12px; }
  .spinner { width: 24px; height: 24px; border: 3px solid var(--border); border-top-color: var(--accent); border-radius: 50%; animation: spin .6s linear infinite; }
  @keyframes spin { to { transform: rotate(360deg); } }

  @media (max-width: 768px) {
    .sidebar { position: fixed; left: -320px; z-index: 99; height: 100vh; transition: left .3s; }
    .sidebar.open { left: 0; }
    .menu-btn { display: block; }
    .content-wrap { padding: 20px 16px 60px; }
  }
  .sidebar-tree::-webkit-scrollbar { width: 4px; }
  .sidebar-tree::-webkit-scrollbar-thumb { background: #ccc; border-radius: 4px; }
  .main::-webkit-scrollbar { width: 6px; }
  .main::-webkit-scrollbar-thumb { background: #ccc; border-radius: 4px; }
</style>
</head>
<body>
<!-- 登录界面 -->
<div class="login-container" id="loginContainer">
  <div class="login-box">
    <h1>🔒 文档浏览器</h1>
    <p>请输入访问密码</p>
    <input type="password" id="passwordInput" placeholder="请输入密码" autocomplete="off">
    <div class="error-msg" id="errorMsg"></div>
    <button onclick="login()">登录</button>
  </div>
</div>

<!-- 主应用界面 -->
<div class="app-container" id="appContainer">
  <button class="menu-btn" onclick="document.querySelector('.sidebar').classList.toggle('open')">&#9776;</button>
  <div class="sidebar">
    <div class="sidebar-header">
      <span>&#128209;</span>文档浏览
    </div>
    <div class="sidebar-search"><input type="text" placeholder="搜索文件..." oninput="filterTree(this.value)"></div>
    <div class="sidebar-tree" id="treeContainer"><div class="loading"><div class="spinner"></div><span>加载中...</span></div></div>
  </div>
  <div class="main" id="mainArea">
    <div class="welcome" id="welcome">
      <div class="big-icon">&#128214;</div>
      <p>点击左侧文件开始浏览</p>
    </div>
    <div class="content-wrap" id="contentArea" style="display:none">
      <div class="breadcrumb" id="breadcrumb"></div>
      <div id="fileContent"></div>
    </div>
  </div>
</div>

<!-- 提示 -->
<div id="toast" class="toast"></div>

<script>
marked.setOptions({
  highlight: function(code, lang) {
    if (lang && hljs.getLanguage(lang)) return hljs.highlight(code, { language: lang }).value;
    return hljs.highlightAuto(code).value;
  },
  breaks: true, gfm: true
});

// 自定义 renderer：重写链接和图片，支持相对路径跳转与图片加载
var IMAGE_EXT = /\.(png|jpe?g|gif|webp|svg|bmp)$/i;
var mdRenderer = new marked.Renderer();

mdRenderer.link = function(href, title, text) {
  if (href == null) return text;
  var isExternal = /^(https?:)?\/\//i.test(href) || /^(mailto:|tel:)/i.test(href);
  var titleAttr = title ? ' title="' + esc(title) + '"' : '';
  if (isExternal) {
    return '<a href="' + esc(href) + '"' + titleAttr + ' target="_blank" rel="noopener noreferrer">' + text + '</a>';
  }
  // 相对路径：交给 SPA 内部处理
  return '<a href="javascript:void(0)" data-link="' + esc(href) + '"' + titleAttr + '>' + text + '</a>';
};

mdRenderer.image = function(href, title, text) {
  if (href == null) return text;
  var isExternal = /^(https?:)?\/\//i.test(href);
  var url;
  if (isExternal) {
    url = href;
  } else if (IMAGE_EXT.test(href)) {
    url = apiBase + '/image?cur=' + encodeURIComponent(currentFilePath) + '&link=' + encodeURIComponent(href);
  } else {
    // 非图片相对资源：交给链接逻辑（不渲染为 img，避免 broken）
    return '<a href="javascript:void(0)" data-link="' + esc(href) + '">' + text + '</a>';
  }
  var titleAttr = title ? ' title="' + esc(title) + '"' : '';
  return '<img src="' + esc(url) + '" alt="' + esc(text) + '"' + titleAttr + ' />';
};

let treeData = null;
let flatNodes = []; // 扁平化的节点列表：{id, relPath, type}
let currentFilePath = ''; // 当前打开文件的 root 相对路径
let isAuthenticated = false;
let sessionKey = null;
let isAdmin = false;
let shareId = decodeURIComponent(location.pathname.split('/').filter(Boolean).pop() || '');
let apiBase = '/api/share/' + encodeURIComponent(shareId);
let appConfig = {
  loginEnabled: true,
  watermarkEnabled: true,
  encryptionEnabled: true,
  copyEnabled: false,
  protectionEnabled: true
};

// AES 解密函数：优先使用原生 Web Crypto（仅 HTTPS / localhost 安全上下文可用），
// 局域网 HTTP 下 crypto.subtle 不可用，回退到 CryptoJS（纯 JS 实现，算法与服务端一致）
async function decryptData(encryptedBase64, key) {
  if (window.crypto && crypto.subtle) {
    var encryptedData = Uint8Array.from(atob(encryptedBase64), function(c) { return c.charCodeAt(0); });
    var iv = encryptedData.slice(0, 16);
    var ciphertext = encryptedData.slice(16);

    var keyData = new TextEncoder().encode(key);
    var hashBuffer = await crypto.subtle.digest('SHA-256', keyData);
    var cryptoKey = await crypto.subtle.importKey('raw', hashBuffer, { name: 'AES-CBC' }, false, ['decrypt']);

    var decryptedBuffer = await crypto.subtle.decrypt({ name: 'AES-CBC', iv: iv }, cryptoKey, ciphertext);
    return new TextDecoder().decode(decryptedBuffer);
  }

  // 回退：CryptoJS 解析 base64(IV[16] + AES-256-CBC 密文)，密钥 = SHA256(sessionKey)
  if (window.CryptoJS) {
    var combined = CryptoJS.enc.Base64.parse(encryptedBase64);
    var iv = CryptoJS.lib.WordArray.create(combined.words.slice(0, 4), 16);
    var ciphertext = CryptoJS.lib.WordArray.create(combined.words.slice(4), combined.sigBytes - 16);
    var aesKey = CryptoJS.SHA256(key);
    var decrypted = CryptoJS.AES.decrypt(
      CryptoJS.lib.CipherParams.create({ ciphertext: ciphertext }),
      aesKey,
      { iv: iv, mode: CryptoJS.mode.CBC, padding: CryptoJS.pad.Pkcs7 }
    );
    return decrypted.toString(CryptoJS.enc.Utf8);
  }

  throw new Error('当前环境不支持解密（需 HTTPS / localhost，或 crypto-js 加载失败）');
}

async function readApiPayload(response) {
  if (response.encrypted === false) {
    return response.data;
  }

  var decrypted = await decryptData(response.data, sessionKey);
  return JSON.parse(decrypted);
}

// 登录功能
function login() {
  var password = document.getElementById('passwordInput').value;
  var errorMsg = document.getElementById('errorMsg');
  var btn = document.querySelector('.login-box button');

  btn.disabled = true;
  btn.textContent = '验证中...';
  errorMsg.textContent = '';

  fetch(apiBase + '/auth', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ password: password })
  })
  .then(function(r) { return r.json(); })
  .then(function(data) {
    if (data.success) {
      enterApp(data);
    } else {
      errorMsg.textContent = data.message || '密码错误';
      btn.disabled = false;
      btn.textContent = '登录';
    }
  })
  .catch(function(err) {
    errorMsg.textContent = '网络错误,请重试';
    btn.disabled = false;
    btn.textContent = '登录';
  });
}

function enterApp(data) {
  isAuthenticated = true;
  sessionKey = data.key;
  isAdmin = data.isAdmin || false;
  document.body.classList.toggle('copy-enabled', !!appConfig.copyEnabled);
  document.body.classList.toggle('protect-copy', !appConfig.copyEnabled);

  document.getElementById('loginContainer').classList.remove('show');
  document.getElementById('appContainer').classList.add('show');
  loadTree();
  initProtection(appConfig);
}

function startWithoutLogin() {
  fetch(apiBase + '/auth', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ password: '' })
  })
  .then(function(r) { return r.json(); })
  .then(function(data) {
    if (data.success) {
      enterApp(data);
      return;
    }

    showLogin(data.message || '初始化失败');
  })
  .catch(function(err) {
    showLogin('初始化失败: ' + err.message);
  });
}

function showLogin(message) {
  var loginContainer = document.getElementById('loginContainer');
  var errorMsg = document.getElementById('errorMsg');
  if (message) errorMsg.textContent = message;
  loginContainer.classList.add('show');
}

// 回车登录
document.addEventListener('DOMContentLoaded', function() {
  document.getElementById('passwordInput').addEventListener('keypress', function(e) {
    if (e.key === 'Enter') login();
  });

  fetch(apiBase + '/app-config')
    .then(function(r) { return r.json(); })
    .then(function(config) {
      appConfig = config || appConfig;
      if (appConfig.loginEnabled) {
        showLogin();
      } else {
        startWithoutLogin();
      }
    })
    .catch(function() {
      showLogin('读取配置失败');
    });
});

// 加载文件树
function loadTree() {
  fetch(apiBase + '/tree')
    .then(function(r) {
      if (r.status === 401) {
        location.reload();
        return;
      }
      return r.json();
    })
    .then(async function(response) {
      if (!response) return;
      console.log('API response:', response);
      var data = await readApiPayload(response);
      console.log('Parsed data:', data);
      console.log('Is array?', Array.isArray(data));
      console.log('Data length:', Array.isArray(data) ? data.length : 'not array');
      treeData = data;
      // 扁平化树，用于按相对路径查找节点
      flatNodes = [];
      flattenNodes(data, '');
      // 如果 data 是数组,直接渲染数组内容
      var html = '';
      if (Array.isArray(data)) {
        data.forEach(function(item) {
          html += renderTree(item);
        });
      } else {
        html = renderTree(data);
      }
      console.log('Generated HTML length:', html.length);
      console.log('Generated HTML (first 500 chars):', html.substring(0, 500));
      document.getElementById('treeContainer').innerHTML = html;
      // 目录默认全部收起
      document.querySelectorAll('.dir-group > .dir-content').forEach(function(el) {
        el.style.display = 'none';
        var arrow = document.getElementById('arrow-' + el.id);
        if (arrow) arrow.classList.remove('open');
      });
    })
    .catch(function(err) {
      console.error('加载文件树失败:', err);
      document.getElementById('treeContainer').innerHTML = '<div style="padding:12px;color:#e74c3c">加载失败: ' + err.message + '</div>';
    });
}

// 扁平化树，记录每个节点的 root 相对路径
function flattenNodes(node, parentRel) {
  if (Array.isArray(node)) {
    node.forEach(function(n) { flattenNodes(n, parentRel); });
    return;
  }
  if (!node) return;
  var rel = node.relPath != null ? node.relPath : (parentRel ? parentRel + '/' + node.name : node.name);
  flatNodes.push({ id: node.id, relPath: rel, type: node.type });
  if (node.children && node.children.length > 0) {
    node.children.forEach(function(c) { flattenNodes(c, rel); });
  }
}

function renderTree(node) {
  if (node.type === 'file') {
    var icon = node.isMd ? '&#128221;' : '&#128196;';
    var cls = node.isMd ? 'md-file' : '';
    // 优先使用 displayName（无后缀），如果没有则使用 name
    var displayName = node.displayName || node.name.replace(/\.[^.]+$/, '');
    return '<div class="file-item ' + cls + '" data-path="' + node.id + '" onclick="showFile(\'' + node.id + '\')">'
      + '<span class="icon">' + icon + '</span><span class="fname">' + esc(displayName) + '</span></div>';
  }

  // 处理目录节点
  if (node.type === 'dir') {
    var html = '<div class="dir-group">'
      + '<div class="dir-header" onclick="toggleDir(\'' + node.id + '\')">'
      + '<span class="arrow" id="arrow-' + node.id + '">&#9654;</span>'
      + '<span class="icon">&#128193;</span><span class="dname">' + esc(node.name) + '</span>'
      + '</div>'
      + '<div class="dir-content" id="' + node.id + '">';

    // 渲染子节点
    if (node.children && node.children.length > 0) {
      var sorted = node.children.slice().sort(function(a, b) {
        if (a.type !== b.type) return a.type === 'dir' ? -1 : 1;
        return a.name.localeCompare(b.name);
      });
      sorted.forEach(function(child) {
        html += renderTree(child);
      });
    }

    html += '</div></div>';
    return html;
  }

  return '';
}

function showFile(fid) {
  document.querySelectorAll('.file-item').forEach(function(el) { el.classList.remove('active'); });
  var el = document.querySelector('.file-item[data-path="' + fid + '"]');
  if (el) el.classList.add('active');

  document.getElementById('welcome').style.display = 'none';
  document.getElementById('contentArea').style.display = 'block';
  document.getElementById('fileContent').innerHTML = '<div class="loading" style="padding:40px"><div class="spinner"></div><span>读取中...</span></div>';

  fetch(apiBase + '/file?id=' + encodeURIComponent(fid))
    .then(function(r) {
      if (r.status === 401) {
        location.reload();
        return;
      }
      return r.json();
    })
    .then(async function(response) {
      if (!response) return;
      var info = await readApiPayload(response);

      var parts = info.path.split('/');
      currentFilePath = info.path;
      document.getElementById('breadcrumb').innerHTML =
        '<span onclick="showWelcome()">&#127968; 根目录</span>' +
        parts.map(function(p) { return ' <span>/</span> <span>' + esc(p) + '</span>'; }).join('');

      var contentEl = document.getElementById('fileContent');
      if (info.isMd) {
        contentEl.innerHTML = '<div class="markdown-body">' + marked.parse(info.content || '', { renderer: mdRenderer }) + '</div>';
      } else {
        contentEl.innerHTML = '<div class="raw-file"><div class="filename">&#128196; ' + esc(info.name) + '</div>'
          + '<div>此文件不是 Markdown 格式,无法预览内容。</div></div>';
      }
      document.querySelector('.sidebar').classList.remove('open');
    });
}

function showWelcome() {
  document.getElementById('welcome').style.display = 'flex';
  document.getElementById('contentArea').style.display = 'none';
}

// 解析相对链接，返回 {id, type} 或 null
function resolveLink(link, curFilePath) {
  if (!link || !curFilePath) return null;
  // 去掉 anchor
  var hashIdx = link.indexOf('#');
  var pathPart = hashIdx >= 0 ? link.substring(0, hashIdx) : link;
  if (!pathPart) return null;
  // 当前文件所在目录（root 相对）
  var baseDir = curFilePath.indexOf('/') >= 0 ? curFilePath.substring(0, curFilePath.lastIndexOf('/')) : '';
  // 规范化路径
  var segs = (baseDir ? baseDir.split('/') : []);
  var parts = pathPart.split('/');
  for (var i = 0; i < parts.length; i++) {
    var p = parts[i];
    if (p === '' || p === '.') continue;
    if (p === '..') { segs.pop(); continue; }
    segs.push(p);
  }
  var normalized = segs.join('/');

  // 在 flatNodes 中匹配
  function find(relPath) {
    for (var i = 0; i < flatNodes.length; i++) {
      if (flatNodes[i].relPath === relPath) return flatNodes[i];
    }
    return null;
  }
  var hit = find(normalized);
  if (hit) return hit;
  // 允许不带 .md 后缀
  if (!IMAGE_EXT.test(normalized) && !/\.[a-z0-9]+$/i.test(normalized)) {
    hit = find(normalized + '.md');
    if (hit) return hit;
  }
  return null;
}

// 展开目录链并定位
function expandDirById(id) {
  var el = document.getElementById(id);
  if (!el) return;
  var cur = el;
  while (cur && cur !== document.body) {
    if (cur.classList && cur.classList.contains('dir-content')) {
      if (cur.style.display === 'none') {
        var arrow = document.getElementById('arrow-' + cur.id);
        cur.style.display = 'block';
        if (arrow) arrow.classList.add('open');
      }
    }
    cur = cur.parentElement;
  }
  var header = document.querySelector('.dir-header');
  // 滚动到该目录
  var target = el.previousElementSibling || el;
  if (target.scrollIntoView) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// 轻量提示
function toast(msg) {
  var t = document.getElementById('toast');
  if (!t) return;
  t.textContent = msg;
  t.classList.add('show');
  clearTimeout(toast._timer);
  toast._timer = setTimeout(function() { t.classList.remove('show'); }, 2500);
}

// 链接点击事件委托
document.addEventListener('click', function(e) {
  var a = e.target.closest && e.target.closest('a[data-link]');
  if (!a) return;
  e.preventDefault();
  var link = a.getAttribute('data-link');
  var target = resolveLink(link, currentFilePath);
  if (!target) { toast('目标文件不存在: ' + link); return; }
  if (target.type === 'file') {
    showFile(target.id);
  } else {
    expandDirById(target.id);
  }
});

function toggleDir(id) {
  var el = document.getElementById(id);
  var arrow = document.getElementById('arrow-' + id);
  if (el.style.display === 'none') {
    el.style.display = 'block'; arrow.classList.add('open');
  } else {
    el.style.display = 'none'; arrow.classList.remove('open');
  }
}

function filterTree(query) {
  query = query.toLowerCase().trim();
  document.querySelectorAll('.file-item').forEach(function(el) {
    var name = el.querySelector('.fname').textContent.toLowerCase();
    el.style.display = (!query || name.includes(query)) ? 'flex' : 'none';
  });
  if (query) {
    document.querySelectorAll('.dir-content').forEach(function(el) { el.style.display = 'block'; });
    document.querySelectorAll('.arrow').forEach(function(el) { el.classList.add('open'); });
  }
}

function esc(s) {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

// 防复制和防截屏保护
function initProtection(config) {
  config = config || {};
  var enableCopyProtection = !config.copyEnabled;
  var enablePageProtection = config.protectionEnabled !== false;

  // ============ 立即锁定函数 ============
  function lockPage() {
    if (!enablePageProtection) return;
    document.body.innerHTML = '<div style="display:flex;align-items:center;justify-content:center;height:100vh;font-size:20px;color:#e74c3c;background:#fff">⚠️ 检测到异常,页面已锁定</div>';
    throw new Error('DevTools detected');
  }

  // ============ 检测是否为真实移动设备 ============
  function isRealMobileDevice() {
    var ua = navigator.userAgent.toLowerCase();
    var isMobileUA = /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/i.test(ua);
    var hasTouchScreen = navigator.maxTouchPoints > 0 || 'ontouchstart' in window;
    var isSmallScreen = window.screen.width < 768 || window.screen.height < 768;
    return isMobileUA && hasTouchScreen && isSmallScreen;
  }

  // 如果是真实移动设备，跳过开发者工具检测（但仍保留防复制）
  var skipDevtoolsCheck = !enablePageProtection || isRealMobileDevice();

  // ============ 方法1: 元素 getter 检测（最可靠）============
  var element = new Image();
  Object.defineProperty(element, 'id', {
    get: function() {
      if (!skipDevtoolsCheck) lockPage();
    }
  });

  // ============ 方法2: 窗口尺寸差异检测 ============
  function checkWindowSize() {
    var threshold = 160;
    var widthDiff = Math.abs(window.outerWidth - window.innerWidth);
    var heightDiff = Math.abs(window.outerHeight - window.innerHeight);
    return widthDiff > threshold || heightDiff > threshold;
  }

  // ============ 方法3: 独立窗口开发者工具检测 ============
  // 当开发者工具作为独立窗口打开时，window.outerWidth === window.innerWidth
  // 但我们可以通过检测 window.screen 和 window 的关系来判断
  function checkDetachedDevtools() {
    // 如果浏览器窗口比屏幕小很多，可能是独立开发者工具窗口
    var screenArea = window.screen.width * window.screen.height;
    var windowArea = window.outerWidth * window.outerHeight;
    // 如果窗口面积小于屏幕面积的40%，可能是开发工具窗口
    if (windowArea < screenArea * 0.4 && windowArea < 800000) {
      return true;
    }
    return false;
  }

  // ============ 方法4: console 时间检测 ============
  function checkConsoleTiming() {
    var start = performance.now();
    for (var i = 0; i < 100; i++) {
      console.log('%c ', 'font-size:0');
    }
    console.clear();
    var end = performance.now();
    return (end - start) > 50;
  }

  // ============ 方法5: 检测全局变量 ============
  function checkDevtoolsGlobals() {
    return !!(
      window.Firebug ||
      window.__BROWSERTOOLS_CONSOLE ||
      window.__REACT_DEVTOOLS_GLOBAL_HOOK__ ||
      window.__VUE_DEVTOOLS_GLOBAL_HOOK__ ||
      window.__REDUX_DEVTOOLS_EXTENSION__ ||
      window.angular ||
      (typeof ng !== 'undefined')
    );
  }

  // ============ 方法6: debugger 时间检测 ============
  function checkDebugger() {
    var start = performance.now();
    try {
      // 使用 Function 构造器动态创建 debugger
      var fn = new Function('debugger');
      fn();
    } catch(e) {}
    var end = performance.now();
    return (end - start) > 100;
  }

  // ============ 综合检测函数 ============
  function detectDevtools() {
    if (skipDevtoolsCheck) return false;

    // 方法1的 console 检测
    console.log('%c', element);
    console.clear();

    // 其他方法
    if (checkWindowSize()) return true;
    if (checkDetachedDevtools()) return true;
    if (checkConsoleTiming()) return true;
    if (checkDevtoolsGlobals()) return true;
    if (checkDebugger()) return true;

    return false;
  }

  // ============ 立即执行检测（在 initProtection 调用时）============
  if (!skipDevtoolsCheck) {
    console.log('%c', element);
    console.clear();

    if (checkWindowSize() || checkDetachedDevtools() || checkDevtoolsGlobals()) {
      lockPage();
    }
  }

  // ============ 禁止右键和快捷键 ============
  if (enableCopyProtection) {
    document.addEventListener('contextmenu', function(e) { e.preventDefault(); });
    document.addEventListener('copy', function(e) { e.preventDefault(); });
    document.addEventListener('cut', function(e) { e.preventDefault(); });
    document.addEventListener('selectstart', function(e) { e.preventDefault(); });
    document.addEventListener('keydown', function(e) {
      if ((e.ctrlKey && (e.key === 'c' || e.key === 'x' || e.key === 'a' || e.key === 's' || e.key === 'p')) ||
          e.key === 'F12' || (e.ctrlKey && e.shiftKey && (e.key === 'I' || e.key === 'J' || e.key === 'C')) ||
          (e.ctrlKey && e.key === 'U')) {
        e.preventDefault();
        return false;
      }
    });
    document.addEventListener('dragstart', function(e) { e.preventDefault(); });
  }

  // ============ 持续检测（高频）============
  var devtools = { open: false };

  function continuousCheck() {
    if (skipDevtoolsCheck) return;

    console.log('%c', element);
    console.clear();

    if (detectDevtools()) {
      if (!devtools.open) {
        devtools.open = true;
        lockPage();
      }
    }
  }

  // 立即检测
  setTimeout(continuousCheck, 0);
  setTimeout(continuousCheck, 100);
  setTimeout(continuousCheck, 300);
  setTimeout(continuousCheck, 500);
  setTimeout(continuousCheck, 1000);
  setTimeout(continuousCheck, 2000);

  // 持续定时检测
  setInterval(continuousCheck, 500);

  // ============ 监听页面可见性变化 ============
  document.addEventListener('visibilitychange', function() {
    if (!document.hidden) {
      // 页面重新可见时立即检测
      setTimeout(continuousCheck, 0);
    }
  });

  // ============ 禁止文字选择样式 ============
  var style = document.createElement('style');
  if (enableCopyProtection) {
    style.textContent = '::selection { background: transparent; } ::-moz-selection { background: transparent; }';
    document.head.appendChild(style);
  }

  // ============ 加载水印 ============
  fetch(apiBase + '/watermark-config')
    .then(function(r) { return r.json(); })
    .then(function(config) {
      if (!config.enabled) return;

      function createWatermark() {
        var container = document.createElement('div');
        container.id = 'wm-' + Math.random().toString(36).substr(2, 9);
        container.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;pointer-events:none;z-index:9999;overflow:hidden';

        for (var i = 0; i < config.count; i++) {
          var mark = document.createElement('div');
          var x = (i % config.gridColumns) * (100 / config.gridColumns);
          var y = Math.floor(i / config.gridColumns) * (100 / Math.ceil(config.count / config.gridColumns));
          var rotation = config.rotations[i % config.rotations.length];
          var color = config.colors[i % config.colors.length];

          mark.style.cssText = 'position:absolute;left:' + x + '%;top:' + y + '%;' +
            'transform:rotate(' + rotation + 'deg);font-size:' + config.fontSize + 'px;color:' + color + ';' +
            'white-space:nowrap;user-select:none;font-family:' + config.fontFamily + ';letter-spacing:' + config.letterSpacing + 'px';
          mark.textContent = config.text;
          container.appendChild(mark);
        }

        document.body.appendChild(container);
        return container;
      }

      var wmElement = createWatermark();

      var observer = new MutationObserver(function(mutations) {
        if (!document.getElementById(wmElement.id)) {
          wmElement = createWatermark();
        }
      });
      observer.observe(document.body, { childList: true, subtree: true });

      setInterval(function() {
        if (!document.getElementById(wmElement.id) || wmElement.children.length < config.count) {
          if (wmElement.parentNode) wmElement.parentNode.removeChild(wmElement);
          wmElement = createWatermark();
        }
      }, config.checkInterval);
    });
}
</script>
</body>
</html>
""";
}
