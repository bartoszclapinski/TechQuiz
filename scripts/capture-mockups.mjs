// Generates PNG screenshots of the HTML mockups in docs/mockups/.
// Hides mockup-page chrome (meta header + disclaimer) so screenshots show
// only the actual UI design.
//
// Run:  pnpm capture-mockups
// Re-run after changing any mockup to refresh the README hero images.

import { chromium } from 'playwright'
import { fileURLToPath, pathToFileURL } from 'node:url'
import { mkdir } from 'node:fs/promises'
import path from 'node:path'

const __filename = fileURLToPath(import.meta.url)
const repoRoot = path.resolve(path.dirname(__filename), '..')
const mockupsDir = path.join(repoRoot, 'docs', 'mockups')
const outDir = path.join(repoRoot, 'docs', 'screenshots')

const targets = [
  { file: 'login-dual-theme.html',         out: 'login-dark.png',      hideLight: true  },
  { file: 'categories-dark.html',          out: 'categories-dark.png'                     },
  { file: 'quiz-multiple-choice-dark.html',out: 'quiz-dark.png'                           },
  { file: 'result-dark.html',              out: 'result-dark.png'                         },
  { file: 'dashboard-dark.html',           out: 'dashboard-dark.png'                      },
  { file: 'quiz-code-output-dark.html',    out: 'quiz-code-dark.png'                      },
]

const hideMockupChrome = `
  body { padding: 0 !important; background: #020617 !important; }
  .mockup-meta, .disclaimer { display: none !important; }
  .mockup-wrapper { max-width: none !important; margin: 0 !important; padding: 16px !important; }
`

await mkdir(outDir, { recursive: true })

const browser = await chromium.launch()
try {
  for (const { file, out, hideLight } of targets) {
    const url = pathToFileURL(path.join(mockupsDir, file)).href
    const ctx = await browser.newContext({
      viewport: { width: 1440, height: 900 },
      deviceScaleFactor: 2,
    })
    const page = await ctx.newPage()
    await page.goto(url, { waitUntil: 'networkidle' })
    await page.addStyleTag({ content: hideMockupChrome })

    if (hideLight) {
      // login-dual-theme.html stacks dark + light themes; keep only the first (dark) section.
      await page.evaluate(() => {
        const inner = document.querySelector('.mockup-wrapper > div')
        if (inner) {
          for (let i = 1; i < inner.children.length; i++) {
            inner.children[i].style.display = 'none'
          }
          // Also hide the "Dark mode — default theme" label paragraph above the design.
          const labels = inner.querySelectorAll(':scope > div > p')
          labels.forEach(p => { p.style.display = 'none' })
        }
      })
    }

    // Give web fonts + injected styles a beat to settle.
    await page.waitForTimeout(300)

    const wrapper = page.locator('.mockup-wrapper')
    await wrapper.screenshot({ path: path.join(outDir, out) })
    await ctx.close()
    console.log(`✓ ${file}  →  docs/screenshots/${out}`)
  }
} finally {
  await browser.close()
}

console.log(`\nDone — ${targets.length} screenshots saved to docs/screenshots/`)
