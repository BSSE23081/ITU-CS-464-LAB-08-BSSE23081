"""Generates every sound used in NEON DASH (Lab 08) from scratch.
Run:  python3 tools/generate_audio.py   (needs numpy)
Output: Assets/Resources/Audio/*.wav  - all original, made by this script (CC0)."""
import os, wave
import numpy as np

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Assets", "Resources", "Audio")
os.makedirs(OUT, exist_ok=True)
SR = 44100
rng = np.random.default_rng(7)

def save(name, x, sr=SR, peak=0.9):
    x = np.asarray(x, dtype=np.float64)
    m = np.max(np.abs(x))
    if m > 0: x = x / m * peak
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr)
        w.writeframes((x * 32767).astype("<i2").tobytes())

def t(d, sr=SR): return np.arange(int(d * sr)) / sr
def decay(d, k, sr=SR): return np.exp(-k * t(d, sr))
def fade(x, a=0.003, r=0.01, sr=SR):
    n = len(x); na = min(int(a * sr), n); nr = min(int(r * sr), n)
    x = x.copy(); x[:na] *= np.linspace(0, 1, na); x[n - nr:] *= np.linspace(1, 0, nr); return x
def sweep(f0, f1, d, sr=SR, shape="sine"):
    tt = t(d, sr); f = f0 + (f1 - f0) * tt / d
    ph = 2 * np.pi * np.cumsum(f) / sr
    if shape == "square": return np.sign(np.sin(ph))
    if shape == "saw": return 2 * ((ph / (2 * np.pi)) % 1) - 1
    if shape == "tri": return 2 * np.abs(2 * ((ph / (2 * np.pi)) % 1) - 1) - 1
    return np.sin(ph)
def tone(f, d, shape="sine", sr=SR): return sweep(f, f, d, sr, shape)
def noise(d, sr=SR): return rng.uniform(-1, 1, int(d * sr))
def lowpass(x, a):
    y = np.zeros_like(x); s = 0.0
    for i, v in enumerate(x): s += a * (v - s); y[i] = s
    return y
def seq(parts, sr=SR):
    return np.concatenate(parts)
def mix(*xs):
    n = max(len(x) for x in xs); o = np.zeros(n)
    for x in xs: o[:len(x)] += x
    return o
def note(n):  # midi -> Hz
    return 440.0 * 2 ** ((n - 69) / 12)

# ------------------------------------------------------------------ UI
save("ui_hover", fade(tone(1800, .05) * decay(.05, 60)) * .5, peak=.35)
save("ui_click", fade(mix(sweep(1100, 700, .08, shape="tri") * decay(.08, 25), noise(.01) * .3)), peak=.6)
save("ui_select", fade(seq([tone(1100, .05, "tri") * decay(.05, 20), tone(1500, .07, "tri") * decay(.07, 25)])), peak=.6)
save("ui_back", fade(sweep(750, 330, .14, shape="tri") * decay(.14, 14)), peak=.6)
w = noise(.25); w = lowpass(w, .12) * np.sin(np.pi * t(.25) / .25)
save("ui_open", fade(mix(w * .8, sweep(280, 800, .25) * np.sin(np.pi * t(.25) / .25) * .4)), peak=.5)
w = noise(.25); w = lowpass(w, .12) * np.sin(np.pi * t(.25) / .25)
save("ui_close", fade(mix(w * .8, sweep(800, 280, .25) * np.sin(np.pi * t(.25) / .25) * .4)), peak=.5)
save("ui_pause", fade(seq([tone(660, .09, "tri") * decay(.09, 12), tone(440, .16, "tri") * decay(.16, 12)])), peak=.65)
save("ui_resume", fade(seq([tone(440, .09, "tri") * decay(.09, 12), tone(660, .16, "tri") * decay(.16, 12)])), peak=.65)
save("ui_success", fade(seq([tone(note(m), .09, "tri") * decay(.09, 9) for m in (72, 76, 79)] +
                            [tone(note(84), .35, "tri") * decay(.35, 6)])), peak=.7)
b = tone(150, .1, "square") * decay(.1, 8)
save("ui_error", fade(seq([b, np.zeros(int(.04 * SR)), tone(120, .14, "square") * decay(.14, 8)])), peak=.55)

