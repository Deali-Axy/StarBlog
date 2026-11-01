import type { ReactNode } from "react"
export function SidebarFrame({ navigation, footer, children }: {
  navigation: ReactNode
  footer: ReactNode
  children: ReactNode
}) { return <div className="mx-auto w-full max-w-[calc(var(--home-width)+3rem)] px-2 min-[70rem]:grid min-[70rem]:grid-cols-[12rem_minmax(0,1fr)] min-[70rem]:gap-10 min-[70rem]:max-w-[calc(var(--home-width)+24rem)] min-[70rem]:px-8 min-[90rem]:gap-14 min-[90rem]:px-12">{navigation}<div className="min-w-0 min-[70rem]:[container-type:inline-size]"><main id="home-content" tabIndex={-1}>{children}</main>{footer}</div></div> }
