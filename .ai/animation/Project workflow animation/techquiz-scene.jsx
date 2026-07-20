// TechQuiz walkthrough → architecture reveal.
// Grounded in the real web/ source: "Momentum" design system (web/src/index.css),
// app-shell, auth split-screen, tracks→categories drill-down, quiz + circular-score result.
const { Stage, Sprite, useTime, useSprite, useTimeline, interpolate, animate, Easing, clamp } = window;

// ── Momentum tokens (web/src/index.css :root, dark) ──────────────────────────
const C = {
  base:    '#17121f',
  surface: '#221a2e',
  elevated:'#2a2137',
  border:  '#2a2137',
  borderS: '#392c4d',
  text:    '#f4f0fa',
  text2:   '#a99fc0',
  muted:   '#7d7291',
  accent:  '#a78bfa',
  accentT: '#c4b5fd',
  accentBg:'rgba(167,139,250,0.15)',
  success: '#22c55e',
  amber:   '#fbbf24',
  amberBg: 'rgba(251,191,36,0.15)',
  danger:  '#ef4444',
  brandfg: '#17121f',
  track:   '#17121f',
};
// Accent themes — swap the brand/button gradient across the whole animation (tweakable).
const THEMES = {
  'Violet → Amber': { brand: 'linear-gradient(135deg, #a78bfa, #fbbf24)', btn: 'linear-gradient(135deg, #8b5cf6, #a78bfa)' },
  'Violet':         { brand: 'linear-gradient(135deg, #8b5cf6, #c4b5fd)', btn: 'linear-gradient(135deg, #7c3aed, #a78bfa)' },
  'Teal':           { brand: 'linear-gradient(135deg, #2dd4bf, #a78bfa)', btn: 'linear-gradient(135deg, #14b8a6, #2dd4bf)' },
};
let BRAND = THEMES['Violet → Amber'].brand; // --brand (logos, icon tiles, progress)
let BTN   = THEMES['Violet → Amber'].btn;   // --btn (buttons)
let SHOW_CURSOR = true;
function applyTheme(name) {
  const th = THEMES[name] || THEMES['Violet → Amber'];
  BRAND = th.brand; BTN = th.btn;
}
const GLOW_A = 'rgba(251,191,36,0.12)';  // hero-glow-1 (amber)
const GLOW_V = 'rgba(167,139,250,0.18)'; // hero-glow-2 (violet)
const SHADOW = '0 24px 60px rgba(0,0,0,0.45)';

const DISP = "'Bricolage Grotesque', 'Geist', system-ui, sans-serif";
const SANS = "'Geist', system-ui, sans-serif";
const MONO = "'JetBrains Mono', ui-monospace, monospace";

const kf = (t, ts, vs, ease = Easing.easeInOutCubic) => interpolate(ts, vs, ease)(t);
const appear = (t, start, dur = 0.5) => clamp((t - start) / dur, 0, 1);

// ── Frame geometry ───────────────────────────────────────────────────────────
const F = { x: 128, y: 70, w: 1024, h: 576, chrome: 36 };
const CA = { x: F.x, y: F.y + F.chrome, w: F.w, h: F.h - F.chrome };

// ── Cursor ───────────────────────────────────────────────────────────────────
function Cursor({ keys }) {
  const { localTime } = useSprite();
  if (!SHOW_CURSOR) return null;
  const ts = keys.map(k => k.t);
  const x = kf(localTime, ts, keys.map(k => k.x), Easing.easeInOutCubic);
  const y = kf(localTime, ts, keys.map(k => k.y), Easing.easeInOutCubic);
  let ripple = null;
  for (const k of keys) if (k.click && localTime >= k.t && localTime < k.t + 0.5) {
    const p = (localTime - k.t) / 0.5;
    ripple = { x: k.x, y: k.y, s: 0.3 + p * 1.4, o: (1 - p) * 0.6 };
  }
  let press = 1;
  for (const k of keys) if (k.click && Math.abs(localTime - k.t) < 0.12) press = 0.86;
  return (
    <React.Fragment>
      {ripple && (
        <div style={{ position: 'absolute', left: ripple.x, top: ripple.y, width: 44, height: 44,
          marginLeft: -22, marginTop: -22, borderRadius: '50%', border: `2px solid ${C.accentT}`,
          opacity: ripple.o, transform: `scale(${ripple.s})`, pointerEvents: 'none', zIndex: 60 }} />
      )}
      <div style={{ position: 'absolute', left: x, top: y, transform: `scale(${press})`,
        transformOrigin: 'top left', pointerEvents: 'none',
        filter: 'drop-shadow(0 3px 6px rgba(0,0,0,0.55))', zIndex: 60 }}>
        <svg width="26" height="26" viewBox="0 0 24 24" fill="none">
          <path d="M5 3l14 8-6 1.4L10 20 5 3z" fill="#fff" stroke="#17121f" strokeWidth="1.2" strokeLinejoin="round" />
        </svg>
      </div>
    </React.Fragment>
  );
}

// ── Shared bits ──────────────────────────────────────────────────────────────
function LogoMark({ size = 32, radius = 10, font = 16 }) {
  return (
    <div style={{ width: size, height: size, background: BRAND, borderRadius: radius, display: 'flex',
      alignItems: 'center', justifyContent: 'center', fontFamily: DISP, fontWeight: 800, fontSize: font,
      color: C.brandfg }}>T</div>
  );
}
function IconTile({ code, muted = false, size = 46 }) {
  const fs = code.length > 3 ? (code.length > 5 ? 10 : 13) : 16;
  return (
    <div style={{ height: size, minWidth: size, padding: '0 8px', borderRadius: 13, display: 'flex',
      alignItems: 'center', justifyContent: 'center', fontFamily: DISP, fontWeight: 800, fontSize: fs,
      background: muted ? C.elevated : BRAND, color: muted ? C.muted : C.brandfg }}>{code}</div>
  );
}
function Pill({ children, muted = false }) {
  return (
    <span style={{ borderRadius: 999, background: C.elevated, padding: '4px 10px', fontFamily: MONO,
      fontSize: 12, fontWeight: 600, color: muted ? C.muted : C.text2 }}>{children}</span>
  );
}

