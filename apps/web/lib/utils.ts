import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function hashString(value: string) {
  let hash = 0
  for (let index = 0; index < value.length; index += 1) {
    hash = (hash << 5) - hash + value.charCodeAt(index)
    hash |= 0
  }
  return Math.abs(hash)
}

export function categoryColor(name: string) {
  const colors = [
    "bg-blue-500",
    "bg-green-500",
    "bg-purple-500",
    "bg-pink-500",
    "bg-indigo-500",
    "bg-orange-500",
    "bg-teal-500",
    "bg-cyan-500",
    "bg-rose-500",
    "bg-amber-500",
  ]
  return colors[hashString(name) % colors.length]
}

export function coverGradient(seed: string) {
  const hue = hashString(seed) % 360
  return `linear-gradient(135deg, hsl(${hue} 72% 58%), hsl(${(hue + 48) % 360} 68% 42%))`
}
