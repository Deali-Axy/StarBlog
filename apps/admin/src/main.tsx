import React, { useEffect, useMemo, useState } from "react";
import ReactDOM from "react-dom/client";
import { App as AntApp, Button, Card, ConfigProvider, Drawer, Form, Input, Layout, Menu, Modal, Select, Space, Spin, Table, Tag, Typography, message } from "antd";
import { ApartmentOutlined, BookOutlined, CommentOutlined, LinkOutlined, LogoutOutlined, PictureOutlined, RocketOutlined, SettingOutlined } from "@ant-design/icons";
import { Refine, Authenticated } from "@refinedev/core";
import routerProvider, { CatchAllNavigate, NavigateToResource } from "@refinedev/react-router";
import { BrowserRouter, Navigate, Outlet, Route, Routes, useLocation, useNavigate } from "react-router-dom";
import { apiBase, platformNames, request } from "./api";
import "./styles.css";

type RecordRow = Record<string, unknown> & { id: string };
type Channel = { id: string; name: string; platform: number; enabled: boolean; appId?: string; hasSecret: boolean; author?: string; theme?: string; publishUrl?: string };
type Publication = { id: string; channelId: string; platform: number; status: number; title: string; renderedContent: string; coverUrl?: string; externalId?: string; externalUrl?: string; errorMessage?: string };

const resources = [
  { name: "posts", meta: { label: "文章" } }, { name: "categories", meta: { label: "分类" } },
  { name: "photos", meta: { label: "摄影" } }, { name: "comments", meta: { label: "评论" } },
  { name: "links", meta: { label: "友链" } }, { name: "config", meta: { label: "站点设置" } },
  { name: "publishing", meta: { label: "发布中继" } },
];

/** Refine 数据提供器使资源语义和路由保持一致；页面使用它背后的同一 API 契约。 */
const dataProvider = {
  getList: async ({ resource }: { resource: string }) => ({ data: await request<RecordRow[]>(`/api/v1/${resource}`), total: 0 }),
  getOne: async () => ({ data: {} }), create: async () => ({ data: {} }), update: async () => ({ data: {} }), deleteOne: async () => ({ data: {} }), getApiUrl: () => apiBase,
} as never;

const authProvider = {
  login: async ({ username, password }: { username: string; password: string }) => {
    const result = await request<{ token: string }>("/api/v1/auth/tokens", { method: "POST", body: JSON.stringify({ username, password }) });
    localStorage.setItem("starblog_token", result.token); return { success: true };
  },
  logout: async () => { localStorage.removeItem("starblog_token"); return { success: true, redirectTo: "/login" }; },
  check: async () => localStorage.getItem("starblog_token") ? { authenticated: true } : { authenticated: false, redirectTo: "/login" },
  getPermissions: async () => null, getIdentity: async () => ({ id: "admin", name: "管理员" }), onError: async () => ({ logout: false }),
} as never;

type InitializationState = { isInitialized: boolean; host?: string; defaultRender?: string };
type LoginValues = { username: string; password: string };
type InitializationValues = LoginValues & { confirmPassword: string; host: string; defaultRender: string };

