#!/usr/bin/env bash
# Derive all publishable variants from the master render.
# Master: 1280x720 @30fps, 44s, crf18.
set -e
cd "$(dirname "$0")"

M=master.mp4
OUT=out
mkdir -p "$OUT"

# README cut: product walkthrough (login -> tracks -> category -> quiz -> result)
# PLUS the architecture reveal. Intro ends ~3.0s; architecture runs 27.7 -> 38.2s.
# Outro (38.2+) is left out — the diagram is the stronger ending for a looping GIF.
SEG_START=2.6
SEG_DUR=35.6

echo "== 1. Web video: full walkthrough incl. architecture reveal (mp4, h264) =="
ffmpeg -y -loglevel error -i "$M" \
  -c:v libx264 -preset slow -crf 24 -pix_fmt yuv420p -movflags +faststart -an \
  "$OUT/techquiz-walkthrough.mp4"

echo "== 2. Poster frame for the <video> element =="
ffmpeg -y -loglevel error -ss 5 -i "$M" -frames:v 1 "$OUT/techquiz-poster.png"

echo "== 3. README GIF: walkthrough + architecture, 1.6x speed, 12fps, 800px, palette-optimised =="
ffmpeg -y -loglevel error -ss "$SEG_START" -t "$SEG_DUR" -i "$M" \
  -vf "setpts=PTS/1.6,fps=12,scale=800:-2:flags=lanczos,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=3:diff_mode=rectangle" \
  "$OUT/techquiz-demo.gif"

echo "== 4. Animated WebP (same cut) — kept for size comparison, not published =="
ffmpeg -y -loglevel error -ss "$SEG_START" -t "$SEG_DUR" -i "$M" \
  -vf "setpts=PTS/1.6,fps=15,scale=960:-2:flags=lanczos" \
  -c:v libwebp -lossless 0 -q:v 45 -compression_level 6 -loop 0 -an \
  "$OUT/techquiz-demo.webp"

echo
echo "== sizes =="
ls -la "$OUT"
du -h "$OUT"/* | sort -h
