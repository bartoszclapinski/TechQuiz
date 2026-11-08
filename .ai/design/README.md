# Design references

Source material the UI was built against. Reference, not implementation — React components use the
project's own CSS-variable tokens defined in `web/src/index.css`, never values copied from these files.

## `momentum-handoff/`

The high-fidelity handoff for the **"Momentum"** design system (violet brand + amber gamification accent,
dual theme, Bricolage Grotesque display font) that the app was redesigned to in iteration 4.10. The
decision and its guardrails are recorded in **ADR-024**.

The handoff's values are no longer verbatim: ADR-026 later raised three tokens for WCAG AA contrast —
dark `--text3` `#7d7291` → `#948aac`, light `--text3` `#9a8fb5` → `#736a88`, light `--ambertext`
`#d97706` → `#a85400` (`--text-muted` / `--amber-text` in the app). Use the values in
`web/src/index.css`, not the handoff's token tables.

| File | What it is |
|---|---|
| `README.md` | The written spec — tokens, components, per-screen layout notes |
| `TechQuiz Momentum.dc.html` | Interactive prototype: switch between Landing / Login / Categories / Quiz / Result / Dashboard, and between light and dark |
| `support.js` | Design Compiler runtime the prototype needs to boot |

### Opening it

Serve the folder over HTTP rather than opening the file directly — the runtime fetches its own modules,
which `file://` blocks:

```bash
cd .ai/design/momentum-handoff
python -m http.server 8000     # then open http://localhost:8000/TechQuiz%20Momentum.dc.html
```

`support.js` was **missing from the original export** and was copied in from
[`.ai/animation/`](../animation/), which uses the same runtime. Without it the prototype renders a blank
page — so keep it alongside the HTML.

### Note on the numbers

The prototype and its spec show invented figures (XP totals, "Level 7", "9 topics" / "9 categories").
They are visual filler.
Per ADR-024 the implementation deliberately shows **real data only**; the taxonomy has grown to 4 tracks
over 18 categories since, and gamification values are derived from actual attempts (ADR-025).

## Not tracked

The design tool's export archive (`.zip`) is gitignored — it only duplicates the extracted folder.
