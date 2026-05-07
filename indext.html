<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>MiniChat — Demo</title>
<link href="https://fonts.googleapis.com/css2?family=Syne:wght@400;600;700;800&family=DM+Mono:wght@300;400&display=swap" rel="stylesheet">
<style>
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

  :root {
    --bg: #0a0a0f;
    --surface: #111118;
    --card: #16161f;
    --border: rgba(255,255,255,0.07);
    --accent: #4A90D9;
    --accent2: #7B5EFF;
    --text: #f0f0f5;
    --muted: #666680;
    --tag-bg: rgba(74,144,217,0.12);
    --tag-color: #82bcff;
  }

  html { scroll-behavior: smooth; }

  body {
    font-family: 'DM Mono', monospace;
    background: var(--bg);
    color: var(--text);
    min-height: 100vh;
    overflow-x: hidden;
  }

  /* Grid background */
  body::before {
    content: '';
    position: fixed;
    inset: 0;
    background-image:
      linear-gradient(rgba(74,144,217,0.03) 1px, transparent 1px),
      linear-gradient(90deg, rgba(74,144,217,0.03) 1px, transparent 1px);
    background-size: 40px 40px;
    pointer-events: none;
    z-index: 0;
  }

  /* Glow blob */
  body::after {
    content: '';
    position: fixed;
    top: -200px;
    left: 50%;
    transform: translateX(-50%);
    width: 800px;
    height: 500px;
    background: radial-gradient(ellipse, rgba(74,144,217,0.08) 0%, transparent 70%);
    pointer-events: none;
    z-index: 0;
  }

  .wrap {
    position: relative;
    z-index: 1;
    max-width: 900px;
    margin: 0 auto;
    padding: 0 24px;
  }

  /* ── HEADER ── */
  header {
    padding: 80px 0 60px;
    text-align: center;
  }

  .badge {
    display: inline-block;
    font-size: 11px;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--tag-color);
    background: var(--tag-bg);
    border: 1px solid rgba(74,144,217,0.2);
    padding: 5px 14px;
    border-radius: 100px;
    margin-bottom: 28px;
  }

  h1 {
    font-family: 'Syne', sans-serif;
    font-size: clamp(48px, 8vw, 80px);
    font-weight: 800;
    line-height: 1;
    letter-spacing: -0.03em;
    margin-bottom: 20px;
  }

  h1 span {
    color: var(--accent);
  }

  .subtitle {
    font-size: 15px;
    color: var(--muted);
    max-width: 460px;
    margin: 0 auto 36px;
    line-height: 1.7;
    font-weight: 300;
  }

  .header-tags {
    display: flex;
    gap: 8px;
    justify-content: center;
    flex-wrap: wrap;
  }

  .tag {
    font-size: 11px;
    letter-spacing: 0.05em;
    color: var(--muted);
    background: var(--surface);
    border: 1px solid var(--border);
    padding: 4px 12px;
    border-radius: 100px;
  }

  /* ── DIVIDER ── */
  .divider {
    height: 1px;
    background: linear-gradient(90deg, transparent, var(--border), transparent);
    margin: 0 0 70px;
  }

  /* ── SECTION LABEL ── */
  .section-label {
    font-size: 11px;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: var(--muted);
    margin-bottom: 32px;
    display: flex;
    align-items: center;
    gap: 12px;
  }
  .section-label::after {
    content: '';
    flex: 1;
    height: 1px;
    background: var(--border);
  }

  /* ── DEMO CARDS ── */
  .demos {
    display: flex;
    flex-direction: column;
    gap: 32px;
    margin-bottom: 80px;
  }

  .demo-card {
    background: var(--card);
    border: 1px solid var(--border);
    border-radius: 16px;
    overflow: hidden;
    transition: border-color 0.3s, transform 0.3s;
    animation: fadeUp 0.5s ease both;
  }
  .demo-card:nth-child(1) { animation-delay: 0.1s; }
  .demo-card:nth-child(2) { animation-delay: 0.2s; }
  .demo-card:nth-child(3) { animation-delay: 0.3s; }

  .demo-card:hover {
    border-color: rgba(74,144,217,0.3);
    transform: translateY(-2px);
  }

  .card-header {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 20px 24px;
    border-bottom: 1px solid var(--border);
  }

  .card-num {
    font-family: 'Syne', sans-serif;
    font-size: 11px;
    font-weight: 700;
    letter-spacing: 0.1em;
    color: var(--accent);
    background: rgba(74,144,217,0.1);
    border: 1px solid rgba(74,144,217,0.2);
    width: 28px;
    height: 28px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  .card-title {
    font-family: 'Syne', sans-serif;
    font-size: 17px;
    font-weight: 700;
    letter-spacing: -0.01em;
    flex: 1;
  }

  .card-icon {
    font-size: 18px;
    opacity: 0.4;
  }

  /* Video wrapper */
  .video-wrap {
    position: relative;
    background: #08080e;
  }

  .video-wrap a {
    display: block;
    text-decoration: none;
  }

  .video-thumb {
    position: relative;
    aspect-ratio: 16/9;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
  }

  /* Fake thumbnail gradient per card */
  .demo-card:nth-child(1) .video-thumb { background: linear-gradient(135deg, #0d1a2e 0%, #0a1525 100%); }
  .demo-card:nth-child(2) .video-thumb { background: linear-gradient(135deg, #0e1a0e 0%, #0a1a10 100%); }
  .demo-card:nth-child(3) .video-thumb { background: linear-gradient(135deg, #1a0d1a 0%, #140a1e 100%); }

  .play-btn {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: var(--accent);
    display: flex;
    align-items: center;
    justify-content: center;
    transition: transform 0.2s, background 0.2s;
    position: relative;
    z-index: 2;
    box-shadow: 0 0 40px rgba(74,144,217,0.4);
  }

  .video-wrap a:hover .play-btn {
    transform: scale(1.1);
    background: #6aaff0;
  }

  .play-btn svg {
    width: 24px;
    height: 24px;
    fill: white;
    margin-left: 4px;
  }

  .video-label {
    position: absolute;
    bottom: 16px;
    left: 16px;
    font-size: 11px;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: rgba(255,255,255,0.4);
    background: rgba(0,0,0,0.5);
    padding: 4px 10px;
    border-radius: 100px;
    backdrop-filter: blur(4px);
  }

  /* decorative lines on thumbnail */
  .video-thumb::before {
    content: '';
    position: absolute;
    inset: 0;
    background-image:
      linear-gradient(rgba(255,255,255,0.015) 1px, transparent 1px),
      linear-gradient(90deg, rgba(255,255,255,0.015) 1px, transparent 1px);
    background-size: 20px 20px;
  }

  .card-footer {
    padding: 14px 24px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-top: 1px solid var(--border);
  }

  .card-desc {
    font-size: 12px;
    color: var(--muted);
    font-weight: 300;
  }

  .open-link {
    font-size: 11px;
    letter-spacing: 0.05em;
    color: var(--accent);
    text-decoration: none;
    display: flex;
    align-items: center;
    gap: 5px;
    opacity: 0.7;
    transition: opacity 0.2s;
  }
  .open-link:hover { opacity: 1; }
  .open-link svg { width: 12px; height: 12px; }

  /* ── FOOTER ── */
  footer {
    border-top: 1px solid var(--border);
    padding: 32px 0;
    text-align: center;
  }

  footer p {
    font-size: 12px;
    color: var(--muted);
    font-weight: 300;
  }

  footer a {
    color: var(--accent);
    text-decoration: none;
  }

  @keyframes fadeUp {
    from { opacity: 0; transform: translateY(20px); }
    to   { opacity: 1; transform: translateY(0); }
  }

  header { animation: fadeUp 0.6s ease both; }
</style>
</head>
<body>
<div class="wrap">

  <header>
    <div class="badge">C# · TCP/IP · .NET</div>
    <h1>Mini<span>Chat</span></h1>
    <p class="subtitle">A console-based TCP chatting application supporting real-time messaging, file transfer, and voice recording.</p>
    <div class="header-tags">
      <span class="tag">TCP Sockets</span>
      <span class="tag">Binary Protocol</span>
      <span class="tag">NAudio</span>
      <span class="tag">Entity Framework</span>
      <span class="tag">SMTP Notifications</span>
    </div>
  </header>

  <div class="divider"></div>

  <p class="section-label">Feature demos</p>

  <div class="demos">

    <!-- Card 1: File Send -->
    <div class="demo-card">
      <div class="card-header">
        <div class="card-num">01</div>
        <span class="card-title">File Transfer</span>
        <span class="card-icon">📁</span>
      </div>
      <div class="video-wrap">
        <a href="https://drive.google.com/file/d/1pOuItX1BO2NGbVJn6eBk-OAbWHYRYljF/view?usp=sharing" target="_blank" rel="noopener">
          <div class="video-thumb">
            <div class="play-btn">
              <svg viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
            </div>
            <span class="video-label">Watch demo</span>
          </div>
        </a>
      </div>
      <div class="card-footer">
        <span class="card-desc">Send any file to another user over TCP with progress tracking</span>
        <a class="open-link" href="https://drive.google.com/file/d/1pOuItX1BO2NGbVJn6eBk-OAbWHYRYljF/view?usp=sharing" target="_blank" rel="noopener">
          Open
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>
        </a>
      </div>
    </div>

    <!-- Card 2: Text Message -->
    <div class="demo-card">
      <div class="card-header">
        <div class="card-num">02</div>
        <span class="card-title">Text Messaging</span>
        <span class="card-icon">💬</span>
      </div>
      <div class="video-wrap">
        <a href="https://drive.google.com/file/d/1dOPhiFcrLyvjGWYwfA0yQc1QxSSzUkdf/view?usp=sharing" target="_blank" rel="noopener">
          <div class="video-thumb">
            <div class="play-btn">
              <svg viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
            </div>
            <span class="video-label">Watch demo</span>
          </div>
        </a>
      </div>
      <div class="card-footer">
        <span class="card-desc">Real-time chat with message history, read receipts & email notifications</span>
        <a class="open-link" href="https://drive.google.com/file/d/1dOPhiFcrLyvjGWYwfA0yQc1QxSSzUkdf/view?usp=sharing" target="_blank" rel="noopener">
          Open
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>
        </a>
      </div>
    </div>

    <!-- Card 3: Voice -->
    <div class="demo-card">
      <div class="card-header">
        <div class="card-num">03</div>
        <span class="card-title">Voice Messages</span>
        <span class="card-icon">🎙️</span>
      </div>
      <div class="video-wrap">
        <a href="https://drive.google.com/file/d/1JIKgtkVQYXesEOnmexvJV3cvaKuNTQEU/view?usp=drive_link" target="_blank" rel="noopener">
          <div class="video-thumb">
            <div class="play-btn">
              <svg viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
            </div>
            <span class="video-label">Watch demo</span>
          </div>
        </a>
      </div>
      <div class="card-footer">
        <span class="card-desc">Record and send WAV audio messages using NAudio microphone capture</span>
        <a class="open-link" href="https://drive.google.com/file/d/1JIKgtkVQYXesEOnmexvJV3cvaKuNTQEU/view?usp=drive_link" target="_blank" rel="noopener">
          Open
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>
        </a>
      </div>
    </div>

  </div>

  <footer>
    <p>Built with C# &amp; .NET — <a href="https://github.com" target="_blank">View on GitHub</a></p>
  </footer>

</div>
</body>
</html>
