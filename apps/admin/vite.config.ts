import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

/**
 * 开发代理必须和 Api 项目的 launchSettings.json 默认端口保持一致。
 * 通过 VITE_API_PROXY_TARGET 允许部署者覆盖本地地址，例如连接 Docker 或远程开发 API。
 * 生产构建不依赖此代理，而是由 VITE_API_URL 提供 API 根地址。
 */
export default defineConfig(({ mode }) => {
  // Vite 会以本管理端目录作为工作目录；使用 "." 避免为了读取环境变量额外引入 Node 类型声明。
  const env = loadEnv(mode, ".", "");
  const apiProxyTarget = env.VITE_API_PROXY_TARGET || "http://localhost:5039";

  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: { "/Api": { target: apiProxyTarget, changeOrigin: true } },
    },
  };
});
