"use client"
import { useHomeDesign } from "./home-design-context"
export function useHomeMode() { const { effectiveMode, changeMode } = useHomeDesign(); return { effectiveMode, changeMode } }