function Login() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [initialization, setInitialization] = useState<InitializationState | null>(null);

  // 登录页加载时查询 SQLite 用户表状态；没有管理员时直接展示一次性初始化表单。
  useEffect(() => {
    void request<InitializationState>("/api/v1/site/initialization")
      .then(setInitialization)
      .catch((error) => {
        message.error(error instanceof Error ? error.message : "无法读取初始化状态");
        // API 暂时不可达时仍展示登录表单，方便已有站点在服务恢复后直接重试。
        setInitialization({ isInitialized: true });
      });
  }, []);

  const login = async (values: LoginValues) => {
    await (authProvider as { login: (value: LoginValues) => Promise<unknown> }).login(values);
    navigate("/");
  };

  const submitLogin = async (values: LoginValues) => {
    setLoading(true);
    try { await login(values); }
    catch (error) { message.error(error instanceof Error ? error.message : "登录失败"); }
    finally { setLoading(false); }
  };

  const submitInitialization = async (values: InitializationValues) => {
    setLoading(true);
    try {
      await request("/api/v1/site/initialization", {
        method: "POST",
        body: JSON.stringify({ username: values.username, password: values.password, host: values.host, defaultRender: values.defaultRender }),
      });
      message.success("管理员已写入 SQLite，正在登录");
      await login(values);
    } catch (error) {
      message.error(error instanceof Error ? error.message : "初始化失败");
    } finally { setLoading(false); }
  };

  const form = initialization === null
    ? <div className="login-loading"><Spin /><span>检查站点状态…</span></div>
    : initialization.isInitialized
      ? <><p className="eyebrow">SIGN IN</p><Typography.Title level={2}>进入控制台</Typography.Title><Form layout="vertical" onFinish={submitLogin}><Form.Item name="username" label="用户名" rules={[{ required: true }]}><Input autoFocus autoComplete="username" /></Form.Item><Form.Item name="password" label="密码" rules={[{ required: true }]}><Input.Password autoComplete="current-password" /></Form.Item><Button htmlType="submit" type="primary" loading={loading} block>登录并继续</Button></Form></>
      : <><p className="eyebrow">FIRST RUN</p><Typography.Title level={2}>创建首个管理员</Typography.Title><p className="setup-hint">账号将直接写入 StarBlog 的 SQLite 数据库；完成后该入口会自动关闭。</p><Form layout="vertical" onFinish={submitInitialization} initialValues={{ host: window.location.origin, defaultRender: "frontend" }}><Form.Item name="username" label="管理员用户名" rules={[{ required: true }, { min: 3 }]}><Input autoFocus autoComplete="username" /></Form.Item><Form.Item name="password" label="管理员密码" rules={[{ required: true }, { min: 8 }]}><Input.Password autoComplete="new-password" /></Form.Item><Form.Item name="confirmPassword" label="确认密码" dependencies={["password"]} rules={[{ required: true }, ({ getFieldValue }) => ({ validator(_, value) { return !value || getFieldValue("password") === value ? Promise.resolve() : Promise.reject(new Error("两次输入的密码不一致")); } })]}><Input.Password autoComplete="new-password" /></Form.Item><Form.Item name="host" label="站点地址" rules={[{ required: true }, { type: "url" }]}><Input placeholder="https://blog.example.com" /></Form.Item><Form.Item name="defaultRender" hidden><Input /></Form.Item><Button htmlType="submit" type="primary" loading={loading} block>初始化并进入后台</Button></Form></>;

  return <main className="login-shell"><section className="login-manifesto"><div className="constellation">✦</div><p className="eyebrow">STARLOG / EDITORIAL SYSTEM</p><h1>写完一篇，<br />让它抵达更多地方。</h1><p>管理内容、渠道与每一次投递，不让发布成为写作的最后一道阻力。</p></section><Card className="login-card" variant="borderless">{form}</Card></main>;
}

function Shell() {
  const navigate = useNavigate(); const location = useLocation();
  const items = [{ key: "/posts", icon: <BookOutlined />, label: "文章库" }, { key: "/categories", icon: <ApartmentOutlined />, label: "分类" }, { key: "/photos", icon: <PictureOutlined />, label: "摄影" }, { key: "/comments", icon: <CommentOutlined />, label: "评论审核" }, { key: "/links", icon: <LinkOutlined />, label: "友链" }, { key: "/publishing", icon: <RocketOutlined />, label: "发布中继" }, { key: "/config", icon: <SettingOutlined />, label: "站点设置" }];
  return <Layout className="app-shell"><Layout.Sider width={252} className="sider"><div className="brand"><span>✦</span><div>STAR<br /><b>BLOG</b></div></div><p className="workspace">EDITORIAL DESK</p><Menu theme="dark" mode="inline" selectedKeys={[location.pathname]} items={items} onClick={({ key }) => navigate(key)} /><Button type="text" className="logout" icon={<LogoutOutlined />} onClick={() => { localStorage.removeItem("starblog_token"); navigate("/login"); }}>退出登录</Button></Layout.Sider><Layout><Layout.Header className="topbar"><span className="status-dot" /> 内容与发布控制台 <span>UTC+8 · {new Date().toLocaleDateString("zh-CN")}</span></Layout.Header><Layout.Content className="content"><Outlet /></Layout.Content></Layout></Layout>;
}

