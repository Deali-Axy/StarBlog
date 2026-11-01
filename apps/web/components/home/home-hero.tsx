"use client"
import type { ComponentType } from "react"
import { useHomeDesign } from "./home-design-context"
import type { HomeHeroProps } from "./home-hero-props"
import { CoverHero } from "./views/cover-hero"
import { PhotographHero } from "./views/photograph-hero"
import { SignatureHero } from "./views/signature-hero"
const heroes = { cover: CoverHero, signature: SignatureHero, photograph: PhotographHero } satisfies Record<ReturnType<typeof useHomeDesign>["config"]["hero"], ComponentType<HomeHeroProps>>
export function HomeHero(props: HomeHeroProps) { const { config } = useHomeDesign(); const Hero = heroes[config.hero]; return <Hero {...props} /> }
