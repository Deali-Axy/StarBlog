import { useEffect, useState } from "react";
import { Form, Image, Input, Modal, Select, Space, message } from "antd";
import { apiBase, request } from "./api";

type CoverPost = { id: string; title: string; coverUrl?: string | null; coverHidden?: boolean; coverFit?: string; coverImages?: { url: string; alt: string }[]; imageCandidates?: { url: string; alt: string }[] };

export function PostCoverEditor({ post, onClose, onSaved }: { post: CoverPost; onClose: () => void; onSaved: () => void }) {
  const [mode, setMode] = useState(post.coverHidden ? "hidden" : post.coverUrl ? "manual" : "auto");
  const [url, setUrl] = useState(post.coverUrl ?? "");
  const [fit, setFit] = useState(post.coverFit ?? "contain");
  const [candidates, setCandidates] = useState(post.coverImages ?? []);
  const [busy, setBusy] = useState(false);
  useEffect(() => {
    let current = true;
    // Detail responses include body candidates even when the chosen illustration is hidden or manual.
    request<CoverPost>(`/api/v1/posts/${post.id}?includeToc=false`).then(value => { if (current) setCandidates(value.imageCandidates ?? value.coverImages ?? []); }).catch(() => {});
    return () => { current = false; };
  }, [post.id]);
  async function save() {
    if (mode === "manual" && !url.trim()) { message.error("请选择或上传一张图片，或填写图片地址"); return; }
    setBusy(true);
    try {
      await request(`/api/v1/posts/${post.id}/cover`, { method: "PUT", body: JSON.stringify({ url: mode === "manual" ? url.trim() : null, hidden: mode === "hidden", fit }) });
      message.success("文章配图已保存"); onSaved(); onClose();
    } catch (error) { message.error(error instanceof Error ? error.message : "保存失败"); }
    finally { setBusy(false); }
  }
  async function upload(file?: File) {
    if (!file) return;
    setBusy(true);
    try {
      const form = new FormData(); form.append("file", file);
      const result = await request<{ url: string }>(`/api/v1/posts/${post.id}/images`, { method: "POST", body: form });
      setUrl(result.url); setMode("manual");
    } catch (error) { message.error(error instanceof Error ? error.message : "上传失败"); }
    finally { setBusy(false); }
  }
  const preview = mode === "hidden" ? "" : mode === "manual" ? url.trim() : candidates[0]?.url;
  const previewUrl = preview?.startsWith("/") ? `${apiBase}${preview}` : preview;
  return <Modal title={`文章配图 · ${post.title}`} open onCancel={onClose} onOk={() => void save()} confirmLoading={busy} okButtonProps={{ disabled: busy }} cancelButtonProps={{ disabled: busy }} width={640}>
    <Form layout="vertical">
      <Form.Item label="显示方式"><Select value={mode} onChange={setMode} options={[{ value: "auto", label: "自动从正文选图" }, { value: "manual", label: "指定图片" }, { value: "hidden", label: "不显示配图" }]} /></Form.Item>
      {mode === "manual" && <>
        <Form.Item label="图片地址"><Input value={url} onChange={event => setUrl(event.target.value)} placeholder="正文图片地址、站内路径或 https://…" /></Form.Item>
        <Form.Item label="上传图片"><Input type="file" accept="image/*" disabled={busy} onChange={event => void upload(event.target.files?.[0])} /></Form.Item>
      </>}
      {mode !== "hidden" && <Form.Item label="单图适配（拼图使用裁切）"><Select value={fit} onChange={setFit} options={[{ value: "contain", label: "完整显示（适合截图、流程图）" }, { value: "cover", label: "填满裁切（适合照片）" }]} /></Form.Item>}
      {candidates.length > 1 && mode !== "hidden" && <Form.Item label="正文候选图"><Space wrap>{candidates.map((image, index) => <button type="button" key={image.url} onClick={() => { setMode("manual"); setUrl(image.url); }} aria-label={`选择正文图片 ${index + 1}`} style={{ width: 96, padding: 4, background: "transparent", border: url === image.url && mode === "manual" ? "2px solid #1677ff" : "1px solid #d9d9d9", borderRadius: 6, cursor: "pointer" }}><img src={image.url.startsWith("/") ? `${apiBase}${image.url}` : image.url} alt={image.alt} loading="lazy" style={{ width: 84, height: 60, objectFit: "contain", display: "block" }} /><span>{index + 1}. {image.alt.slice(0, 7) || "正文图片"}</span></button>)}</Space></Form.Item>}
      {previewUrl ? <Image src={previewUrl} alt="文章配图预览" style={{ width: "100%", height: 240, objectFit: fit === "cover" ? "cover" : "contain", background: "#f5f5f5" }} /> : <p>{mode === "hidden" ? "这篇文章以纯文字排版显示。" : "没有合适的正文配图时，以纯文字排版显示。"}</p>}
    </Form>
  </Modal>;
}