function Dashboard() { return <section className="dashboard"><p className="eyebrow">TODAY'S DESK</p><h2>文章先被读懂，才值得被投递。</h2><div className="dashboard-grid"><Card><b>01</b><h3>管理原文</h3><p>在文章库中编辑、筛选与检查发布状态。</p></Card><Card><b>02</b><h3>生成快照</h3><p>选择渠道后冻结一份可审核的内容版本。</p></Card><Card><b>03</b><h3>确认投递</h3><p>公众号发往草稿箱；其他平台交给官方编辑器。</p></Card></div></section>; }

type ResourceConfig = { title: string; endpoint: string; route: string; columns: string[]; idKey?: string; createTemplate?: Record<string, unknown>; readOnly?: boolean };

/**
 * 每个资源都显式声明后台 API 路径与最小创建模板。
 * 编辑器允许保留未在表格中呈现的字段，使管理端不会在保存时意外丢弃内容数据。
 */
const resourceConfig: Record<string, ResourceConfig> = {
  posts: { title: "文章库", endpoint: "/api/v1/posts?page=1&pageSize=100", route: "/api/v1/posts", columns: ["title", "slug", "status", "isPublish", "lastUpdateTime"], createTemplate: { title: "未命名文章", slug: "", status: "", summary: "", content: "", categoryId: 1, zipEncoding: "utf-8" } },
  categories: { title: "分类", endpoint: "/api/v1/categories?page=1&pageSize=100", route: "/api/v1/categories", columns: ["id", "name", "parentId", "visible"], createTemplate: { name: "新分类", parentId: 0, visible: true } },
  photos: { title: "摄影", endpoint: "/api/v1/photos?page=1&pageSize=100", route: "/api/v1/photos", columns: ["title", "location", "width", "height", "createTime"], createTemplate: { title: "", location: "" } },
  comments: { title: "评论审核", endpoint: "/api/v1/admin/comments?page=1&pageSize=100", route: "/api/v1/admin/comments", columns: ["content", "postId", "visible", "isNeedAudit", "creationTime"], readOnly: true },
  links: { title: "友链", endpoint: "/api/v1/admin/links", route: "/api/v1/admin/links", columns: ["name", "description", "url", "visible"], createTemplate: { name: "", description: "", url: "https://", visible: true } },
  config: { title: "站点设置", endpoint: "/api/v1/admin/settings", route: "/api/v1/admin/settings", idKey: "key", columns: ["key", "value", "description"], createTemplate: { key: "", value: "", description: "" } },
};

function valueCell(value: unknown) { if (typeof value === "boolean") return <Tag color={value ? "green" : "default"}>{value ? "是" : "否"}</Tag>; if (typeof value === "string" && value.length > 96) return `${value.slice(0, 96)}…`; return String(value ?? "—"); }

function ResourcePage({ resource }: { resource: string }) {
  const config = resourceConfig[resource];
  const identifier = (row: RecordRow) => String(row[config.idKey ?? "id"] ?? "");
  const [data, setData] = useState<RecordRow[]>([]); const [busy, setBusy] = useState(true); const [selected, setSelected] = useState<RecordRow | null>(null);
  const [editing, setEditing] = useState<RecordRow | null | "new">(null); const [json, setJson] = useState(""); const [saving, setSaving] = useState(false); const [photoFile, setPhotoFile] = useState<File | null>(null);
  const load = async () => { setBusy(true); try { const result = await request<RecordRow[] | { items?: RecordRow[]; data?: RecordRow[] }>(config.endpoint); setData(Array.isArray(result) ? result : result.items ?? result.data ?? []); } catch (error) { message.error(error instanceof Error ? error.message : "读取失败"); } finally { setBusy(false); } };
  useEffect(() => { void load(); }, [resource]);
  const openEditor = (row: RecordRow | "new") => { setEditing(row); setPhotoFile(null); setJson(JSON.stringify(row === "new" ? config.createTemplate : row, null, 2)); };
  const save = async () => {
    try {
      setSaving(true); const body = JSON.parse(json) as Record<string, unknown>;
      if (resource === "photos" && editing === "new") {
        if (!photoFile) throw new Error("请先选择图片文件");
        const form = new FormData(); form.append("title", String(body.title ?? "")); form.append("location", String(body.location ?? "")); form.append("file", photoFile);
        const token = localStorage.getItem("starblog_token"); const response = await fetch(`${apiBase}${config.route}`, { method: "POST", headers: token ? { Authorization: `Bearer ${token}` } : {}, body: form });
        if (!response.ok) throw new Error("图片上传失败");
      } else {
        const isNew = editing === "new"; const id = isNew ? "" : `/${identifier(editing as RecordRow)}`;
        await request(`${config.route}${id}`, { method: isNew ? "POST" : "PUT", body: JSON.stringify(body) });
      }
      message.success("已保存"); setEditing(null); await load();
    } catch (error) { message.error(error instanceof Error ? error.message : "保存失败"); } finally { setSaving(false); }
  };
  const remove = (row: RecordRow) => Modal.confirm({ title: "确认删除？", content: "该操作不能撤销。", okButtonProps: { danger: true }, onOk: async () => { await request(`${config.route}/${identifier(row)}`, { method: "DELETE" }); message.success("已删除"); await load(); } });
  const commentAction = async (row: RecordRow, action: "approval" | "rejection") => { try { await request(`${config.route}/${identifier(row)}/${action}`, { method: "PATCH", body: JSON.stringify({ reason: "由管理后台审核" }) }); message.success(action === "approval" ? "评论已通过" : "评论已拒绝"); await load(); } catch (error) { message.error(error instanceof Error ? error.message : "审核失败"); } };
  const postAction = async (row: RecordRow, action: "featured-post" | "top-placement" | "translations") => { try { await request(`/api/v1/posts/${identifier(row)}/${action}`, { method: action === "top-placement" ? "PUT" : "POST" }); message.success(action === "translations" ? "已提交英文翻译" : "文章状态已更新"); } catch (error) { message.error(error instanceof Error ? error.message : "操作失败"); } };
  const columns = [...config.columns.map((key) => ({ title: key, dataIndex: key, key, render: valueCell })), { title: "操作", key: "action", render: (_: unknown, record: RecordRow) => <Space size="small"><Button type="link" onClick={() => setSelected(record)}>查看</Button>{!config.readOnly && <Button type="link" onClick={() => openEditor(record)}>编辑</Button>}{!config.readOnly && <Button danger type="link" onClick={() => remove(record)}>删除</Button>}{resource === "comments" && <><Button type="link" onClick={() => void commentAction(record, "approval")}>通过</Button><Button danger type="link" onClick={() => void commentAction(record, "rejection")}>拒绝</Button></>}{resource === "posts" && <><Button type="link" onClick={() => void postAction(record, "featured-post")}>推荐</Button><Button type="link" onClick={() => void postAction(record, "top-placement")}>置顶</Button><Button type="link" onClick={() => void postAction(record, "translations")}>翻译</Button></>}</Space> }];
  return <section className="page"><div className="page-title"><div><p className="eyebrow">RESOURCE / {resource.toUpperCase()}</p><h2>{config.title}</h2></div><Space>{!config.readOnly && <Button type="primary" onClick={() => openEditor("new")}>新增</Button>}<Button onClick={() => void load()}>刷新</Button></Space></div><Card className="paper-card"><Table rowKey={config.idKey ?? "id"} loading={busy} dataSource={data} pagination={{ pageSize: 12 }} columns={columns} scroll={{ x: true }} /></Card><Drawer title="资源详情" width={560} open={!!selected} onClose={() => setSelected(null)}><pre className="json-view">{JSON.stringify(selected, null, 2)}</pre></Drawer><Modal title={editing === "new" ? `新增${config.title}` : `编辑${config.title}`} open={editing !== null} onCancel={() => setEditing(null)} onOk={() => void save()} confirmLoading={saving} width={760}><p className="form-help">以 JSON 编辑 API 支持的字段；未知字段会由服务端忽略。图片创建需额外选择上传文件。</p>{resource === "photos" && editing === "new" && <Input type="file" accept="image/*" onChange={(event) => setPhotoFile(event.target.files?.[0] ?? null)} />}<Input.TextArea value={json} onChange={(event) => setJson(event.target.value)} rows={18} className="json-editor" /></Modal></section>;
}

function PublishingStudio() {
  const [channels, setChannels] = useState<Channel[]>([]); const [posts, setPosts] = useState<RecordRow[]>([]); const [active, setActive] = useState<Publication | null>(null); const [form] = Form.useForm(); const [publishing, setPublishing] = useState(false);
  const load = async () => { try { const [channelData, postData] = await Promise.all([request<Channel[]>("/api/v1/admin/publication-channels"), request<RecordRow[] | { items?: RecordRow[]; data?: RecordRow[] }>("/api/v1/posts?page=1&pageSize=100")]); setChannels(channelData); setPosts(Array.isArray(postData) ? postData : postData.items ?? postData.data ?? []); } catch (error) { message.error(error instanceof Error ? error.message : "加载发布工作台失败"); } };
  useEffect(() => { void load(); }, []);
  const saveChannel = async (values: Record<string, unknown>) => { try { await request("/api/v1/admin/publication-channels", { method: "POST", body: JSON.stringify({ ...values, platform: Number(values.platform) }) }); message.success("渠道已保存"); form.resetFields(); await load(); } catch (error) { message.error(error instanceof Error ? error.message : "保存失败"); } };
  const prepare = async (postId: string, channelId: string) => { try { const item = await request<Publication>(`/api/v1/admin/posts/${postId}/publications`, { method: "POST", body: JSON.stringify({ channelId }) }); setActive(item); message.success("已生成发布快照"); } catch (error) { message.error(error instanceof Error ? error.message : "生成快照失败"); } };
  const publish = async () => { if (!active) return; setPublishing(true); try { const item = await request<Publication>(`/api/v1/admin/publications/${active.id}/deliveries`, { method: "POST" }); setActive(item); if (item.platform !== 1) { await navigator.clipboard.writeText(item.renderedContent); if (item.externalUrl) window.open(item.externalUrl, "_blank", "noopener,noreferrer"); message.success("内容已复制，并打开平台编辑器"); } else message.success("已投递到公众号草稿箱"); } catch (error) { message.error(error instanceof Error ? error.message : "投递失败"); } finally { setPublishing(false); } };
  return <section className="page publishing"><div className="page-title"><div><p className="eyebrow">SYNDICATION / CONTROL ROOM</p><h2>发布中继</h2></div></div><div className="publish-grid"><Card className="paper-card" title="渠道配置"><Form form={form} layout="vertical" onFinish={saveChannel} initialValues={{ platform: 1, enabled: true, theme: "native" }}><Form.Item name="name" label="渠道名称" rules={[{ required: true }]}><Input placeholder="例如：StarBlog 公众号" /></Form.Item><Form.Item name="platform" label="平台" rules={[{ required: true }]}><Select options={Object.entries(platformNames).map(([value, label]) => ({ value: Number(value), label }))} /></Form.Item><Form.Item name="appId" label="AppId（公众号必填）"><Input /></Form.Item><Form.Item name="appSecret" label="AppSecret（只写入，不回显）"><Input.Password /></Form.Item><Form.Item name="author" label="作者"><Input /></Form.Item><Form.Item name="theme" label="公众号主题"><Select options={[{ value: "native", label: "清新绿" }, { value: "ink", label: "墨蓝" }]} /></Form.Item><Button htmlType="submit" type="primary">保存渠道</Button></Form><div className="channel-list">{channels.map((channel) => <div key={channel.id}><Tag color={channel.enabled ? "green" : "default"}>{platformNames[channel.platform]}</Tag><b>{channel.name}</b><small>{channel.hasSecret ? "已配置密钥" : "未配置密钥"}</small></div>)}</div></Card><Card className="paper-card" title="选择文章与渠道"><Table size="small" rowKey="id" dataSource={posts} pagination={{ pageSize: 7 }} columns={[{ title: "文章", dataIndex: "title" }, { title: "渠道", render: (_, post) => <Select placeholder="选择渠道" style={{ minWidth: 160 }} options={channels.filter((item) => item.enabled).map((item) => ({ value: item.id, label: `${platformNames[item.platform]} · ${item.name}` }))} onChange={(channelId) => void prepare(post.id, channelId)} /> }]} /></Card></div><Card className="preview-card" title="发布快照">{active ? <><Space><Tag color={active.status === 3 ? "green" : "gold"}>{active.status === 3 ? "已投递" : "待投递"}</Tag><span>{platformNames[active.platform]}</span></Space><h3>{active.title}</h3>{active.errorMessage && <p className="publish-note">{active.errorMessage}</p>}<pre className="content-snapshot">{active.renderedContent}</pre><Button type="primary" icon={<RocketOutlined />} loading={publishing} onClick={() => void publish()}>{active.platform === 1 ? "投递公众号草稿箱" : "复制内容并打开编辑器"}</Button></> : <div className="empty-state">从右侧选一篇文章和一个渠道，先生成可审核的内容快照。</div>}</Card></section>;
}

function App() { return <Refine dataProvider={dataProvider} authProvider={authProvider} routerProvider={routerProvider} resources={resources} options={{ syncWithLocation: true, warnWhenUnsavedChanges: true }}><Routes><Route path="/login" element={<Login />} /><Route element={<Authenticated key="admin-area" fallback={<Navigate to="/login" replace />}><Shell /></Authenticated>}><Route index element={<Dashboard />} /><Route path="posts" element={<ResourcePage resource="posts" />} /><Route path="categories" element={<ResourcePage resource="categories" />} /><Route path="photos" element={<ResourcePage resource="photos" />} /><Route path="comments" element={<ResourcePage resource="comments" />} /><Route path="links" element={<ResourcePage resource="links" />} /><Route path="config" element={<ResourcePage resource="config" />} /><Route path="publishing" element={<PublishingStudio />} /></Route><Route path="*" element={<CatchAllNavigate to="/" />} /></Routes></Refine>; }

ReactDOM.createRoot(document.getElementById("root")!).render(<React.StrictMode><ConfigProvider theme={{ token: { colorPrimary: "#16a085", borderRadius: 10, fontFamily: "Inter, 'Microsoft YaHei', sans-serif" } }}><AntApp><BrowserRouter><App /></BrowserRouter></AntApp></ConfigProvider></React.StrictMode>);
