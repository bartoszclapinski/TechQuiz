/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  darkMode: ['class', '[data-theme="dark"]'],
  theme: {
    extend: {
      colors: {
        base: 'var(--bg-base)',
        surface: 'var(--bg-surface)',
        elevated: 'var(--bg-elevated)',
        primary: 'var(--text-primary)',
        secondary: 'var(--text-secondary)',
        muted: 'var(--text-muted)',
        accent: {
          DEFAULT: 'var(--accent)',
          bg: 'var(--accent-bg)',
          text: 'var(--accent-text)',
        },
        amber: {
          text: 'var(--amber-text)',
          bg: 'var(--amber-bg)',
        },
        brandfg: 'var(--brand-fg)',
        track: 'var(--track)',
        success: 'var(--success)',
        warning: 'var(--warning)',
        danger: 'var(--danger)',
      },
      borderColor: {
        DEFAULT: 'var(--border-default)',
        default: 'var(--border-default)',
        strong: 'var(--border-strong)',
      },
      backgroundImage: {
        brand: 'var(--brand)',
        btn: 'var(--btn)',
        'card-grad': 'var(--card-grad)',
      },
      boxShadow: {
        float: 'var(--shadow)',
        focus: '0 0 0 3px var(--focus-ring)',
      },
      borderRadius: {
        pill: '999px',
      },
      fontFamily: {
        display: ['"Bricolage Grotesque"', 'Geist', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        sans: ['Geist', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', 'monospace'],
      },
    },
  },
  plugins: [],
}