// ── Browser frame (persistent chrome) ─────────────────────────────────────────
function BrowserFrame({ url, opacity, children }) {
  return (
    <div style={{ position: 'absolute', left: F.x, top: F.y, width: F.w, height: F.h,
      background: C.base, borderRadius: 14, overflow: 'hidden', opacity,
      border: `1px solid ${C.border}`, boxShadow: SHADOW, fontFamily: SANS, color: C.text }}>
      <div style={{ height: F.chrome, background: C.surface, borderBottom: `1px solid ${C.border}`,
        display: 'flex', alignItems: 'center', gap: 8, padding: '0 14px' }}>
        <div style={{ display: 'flex', gap: 6 }}>
          {['#ef4444', '#fbbf24', '#22c55e'].map(c => (
            <div key={c} style={{ width: 10, height: 10, borderRadius: '50%', background: c, opacity: 0.85 }} />
          ))}
        </div>
        <div style={{ flex: 1, display: 'flex', justifyContent: 'center' }}>
          <div style={{ background: C.base, border: `1px solid ${C.border}`, borderRadius: 7,
            padding: '4px 14px', fontFamily: MONO, fontSize: 11, color: C.text2,
            minWidth: 260, textAlign: 'center' }}>{url}</div>
        </div>
        <div style={{ width: 52 }} />
      </div>
      <div style={{ position: 'relative', width: '100%', height: F.h - F.chrome }}>{children}</div>
    </div>
  );
}

