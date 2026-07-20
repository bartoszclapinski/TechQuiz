// Full deterministic render of the DC animation -> master MP4.
// Frame-steps the runtime via its export protocol and pipes PNGs straight to ffmpeg.
const http = require('http');
const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');
// Resolved from the repo's own node_modules by walking up from this file.
const { chromium } = require('playwright');

const ROOT = path.join(__dirname, 'Project workflow animation');
const OUT = process.argv[2] || path.join(__dirname, 'master.mp4');
const FPS = Number(process.argv[3] || 30);

const MIME = { '.html': 'text/html', '.js': 'text/javascript', '.jsx': 'text/javascript', '.png': 'image/png' };

function serve() {
  return new Promise((resolve) => {
    const server = http.createServer((req, res) => {
      const rel = decodeURIComponent(req.url.split('?')[0]);
      const file = path.join(ROOT, rel === '/' ? '/TechQuiz Walkthrough.dc.html' : rel);
      fs.readFile(file, (err, data) => {
        if (err) { res.writeHead(404); return res.end('nope'); }
        res.writeHead(200, { 'Content-Type': MIME[path.extname(file).toLowerCase()] || 'application/octet-stream' });
        res.end(data);
      });
    });
    server.listen(0, () => resolve(server));
  });
}

const write = (stream, buf) =>
  stream.write(buf) ? Promise.resolve() : new Promise((r) => stream.once('drain', r));

(async () => {
  const server = await serve();
  const port = server.address().port;
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1600, height: 1100 }, deviceScaleFactor: 1 });

  await page.goto(`http://127.0.0.1:${port}/TechQuiz%20Walkthrough.dc.html`, { waitUntil: 'networkidle' });
  const sel = 'svg[data-om-exportable-video-with-duration-secs]';
  await page.waitForSelector(sel, { timeout: 60000 });
  const duration = Number(await page.getAttribute(sel, 'data-om-exportable-video-with-duration-secs'));

  await page.$eval(sel, (el) => { el.style.transform = 'none'; el.style.boxShadow = 'none'; });
  await page.evaluate(() => document.fonts.ready);
  await page.waitForTimeout(800);

  const total = Math.round(duration * FPS);
  console.log(`duration=${duration}s fps=${FPS} frames=${total}`);

  const ff = spawn('ffmpeg', [
    '-y', '-f', 'image2pipe', '-framerate', String(FPS), '-i', '-',
    // Element screenshots can land on an odd height (1280x721); H.264 needs even dims.
    '-vf', 'crop=trunc(iw/2)*2:trunc(ih/2)*2',
    '-c:v', 'libx264', '-preset', 'slow', '-crf', '18',
    '-pix_fmt', 'yuv420p', '-movflags', '+faststart',
    OUT,
  ], { stdio: ['pipe', 'ignore', 'pipe'] });
  let ffErr = '';
  ff.stderr.on('data', (d) => { ffErr += d.toString(); });

  const el = await page.$(sel);
  for (let i = 0; i < total; i++) {
    const t = i / FPS;
    await page.$eval(sel, (node, time) => {
      node.dispatchEvent(new CustomEvent('data-om-seek-to-time-frame', { detail: { time } }));
    }, t);
    await page.evaluate(() => new Promise((r) => requestAnimationFrame(() => requestAnimationFrame(r))));
    const buf = await el.screenshot({ type: 'png' });
    await write(ff.stdin, buf);
    if (i % 60 === 0) console.log(`  frame ${i}/${total} (t=${t.toFixed(1)}s)`);
  }

  ff.stdin.end();
  await new Promise((resolve, reject) => {
    ff.on('close', (code) => (code === 0 ? resolve() : reject(new Error('ffmpeg failed:\n' + ffErr.slice(-2000)))));
  });

  await browser.close();
  server.close();
  console.log('done ->', OUT);
})();
