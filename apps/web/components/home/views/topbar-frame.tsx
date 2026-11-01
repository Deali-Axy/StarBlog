import type { ReactNode } from "react"
export function TopbarFrame({ navigation, footer, children }: {
  navigation: ReactNode
  footer: ReactNode
  children: ReactNode
}) { return <div className="mx-auto w-full max-w-[calc(var(--home-width)+3rem)] px-2 min-[70rem]:px-6">{navigation}<div className="min-w-0 min-[70rem]:[container-type:inline-size]"><main id="home-content" tabIndex={-1}>{children}</main>{footer}</div></div> }
