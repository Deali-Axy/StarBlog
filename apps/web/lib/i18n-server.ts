import { cookies } from "next/headers"

import { localeCookie, messages, parseLocale } from "@/lib/i18n"

export async function getI18n() {
  const locale = parseLocale((await cookies()).get(localeCookie)?.value)
  return { locale, t: messages[locale] }
}
