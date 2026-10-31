import type { CSSProperties } from "react"
import type { HomeBaseColors, HomeColorMode, HomeResolvedConfig, HomeThemeSettings } from "@/lib/home-variant"

type Palette = Record<HomeColorMode, HomeBaseColors & { card: string; muted: string; mutedForeground: string; border: string; accent: string }>
export const HOME_PALETTE_COLORS: Record<HomeThemeSettings["palette"], Palette> = {
  paper: {
    light: { background: "#f6f3ec", foreground: "#292720", primary: "#a34e36", card: "#fbf9f4", muted: "#eae6dc", mutedForeground: "#726b60", border: "#dcd5c8", accent: "#eae3d7" },
    dark: { background: "#191b1a", foreground: "#eee9df", primary: "#e7a17e", card: "#222522", muted: "#292c28", mutedForeground: "#b0ab9f", border: "#3b3e36", accent: "#32342c" },
  },
  slate: {
    light: { background: "#f0f3f7", foreground: "#243249", primary: "#3f5e9b", card: "#fafcff", muted: "#e1e7ef", mutedForeground: "#5c6b80", border: "#cbd4e1", accent: "#dde6f5" },
    dark: { background: "#171d29", foreground: "#e7edf7", primary: "#a2bdf1", card: "#202a3a", muted: "#2b3547", mutedForeground: "#a8b6cc", border: "#3b4960", accent: "#33435f" },
  },
  forest: {
    light: { background: "#edf1e8", foreground: "#243c30", primary: "#3b684d", card: "#f7faf3", muted: "#dde5d6", mutedForeground: "#5d7160", border: "#c6d3bf", accent: "#d5e4cc" },
    dark: { background: "#16221c", foreground: "#e6efdf", primary: "#b0d19b", card: "#213027", muted: "#2d3d30", mutedForeground: "#a5b99f", border: "#405542", accent: "#354d36" },
  },
  ink: {
    light: { background: "#f3f3f0", foreground: "#252525", primary: "#343434", card: "#ffffff", muted: "#e6e6e1", mutedForeground: "#686862", border: "#d0d0c9", accent: "#dfdfd9" },
    dark: { background: "#151515", foreground: "#eeeeea", primary: "#e0e0d4", card: "#202020", muted: "#2b2b29", mutedForeground: "#aeaea4", border: "#40403b", accent: "#353530" },
  },
}

function mix(a: string, b: string, ratio: number) {
  const channels = [1, 3, 5].map(offset => Math.round(parseInt(a.slice(offset, offset + 2), 16) * (1 - ratio) + parseInt(b.slice(offset, offset + 2), 16) * ratio))
  return `#${channels.map(channel => channel.toString(16).padStart(2, "0")).join("")}`
}
function luminance(hex: string) {
  return [1, 3, 5].map(offset => parseInt(hex.slice(offset, offset + 2), 16) / 255).map(v => v <= 0.04045 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4).reduce((sum, v, i) => sum + v * [0.2126, 0.7152, 0.0722][i], 0)
}
export function homeColors(theme: HomeThemeSettings, mode: HomeColorMode) {
  const palette = HOME_PALETTE_COLORS[theme.palette][mode]
  const colors = { ...palette, ...theme.colors[mode] }
  if (Object.keys(theme.colors[mode]).length) {
    colors.card = mix(colors.background, colors.foreground, 0.025)
    colors.muted = mix(colors.background, colors.foreground, 0.07)
    colors.mutedForeground = mix(colors.background, colors.foreground, 0.68)
    colors.border = mix(colors.background, colors.foreground, 0.2)
    colors.accent = mix(colors.background, colors.primary, 0.13)
  }
  const l = luminance(colors.primary)
  return { ...colors, primaryForeground: (l + 0.05) / 0.05 > 1.05 / (l + 0.05) ? "#000000" : "#ffffff" }
}

/** Runtime values only. Both modes are emitted so the root theme initializes before hydration. */
export function homeThemeStyle(config: HomeResolvedConfig): CSSProperties {
  const style: Record<string, string | number> = {
    "--home-width": `${{ narrow: 56, standard: 72, wide: 88 }[config.contentWidth]}rem`,
    "--home-density": { compact: 0.85, standard: 1, relaxed: 1.2 }[config.theme.density],
    "--home-scale": config.theme.fontScale / 100,
    "--home-heading-font": config.theme.typography === "modern" ? 'var(--font-sans, "Noto Sans SC"), "PingFang SC", sans-serif' : 'var(--font-newsreader, "Newsreader"), var(--font-serif-sc, "Noto Serif SC"), "Songti SC", serif',
    "--home-border-style": config.theme.border === "none" ? "solid" : config.theme.border,
  }
  for (const mode of ["light", "dark"] as const) {
    const colors = homeColors(config.theme, mode)
    for (const [key, value] of Object.entries(colors)) {
      const name = key.replace(/[A-Z]/g, letter => `-${letter.toLowerCase()}`)
      style[`--home-${name}-${mode}`] = value
    }
  }
  return style as CSSProperties
}
