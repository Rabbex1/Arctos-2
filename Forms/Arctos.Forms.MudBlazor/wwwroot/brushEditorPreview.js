export function create(canvas, callback) {
    const ctx = canvas.getContext('2d'), events = new AbortController();
    let state, drag = null, frame = 0, pending = null, disposed = false;
    const wrap = a => (a % 360 + 360) % 360;
    const css = c => `rgba(${c[0]},${c[1]},${c[2]},${c[3] / 255})`;
    const pad = 9, length = 222, corner = length * .06;
    const map = (x, y) => [pad + x * length, pad + y * length];
    function outline() {
        ctx.beginPath();
        if (state.mode === 'Radial' || state.mode === 'Angular') ctx.arc(120, 120, length / 2, 0, Math.PI * 2);
        else if (state.mode === 'Linear') ctx.rect(pad, pad, length, length);
        else ctx.roundRect(pad, pad, length, length, corner);
    }
    function inside(x, y) {
        if (x < 0 || x > 1 || y < 0 || y > 1) return false;
        if (state.mode === 'Radial' || state.mode === 'Angular') return (x - .5) ** 2 + (y - .5) ** 2 <= .25;
        if (state.mode === 'Linear') return true;
        const dx = Math.max(.06 - x, 0, x - .94), dy = Math.max(.06 - y, 0, y - .94);
        return dx * dx + dy * dy <= .06 ** 2;
    }
    function sample(stops, t) {
        if (t <= stops[0].offset) return stops[0].color;
        for (let i = 1; i < stops.length; i++) if (t <= stops[i].offset) {
            const a = stops[i - 1], b = stops[i], f = b.offset === a.offset ? 1 : (t - a.offset) / (b.offset - a.offset);
            return a.color.map((v, c) => Math.round(v + (b.color[c] - v) * f));
        }
        return stops[stops.length - 1].color;
    }
    function draw() {
        if (!state || disposed) return;
        const dpr = window.devicePixelRatio || 1, size = Math.round(240 * dpr);
        if (canvas.width !== size) { canvas.width = size; canvas.height = size; }
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0); ctx.clearRect(0, 0, 240, 240);
        // Inset allows endpoint handles to remain fully visible.
        ctx.save(); outline(); ctx.clip();
        for (let y = 0; y < 240; y += 8) for (let x = 0; x < 240; x += 8) { ctx.fillStyle = ((x + y) / 8) % 2 ? '#ddd' : '#fff'; ctx.fillRect(x, y, 8, 8); }
        if (!state.clear) {
            const image = ctx.createImageData(size, size), p = image.data;
            for (let y = 0; y < size; y++) for (let x = 0; x < size; x++) {
                const nx = ((x + .5) / dpr - pad) / length, ny = ((y + .5) / dpr - pad) / length;
                // putImageData ignores the canvas clip, so mask pixels to the same outline.
                if (!inside(nx, ny)) continue;
                let c = state.solid, t = 0;
                if (state.mode === 'Linear') {
                    const dx = state.x2 - state.x1, dy = state.y2 - state.y1;
                    t = ((nx - state.x1) * dx + (ny - state.y1) * dy) / (dx * dx + dy * dy);
                } else if (state.mode === 'Radial') t = Math.hypot((nx - state.x1) / state.x2, (ny - state.y1) / state.y2);
                else if (state.mode === 'Angular') {
                    t = wrap(Math.atan2(nx - state.x1, state.y1 - ny) * 180 / Math.PI - state.x2) / (state.y2 - state.x2);
                    if (t > 1) continue;
                }
                if (state.stops.length) c = sample(state.stops, t);
                // Composite over the same checkerboard without an intermediate image.
                const bg = ((Math.floor(x / dpr / 8) + Math.floor(y / dpr / 8)) % 2) ? 221 : 255;
                const at = (y * size + x) * 4, alpha = c[3] / 255;
                for (let k = 0; k < 3; k++) p[at + k] = Math.round(c[k] * alpha + bg * (1 - alpha));
                p[at + 3] = 255;
            }
            ctx.putImageData(image, 0, 0);
        }
        ctx.restore();
        ctx.strokeStyle = '#888'; ctx.lineWidth = 1; outline(); ctx.stroke();
        if (state.handles.length && state.mode !== 'Angular') {
            ctx.save(); outline(); ctx.clip();
            const end = state.mode === 'Radial' ? state.guide : [state.x2, state.y2];
            ctx.beginPath(); ctx.moveTo(...map(state.x1, state.y1)); ctx.lineTo(...map(...end));
            ctx.strokeStyle = '#000'; ctx.lineWidth = 2; ctx.stroke(); ctx.strokeStyle = '#fff'; ctx.lineWidth = 1; ctx.setLineDash([4, 4]); ctx.stroke(); ctx.setLineDash([]);
            ctx.restore();
        }
        if (state.mode === 'Radial') {
            ctx.beginPath(); ctx.arc(120, 120, length / (2 * state.viewScale), 0, Math.PI * 2);
            ctx.strokeStyle = '#000'; ctx.lineWidth = 3; ctx.stroke();
            ctx.strokeStyle = '#fff'; ctx.lineWidth = 1; ctx.stroke();
        }
        for (const h of state.handles) {
            if (h.offscreen) {
                const [x, y] = map(h.x, h.y), magnitude = Math.hypot(x - 120, y - 120);
                const dx = (x - 120) / magnitude, dy = (y - 120) / magnitude;
                ctx.beginPath(); ctx.moveTo(x, y);
                ctx.lineTo(x - 15 * dx - 7 * dy, y - 15 * dy + 7 * dx);
                ctx.lineTo(x - 15 * dx + 7 * dy, y - 15 * dy - 7 * dx); ctx.closePath();
                ctx.fillStyle = '#fff'; ctx.fill(); ctx.strokeStyle = '#000'; ctx.lineWidth = 2; ctx.stroke();
                continue;
            }
            ctx.beginPath(); ctx.arc(...map(h.x, h.y), h.selected ? 7 : 5, 0, Math.PI * 2);
            ctx.fillStyle = css([h.color.red, h.color.green, h.color.blue, h.color.alpha]); ctx.fill();
            ctx.strokeStyle = '#000'; ctx.lineWidth = 3; ctx.stroke(); ctx.strokeStyle = '#fff'; ctx.lineWidth = 1; ctx.stroke();
        }
    }
    function point(e) { const r = canvas.getBoundingClientRect(); return [(e.clientX - r.left) * 240 / r.width, (e.clientY - r.top) * 240 / r.height]; }
    function send(first = false) {
        if (frame) { cancelAnimationFrame(frame); frame = 0; }
        if (!pending || disposed) return;
        const { id, x, y } = pending; pending = null;
        callback.invokeMethodAsync('Drag', id, (x - pad) / length, (y - pad) / length, first).catch(e => { if (!disposed) console.error(e); });
    }
    const on = (name, fn) => canvas.addEventListener(name, fn, { signal: events.signal });
    on('pointerdown', e => {
        if (!state || e.button !== 0 || drag) return;
        const [x, y] = point(e);
        const distance = h => Math.hypot(pad + h.x * length - x, pad + h.y * length - y);
        const hit = state.handles.filter(h => !h.offscreen).sort((a, b) => distance(a) - distance(b))[0];
        if (!hit || distance(hit) > 14) return;
        drag = { id: hit.id, pointer: e.pointerId }; pending = { id: drag.id, x, y }; send(true);
        canvas.setPointerCapture(e.pointerId); e.preventDefault();
    });
    on('pointermove', e => { if (drag?.pointer === e.pointerId) { const [x, y] = point(e); pending = { id: drag.id, x, y }; if (!frame) frame = requestAnimationFrame(() => send()); } });
    on('pointerup', e => { if (drag?.pointer === e.pointerId) { const [x, y] = point(e); pending = { id: drag.id, x, y }; send(); drag = null; canvas.releasePointerCapture(e.pointerId); } });
    on('pointercancel', () => { send(); drag = null; });
    const observer = new ResizeObserver(draw); observer.observe(canvas);
    return { update(value) { state = value; draw(); }, dispose() { disposed = true; events.abort(); observer.disconnect(); if (frame) cancelAnimationFrame(frame); } };
}
