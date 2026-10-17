import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"

const source = await readFile(new URL("../lib/photography/image-cache.ts", import.meta.url), "utf8")
const { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext, target: ts.ScriptTarget.ES2020 } })
const { LightboxImageCache } = await import(`data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`)

function deferred() {
  let resolve, reject
  const promise = new Promise((yes, no) => { resolve = yes; reject = no })
  return { promise, resolve, reject }
}

function setup() {
  const images = []
  const cache = new LightboxImageCache(() => {
    const decoding = deferred()
    const image = {
      src: "", onload: null, onerror: null, decoded: decoding,
      decode() { return decoding.promise },
      removeAttribute(name) { if (name === "src") this.src = "" },
    }
    images.push(image)
    return image
  })
  return { cache, images }
}

const tick = () => new Promise(resolve => setImmediate(resolve))
async function ready(resource) {
  resource.image.onload()
  resource.image.decoded.resolve()
  await resource.promise
}

test("a downloaded original stays pending until decoding completes", async () => {
  const { cache, images } = setup()
  const original = cache.load("/first.jpg")
  let settled = false
  void original.promise.then(() => { settled = true })
  images[0].onload()
  await tick()
  assert.equal(settled, false)
  assert.equal(original.status, "loading")
  images[0].decoded.resolve()
  assert.equal(await original.promise, images[0])
  assert.equal(original.status, "ready")
})

test("prefetching and viewing share the pending request and decoded element", async () => {
  const { cache, images } = setup()
  const prefetched = cache.load("/next.jpg", "low")
  assert.equal(images[0].fetchPriority, "low")
  assert.equal(cache.load("/next.jpg"), prefetched)
  assert.equal(images[0].fetchPriority, "high")
  await ready(prefetched)
  assert.equal(cache.load("/next.jpg").image, images[0])
  assert.equal(images.length, 1)
})

test("retrying a failed original does not reload successful neighbours", async () => {
  const { cache, images } = setup()
  const neighbour = cache.load("/previous.jpg", "low")
  await ready(neighbour)
  const failed = cache.load("/current.jpg")
  const rejected = assert.rejects(failed.promise)
  failed.image.onerror()
  await rejected
  const retried = cache.load("/current.jpg")
  assert.notEqual(retried, failed)
  assert.equal(retried.image.src, "/current.jpg?lightboxRetry=1")
  assert.equal(cache.load("/previous.jpg"), neighbour)
  assert.equal(images.length, 3)
  await ready(retried)
})

test("moving the viewing window cancels stale requests and evicts decoded images", async () => {
  const { cache, images } = setup()
  const previous = cache.load("/previous.jpg", "low")
  const current = cache.load("/current.jpg")
  await ready(current)
  const next = cache.load("/next.jpg", "low")
  const rejected = assert.rejects(previous.promise, { name: "AbortError" })
  const staleLoad = previous.image.onload
  cache.retain(["/current.jpg", "/next.jpg", "/upcoming.jpg"])
  await rejected
  assert.equal(cache.peek("/previous.jpg"), undefined)
  assert.equal(previous.image.src, "")
  previous.image.decoded.resolve()
  staleLoad()
  await tick()
  assert.equal(previous.status, "failed")
  assert.equal(cache.peek("/previous.jpg"), undefined)
  assert.equal(cache.load("/next.jpg"), next)
  const cancelled = assert.rejects(next.promise, { name: "AbortError" })
  cache.clear()
  await cancelled
  assert.equal(cache.peek("/current.jpg"), undefined)
  assert.equal(cache.peek("/next.jpg"), undefined)
  assert.equal(images.length, 3)
})

test("a decode failure is retriable and never becomes a displayable image", async () => {
  const { cache } = setup()
  const original = cache.load("/broken.jpg")
  const rejected = assert.rejects(original.promise, /decode failed/)
  original.image.onload()
  original.image.decoded.reject(new Error("decode failed"))
  await rejected
  assert.equal(original.status, "failed")
  const retried = cache.load("/broken.jpg")
  await ready(retried)
  assert.equal(retried.status, "ready")
})
