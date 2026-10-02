"use client"

import { useEffect, useState } from "react"
import { useTheme } from "next-themes"

import { Button } from "@/components/ui/button"

export function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme()
  const [mounted, setMounted] = useState(false)

  useEffect(() => {
    setMounted(true)
  }, [])

  return (
    <Button
      variant="ghost"
      size="sm"
      className="h-8 min-w-8 px-2 text-xs tracking-[0.2em]"
      aria-label="切换昼夜"
      onClick={() => {
        const dark = document.documentElement.classList.contains("dark")
        setTheme(dark ? "light" : "dark")
      }}
    >
      {mounted ? (resolvedTheme === "dark" ? "夜" : "昼") : null}
    </Button>
  )
}
