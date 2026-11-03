"use client"
import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogPortal, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { homeColors, homeThemeStyle } from "@/lib/home-theme"
import { HOME_VARIANT_OPTIONS, homeDesignHref, homeSettingOptions, homeSettingValue, type HomeBaseColors, type HomeColorMode, type HomeSettingPath } from "@/lib/home-variant"
import { cn } from "@/lib/utils"
import { Copy, Palette, RotateCcw, X } from "lucide-react"
import { useState } from "react"
import themeStyles from "./home-design.module.css"
type Choice = {
  readonly id: string
  readonly labelZh: string
  readonly labelEn: string
}
const actionClass = "inline-flex min-h-11 items-center justify-center gap-2 rounded-lg px-3 text-xs hover:bg-accent disabled:opacity-40"
function FieldHeader({ path, label }: {
  path: HomeSettingPath
  label: string
}) {
  const { t } = useI18n()
  const { preferences, setSetting } = useHomeDesign()
  const customized = homeSettingValue(preferences.overrides, path) !== undefined
  return <div className="mb-2 flex flex-wrap items-center justify-between gap-1">
    <span className="flex items-center gap-2 text-xs font-medium">{label}{customized ? <span className="text-[10px] font-normal text-primary">{t.homeCustomized}</span> : null}</span>
    <button type="button" disabled={!customized} onClick={() => setSetting(path)} className="min-h-11 px-2 text-[11px] text-muted-foreground hover:text-primary disabled:opacity-30" aria-label={`${label}: ${t.homeRestore}`}><RotateCcw size={12} className="mr-1 inline" aria-hidden="true" />{t.homeRestore}</button>
  </div>
}
function ChoiceField({ path, label, onLinked }: {
  path: keyof typeof homeSettingOptions
  label: string
  onLinked: () => void
}) {
  const { locale } = useI18n()
  const { config, setSetting } = useHomeDesign()
  return <fieldset className="mb-4" data-home-setting={path}>
    <legend className="sr-only">{label}</legend>
    <FieldHeader path={path} label={label} />
    <div className="grid grid-cols-2 gap-2">
      {(homeSettingOptions[path] as readonly Choice[]).map(option => <label key={option.id} className={cn("flex min-h-11 items-center gap-[.5rem] border border-border rounded-[.5rem] [padding:.65rem_.75rem] [text-align:start] text-[.8125rem] [&:is([aria-checked='true'],_[data-checked='true'])]:bg-accent [&:is([aria-checked='true'],_[data-checked='true'])]:[border-color:var(--primary)] [&:is([aria-checked='true'],_[data-checked='true'])]:text-primary [&_input]:[accent-color:var(--primary)]", "cursor-pointer")} data-checked={homeSettingValue(config, path) === option.id}>
        <input type="radio" name={path} value={option.id} checked={homeSettingValue(config, path) === option.id} onChange={() => {
          setSetting(path, option.id); if(path === "frame" || path === "navigation")
            onLinked()
        }} />
        <span>{locale === "zh" ? option.labelZh : option.labelEn}</span>
      </label>)}
    </div>
  </fieldset>
}
function ColorField({ mode, color, label }: {
  mode: HomeColorMode
  color: keyof HomeBaseColors
  label: string
}) {
  const { config, setSetting } = useHomeDesign()
  const path = `theme.colors.${mode}.${color}` as const
  const value = homeColors(config.theme, mode)[color]
  return <div className="mb-2" data-home-setting={path}>
    <FieldHeader path={path} label={label} />
    <div className="flex items-center gap-3 rounded-lg border border-border p-2">
      <input type="color" aria-label={`${label} (${mode})`} value={value} onChange={event => setSetting(path, event.target.value)} className="size-11 cursor-pointer border-0 bg-transparent" />
      <input key={value} type="text" defaultValue={value} aria-label={`${label} HEX (${mode})`} pattern="#[0-9A-Fa-f]{6}" maxLength={7} spellCheck={false} className="min-h-11 min-w-0 flex-1 bg-transparent font-mono text-sm uppercase outline-none" onBlur={event => {
        if(/^#[0-9a-f]{6}$/i.test(event.target.value))
          setSetting(path, event.target.value)
        else
          event.target.value = value
      }} onKeyDown={event => {
        if(event.key === "Enter")
          event.currentTarget.blur()
      }} />
    </div>
  </div>
}
export function HomeDesignSwitcher() {
  const { t, locale } = useI18n()
  const design = useHomeDesign()
  const [tab, setTab] = useState("preset")
  const [colorMode, setColorMode] = useState<HomeColorMode>("light")
  const [notice, setNotice] = useState("")
  const [shareStatus, setShareStatus] = useState("")
  const [shareFallback, setShareFallback] = useState("")
  const groups = [{ id: "preset", label: t.homeGroupPreset }, { id: "appearance", label: t.homeGroupAppearance }, { id: "layout", label: t.homeGroupLayout }, { id: "chrome", label: t.homeGroupChrome }, { id: "sections", label: t.homeGroupSections }]
  function openChange(open: boolean) {
    setNotice("")
    setShareStatus("")
    setShareFallback("")
    if(open) {
      setColorMode(design.effectiveMode === "dark" ? "dark" : "light")
      design.begin()
    }
    else
      design.cancel()
  }
  async function share() {
    const url = new URL(homeDesignHref(window.location.href, design.preferences), window.location.origin).href
    try {
      await navigator.clipboard.writeText(url)
      setShareStatus(t.homeCopied)
      setShareFallback("")
    }
    catch {
      setShareStatus(t.homeCopyFallback)
      setShareFallback(url)
    }
  }
  const field = (path: keyof typeof homeSettingOptions, label: string) => <ChoiceField path={path} label={label} onLinked={() => setNotice(t.homeLinkedLayout)} />
  return <Dialog modal={false} open={design.open} onOpenChange={openChange}>
    <DialogTrigger asChild><button type="button" data-home-design-trigger aria-label={t.homeCustomize} title={t.homeCustomize} className="fixed right-[max(20px,env(safe-area-inset-right))] bottom-[calc(24px+env(safe-area-inset-bottom))] z-45 grid size-12 cursor-pointer place-items-center rounded-full border border-primary bg-card text-primary shadow-lg hover:bg-accent"><Palette size={20} aria-hidden="true" /></button></DialogTrigger>
    <DialogPortal>
      <DialogContent data-home-design-menu data-home-mode={design.previewMode} className={cn(themeStyles.theme, "fixed [z-index:60] [right:max(1rem,_env(safe-area-inset-right))] [bottom:calc(5.5rem_+_env(safe-area-inset-bottom))] flex flex-col w-[27rem] [max-width:calc(100vw_-_2rem)] [max-height:calc(100dvh_-_8rem)] border border-border rounded-[1rem] [box-shadow:0_14px_60px_#0003] bg-card [&_button]:cursor-pointer [&_button:disabled]:[cursor:default] [&_button:disabled]:[opacity:.4] max-[39.999rem]:right-0 max-[39.999rem]:bottom-0 max-[39.999rem]:w-full max-[39.999rem]:max-w-full max-[39.999rem]:max-h-[75dvh] max-[39.999rem]:[border-radius:1rem_1rem_0_0] max-[39.999rem]:pb-[env(safe-area-inset-bottom)] motion-reduce:[&_*]:[scroll-behavior:auto]! motion-reduce:[&_*]:[animation:none]! motion-reduce:[&_*]:[transition:none]!")} style={homeThemeStyle(design.config)} aria-describedby="home-customize-description" onInteractOutside={event => event.preventDefault()}>
        <div className="flex shrink-0 items-start justify-between gap-3 px-5 pt-5 pb-3">
          <div><DialogTitle className="font-heading text-2xl">{t.homeCustomize}</DialogTitle><DialogDescription id="home-customize-description" className="mt-2 text-xs leading-6 text-muted-foreground">{t.homePreviewHint}</DialogDescription></div>
          <DialogClose asChild><button type="button" aria-label={t.close} className="grid size-11 shrink-0 place-items-center rounded-lg hover:bg-accent"><X size={18} aria-hidden="true" /></button></DialogClose>
        </div>
        <Tabs value={tab} onValueChange={setTab} className="flex min-h-0 flex-1 flex-col">
          <TabsList className={"flex gap-[.25rem] [padding:.5rem_1rem] [border-block:1px_solid_var(--border)] [&_button]:[flex:1] [&_button]:min-h-11 [&_button]:[padding:.5rem_.25rem] [&_button]:text-[.75rem] [&_button]:rounded-[.4rem] [&_button[data-state='active']]:text-primary [&_button[data-state='active']]:bg-accent"} aria-label={t.homeCustomize}>{groups.map(group => <TabsTrigger key={group.id} value={group.id}>{group.label}</TabsTrigger>)}</TabsList>
          <div className={"overflow-y-auto overscroll-contain p-[1.25rem] min-h-0"}>
            <TabsContent value="preset">
              <fieldset className="grid gap-3"><legend className="sr-only">{t.homeGroupPreset}</legend>
                {HOME_VARIANT_OPTIONS.map(option => <label key={option.id} data-checked={design.preferences.preset === option.id} className={cn("flex min-h-11 items-center gap-[.5rem] border border-border rounded-[.5rem] [padding:.65rem_.75rem] [text-align:start] text-[.8125rem] [&:is([aria-checked='true'],_[data-checked='true'])]:bg-accent [&:is([aria-checked='true'],_[data-checked='true'])]:[border-color:var(--primary)] [&:is([aria-checked='true'],_[data-checked='true'])]:text-primary [&_input]:[accent-color:var(--primary)]", "cursor-pointer items-start! py-4!")}>
                  <input type="radio" name="home-preset" value={option.id} className="mt-1.5" checked={design.preferences.preset === option.id} onChange={() => design.selectPreset(option.id)} />
                  <span><span className="block font-heading text-lg">{locale === "zh" ? option.labelZh : option.labelEn}</span><span className="mt-2 block text-xs leading-6 text-muted-foreground">{option.id === "journal" ? t.homePresetJournalHint : option.id === "desk" ? t.homePresetDeskHint : t.homePresetEditorialHint}</span></span>
                </label>)}
              </fieldset>
              <button type="button" onClick={design.resetAll} className={cn(actionClass, "mt-5 border border-border")}><RotateCcw size={14} aria-hidden="true" />{t.homeRestoreAll}</button>
              <p className="mt-5 text-xs leading-6 text-muted-foreground">{t.homeSaveHint}</p>
            </TabsContent>
            <TabsContent value="appearance">
              {field("theme.palette", t.homePalette)}
              <fieldset className="my-5"><legend className="mb-3 text-xs font-medium">{t.homeMode}</legend><div className="grid grid-cols-3 gap-2">{([{ id: "system", label: t.homeSystem }, { id: "light", label: t.homeLight }, { id: "dark", label: t.homeDark }] as const).map(option => <label key={option.id} className={"flex min-h-11 items-center gap-[.5rem] border border-border rounded-[.5rem] [padding:.65rem_.75rem] [text-align:start] text-[.8125rem] [&:is([aria-checked='true'],_[data-checked='true'])]:bg-accent [&:is([aria-checked='true'],_[data-checked='true'])]:[border-color:var(--primary)] [&:is([aria-checked='true'],_[data-checked='true'])]:text-primary [&_input]:[accent-color:var(--primary)]"} data-checked={design.mode === option.id}><input type="radio" name="home-mode" value={option.id} checked={design.mode === option.id} onChange={() => design.changeMode(option.id)} />{option.label}</label>)}</div></fieldset>
              {field("theme.typography", t.homeTypography)}{field("theme.density", t.homeDensity)}
              <div className="mb-5" data-home-setting="theme.fontScale"><FieldHeader path="theme.fontScale" label={t.homeFontScale} /><label className="flex min-h-11 items-center gap-4"><input type="range" min={90} max={115} step={5} value={design.config.theme.fontScale} aria-label={t.homeFontScale} onChange={event => design.setSetting("theme.fontScale", Number(event.target.value))} className="min-w-0 flex-1 accent-primary" /><output className="font-mono text-sm">{design.config.theme.fontScale}%</output></label></div>
              {field("theme.texture", t.homeTexture)}{field("theme.border", t.homeBorder)}{field("theme.shadow", t.homeShadow)}
              <details className="mt-6 border-t border-border pt-4"><summary className="cursor-pointer py-3 text-sm font-medium">{t.homeAdvancedColors}</summary><p className="my-3 text-xs leading-6 text-muted-foreground">{t.homeColorsHint}</p>
                <div role="group" aria-label={t.homeAdvancedColors} className="mb-3 flex gap-2">{(["light", "dark"] as const).map(mode => <button type="button" key={mode} aria-pressed={colorMode === mode} onClick={() => setColorMode(mode)} className={cn(actionClass, colorMode === mode && "bg-accent text-primary")}>{mode === "light" ? t.homeLight : t.homeDark}</button>)}</div>
                <ColorField mode={colorMode} color="primary" label={t.homePrimary} /><ColorField mode={colorMode} color="background" label={t.homeBackground} /><ColorField mode={colorMode} color="foreground" label={t.homeForeground} />
              </details>
            </TabsContent>
            <TabsContent value="layout">{field("frame", t.homeFrame)}{field("navigation", t.homeNavigationStyle)}{field("contentWidth", t.homeWidth)}</TabsContent>
            <TabsContent value="chrome">{field("hero", t.homeHeroStyle)}<p className="mb-4 text-xs leading-6 text-muted-foreground">{t.homePhotoHint}</p>{field("footer", t.homeFooterStyle)}</TabsContent>
            <TabsContent value="sections">{field("sections.featuredPosts", t.homeFeaturedPostsStyle)}{field("sections.latestPosts", t.homeLatestPostsStyle)}{field("sections.interests", t.homeInterestsStyle)}{field("sections.categories", t.featuredCategories)}{field("sections.photos", t.homePhotosStyle)}{field("sections.links", t.homeLinksStyle)}</TabsContent>
            <p role="status" aria-live="polite" className="mt-3 text-xs leading-6 text-primary">{notice}</p>
          </div>
        </Tabs>
        <div className="shrink-0 px-5 text-xs leading-6 text-muted-foreground" aria-live="polite">{design.dirty ? t.homeUnsaved : Object.keys(design.preferences.overrides).length ? t.homeCustomized : t.homePresetDefault}</div>
        <div className={"shrink-0 flex flex-wrap [justify-content:flex-end] gap-[.5rem] [padding:.75rem_1rem] [border-top:1px_solid_var(--border)]"}>
          <button type="button" aria-label={t.homeShare} title={t.homeShareHint} onClick={() => void share()} className={cn(actionClass, "mr-auto")}><Copy size={15} aria-hidden="true" />{t.homeShare}</button>
          <button type="button" onClick={design.undo} disabled={!design.dirty} aria-label={t.homeUndo} title={t.homeUndo} className={actionClass}><RotateCcw size={15} aria-hidden="true" /></button>
          <DialogClose asChild><button type="button" className={actionClass}>{t.homeCancel}</button></DialogClose>
          <button type="button" onClick={design.apply} disabled={!design.canApply} className={cn(actionClass, "bg-primary text-primary-foreground hover:bg-primary/90")}>{t.homeApply}</button>
        </div>
        {shareStatus ? <div className="shrink-0 border-t border-border px-5 py-3"><p role="status" className="text-xs leading-6 text-muted-foreground">{shareStatus}</p>{shareFallback ? <input readOnly value={shareFallback} aria-label={t.homeShare} onFocus={event => event.target.select()} className="mt-2 min-h-11 w-full rounded border border-border bg-background p-2 text-xs" /> : null}</div> : null}
      </DialogContent>
    </DialogPortal>
  </Dialog>
}