// App top nav (web/src/components/app-shell.tsx)
function AppNav({ active }) {
  const items = ['Dashboard', 'Categories', 'Generate', 'Pool', 'History', 'Daily review'];
  return (
    <div style={{ borderBottom: `1px solid ${C.border}`, background: C.surface, padding: '13px 20px',
      display: 'flex', alignItems: 'center', gap: 20 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 9 }}>
        <LogoMark size={30} radius={10} font={15} />
        <span style={{ fontFamily: DISP, fontSize: 17, fontWeight: 700, letterSpacing: '-0.02em' }}>TechQuiz</span>
      </div>
      <nav style={{ display: 'flex', alignItems: 'center', gap: 2, flex: 1 }}>
        {items.map(l => {
          const on = l === active;
          return (
            <span key={l} style={{ borderRadius: 999, padding: '8px 14px', fontSize: 15,
              background: on ? C.elevated : 'transparent', color: on ? C.text : C.text2,
              fontWeight: on ? 600 : 500 }}>{l}</span>
          );
        })}
      </nav>
      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
        <div style={{ color: C.text2, display: 'flex' }}>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" /></svg>
        </div>
        <div style={{ width: 22, height: 22, borderRadius: 999, border: `1px solid ${C.borderS}`, color: C.text2,
          display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" /></svg>
        </div>
        <div style={{ width: 34, height: 34, borderRadius: 999, background: BRAND, display: 'flex',
          alignItems: 'center', justifyContent: 'center', fontFamily: DISP, fontWeight: 700, fontSize: 13, color: C.brandfg }}>DE</div>
      </div>
    </div>
  );
}

function ScreenFade({ children }) {
  const { localTime, duration } = useSprite();
  const o = Math.min(appear(localTime, 0, 0.4), 1 - clamp((localTime - (duration - 0.4)) / 0.4, 0, 1));
  const sc = 0.985 + 0.015 * appear(localTime, 0, 0.5);
  return (
    <div style={{ position: 'absolute', left: CA.x, top: CA.y, width: CA.w, height: CA.h,
      overflow: 'hidden', opacity: o, transform: `scale(${sc})`, transformOrigin: 'center',
      // Base text colour. The Stage root only sets a background, so anything that
      // doesn't name its own colour would otherwise inherit the initial black.
      color: C.text }}>{children}</div>
  );
}

// ── Login: split-screen auth (auth-layout + login-page + auth-hero) ───────────
function LoginScreen() {
  const { localTime } = useSprite();
  const rise = kf(localTime, [0, 0.6], [18, 0], Easing.easeOutCubic);
  const demoHot = localTime > 3.4;
  return (
    <ScreenFade>
      <div style={{ width: '100%', height: '100%', background: C.base, display: 'grid', gridTemplateColumns: '1fr 1fr' }}>
        {/* Left — form column */}
        <div style={{ display: 'flex', flexDirection: 'column', padding: '28px 40px', position: 'relative' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 30 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 9 }}>
              <LogoMark size={32} radius={11} font={17} />
              <span style={{ fontFamily: DISP, fontSize: 18, fontWeight: 700, letterSpacing: '-0.02em' }}>TechQuiz</span>
            </div>
          </div>
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column', justifyContent: 'center',
            transform: `translateY(${rise}px)` }}>
            <div style={{ width: '100%', maxWidth: 340 }}>
              <h1 style={{ fontFamily: DISP, fontSize: 34, fontWeight: 800, letterSpacing: '-0.02em', lineHeight: 1.08, margin: '0 0 8px' }}>Welcome back! 👋</h1>
              <p style={{ fontSize: 15, color: C.text2, margin: '0 0 22px', lineHeight: 1.5 }}>Continue where you left off, or start fresh with a new category.</p>
              {[['Email', 'you@example.com'], ['Password', '••••••••••']].map(([lab, ph]) => (
                <div key={lab} style={{ marginBottom: 14 }}>
                  <div style={{ fontSize: 13, fontWeight: 500, color: C.text2, marginBottom: 6 }}>{lab}</div>
                  <div style={{ background: C.elevated, border: `1px solid ${C.border}`, borderRadius: 14,
                    padding: '12px 16px', fontSize: 15, color: C.muted }}>{ph}</div>
                </div>
              ))}
              <button style={{ width: '100%', marginTop: 4, background: BTN, color: '#fff', border: 'none',
                padding: '13px', borderRadius: 14, fontSize: 15, fontWeight: 600, fontFamily: SANS, boxShadow: SHADOW }}>Sign in</button>
              <div style={{ display: 'flex', alignItems: 'center', gap: 12, margin: '16px 0' }}>
                <div style={{ height: 1, flex: 1, background: C.border }} />
                <span style={{ fontFamily: MONO, fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.08em', color: C.muted }}>or</span>
                <div style={{ height: 1, flex: 1, background: C.border }} />
              </div>
              <button style={{ width: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8,
                background: 'transparent', border: `1px solid ${C.borderS}`, borderRadius: 14, padding: '13px',
                fontSize: 15, fontWeight: 600, color: C.text, fontFamily: SANS,
                boxShadow: demoHot ? `0 0 0 3px ${C.accentBg}` : 'none' }}>
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4" /><polyline points="10 17 15 12 10 7" /><line x1="15" y1="12" x2="3" y2="12" /></svg>
                Continue as demo
              </button>
            </div>
          </div>
          <p style={{ fontFamily: MONO, fontSize: 12, color: C.muted, margin: 0 }}>© 2026 TechQuiz · v0.1.0</p>
        </div>

        {/* Right — hero */}
        <div style={{ position: 'relative', overflow: 'hidden', background: C.elevated, padding: '0 44px',
          display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
          <div style={{ position: 'absolute', top: -140, right: -120, width: 420, height: 420,
            background: `radial-gradient(circle, ${GLOW_A}, transparent 62%)` }} />
          <div style={{ position: 'absolute', bottom: -160, left: -140, width: 420, height: 420,
            background: `radial-gradient(circle, ${GLOW_V}, transparent 62%)` }} />
          <div style={{ position: 'relative', maxWidth: 360 }}>
            <p style={{ fontFamily: MONO, fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.14em', color: C.muted, margin: '0 0 12px' }}>Sharpen your skills</p>
            <h2 style={{ fontFamily: DISP, fontSize: 30, fontWeight: 800, letterSpacing: '-0.02em', lineHeight: 1.12, margin: '0 0 22px' }}>AI-generated quizzes that adapt to how you learn.</h2>
            {/* highest score card */}
            <div style={{ background: C.surface, border: `1px solid ${C.border}`, borderRadius: 20, padding: 18, boxShadow: SHADOW, marginBottom: 14 }}>
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                  <IconTile code="EF" size={42} />
                  <div>
                    <p style={{ fontFamily: MONO, fontSize: 11, textTransform: 'uppercase', letterSpacing: '0.08em', color: C.accentT, margin: '0 0 2px' }}>Highest score</p>
                    <p style={{ fontFamily: DISP, fontSize: 16, fontWeight: 700, margin: 0 }}>EF Core</p>
                  </div>
                </div>
                <span style={{ fontFamily: MONO, fontSize: 20, fontWeight: 700 }}>94%</span>
              </div>
              <div style={{ height: 9, borderRadius: 999, background: C.track, overflow: 'hidden' }}>
                <div style={{ height: '100%', width: '94%', borderRadius: 999, background: BRAND }} />
              </div>
            </div>
            {/* stat tiles */}
            <div style={{ display: 'flex', gap: 12 }}>
              {[['🔥 12', 'day streak'], ['182', 'Skill IQ'], ['84%', 'accuracy']].map(([v, l]) => (
                <div key={l} style={{ flex: 1, background: C.surface, border: `1px solid ${C.border}`, borderRadius: 16, padding: '14px 12px' }}>
                  <div style={{ fontFamily: DISP, fontSize: 22, fontWeight: 800 }}>{v}</div>
                  <div style={{ fontSize: 12, color: C.muted, marginTop: 2 }}>{l}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
      <Cursor keys={[
        { t: 0, x: 572, y: 274 }, { t: 1.4, x: 210, y: 274 }, { t: 2.6, x: 210, y: 434 },
        { t: 3.3, x: 210, y: 488, click: true }, { t: 4.1, x: 210, y: 488 },
      ]} />
    </ScreenFade>
  );
}

// ── Categories: tracks grid (categories-page.tsx TrackTile) ───────────────────
const TRACKS = [
  { code: '.NET', name: '.NET', desc: 'The C# language and runtime, ASP.NET Core, EF Core, ADO.NET, unit testing, and design patterns.', topics: 6, q: 118 },
  { code: 'DB', name: 'Databases', desc: 'Relational databases and SQL — DBMS concepts, schema design, normalization, querying, and DDL.', topics: 5, q: 74 },
  { code: 'FE', name: 'Front-End', desc: 'The JavaScript language, its async model, TypeScript’s type system, and core HTML and CSS.', topics: 4, q: 61 },
  { code: 'ENG', name: 'Engineering Practices', desc: 'Git and version control, CI/CD, and clean-code practices.', topics: 3, q: 39 },
];
function TracksScreen() {
  const { localTime } = useSprite();
  const hot = localTime > 2.9;
  return (
    <ScreenFade>
      <div style={{ width: '100%', height: '100%', background: C.base, overflow: 'hidden' }}>
        <AppNav active="Categories" />
        <div style={{ padding: '26px 34px' }}>
          <h1 style={{ fontFamily: DISP, fontSize: 32, fontWeight: 800, letterSpacing: '-0.02em', margin: '0 0 5px' }}>Pick a category</h1>
          <p style={{ fontSize: 15, color: C.text2, margin: '0 0 22px' }}>4 tracks · 18 topics — start where you feel strong, or challenge a weak spot.</p>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 14 }}>
            {TRACKS.map((tr, i) => {
              const isTarget = i === 0;
              const rise = kf(localTime, [0.2 + i * 0.07, 0.8 + i * 0.07], [16, 0], Easing.easeOutCubic);
              const op = appear(localTime, 0.2 + i * 0.07, 0.5);
              const active = isTarget && hot;
              return (
                <div key={tr.name} style={{ background: C.surface, border: `1px solid ${active ? C.borderS : C.border}`,
                  borderRadius: 20, padding: 20, opacity: op,
                  transform: `translateY(${rise}px)${active ? ' scale(1.015)' : ''}` }}>
                  <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 14 }}>
                    <IconTile code={tr.code} />
                    <Pill>{tr.topics} topics</Pill>
                  </div>
                  <h3 style={{ fontFamily: DISP, fontSize: 19, fontWeight: 700, margin: '0 0 4px' }}>{tr.name}</h3>
                  <p style={{ fontSize: 13.5, color: C.text2, lineHeight: 1.5, margin: '0 0 14px' }}>{tr.desc}</p>
                  <p style={{ fontFamily: MONO, fontSize: 12, color: C.muted, margin: 0 }}>{tr.q} questions →</p>
                </div>
              );
            })}
            {/* Practical Challenges dashed tile */}
            <div style={{ background: C.base, border: `1px dashed ${C.borderS}`, borderRadius: 20, padding: 20, opacity: appear(localTime, 0.55, 0.5) }}>
              <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 14 }}>
                <IconTile code="</>" muted />
                <Pill muted>hands-on</Pill>
              </div>
              <h3 style={{ fontFamily: DISP, fontSize: 19, fontWeight: 700, margin: '0 0 4px' }}>Practical Challenges</h3>
              <p style={{ fontSize: 13.5, color: C.text2, lineHeight: 1.5, margin: '0 0 14px' }}>Write and run code against automated tests instead of picking an answer.</p>
              <p style={{ fontFamily: MONO, fontSize: 12, color: C.muted, margin: 0 }}>Open editor →</p>
            </div>
          </div>
        </div>
      </div>
      <Cursor keys={[
        { t: 0, x: 722, y: 214 }, { t: 1.4, x: 189, y: 285 },
        { t: 2.8, x: 189, y: 285, click: true }, { t: 3.6, x: 189, y: 285 },
      ]} />
    </ScreenFade>
  );
}

// ── Categories drill-down: .NET track's category cards ────────────────────────
const DOTNET_CATS = [
  { code: 'C#', name: 'C#/.NET', desc: 'Reflection, serialization, threads, async/await, and the Task Parallel Library.', q: 22, score: 87 },
  { code: 'ASP', name: 'ASP.NET Core', desc: 'MVC, the middleware pipeline, DI, routing, minimal APIs, and auth.', q: 20, score: 78 },
  { code: 'EF', name: 'EF Core', desc: 'DbContext, migrations, model configuration, loading strategies, and change tracking.', q: 21, score: 94 },
  { code: 'ADO', name: 'ADO.NET', desc: 'The provider model, parameterized queries, transactions, and connection pooling.', q: 18, score: 71 },
  { code: 'test-tube', name: 'Unit Testing', desc: 'xUnit, NUnit, MSTest, the AAA pattern, test doubles, and Moq.', q: 17, score: 0 },
  { code: 'GoF', name: 'Design Patterns', desc: 'GoF creational, structural and behavioral patterns, SOLID, and Clean Architecture.', q: 20, score: 66 },
];
function CategoryCardsScreen() {
  const { localTime } = useSprite();
  const hot = localTime > 2.5;
  return (
    <ScreenFade>
      <div style={{ width: '100%', height: '100%', background: C.base, overflow: 'hidden' }}>
        <AppNav active="Categories" />
        <div style={{ padding: '22px 34px' }}>
          <div style={{ marginBottom: 16 }}>
            <div style={{ display: 'inline-flex', alignItems: 'center', gap: 6, fontSize: 14, fontWeight: 500, color: C.text2, marginBottom: 8 }}>← All tracks</div>
            <h1 style={{ fontFamily: DISP, fontSize: 30, fontWeight: 800, letterSpacing: '-0.02em', margin: '0 0 4px' }}>.NET</h1>
            <p style={{ fontSize: 14.5, color: C.text2, margin: 0 }}>The .NET platform end to end — the C# language and runtime, ASP.NET Core, EF Core, ADO.NET, testing, and patterns.</p>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 12 }}>
            {DOTNET_CATS.map((cat, i) => {
              const isTarget = i === 0;
              const rise = kf(localTime, [0.15 + i * 0.06, 0.75 + i * 0.06], [14, 0], Easing.easeOutCubic);
              const op = appear(localTime, 0.15 + i * 0.06, 0.5);
              const active = isTarget && hot;
              const started = cat.score > 0;
              return (
                <div key={cat.name} style={{ background: C.surface, border: `1px solid ${active ? C.borderS : C.border}`,
                  borderRadius: 20, padding: 18, opacity: op,
                  transform: `translateY(${rise}px)${active ? ' scale(1.02)' : ''}`,
                  boxShadow: active ? `0 0 0 3px ${C.accentBg}` : 'none' }}>
                  <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 14 }}>
                    <IconTile code={cat.code} size={42} />
                    <Pill>{cat.q} q</Pill>
                  </div>
                  <h3 style={{ fontFamily: DISP, fontSize: 18, fontWeight: 700, margin: '0 0 4px' }}>{cat.name}</h3>
                  <p style={{ fontSize: 13, color: C.text2, lineHeight: 1.45, margin: '0 0 14px' }}>{cat.desc}</p>
                  {started ? (
                    <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                      <div style={{ height: 7, borderRadius: 999, background: C.track, flex: 1, overflow: 'hidden' }}>
                        <div style={{ height: '100%', width: `${cat.score}%`, borderRadius: 999, background: BRAND }} />
                      </div>
                      <span style={{ fontFamily: MONO, fontSize: 13, fontWeight: 600, color: C.amber }}>{cat.score}%</span>
                    </div>
                  ) : (
                    <p style={{ fontFamily: MONO, fontSize: 12, color: C.muted, margin: 0 }}>Not started</p>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </div>
      <Cursor keys={[
        { t: 0, x: 692, y: 214 }, { t: 1.3, x: 189, y: 284 },
        { t: 2.4, x: 189, y: 284, click: true }, { t: 3.2, x: 189, y: 284 },
      ]} />
    </ScreenFade>
  );
}

// ── Quiz (quiz-page.tsx) ──────────────────────────────────────────────────────
const OPTIONS = ['private', 'internal', 'protected', 'public'];
function QuizScreen() {
  const { localTime } = useSprite();
  const selIdx = localTime > 3.2 ? 1 : -1;
  const progress = kf(localTime, [0.3, 1], [20, 30], Easing.easeOutCubic);
  const nextHot = localTime > 4.1;
  return (
    <ScreenFade>
      <div style={{ width: '100%', height: '100%', background: C.base, display: 'flex', flexDirection: 'column' }}>
        <div style={{ borderBottom: `1px solid ${C.border}`, background: C.surface, padding: '13px 26px', display: 'flex', alignItems: 'center', gap: 16 }}>
          <span style={{ fontFamily: MONO, fontSize: 13, fontWeight: 600, color: C.text2, whiteSpace: 'nowrap' }}>C#/.NET · 3 / 10</span>
          <div style={{ height: 7, borderRadius: 999, background: C.track, flex: 1, overflow: 'hidden' }}>
            <div style={{ height: '100%', width: `${progress}%`, borderRadius: 999, background: BRAND, transition: 'none' }} />
          </div>
          <div style={{ width: 34, height: 34, border: `1px solid ${C.borderS}`, borderRadius: 10, display: 'flex', alignItems: 'center', justifyContent: 'center', color: C.text2 }}>
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></svg>
          </div>
        </div>
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '14px 24px' }}>
          <div style={{ width: '100%', maxWidth: 600 }}>
            <span style={{ display: 'inline-block', fontFamily: MONO, fontSize: 11, fontWeight: 600, textTransform: 'uppercase',
              letterSpacing: '0.08em', color: C.amber, background: C.amberBg, padding: '5px 10px', borderRadius: 999, marginBottom: 12 }}>Medium</span>
            <h2 style={{ fontFamily: DISP, fontSize: 25, fontWeight: 800, letterSpacing: '-0.01em', lineHeight: 1.25, margin: '0 0 16px' }}>
              Which access modifier makes a member accessible only within the same assembly?</h2>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
              {OPTIONS.map((opt, i) => {
                const sel = i === selIdx;
                return (
                  <div key={opt} style={{ display: 'flex', alignItems: 'center', gap: 16, background: C.surface,
                    border: `1px solid ${sel ? C.accent : C.border}`, borderRadius: 16, padding: '12px 18px',
                    fontSize: 15, boxShadow: sel ? `0 0 0 3px ${C.accentBg}` : 'none' }}>
                    <span style={{ width: 28, height: 28, borderRadius: 9, display: 'flex', alignItems: 'center', justifyContent: 'center',
                      fontFamily: MONO, fontSize: 14, fontWeight: 700,
                      background: sel ? BTN : C.elevated, color: sel ? '#fff' : C.muted }}>{i + 1}</span>
                    <span>{opt}</span>
                  </div>
                );
              })}
            </div>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginTop: 16, paddingTop: 14, borderTop: `1px solid ${C.border}` }}>
              <p style={{ fontFamily: MONO, fontSize: 13, color: C.muted, margin: 0 }}>Press 1-4 to select, Enter to continue</p>
              <button style={{ display: 'flex', alignItems: 'center', gap: 6, background: BTN, color: '#fff', border: 'none',
                padding: '12px 24px', borderRadius: 999, fontSize: 15, fontWeight: 600, fontFamily: SANS, boxShadow: SHADOW,
                outline: nextHot ? `3px solid ${C.accentBg}` : 'none' }}>
                Next <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><line x1="5" y1="12" x2="19" y2="12" /><polyline points="12 5 19 12 12 19" /></svg>
              </button>
            </div>
          </div>
        </div>
      </div>
      <Cursor keys={[
        { t: 0, x: 722, y: 394 }, { t: 1.6, x: 512, y: 291 },
        { t: 3.1, x: 512, y: 291, click: true },
        { t: 4.4, x: 761, y: 495, click: true }, { t: 5.2, x: 761, y: 495 },
      ]} />
    </ScreenFade>
  );
}

// ── Result (result-page.tsx: circular score + reward tiles) ───────────────────
function ResultScreen() {
  const { localTime } = useSprite();
  const score = Math.round(kf(localTime, [0.5, 2], [0, 87], Easing.easeOutCubic));
  const discS = kf(localTime, [0.1, 0.8], [0.85, 1], Easing.easeOutBack);
  return (
    <ScreenFade>
      <div style={{ width: '100%', height: '100%', background: C.base, overflow: 'hidden', position: 'relative' }}>
        <AppNav active="Categories" />
        {/* glows */}
        <div style={{ position: 'absolute', left: '50%', top: 30, width: 720, height: 440, transform: 'translateX(-50%)',
          background: `radial-gradient(circle, ${GLOW_V}, transparent 65%)`, pointerEvents: 'none' }} />
        <div style={{ position: 'absolute', right: '10%', top: 60, width: 320, height: 320,
          background: `radial-gradient(circle, ${GLOW_A}, transparent 62%)`, pointerEvents: 'none' }} />
        <div style={{ position: 'relative', maxWidth: 680, margin: '0 auto', padding: '18px 24px', display: 'flex', flexDirection: 'column', alignItems: 'center', textAlign: 'center' }}>
          <span style={{ display: 'inline-flex', alignItems: 'center', gap: 8, background: C.amberBg, color: C.amber,
            padding: '7px 16px', borderRadius: 999, fontFamily: MONO, fontSize: 13, fontWeight: 600, marginBottom: 16 }}>Great work</span>
          {/* circular score */}
          <div style={{ width: 150, height: 150, borderRadius: '50%', border: `1px solid ${C.border}`, background: C.surface,
            boxShadow: SHADOW, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
            transform: `scale(${discS})` }}>
            <span style={{ fontFamily: DISP, fontSize: 52, fontWeight: 800, letterSpacing: '-0.03em', lineHeight: 1 }}>{score}%</span>
            <span style={{ fontFamily: MONO, fontSize: 13, color: C.muted, marginTop: 6 }}>13 / 15 correct</span>
          </div>
          <h1 style={{ fontFamily: DISP, fontSize: 34, fontWeight: 800, letterSpacing: '-0.02em', lineHeight: 1.1, margin: '20px 0 6px' }}>Quiz complete! 🎉</h1>
          <p style={{ fontSize: 16, color: C.text2, margin: '0 0 20px', opacity: appear(localTime, 1.4, 0.5) }}>
            Great run on <b style={{ color: C.text }}>C#/.NET</b> — up 5% from last time.</p>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 12, width: '100%', maxWidth: 480 }}>
            {[['13/15', 'Correct', C.amber, DISP], ['4:12', 'Time', C.text, MONO], ['+5%', 'vs last', C.success, DISP]].map(([v, l, col, ff], i) => (
              <div key={l} style={{ background: C.surface, border: `1px solid ${C.border}`, borderRadius: 16, padding: 16,
                opacity: appear(localTime, 1.0 + i * 0.14, 0.5),
                transform: `translateY(${kf(localTime, [1.0 + i * 0.14, 1.5 + i * 0.14], [10, 0], Easing.easeOutCubic)}px)` }}>
                <div style={{ fontFamily: ff, fontSize: 22, fontWeight: 800, color: col }}>{v}</div>
                <div style={{ fontFamily: MONO, fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.08em', color: C.muted, marginTop: 4 }}>{l}</div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </ScreenFade>
  );
}

// ── Intro ────────────────────────────────────────────────────────────────────
function Intro() {
  const { localTime } = useSprite();
  const logoS = kf(localTime, [0.1, 0.9], [0.4, 1], Easing.easeOutBack);
  const logoO = appear(localTime, 0.1, 0.4);
  const nameO = appear(localTime, 0.7, 0.5);
  const nameX = kf(localTime, [0.7, 1.3], [-14, 0], Easing.easeOutCubic);
  const tagO = appear(localTime, 1.3, 0.6);
  return (
    <div style={{ position: 'absolute', inset: 0, background: C.base, color: C.text, display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center' }}>
      <div style={{ position: 'absolute', width: 640, height: 640, borderRadius: '50%', background: `radial-gradient(circle, ${GLOW_V}, transparent 60%)` }} />
      <div style={{ position: 'absolute', width: 520, height: 520, borderRadius: '50%', top: '58%', left: '58%', background: `radial-gradient(circle, ${GLOW_A}, transparent 62%)` }} />
      <div style={{ display: 'flex', alignItems: 'center', gap: 22, position: 'relative' }}>
        <div style={{ width: 96, height: 96, background: BRAND, borderRadius: 24, display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontFamily: DISP, fontWeight: 800, fontSize: 56, color: C.brandfg, opacity: logoO, transform: `scale(${logoS})`,
          boxShadow: '0 24px 70px rgba(167,139,250,0.35)' }}>T</div>
        <div style={{ opacity: nameO, transform: `translateX(${nameX}px)` }}>
          <div style={{ fontFamily: DISP, fontSize: 62, fontWeight: 800, letterSpacing: '-0.03em', color: C.text, lineHeight: 1 }}>TechQuiz</div>
        </div>
      </div>
      <p style={{ marginTop: 26, fontSize: 20, color: C.text2, opacity: tagO, letterSpacing: '-0.01em', position: 'relative', maxWidth: 620, textAlign: 'center', lineHeight: 1.4 }}>
        AI-generated quizzes that adapt to <span style={{ color: C.accentT }}>how you learn</span>
      </p>
    </div>
  );
}

// ── Caption ──────────────────────────────────────────────────────────────────
function Caption({ text }) {
  const { localTime, duration } = useSprite();
  const o = Math.min(appear(localTime, 0.15, 0.4), 1 - clamp((localTime - (duration - 0.4)) / 0.4, 0, 1));
  const y = kf(localTime, [0.15, 0.7], [12, 0], Easing.easeOutCubic);
  return (
    <div style={{ position: 'absolute', left: 0, right: 0, bottom: 24, display: 'flex', justifyContent: 'center', opacity: o, transform: `translateY(${y}px)` }}>
      <div style={{ background: 'rgba(34,26,46,0.92)', border: `1px solid ${C.borderS}`, borderRadius: 999, padding: '8px 20px',
        fontFamily: MONO, fontSize: 14, color: C.accentT, letterSpacing: '0.02em', boxShadow: SHADOW }}>{text}</div>
    </div>
  );
}

// ── Architecture reveal ──────────────────────────────────────────────────────
const NODES = {
  fe:  { x: 74,  y: 296, w: 250, h: 132 },
  api: { x: 500, y: 246, w: 280, h: 232 },
  db:  { x: 956, y: 206, w: 250, h: 92 },
  seq: { x: 956, y: 322, w: 250, h: 92 },
  ai:  { x: 956, y: 438, w: 250, h: 92 },
};
const cy = (n) => n.y + n.h / 2;
function FlowLine({ from, to, color, appearAt, label, labelY }) {
  const t = useTime();
  const { localTime } = useSprite();
  const a = clamp((localTime - appearAt) / 0.6, 0, 1);
  const { x1, y1, x2, y2 } = from;
  const dots = [];
  if (localTime > appearAt + 0.4) for (let i = 0; i < 3; i++) {
    const p = ((t * 0.55) + i / 3) % 1;
    dots.push({ x: x1 + (x2 - x1) * p, y: y1 + (y2 - y1) * p, o: Math.sin(p * Math.PI) });
  }
  return (
    <React.Fragment>
      <line x1={x1} y1={y1} x2={x1 + (x2 - x1) * a} y2={y1 + (y2 - y1) * a} stroke={color} strokeWidth="1.5" strokeOpacity="0.5" strokeDasharray="5 5" />
      {dots.map((d, i) => <circle key={i} cx={d.x} cy={d.y} r="3.5" fill={color} opacity={d.o} />)}
      {label && a > 0.9 && <text x={(x1 + x2) / 2} y={labelY} fill={C.text2} fontSize="11" fontFamily={MONO} textAnchor="middle">{label}</text>}
    </React.Fragment>
  );
}
function ArchNode({ n, appearAt, children }) {
  const { localTime } = useSprite();
  const o = appear(localTime, appearAt, 0.5);
  const s = kf(localTime, [appearAt, appearAt + 0.5], [0.9, 1], Easing.easeOutBack);
  return <div style={{ position: 'absolute', left: n.x, top: n.y, width: n.w, height: n.h, opacity: o, transform: `scale(${s})`, transformOrigin: 'center' }}>{children}</div>;
}
function Architecture() {
  const { localTime } = useSprite();
  const layers = ['Domain', 'Application', 'Infrastructure', 'API'];
  const rightNode = (title, sub, color) => (
    <div style={{ width: '100%', height: '100%', background: C.surface, border: `1px solid ${C.border}`, borderRadius: 16,
      padding: '14px 18px', display: 'flex', flexDirection: 'column', justifyContent: 'center', boxShadow: SHADOW }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        <div style={{ width: 8, height: 8, borderRadius: 2, background: color }} />
        <span style={{ fontFamily: DISP, fontSize: 16, fontWeight: 700 }}>{title}</span>
      </div>
      <span style={{ fontFamily: MONO, fontSize: 11, color: C.text2, marginTop: 6 }}>{sub}</span>
    </div>
  );
  return (
    <div style={{ position: 'absolute', inset: 0, background: C.base, color: C.text }}>
      <div style={{ position: 'absolute', left: '50%', top: 20, width: 760, height: 420, transform: 'translateX(-50%)', background: `radial-gradient(circle, ${GLOW_V}, transparent 68%)` }} />
      <svg width="1280" height="720" style={{ position: 'absolute', inset: 0 }}>
        <FlowLine from={{ x1: NODES.fe.x + NODES.fe.w, y1: cy(NODES.fe), x2: NODES.api.x, y2: cy(NODES.fe) }} color={C.accent} appearAt={1.2} label="HTTPS · JWT" labelY={cy(NODES.fe) - 12} />
        <FlowLine from={{ x1: NODES.api.x + NODES.api.w, y1: cy(NODES.api) - 42, x2: NODES.db.x, y2: cy(NODES.db) }} color={C.success} appearAt={2.6} />
        <FlowLine from={{ x1: NODES.api.x + NODES.api.w, y1: cy(NODES.api), x2: NODES.seq.x, y2: cy(NODES.seq) }} color={C.amber} appearAt={3.4} />
        <FlowLine from={{ x1: NODES.api.x + NODES.api.w, y1: cy(NODES.api) + 42, x2: NODES.ai.x, y2: cy(NODES.ai) }} color={C.accentT} appearAt={4.2} />
      </svg>

      <div style={{ position: 'absolute', top: 66, left: 0, right: 0, textAlign: 'center', opacity: appear(localTime, 0, 0.5),
        transform: `translateY(${kf(localTime, [0, 0.6], [-12, 0], Easing.easeOutCubic)}px)` }}>
        <p style={{ fontFamily: MONO, fontSize: 12, color: C.accentT, textTransform: 'uppercase', letterSpacing: '0.2em', margin: '0 0 6px' }}>Under the hood</p>
        <h2 style={{ fontFamily: DISP, fontSize: 32, fontWeight: 800, letterSpacing: '-0.02em', color: C.text, margin: 0 }}>How TechQuiz works</h2>
      </div>

      <ArchNode n={NODES.fe} appearAt={0.5}>
        <div style={{ width: '100%', height: '100%', background: C.surface, border: `1px solid ${C.border}`, borderRadius: 16, padding: 16, display: 'flex', flexDirection: 'column', justifyContent: 'center', boxShadow: SHADOW }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
            <div style={{ width: 30, height: 30, background: C.accentBg, borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center', color: C.accentT, fontFamily: MONO, fontWeight: 700, fontSize: 12 }}>‹/›</div>
            <span style={{ fontFamily: DISP, fontSize: 16, fontWeight: 700 }}>React Frontend</span>
          </div>
          <span style={{ fontFamily: MONO, fontSize: 11, color: C.text2, lineHeight: 1.6 }}>React 19 · TypeScript<br />Vite · TanStack Query</span>
        </div>
      </ArchNode>

      <ArchNode n={NODES.api} appearAt={1.2}>
        <div style={{ width: '100%', height: '100%', background: C.surface, border: `1px solid ${C.borderS}`, borderRadius: 16, padding: 16, display: 'flex', flexDirection: 'column', boxShadow: SHADOW }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 12 }}>
            <div style={{ width: 30, height: 30, background: BTN, borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#fff', fontFamily: DISP, fontWeight: 800, fontSize: 12 }}>.N</div>
            <div>
              <div style={{ fontFamily: DISP, fontSize: 16, fontWeight: 700, lineHeight: 1.1 }}>ASP.NET Core API</div>
              <div style={{ fontFamily: MONO, fontSize: 10, color: C.text2 }}>Clean Architecture · CQRS</div>
            </div>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            {layers.map((l, i) => {
              const inner = i === 0;
              return (
                <div key={l} style={{ background: inner ? C.accentBg : C.base, border: `1px solid ${inner ? 'rgba(167,139,250,0.4)' : C.border}`, borderRadius: 8,
                  padding: '7px 12px', fontFamily: MONO, fontSize: 12, color: inner ? C.accentT : C.text2,
                  opacity: appear(localTime, 1.6 + i * 0.18, 0.4),
                  transform: `translateX(${kf(localTime, [1.6 + i * 0.18, 2 + i * 0.18], [10, 0], Easing.easeOutCubic)}px)` }}>{l}</div>
              );
            })}
          </div>
        </div>
      </ArchNode>

      <ArchNode n={NODES.db} appearAt={2.6}>{rightNode('PostgreSQL', 'EF Core 9', C.success)}</ArchNode>
      <ArchNode n={NODES.seq} appearAt={3.4}>{rightNode('Seq', 'Structured logs', C.amber)}</ArchNode>
      <ArchNode n={NODES.ai} appearAt={4.2}>{rightNode('AI Provider', 'OpenAI · Claude', C.accentT)}</ArchNode>

      <div style={{ position: 'absolute', bottom: 44, left: 0, right: 0, textAlign: 'center', opacity: appear(localTime, 5, 0.6) }}>
        <span style={{ fontFamily: MONO, fontSize: 13, color: C.text2 }}>JWT auth · MediatR · encrypted per-user API keys · Docker · CI/CD</span>
      </div>
    </div>
  );
}

// ── Outro ────────────────────────────────────────────────────────────────────
function Outro() {
  const { localTime } = useSprite();
  const chips = ['ASP.NET Core 9', 'EF Core', 'PostgreSQL', 'React 19', 'TypeScript', 'MediatR', 'Docker', 'xUnit'];
  return (
    <div style={{ position: 'absolute', inset: 0, background: C.base, color: C.text, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
      <div style={{ position: 'absolute', width: 620, height: 620, borderRadius: '50%', background: `radial-gradient(circle, ${GLOW_V}, transparent 60%)` }} />
      <div style={{ position: 'absolute', width: 460, height: 460, borderRadius: '50%', top: '60%', left: '60%', background: `radial-gradient(circle, ${GLOW_A}, transparent 62%)` }} />
      <div style={{ display: 'flex', alignItems: 'center', gap: 16, opacity: appear(localTime, 0.1, 0.5), transform: `scale(${kf(localTime, [0.1, 0.7], [0.9, 1], Easing.easeOutBack)})`, position: 'relative' }}>
        <div style={{ width: 56, height: 56, background: BRAND, borderRadius: 14, display: 'flex', alignItems: 'center', justifyContent: 'center', fontFamily: DISP, fontWeight: 800, fontSize: 32, color: C.brandfg }}>T</div>
        <span style={{ fontFamily: DISP, fontSize: 42, fontWeight: 800, letterSpacing: '-0.03em', color: C.text }}>TechQuiz</span>
      </div>
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, justifyContent: 'center', maxWidth: 560, marginTop: 28, opacity: appear(localTime, 0.6, 0.6), position: 'relative' }}>
        {chips.map((c, i) => (
          <span key={c} style={{ fontFamily: MONO, fontSize: 13, color: C.accentT, background: C.accentBg, border: '1px solid rgba(167,139,250,0.25)', borderRadius: 999, padding: '6px 14px',
            opacity: appear(localTime, 0.7 + i * 0.05, 0.4), transform: `translateY(${kf(localTime, [0.7 + i * 0.05, 1.1 + i * 0.05], [8, 0], Easing.easeOutCubic)}px)` }}>{c}</span>
        ))}
      </div>
      <div style={{ marginTop: 34, textAlign: 'center', opacity: appear(localTime, 1.5, 0.6), position: 'relative' }}>
        <p style={{ fontFamily: DISP, fontSize: 18, fontWeight: 700, color: C.text, margin: '0 0 4px' }}>Bartosz Clapinski</p>
        <p style={{ fontFamily: MONO, fontSize: 13, color: C.text2, margin: 0 }}>github.com/bartoszclapinski/TechQuiz</p>
      </div>
    </div>
  );
}

// ── Root ─────────────────────────────────────────────────────────────────────
function currentUrl(t) {
  if (t < 8.1) return 'techquiz-web.onrender.com/login';
  if (t < 16.3) return 'techquiz-web.onrender.com/categories';
  if (t < 22.0) return 'techquiz-web.onrender.com/quiz';
  return 'techquiz-web.onrender.com/result';
}
function ProductWalkthrough() {
  const { localTime, duration } = useSprite();
  const t = useTime();
  const frameO = Math.min(appear(localTime, 0, 0.4), 1 - clamp((localTime - (duration - 0.4)) / 0.4, 0, 1));
  return (
    <React.Fragment>
      <BrowserFrame url={currentUrl(t)} opacity={frameO} />
      <Sprite start={3.2} end={8.1}><LoginScreen /></Sprite>
      <Sprite start={7.9} end={12.3}><TracksScreen /></Sprite>
      <Sprite start={12.1} end={16.3}><CategoryCardsScreen /></Sprite>
      <Sprite start={16.1} end={22.1}><QuizScreen /></Sprite>
      <Sprite start={21.9} end={27.6}><ResultScreen /></Sprite>
    </React.Fragment>
  );
}
function TechQuizScene(props) {
  props = props || {};
  applyTheme(props.accent);
  SHOW_CURSOR = props.showCursor !== false;
  const caps = props.showCaptions !== false;
  const arch = props.showArchitecture !== false;
  const outroStart = arch ? 38.2 : 27.7;
  const outroEnd = outroStart + 5.8;
  return (
    <Stage width={1280} height={720} duration={outroEnd} background={C.base} persistKey="techquiz">
      <Sprite start={0} end={3.4}><Intro /></Sprite>

      <Sprite start={3.0} end={27.7}><ProductWalkthrough /></Sprite>
      {caps && <Sprite start={3.4} end={7.9}><Caption text="Sign in with the demo account" /></Sprite>}
      {caps && <Sprite start={8.1} end={12.1}><Caption text="Browse tracks" /></Sprite>}
      {caps && <Sprite start={12.3} end={16.1}><Caption text="Pick a category" /></Sprite>}
      {caps && <Sprite start={16.3} end={21.9}><Caption text="Answer questions" /></Sprite>}
      {caps && <Sprite start={22.1} end={27.5}><Caption text="See your score & review" /></Sprite>}

      {arch && <Sprite start={27.7} end={38.2}><Architecture /></Sprite>}
      <Sprite start={outroStart} end={outroEnd}><Outro /></Sprite>
    </Stage>
  );
}
window.TechQuizScene = TechQuizScene;
