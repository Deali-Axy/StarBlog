import { createRequire } from "node:module"
import { readFile, writeFile } from "node:fs/promises"
const require = createRequire(import.meta.resolve("next/package.json"))
const sharp = require("sharp")
const mark = await readFile(new URL("../public/brand/mote/mark.svg", import.meta.url), "utf8")
const image = `<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="630" viewBox="0 0 1200 630">
<defs><pattern id="dots" width="24" height="24" patternUnits="userSpaceOnUse"><circle cx="1" cy="1" r="1" fill="#6250B3" opacity=".13"/></pattern></defs>
<rect width="1200" height="630" fill="#F4F1FA"/><rect width="1200" height="630" fill="url(#dots)"/>
<rect x="48" y="48" width="1104" height="534" rx="24" fill="#FCFAFF" stroke="#D8D0EC"/>
<path d="M100 427H1100" stroke="#D8D0EC"/>
<image x="86" y="100" width="128" height="128" href="data:image/svg+xml;base64,${Buffer.from(mark).toString("base64")}"/>
<text x="100" y="330" font-family="sans-serif" font-size="100" font-weight="700" fill="#30274A">StarBlog</text>
<text x="104" y="386" font-family="sans-serif" font-size="27" fill="#625B70">Notes on code, photography and life.</text>
<text x="104" y="492" font-family="sans-serif" font-size="18" letter-spacing="4" fill="#6250B3">KEEP CURIOUS. KEEP CREATING.</text>
<circle cx="995" cy="240" r="69" fill="#6250B3" opacity=".07"/>
<circle cx="1047" cy="316" r="28" fill="#6250B3" opacity=".13"/>
<path d="M894 180v32m-16-16h32M1036 118v24m-12-12h24" stroke="#6250B3" stroke-width="2"/>
</svg>`
await writeFile(new URL("../public/og-default.png", import.meta.url), await sharp(Buffer.from(image)).png().toBuffer())
console.log("Generated public/og-default.png (1200 × 630)")
