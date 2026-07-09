static class AdminTemplate
{
    public const string Page = """
<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>DocShowcase 管理后台</title>
<style>
  * { box-sizing: border-box; }
  body {
    margin: 0; min-height: 100vh; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Microsoft YaHei", sans-serif;
    color: #172033; background: #f4f7fb;
  }
  .admin-lock {
    position: fixed; inset: 0; z-index: 30; background: #f4f7fb; display: none; align-items: center; justify-content: center; padding: 20px;
  }
  .admin-lock.show { display: flex; }
  .admin-login-box {
    width: min(420px, 100%); background: #fff; border: 1px solid #e2e8f0; border-radius: 8px;
    box-shadow: 0 18px 50px rgba(15, 23, 42, .12); padding: 28px;
  }
  .admin-login-box h1 { margin: 0 0 8px; font-size: 22px; }
  .admin-login-box p { margin: 0 0 20px; color: #64748b; font-size: 14px; }
  .admin-error { min-height: 20px; color: #dc2626; font-size: 13px; margin-top: 10px; }
  .layout { display: grid; grid-template-columns: 248px 1fr; min-height: 100vh; }
  .nav {
    background: #111827; color: #d7dee9; padding: 22px 16px; display: flex; flex-direction: column; gap: 18px;
  }
  .brand { color: #fff; font-size: 20px; font-weight: 800; letter-spacing: .2px; }
  .brand small { display: block; color: #9ca3af; font-size: 12px; font-weight: 500; margin-top: 6px; }
  .nav button {
    width: 100%; border: 0; border-radius: 8px; padding: 11px 12px; text-align: left; cursor: pointer;
    background: transparent; color: #cbd5e1; font-size: 14px;
  }
  .nav button.active, .nav button:hover { background: #243044; color: #fff; }
  .main { padding: 28px; overflow: auto; }
  .topbar { display: flex; justify-content: space-between; align-items: center; gap: 16px; margin-bottom: 22px; }
  .topbar h1 { margin: 0; font-size: 26px; color: #101827; }
  .topbar p { margin: 6px 0 0; color: #64748b; font-size: 14px; }
  .btn {
    border: 0; border-radius: 8px; padding: 10px 14px; cursor: pointer; background: #2563eb; color: #fff;
    font-weight: 700; font-size: 14px;
  }
  .btn.secondary { background: #e2e8f0; color: #172033; }
  .btn.danger { background: #dc2626; }
  .btn.ghost { background: transparent; color: #2563eb; padding: 6px 8px; }
  .grid { display: grid; grid-template-columns: minmax(320px, 420px) 1fr; gap: 18px; align-items: start; }
  .panel {
    background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; box-shadow: 0 8px 24px rgba(15, 23, 42, .06);
  }
  .panel-header { padding: 16px 18px; border-bottom: 1px solid #e2e8f0; display: flex; justify-content: space-between; align-items: center; gap: 12px; }
  .panel-header h2 { margin: 0; font-size: 16px; }
  .panel-body { padding: 18px; }
  .share-list { display: flex; flex-direction: column; gap: 10px; }
  .share-item {
    border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; cursor: pointer; background: #fff;
  }
  .share-item.active { border-color: #2563eb; box-shadow: 0 0 0 2px rgba(37, 99, 235, .12); }
  .share-title { display: flex; justify-content: space-between; gap: 10px; align-items: center; font-weight: 800; }
  .share-path { color: #64748b; font-size: 12px; margin-top: 7px; word-break: break-all; }
  .badge { border-radius: 999px; padding: 3px 8px; font-size: 12px; font-weight: 800; }
  .badge.on { color: #166534; background: #dcfce7; }
  .badge.off { color: #991b1b; background: #fee2e2; }
  .actions { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 10px; }
  .form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 14px; }
  .field { display: flex; flex-direction: column; gap: 7px; }
  .field.full { grid-column: 1 / -1; }
  label { color: #475569; font-size: 12px; font-weight: 800; }
  input, textarea, select {
    width: 100%; border: 1px solid #cbd5e1; border-radius: 8px; padding: 10px 11px; font: inherit; background: #fff;
  }
  textarea { min-height: 82px; resize: vertical; }
  .path-row { display: grid; grid-template-columns: 1fr auto; gap: 8px; }
  .switches { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px; }
  .switch {
    border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; display: flex; justify-content: space-between; gap: 12px; align-items: center;
  }
  .switch strong { display: block; font-size: 14px; }
  .switch span { display: block; font-size: 12px; color: #64748b; margin-top: 3px; }
  .switch input { width: 18px; height: 18px; }
  .empty { color: #64748b; text-align: center; padding: 32px 12px; }
  .section { display: none; }
  .section.active { display: block; }
  .toast { position: fixed; right: 22px; bottom: 22px; background: #111827; color: #fff; padding: 12px 14px; border-radius: 8px; display: none; z-index: 20; }
  .modal { position: fixed; inset: 0; background: rgba(15,23,42,.55); display: none; align-items: center; justify-content: center; padding: 20px; z-index: 10; }
  .modal.show { display: flex; }
  .modal-box { background: #fff; width: min(780px, 96vw); max-height: 86vh; border-radius: 8px; display: flex; flex-direction: column; }
  .modal-head { padding: 16px 18px; border-bottom: 1px solid #e2e8f0; display: flex; justify-content: space-between; gap: 12px; align-items: center; }
  .modal-body { padding: 16px 18px; overflow: auto; }
  .dir-row { display: flex; justify-content: space-between; gap: 12px; align-items: center; padding: 10px 0; border-bottom: 1px solid #eef2f7; }
  .dir-name { font-weight: 700; word-break: break-all; }
  .dir-path { font-size: 12px; color: #64748b; word-break: break-all; margin-top: 3px; }
  .log-toolbar {
    display: grid; grid-template-columns: 180px 1fr 180px auto; gap: 10px; align-items: end; margin: 18px 0;
  }
  .log-stats { display: flex; flex-wrap: wrap; gap: 10px; margin: 12px 0; }
  .log-stat { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 9px 11px; font-size: 13px; }
  .log-card { border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; margin-bottom: 10px; background: #fff; }
  .log-card-head { display: flex; justify-content: space-between; gap: 12px; align-items: center; }
  .log-event { font-weight: 800; color: #172033; }
  .log-time { color: #64748b; font-size: 12px; white-space: nowrap; }
  .log-meta { color: #475569; font-size: 12px; margin-top: 7px; word-break: break-all; }
  .log-detail { color: #172033; font-size: 13px; margin-top: 8px; word-break: break-all; }
  @media (max-width: 900px) {
    .layout { grid-template-columns: 1fr; }
    .nav { position: sticky; top: 0; z-index: 2; }
    .grid, .form-grid, .switches, .log-toolbar { grid-template-columns: 1fr; }
  }
</style>
</head>
<body>
<div class="admin-lock" id="adminLock">
  <div class="admin-login-box">
    <h1>管理后台登录</h1>
    <p>请输入管理员密码后继续配置共享目录和访问策略。</p>
    <div class="field">
      <label>管理员密码</label>
      <input id="adminPasswordInput" type="password" autocomplete="current-password" placeholder="请输入管理员密码">
    </div>
    <button class="btn" style="width:100%;margin-top:14px" onclick="adminLogin()">登录</button>
    <div class="admin-error" id="adminLoginError"></div>
  </div>
</div>
<div class="layout">
  <aside class="nav">
    <div class="brand">DocShowcase<small>共享文档管理后台</small></div>
    <div>
      <button class="active" data-tab="shares" onclick="showTab('shares')">共享文档</button>
      <button data-tab="settings" onclick="showTab('settings')">功能设置</button>
      <button data-tab="watermark" onclick="showTab('watermark')">水印设置</button>
      <button data-tab="logs" onclick="showTab('logs')">运行信息</button>
    </div>
  </aside>
  <main class="main">
    <div class="topbar">
      <div>
        <h1>共享文档控制台</h1>
        <p>添加服务器目录，生成独立访问链接，并为每个共享单独控制登录、水印、加密和复制策略。</p>
      </div>
      <button class="btn" onclick="newShare()">增加共享文档</button>
    </div>

    <section id="tab-shares" class="section active">
      <div class="grid">
        <div class="panel">
          <div class="panel-header"><h2>共享列表</h2><button class="btn secondary" onclick="loadConfig()">刷新</button></div>
          <div class="panel-body"><div id="shareList" class="share-list"></div></div>
        </div>
        <div class="panel">
          <div class="panel-header"><h2>共享信息</h2><button class="btn" onclick="saveSelected()">保存</button></div>
          <div class="panel-body" id="shareEditor"></div>
        </div>
      </div>
    </section>

    <section id="tab-settings" class="section">
      <div class="panel">
        <div class="panel-header"><h2>功能设置</h2><button class="btn" onclick="saveSelected()">保存</button></div>
        <div class="panel-body" id="featureEditor"></div>
      </div>
    </section>

    <section id="tab-watermark" class="section">
      <div class="panel">
        <div class="panel-header"><h2>水印设置</h2><button class="btn" onclick="saveSelected()">保存</button></div>
        <div class="panel-body" id="watermarkEditor"></div>
      </div>
    </section>

    <section id="tab-logs" class="section">
      <div class="panel">
        <div class="panel-header"><h2>运行信息</h2><button class="btn secondary" onclick="loadLogs()">刷新日志</button></div>
        <div class="panel-body">
          <div id="summary"></div>
          <div class="log-toolbar">
            <div class="field">
              <label>IP 筛选</label>
              <input id="logIpFilter" placeholder="例如 127.0.0.1" oninput="renderLogs()">
            </div>
            <div class="field">
              <label>关键词</label>
              <input id="logKeywordFilter" placeholder="文件名、目录、共享名称、请求路径" oninput="renderLogs()">
            </div>
            <div class="field">
              <label>事件类型</label>
              <select id="logEventFilter" onchange="renderLogs()">
                <option value="">全部事件</option>
                <option value="login_success">登录成功</option>
                <option value="login_failed">登录失败</option>
                <option value="view_tree">查看文件树</option>
                <option value="view_file">查看文件</option>
              </select>
            </div>
            <button class="btn secondary" onclick="clearLogFilters()">清空筛选</button>
          </div>
          <div id="logStats" class="log-stats"></div>
          <div id="logList"></div>
        </div>
      </div>
    </section>
  </main>
</div>

<div class="modal" id="dirModal">
  <div class="modal-box">
    <div class="modal-head">
      <strong>选择服务器目录</strong>
      <button class="btn secondary" onclick="closeDirPicker()">关闭</button>
    </div>
    <div class="modal-body">
      <div class="path-row">
        <input id="browsePath" placeholder="输入目录路径或从列表选择">
        <button class="btn secondary" onclick="browsePath(document.getElementById('browsePath').value)">打开</button>
      </div>
      <div class="actions">
        <button class="btn" onclick="useCurrentPath()">使用当前目录</button>
        <button class="btn secondary" onclick="browsePath('')">查看磁盘</button>
        <button class="btn secondary" id="upBtn" onclick="goParent()">上一级</button>
      </div>
      <div id="dirList"></div>
    </div>
  </div>
</div>

<div class="toast" id="toast"></div>

<script>
let state = null;
let selectedId = null;
let pickerTarget = null;
let currentBrowse = { currentPath: '', parentPath: null, directories: [] };
let allLogs = [];

function api(url, options) {
  return fetch(url, options).then(async function(r) {
    var text = await r.text();
    var data = text ? JSON.parse(text) : {};
    if (r.status === 401) {
      showAdminLogin();
      throw new Error(data.message || '请先登录管理后台');
    }
    if (!r.ok) throw new Error(data.message || '请求失败');
    return data;
  });
}

function showAdminLogin() {
  document.getElementById('adminLock').classList.add('show');
  setTimeout(function() { document.getElementById('adminPasswordInput').focus(); }, 0);
}

function hideAdminLogin() {
  document.getElementById('adminLock').classList.remove('show');
  document.getElementById('adminLoginError').textContent = '';
}

async function adminLogin() {
  var input = document.getElementById('adminPasswordInput');
  var error = document.getElementById('adminLoginError');
  error.textContent = '';
  try {
    await fetch('/api/admin/auth', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password: input.value })
    }).then(async function(r) {
      var data = await r.json();
      if (!r.ok || !data.success) throw new Error(data.message || '登录失败');
    });
    hideAdminLogin();
    await loadConfig();
  } catch (err) {
    error.textContent = err.message;
  }
}

function esc(s) {
  return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function toast(message) {
  var el = document.getElementById('toast');
  el.textContent = message;
  el.style.display = 'block';
  clearTimeout(window.__toastTimer);
  window.__toastTimer = setTimeout(function() { el.style.display = 'none'; }, 2200);
}

function showTab(tab) {
  document.querySelectorAll('.section').forEach(function(el) { el.classList.remove('active'); });
  document.querySelectorAll('.nav button').forEach(function(el) { el.classList.toggle('active', el.dataset.tab === tab); });
  document.getElementById('tab-' + tab).classList.add('active');
  if (tab === 'logs') loadLogs();
}

async function loadConfig() {
  state = await api('/api/admin/config');
  if (!selectedId && state.shares.length) selectedId = state.shares[0].id;
  if (selectedId && !state.shares.some(function(s) { return s.id === selectedId; })) {
    selectedId = state.shares.length ? state.shares[0].id : null;
  }
  render();
}

function selectedShare() {
  if (!state || !selectedId) return null;
  return state.shares.find(function(s) { return s.id === selectedId; }) || null;
}

function render() {
  renderShares();
  renderEditors();
  renderSummary();
}

function renderShares() {
  var list = document.getElementById('shareList');
  if (!state.shares.length) {
    list.innerHTML = '<div class="empty">还没有共享目录，点击右上角添加。</div>';
    return;
  }

  list.innerHTML = state.shares.map(function(s) {
    var link = location.origin + '/s/' + s.id;
    return '<div class="share-item ' + (s.id === selectedId ? 'active' : '') + '" onclick="selectShare(\'' + esc(s.id) + '\')">'
      + '<div class="share-title"><span>' + esc(s.name) + '</span><span class="badge ' + (s.enabled ? 'on' : 'off') + '">' + (s.enabled ? '运行中' : '已关闭') + '</span></div>'
      + '<div class="share-path">' + esc(s.path) + '</div>'
      + '<div class="actions">'
      + '<button class="btn ghost" onclick="event.stopPropagation(); window.open(\'' + link + '\', \'_blank\')">打开</button>'
      + '<button class="btn ghost" onclick="event.stopPropagation(); copyText(\'' + link + '\')">复制链接</button>'
      + '<button class="btn ghost" onclick="event.stopPropagation(); toggleShare(\'' + esc(s.id) + '\',' + (!s.enabled) + ')">' + (s.enabled ? '关闭' : '启动') + '</button>'
      + '</div></div>';
  }).join('');
}

function selectShare(id) {
  selectedId = id;
  render();
}

function renderEditors() {
  var share = selectedShare();
  if (!share) {
    var empty = '<div class="empty">请选择或新增一个共享目录。</div>';
    document.getElementById('shareEditor').innerHTML = empty;
    document.getElementById('featureEditor').innerHTML = empty;
    document.getElementById('watermarkEditor').innerHTML = empty;
    return;
  }

  document.getElementById('shareEditor').innerHTML =
    '<div class="form-grid">'
    + field('名称', 'shareName', share.name)
    + '<div class="field"><label>状态</label><select id="shareEnabled"><option value="true">启动</option><option value="false">关闭</option></select></div>'
    + '<div class="field full"><label>目录路径</label><div class="path-row"><input id="sharePath" value="' + esc(share.path) + '"><button class="btn secondary" onclick="openDirPicker(\'sharePath\')">选择</button></div></div>'
    + '<div class="field full"><label>访问链接</label><div class="path-row"><input readonly value="' + esc(location.origin + '/s/' + share.id) + '"><button class="btn secondary" onclick="copyText(\'' + esc(location.origin + '/s/' + share.id) + '\')">复制</button></div></div>'
    + '</div><div class="actions"><button class="btn danger" onclick="deleteSelected()">删除共享</button></div>';
  document.getElementById('shareEnabled').value = String(share.enabled);

  document.getElementById('featureEditor').innerHTML =
    '<div class="switches">'
    + sw('loginEnabled', '启用登录', '访问共享前需要输入密码', share.settings.loginEnabled)
    + sw('encryptionEnabled', '启用传输加密', '文件树和内容接口返回加密数据', share.settings.encryptionEnabled)
    + sw('copyEnabled', '允许复制', '允许选择文字、复制和右键菜单', share.settings.copyEnabled)
    + sw('protectionEnabled', '启用页面保护', '拦截打印、快捷键和开发工具检测', share.settings.protectionEnabled)
    + sw('watermarkEnabled', '启用水印', '水印总开关，可叠加水印自身开关', share.settings.watermarkEnabled)
    + '</div><div class="form-grid" style="margin-top:16px">'
    + field('访问密码', 'accessPassword', share.settings.accessPassword)
    + field('管理员密码', 'adminPassword', share.settings.adminPassword)
    + '</div>';

  var w = share.settings.watermark;
  document.getElementById('watermarkEditor').innerHTML =
    '<div class="switches">' + sw('wmEnabled', '水印自身开关', '关闭后仅当前共享不显示水印', w.enabled) + '</div>'
    + '<div class="form-grid" style="margin-top:16px">'
    + field('水印文本', 'wmText', w.text, true)
    + numField('数量', 'wmCount', w.count)
    + numField('字号', 'wmFontSize', w.fontSize)
    + field('字体', 'wmFontFamily', w.fontFamily)
    + numField('字距', 'wmLetterSpacing', w.letterSpacing)
    + numField('网格列数', 'wmGridColumns', w.gridColumns)
    + numField('检查间隔(ms)', 'wmCheckInterval', w.checkInterval)
    + textArea('颜色，每行一个 CSS 颜色', 'wmColors', (w.colors || []).join('\\n'))
    + field('旋转角度，逗号分隔', 'wmRotations', (w.rotations || []).join(', '))
    + '</div>';
}

function field(label, id, value, full) {
  return '<div class="field ' + (full ? 'full' : '') + '"><label>' + label + '</label><input id="' + id + '" value="' + esc(value) + '"></div>';
}

function numField(label, id, value) {
  return '<div class="field"><label>' + label + '</label><input id="' + id + '" type="number" value="' + esc(value) + '"></div>';
}

function textArea(label, id, value) {
  return '<div class="field full"><label>' + label + '</label><textarea id="' + id + '">' + esc(value) + '</textarea></div>';
}

function sw(id, title, desc, checked) {
  return '<label class="switch"><span><strong>' + title + '</strong><span>' + desc + '</span></span><input id="' + id + '" type="checkbox" ' + (checked ? 'checked' : '') + '></label>';
}

function readSettings() {
  return {
    loginEnabled: document.getElementById('loginEnabled').checked,
    watermarkEnabled: document.getElementById('watermarkEnabled').checked,
    encryptionEnabled: document.getElementById('encryptionEnabled').checked,
    copyEnabled: document.getElementById('copyEnabled').checked,
    protectionEnabled: document.getElementById('protectionEnabled').checked,
    accessPassword: document.getElementById('accessPassword').value,
    adminPassword: document.getElementById('adminPassword').value,
    watermark: {
      enabled: document.getElementById('wmEnabled').checked,
      text: document.getElementById('wmText').value,
      count: Number(document.getElementById('wmCount').value || 0),
      fontSize: Number(document.getElementById('wmFontSize').value || 12),
      fontFamily: document.getElementById('wmFontFamily').value,
      letterSpacing: Number(document.getElementById('wmLetterSpacing').value || 0),
      colors: document.getElementById('wmColors').value.split(/\\r?\\n/).map(function(x) { return x.trim(); }).filter(Boolean),
      rotations: document.getElementById('wmRotations').value.split(',').map(function(x) { return Number(x.trim()); }).filter(function(x) { return !Number.isNaN(x); }),
      gridColumns: Number(document.getElementById('wmGridColumns').value || 1),
      checkInterval: Number(document.getElementById('wmCheckInterval').value || 2000)
    }
  };
}

async function saveSelected() {
  var share = selectedShare();
  if (!share) return toast('没有选中的共享');
  await api('/api/admin/shares/' + encodeURIComponent(share.id), {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: document.getElementById('shareName').value,
      path: document.getElementById('sharePath').value,
      enabled: document.getElementById('shareEnabled').value === 'true',
      settings: readSettings()
    })
  });
  toast('已保存');
  await loadConfig();
}

async function newShare() {
  selectedId = null;
  pickerTarget = '__newShare';
  document.getElementById('dirModal').classList.add('show');
  await browsePath('');
  toast('请选择要共享的目录');
}

async function toggleShare(id, enabled) {
  await api('/api/admin/shares/' + encodeURIComponent(id), {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ enabled: enabled })
  });
  toast(enabled ? '已启动共享' : '已关闭共享');
  await loadConfig();
}

async function deleteSelected() {
  var share = selectedShare();
  if (!share || !confirm('确定删除共享 "' + share.name + '"？不会删除原目录文件。')) return;
  await api('/api/admin/shares/' + encodeURIComponent(share.id), { method: 'DELETE' });
  selectedId = null;
  toast('已删除共享');
  await loadConfig();
}

function copyText(text) {
  navigator.clipboard.writeText(text).then(function() { toast('已复制'); }, function() { toast(text); });
}

function renderSummary() {
  if (!state) return;
  var running = state.shares.filter(function(s) { return s.enabled; }).length;
  document.getElementById('summary').innerHTML =
    '<p><strong>共享总数：</strong>' + state.shares.length + '</p>'
    + '<p><strong>运行中：</strong>' + running + '</p>'
    + '<p><strong>配置存储：</strong>程序输出目录下的 <code>docshowcase.config.json</code></p>';
}

async function loadLogs() {
  var target = document.getElementById('logList');
  if (!target) return;
  target.innerHTML = '<div class="empty">正在加载访问日志...</div>';
  try {
    allLogs = await api('/api/admin/logs') || [];
    renderLogs();
  } catch (err) {
    target.innerHTML = '<div class="empty">' + esc(err.message) + '</div>';
  }
}

function renderLogs() {
  var target = document.getElementById('logList');
  var stats = document.getElementById('logStats');
  if (!target || !stats) return;

  var ip = (document.getElementById('logIpFilter').value || '').toLowerCase().trim();
  var keyword = (document.getElementById('logKeywordFilter').value || '').toLowerCase().trim();
  var eventType = document.getElementById('logEventFilter').value || '';
  var eventNames = {
    login_success: '登录成功',
    login_failed: '登录失败',
    view_tree: '查看文件树',
    view_file: '查看文件'
  };

  var filtered = allLogs.filter(function(log) {
    var ipOk = !ip || String(log.ipAddress || '').toLowerCase().includes(ip);
    var eventOk = !eventType || log.eventType === eventType;
    var haystack = [
      log.eventType,
      eventNames[log.eventType],
      log.ipAddress,
      log.details,
      log.requestPath,
      log.queryString,
      log.method,
      log.browser,
      log.operatingSystem,
      log.device,
      log.userAgent
    ].join(' ').toLowerCase();
    var keywordOk = !keyword || haystack.includes(keyword);
    return ipOk && eventOk && keywordOk;
  }).reverse();

  var shown = filtered.slice(0, 300);
  var uniqueIps = new Set(filtered.map(function(log) { return log.ipAddress || ''; }).filter(Boolean)).size;
  var fileViews = filtered.filter(function(log) { return log.eventType === 'view_file'; }).length;
  var failedLogins = filtered.filter(function(log) { return log.eventType === 'login_failed'; }).length;
  stats.innerHTML =
    '<span class="log-stat">匹配记录：<strong>' + filtered.length + '</strong></span>'
    + '<span class="log-stat">展示上限：<strong>' + shown.length + '/300</strong></span>'
    + '<span class="log-stat">独立 IP：<strong>' + uniqueIps + '</strong></span>'
    + '<span class="log-stat">文件访问：<strong>' + fileViews + '</strong></span>'
    + '<span class="log-stat">登录失败：<strong>' + failedLogins + '</strong></span>';

  if (!shown.length) {
    target.innerHTML = '<div class="empty">没有符合筛选条件的访问日志。</div>';
    return;
  }

  target.innerHTML = shown.map(function(log) {
    var time = new Date(log.timestamp).toLocaleString('zh-CN');
    var request = (log.method || '') + ' ' + (log.requestPath || '') + (log.queryString || '');
    return '<div class="log-card">'
      + '<div class="log-card-head"><div class="log-event">' + esc(eventNames[log.eventType] || log.eventType) + '</div><div class="log-time">' + esc(time) + '</div></div>'
      + '<div class="log-meta">IP: ' + esc(log.ipAddress || '') + (log.remotePort ? ':' + esc(log.remotePort) : '') + ' · ' + esc(log.device || '') + ' · ' + esc(log.operatingSystem || '') + ' · ' + esc(log.browser || '') + '</div>'
      + '<div class="log-meta">请求: ' + esc(request) + '</div>'
      + (log.details ? '<div class="log-detail">' + esc(log.details) + '</div>' : '')
      + (log.referer ? '<div class="log-meta">来源: ' + esc(log.referer) + '</div>' : '')
      + '</div>';
  }).join('');
}

function clearLogFilters() {
  document.getElementById('logIpFilter').value = '';
  document.getElementById('logKeywordFilter').value = '';
  document.getElementById('logEventFilter').value = '';
  renderLogs();
}

function openDirPicker(targetId) {
  pickerTarget = targetId;
  document.getElementById('dirModal').classList.add('show');
  browsePath(document.getElementById(targetId).value || '');
}

function closeDirPicker() {
  document.getElementById('dirModal').classList.remove('show');
}

async function browsePath(path) {
  currentBrowse = await api('/api/admin/fs' + (path ? '?path=' + encodeURIComponent(path) : ''));
  document.getElementById('browsePath').value = currentBrowse.currentPath || '';
  document.getElementById('upBtn').disabled = !currentBrowse.parentPath;
  document.getElementById('dirList').innerHTML = currentBrowse.directories.length
    ? currentBrowse.directories.map(function(d) {
        return '<div class="dir-row"><div><div class="dir-name">' + esc(d.name) + '</div><div class="dir-path">' + esc(d.path) + '</div></div>'
          + '<div class="actions"><button class="btn secondary" data-path="' + esc(d.path) + '" onclick="browsePath(this.dataset.path)">打开</button><button class="btn" data-path="' + esc(d.path) + '" onclick="choosePath(this.dataset.path)">选择</button></div></div>';
      }).join('')
    : '<div class="empty">没有可进入的子目录。</div>';
}

function goParent() {
  if (currentBrowse.parentPath) browsePath(currentBrowse.parentPath);
}

async function choosePath(path) {
  if (pickerTarget === '__newShare') {
    closeDirPicker();
    var defaultName = path.split(/[\\\\/]/).filter(Boolean).pop() || '文档共享';
    var name = prompt('共享名称（可留空使用目录名）', defaultName) || defaultName;
    var share = await api('/api/admin/shares', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: name, path: path })
    });
    selectedId = share.id;
    pickerTarget = null;
    toast('已添加共享');
    await loadConfig();
    return;
  }

  if (pickerTarget) document.getElementById(pickerTarget).value = path;
  closeDirPicker();
}

function useCurrentPath() {
  if (currentBrowse.currentPath) choosePath(currentBrowse.currentPath);
}

document.getElementById('adminPasswordInput').addEventListener('keydown', function(e) {
  if (e.key === 'Enter') adminLogin();
});

loadConfig().catch(function(err) {
  toast(err.message);
});
</script>
</body>
</html>
""";
}