# ------------------------------------------------------------------ gameplay SFX
save("sfx_jump", fade(sweep(280, 720, .17, shape="square") * decay(.17, 9) * .6 + sweep(280, 720, .17) * decay(.17, 9) * .4), peak=.7)
save("sfx_coin", fade(seq([tone(1319, .06, "tri") * decay(.06, 8), tone(1760, .28, "tri") * decay(.28, 9)])), peak=.7)
save("sfx_hit", fade(mix(lowpass(noise(.3), .25) * decay(.3, 12), sweep(160, 55, .3) * decay(.3, 8) * 1.2)), peak=.95)
d = .75
save("sfx_death", fade(mix(sweep(520, 70, d, shape="saw") * decay(d, 3.5) * .6,
                           lowpass(noise(d), .15) * decay(d, 5) * .8, sweep(120, 40, d) * decay(d, 4))), peak=.95)
arp = [tone(note(m), .12, "tri") * decay(.12, 6) for m in (72, 76, 79, 84, 88)]
chord = mix(*[tone(note(m), .7, "tri") * decay(.7, 4) for m in (72, 76, 79, 84)])
save("sfx_win", fade(seq(arp + [chord])), peak=.85)
dn = [tone(note(m), .2, "saw") * decay(.2, 6) * .5 for m in (69, 65, 62)]
fin = tone(note(57), .7, "saw") * decay(.7, 4) * .5
save("sfx_lose", fade(lowpass(seq(dn + [fin]), .35)), peak=.85)

# ------------------------------------------------------------------ music (seamless loops)
MSR = 22050
def music(name, bpm, bars, chords, style, peak=.7):
    spb = 60.0 / bpm; bar = 4 * spb; total = int(bars * bar * MSR)
    buf = np.zeros(total + MSR * 2)  # tail so notes can ring out, then folded back to the start
    def put(start, x, g=1.0):
        i = int(start * MSR); buf[i:i + len(x)] += x[:len(buf) - i] * g
    for b in range(bars):
        root, quality = chords[b % len(chords)]
        tri = [0, 3 if quality == "m" else 4, 7]
        t0 = b * bar
        # pad
        for k in tri + [12]:
            n = tone(note(root + 12 + k), bar + .6, "saw", MSR)
            n = lowpass(n, .04) * np.minimum(1, t(bar + .6, MSR) / .4) * np.minimum(1, (bar + .6 - t(bar + .6, MSR)) / .5)
            put(t0, n, style["pad"])
        # bass: eighths
        for e in range(8):
            if style["bass_pattern"][e]:
                n = tone(note(root - 12), spb / 2 * 1.1, "saw", MSR); n = lowpass(n, .12) * decay(spb / 2 * 1.1, 6, MSR)
                put(t0 + e * spb / 2, n, style["bass"])
        # arp
        steps = style["arp_steps"]
        for s in range(steps):
            m = root + 24 + [tri[0], tri[1], tri[2], 12 + tri[0]][s % 4] + (12 if (s // 4) % 2 else 0)
            ln = bar / steps * 1.6
            n = tone(note(m), ln, style["arp_wave"], MSR) * decay(ln, style["arp_decay"], MSR)
            put(t0 + s * bar / steps, lowpass(n, .5), style["arp"])
        # drums
        for q in range(4):
            if style["kick"][q]:
                k = sweep(130, 45, .22, MSR) * decay(.22, 14, MSR); put(t0 + q * spb, k, style["kick_vol"])
            if style["hat"]:
                h = lowpass(noise(.05, MSR), .9) - lowpass(noise(.05, MSR), .3)
                put(t0 + q * spb + spb / 2, h * decay(.05, 60, MSR), style["hat"])
    body = buf[:total].copy(); tail = buf[total:]
    body[:len(tail)] += tail      # wrap the ring-out onto the beginning: no click at the loop point
    save(name, body, sr=MSR, peak=peak)

am = [(57, "m"), (53, "M"), (48, "M"), (55, "M")]
music("music_menu", 88, 8, am * 2, dict(pad=.35, bass=.35, bass_pattern=[1,0,0,1,0,0,1,0], arp=.22, arp_steps=8,
      arp_wave="tri", arp_decay=7, kick=[1,0,1,0], kick_vol=.5, hat=.05), peak=.55)
music("music_gameplay", 126, 8, am * 2, dict(pad=.18, bass=.55, bass_pattern=[1,1,1,1,1,1,1,1], arp=.3, arp_steps=16,
      arp_wave="saw", arp_decay=12, kick=[1,1,1,1], kick_vol=.8, hat=.1), peak=.7)
fm = [(53, "M"), (55, "M"), (57, "m"), (52, "m")]
music("music_results", 76, 4, fm, dict(pad=.4, bass=.3, bass_pattern=[1,0,0,0,1,0,0,0], arp=.2, arp_steps=4,
      arp_wave="tri", arp_decay=3, kick=[0,0,0,0], kick_vol=0, hat=0), peak=.5)
print("done:", sorted(os.listdir(OUT)))
