import { ArrowDown, ArrowUp, ArrowUpRight, Asterisk, Camera, Code2, Mail, PenLine } from "lucide-react"
import Image from "next/image"
import Link from "next/link"
import type { ReactNode } from "react"

import { aboutProfile, aboutProjects, aboutToolkit } from "@/lib/about"
import type { messages } from "@/lib/i18n"
import { routes } from "@/lib/routes"
import type { getSite } from "@/lib/site"
import { pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

type AboutProps = { site: ReturnType<typeof getSite>; t: (typeof messages)["zh"] }

function SectionHeading({ id, label, title, children }: {
  id: string
  label: string
  title: string
  children?: ReactNode
}) {
  return (
    <div className="grid gap-5 md:grid-cols-[12rem_minmax(0,1fr)] lg:grid-cols-[15rem_minmax(0,1fr)]">
      <p className="pt-2 font-mono text-xs tracking-wider text-primary">{label}</p>
      <div>
        <h2 id={id} className="font-heading text-3xl leading-snug tracking-tight sm:text-4xl">{title}</h2>
        {children ? <p className="mt-4 max-w-2xl text-sm leading-7 text-muted-foreground">{children}</p> : null}
      </div>
    </div>
  )
}

function TextLink({ href, children, external = false }: { href: string; children: ReactNode; external?: boolean }) {
  return (
    <Link
      href={href}
      {...(external ? { target: "_blank", rel: "noreferrer" } : {})}
      className="group inline-flex min-h-11 items-center gap-2 text-sm underline decoration-border underline-offset-5 transition-colors hover:text-primary hover:decoration-primary"
    >
      {children}<ArrowUpRight aria-hidden="true" className="size-4 shrink-0 transition-transform group-hover:-translate-y-0.5 group-hover:translate-x-0.5 motion-reduce:transform-none" />
    </Link>
  )
}

export function AboutProfile({ site, t }: AboutProps) {
  const copy = t.aboutPage
  const identity = site.authorName.match(/^(.+?)\s*[(（]([^()（）]+)[)）]\s*$/u)
  const name = identity?.[1]?.trim() || site.authorName
  const alias = identity?.[2]?.trim()
  const stories = [
    { word: "BUILD", Icon: Code2, title: copy.buildTitle, description: copy.buildDescription, link: copy.buildLink, href: site.githubUrl, external: true },
    { word: "WRITE", Icon: PenLine, title: copy.writeTitle, description: copy.writeDescription, link: copy.writeLink, href: routes.blog },
    { word: "OBSERVE", Icon: Camera, title: copy.observeTitle, description: copy.observeDescription, link: copy.observeLink, href: routes.photography },
  ]

  return (
    <article
      id="about-top"
      aria-labelledby="about-name"
      className={cn(pageContainerClassName, "scroll-mt-32 pt-8 pb-10 sm:pt-12 [&_section[id]]:scroll-mt-32 [&_a:focus-visible]:outline-2 [&_a:focus-visible]:outline-offset-4 [&_a:focus-visible]:outline-primary")}
    >
      <div className="flex items-center justify-between gap-4 border-b border-foreground pb-4">
        <p className="font-mono text-xs tracking-wider text-muted-foreground">{copy.eyebrow}</p>
        <Asterisk aria-hidden="true" className="size-5 shrink-0 text-primary" />
      </div>

      <header className="grid gap-12 pt-10 pb-12 md:grid-cols-[1.2fr_1fr] md:items-center md:gap-10 md:pt-14 md:pb-16 lg:gap-20">
        <div className="min-w-0">
          <p className="font-heading text-xl text-muted-foreground sm:text-2xl">{copy.greeting}</p>
          <h1 id="about-name" className="mt-3 font-heading text-7xl leading-tight tracking-tighter wrap-anywhere sm:text-8xl lg:text-9xl">
            {name}<span className="text-primary">.</span>
          </h1>
          {alias ? <p className="mt-1 font-heading text-3xl tracking-tight text-primary italic sm:text-4xl">{alias} /</p> : null}
          <p className="mt-7 text-sm leading-7 text-muted-foreground">{site.authorTitle}<span aria-hidden="true" className="px-2 text-primary">↗</span>{copy.intro}</p>
          <p className="mt-5 max-w-md font-heading text-2xl leading-relaxed sm:text-3xl">
            {copy.statement}<br />{copy.statementEnd}
          </p>
          <p className="mt-4 max-w-md text-sm leading-7 text-muted-foreground">{copy.description}</p>
          <div className="mt-7 flex flex-wrap items-center gap-x-7 gap-y-2">
            <a href="#about-story" className="group inline-flex min-h-11 items-center gap-3 text-sm">
              <span className="grid size-10 place-items-center rounded-full border border-primary text-primary transition-colors group-hover:bg-primary group-hover:text-primary-foreground">
                <ArrowDown aria-hidden="true" className="size-4" />
              </span>
              {copy.readOn}
            </a>
            <TextLink href={`mailto:${aboutProfile.email}`}>{copy.emailAction}</TextLink>
          </div>
        </div>

        <div className="relative mx-auto w-full max-w-sm px-5 pt-5 pb-8 md:max-w-none md:px-4 lg:px-7">
          <div aria-hidden="true" className="absolute inset-x-0 top-0 bottom-7 border border-dashed border-primary/35" />
          <span aria-hidden="true" className="absolute -top-3 left-4 bg-background px-2 font-mono text-xs tracking-widest text-primary">FIG. 01 / PORTRAIT</span>
          <figure className="relative rotate-3 border border-border bg-card p-3 pb-5 shadow-lg transition-transform duration-300 hover:rotate-0 motion-reduce:transition-none">
            <Image src={aboutProfile.avatar} width={1024} height={1024} alt={copy.portraitAlt} priority sizes="(min-width: 1152px) 390px, (min-width: 768px) 40vw, 330px" className="aspect-square w-full border border-border/50 object-cover" />
            <figcaption className="mt-4 flex items-center justify-between gap-3 px-1">
              <p className="font-heading text-sm italic sm:text-base">{copy.portraitCaption}</p>
              <Asterisk aria-hidden="true" className="size-6 shrink-0 text-primary" />
            </figcaption>
          </figure>
          <p className="relative mt-7 text-center font-mono text-xs text-muted-foreground">{copy.portraitNote}</p>
        </div>
      </header>

      <nav aria-label={copy.indexLabel} className="flex flex-wrap items-center justify-between gap-x-8 gap-y-1 border-y border-foreground py-2">
        {[
          ["01", "#about-story", copy.storyNav],
          ["02", "#about-projects", copy.projectsNav],
          ["03", "#about-toolkit", copy.toolkitNav],
          ["04", "#about-contact", copy.contactNav],
        ].map(([number, href, label]) => (
          <a key={href} href={href} className="group flex min-h-11 items-center gap-3 text-sm hover:text-primary">
            <span className="font-mono text-xs text-primary">{number}</span>{label}<ArrowDown aria-hidden="true" className="size-3 text-muted-foreground transition-transform group-hover:translate-y-0.5 motion-reduce:transform-none" />
          </a>
        ))}
      </nav>

      <section id="about-story" aria-labelledby="about-story-title" className="py-14 sm:py-20">
        <SectionHeading id="about-story-title" label={copy.storyLabel} title={copy.storyTitle}>{copy.storyIntro}</SectionHeading>
        <p className="mt-6 border-l-2 border-primary pl-4 text-sm leading-7 text-muted-foreground md:ml-48 lg:ml-60">{site.authorBio}</p>
        <div className="mt-10 grid gap-10 md:grid-cols-3 md:gap-7">
          {stories.map(({ word, Icon, title, description, link, href, external }) => (
            <div key={word} className="flex min-w-0 flex-col border-t border-border pt-5">
              <div className="flex items-center justify-between text-primary">
                <span className="font-mono text-xs tracking-widest">{word}</span><Icon aria-hidden="true" className="size-5 stroke-1" />
              </div>
              <h3 className="mt-5 font-heading text-xl leading-relaxed">{title}</h3>
              <p className="mt-3 flex-1 text-sm leading-7 text-muted-foreground">{description}</p>
              <div className="mt-4"><TextLink href={href} external={external}>{link}</TextLink></div>
            </div>
          ))}
        </div>
      </section>

      <section id="about-projects" aria-labelledby="about-projects-title" className="border-t border-foreground py-14 sm:py-20">
        <SectionHeading id="about-projects-title" label={copy.projectsLabel} title={copy.projectsTitle}>{copy.projectsIntro}</SectionHeading>
        <div className="mt-10 border-t border-border">
          {aboutProjects.map((project, index) => (
            <a key={project.id} href={project.href} target="_blank" rel="noreferrer" className="group grid gap-4 border-b border-border py-7 transition-colors hover:bg-primary/5 sm:grid-cols-[2.5rem_minmax(0,1fr)_auto] sm:gap-5 sm:py-8">
              <span aria-hidden="true" className="pt-1 font-mono text-xs text-muted-foreground">/ {String(index + 1).padStart(2, "0")}</span>
              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
                  <h3 className="font-heading text-3xl tracking-tight transition-colors group-hover:text-primary">{project.name}</h3>
                  {project.id === "starblog" ? <span className="inline-flex items-center gap-2 border border-primary/30 px-2 py-1 text-xs text-primary"><span aria-hidden="true" className="size-1.5 rounded-full bg-primary" />{copy.currentSite}</span> : null}
                </div>
                <p className="mt-3 max-w-2xl text-sm leading-7 text-muted-foreground">{copy.projectDescriptions[project.id]}</p>
                <p className="mt-3 font-mono text-xs leading-6 text-muted-foreground">{project.stack}</p>
              </div>
              <div className="flex items-center gap-3 self-start pt-1 text-xs text-muted-foreground group-hover:text-primary">
                <span className="sm:sr-only lg:not-sr-only">{copy.sourceCode}</span><ArrowUpRight aria-hidden="true" className="size-5 transition-transform group-hover:-translate-y-1 group-hover:translate-x-1 motion-reduce:transform-none" />
              </div>
            </a>
          ))}
        </div>
        <div className="mt-5 flex justify-end"><TextLink href={site.githubUrl} external>{copy.allProjects}</TextLink></div>
      </section>

      <section id="about-toolkit" aria-labelledby="about-toolkit-title" className="border-t border-foreground py-14 sm:py-20">
        <SectionHeading id="about-toolkit-title" label={copy.toolkitLabel} title={copy.toolkitTitle}>{copy.toolkitIntro}</SectionHeading>
        <div className="mt-9 grid gap-x-10 md:ml-48 md:grid-cols-2 lg:ml-60">
          {aboutToolkit.map(({ id, skills }) => (
            <div key={id} className="border-t border-border py-5">
              <h3 className="text-sm font-medium">{copy.toolkitGroups[id]}</h3>
              <ul className="mt-2 flex flex-wrap gap-x-4 font-mono text-xs text-muted-foreground">
                {skills.map(skill => <li key={skill}><Link href={routes.searchResults(skill)} className="inline-flex min-h-11 items-center underline decoration-border underline-offset-4 hover:text-primary hover:decoration-primary">{skill}</Link></li>)}
              </ul>
            </div>
          ))}
        </div>
      </section>

      <aside aria-labelledby="about-offscreen" className="relative overflow-hidden bg-foreground px-6 py-9 text-background sm:px-10 sm:py-12">
        <Asterisk aria-hidden="true" className="pointer-events-none absolute -top-10 -right-10 size-56 rotate-12 stroke-1 opacity-10 sm:size-72" />
        <div className="relative grid gap-8 md:grid-cols-[12rem_minmax(0,1fr)] lg:grid-cols-[15rem_minmax(0,1fr)]">
          <div>
            <Camera aria-hidden="true" className="mb-5 size-8 stroke-1" />
            <p className="max-w-40 font-mono text-xs leading-6 tracking-wider opacity-70">{copy.asideLabel}</p>
          </div>
          <div>
            <h2 id="about-offscreen" className="max-w-xl font-heading text-3xl leading-relaxed tracking-tight">{copy.asideTitle}</h2>
            <p className="mt-4 max-w-xl text-sm leading-7 opacity-75">{copy.asideDescription}</p>
            <ul className="mt-6 flex flex-wrap gap-x-6 gap-y-3 font-mono text-xs opacity-70">
              {copy.asideTags.map(tag => <li key={tag}><span aria-hidden="true">/ </span>{tag}</li>)}
            </ul>
          </div>
        </div>
      </aside>

      <section id="about-contact" aria-labelledby="about-contact-title" className="pt-16 pb-14 sm:pt-20 sm:pb-20">
        <SectionHeading id="about-contact-title" label={copy.contactLabel} title={copy.contactTitle}>{copy.contactDescription}</SectionHeading>
        <div className="mt-9 grid gap-10 lg:grid-cols-[1fr_1.1fr] lg:gap-14">
          <div>
            <a href={`mailto:${aboutProfile.email}`} className="group flex items-center justify-between gap-4 border-y border-foreground py-6">
              <div>
                <span className="flex items-center gap-2 text-xs text-muted-foreground"><Mail aria-hidden="true" className="size-4" />{copy.emailAction}</span>
                <span className="mt-3 block font-heading text-2xl tracking-tight wrap-anywhere group-hover:text-primary sm:text-3xl">{aboutProfile.email}</span>
              </div>
              <ArrowUpRight aria-hidden="true" className="size-7 shrink-0 text-primary transition-transform group-hover:-translate-y-1 group-hover:translate-x-1 motion-reduce:transform-none" />
            </a>
            <p className="mt-6 font-mono text-xs text-muted-foreground">{copy.elsewhere}</p>
            <div className="mt-2 flex flex-wrap gap-x-6 gap-y-1">
              <TextLink href={site.githubUrl} external>GitHub</TextLink>
              <TextLink href={site.zhihuUrl} external>{t.zhihu}</TextLink>
              <TextLink href={site.cnblogsUrl} external>{t.cnblogs}</TextLink>
              <TextLink href={aboutProfile.website} external>{copy.personalSite}</TextLink>
            </div>
          </div>
          <div className="border border-border bg-card p-5 sm:p-6">
            <h3 className="font-heading text-xl">{copy.channelsTitle}</h3>
            <p className="mt-2 text-xs leading-6 text-muted-foreground">{copy.channelsHint}</p>
            <div className="mt-5 grid grid-cols-2 gap-5 sm:gap-8">
              {aboutProfile.channels.map(channel => (
                <div key={channel.id} className="min-w-0">
                  <Image src={channel.qr} alt={copy.qrAlt(channel.name)} width={344} height={344} sizes="(min-width: 640px) 140px, 35vw" className="aspect-square w-full max-w-36 border border-border bg-white object-contain p-2" />
                  <p className="mt-3 text-sm font-medium">{channel.name}</p>
                  <p className="mt-1 text-xs leading-6 text-muted-foreground">{copy.channelDescriptions[channel.id]}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      <div className="flex items-center justify-between gap-4 border-t border-foreground pt-6">
        <p className="flex items-center gap-3 font-heading text-sm italic sm:text-base"><Asterisk aria-hidden="true" className="size-5 text-primary" />{copy.closing}</p>
        <a href="#about-top" className="inline-flex min-h-11 shrink-0 items-center gap-2 text-xs text-muted-foreground hover:text-primary">{copy.backToTop}<ArrowUp aria-hidden="true" className="size-3" /></a>
      </div>
    </article>
  )
}
