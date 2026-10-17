// Keep these equations aligned with Arctos.Forms.ColourTriangle. No rendering library.
const radius = 0.39;
const clamp = (v, min = 0, max = 1) => Math.min(max, Math.max(min, v));
const wrap = h => (h % 360 + 360) % 360;
const byte = v => clamp(Math.floor(v * 255 + 0.5), 0, 255);
const vertex = (h, offset = 0, r = radius) => {
    const angle = (h + offset - 90) * Math.PI / 180;
    return [0.5 + r * Math.cos(angle), 0.5 + r * Math.sin(angle)];
};
function pureHue(h) {
    const sector = wrap(h) / 60, x = 1 - Math.abs(sector % 2 - 1);
    const rgb = sector < 1 ? [1, x, 0] : sector < 2 ? [x, 1, 0] : sector < 3 ? [0, 1, x]
        : sector < 4 ? [0, x, 1] : sector < 5 ? [x, 0, 1] : [1, 0, x];
    return rgb.map(byte);
}
function weightsAt(p, vertices) {
    const [a, b, c] = vertices;
    const denominator = (b[1] - c[1]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[1] - c[1]);
    const h = ((b[1] - c[1]) * (p[0] - c[0]) + (c[0] - b[0]) * (p[1] - c[1])) / denominator;
    const w = ((c[1] - a[1]) * (p[0] - c[0]) + (a[0] - c[0]) * (p[1] - c[1])) / denominator;
    return [h, w, 1 - h - w];
}
const inside = w => w.every(v => v >= 0);
function closestWeights(p, vertices) {
    const w = weightsAt(p, vertices);
    if (inside(w)) return w;
    let closest, distance = Infinity;
    for (let i = 0; i < 3; i++) {
        const a = vertices[i], b = vertices[(i + 1) % 3];
        const dx = b[0] - a[0], dy = b[1] - a[1];
        const t = clamp(((p[0] - a[0]) * dx + (p[1] - a[1]) * dy) / (dx * dx + dy * dy));
        const q = [a[0] + t * dx, a[1] + t * dy];
        const d = (p[0] - q[0]) ** 2 + (p[1] - q[1]) ** 2;
        if (d < distance) { distance = d; closest = q; }
    }
    const result = weightsAt(closest, vertices).map(v => Math.max(0, v));
    const sum = result.reduce((a, b) => a + b, 0);
    return result.map(v => v / sum);
}

export function create(root, callback, initial) {
    const wheel = root.querySelector('.colour-wheel');
    const ring = root.querySelector('.hue-ring');
    const canvas = root.querySelector('canvas');
    const ctx = canvas.getContext('2d');
    const hueMarker = root.querySelector('.hue-marker');
    const colorMarker = root.querySelector('.colour-marker');
    const fields = Object.fromEntries([...root.querySelectorAll('[data-channel]')].map(e => [e.dataset.channel, e]));
    const events = new AbortController();
    let rgba = [...initial], hue = 0, weights = [0, 1, 0], vertices;
    let format = 'RGB';
    function channelValues() {
        if (format === 'RGB') return rgba.slice(0, 3);
        const max = Math.max(...rgba.slice(0, 3)) / 255, min = Math.min(...rgba.slice(0, 3)) / 255;
        const delta = max - min, light = (max + min) / 2;
        return format === 'HSV' ? [hue, max ? delta / max * 100 : 0, max * 100]
            : [hue, delta ? delta / (1 - Math.abs(2 * light - 1)) * 100 : 0, light * 100];
    }
    let renderedHue = NaN, renderedSize = 0, revision = 0, frame = 0, disposed = false, drag = null;

    function decode(color) {
        const [r, g, b] = color.map(v => v / 255);
        const max = Math.max(r, g, b), min = Math.min(r, g, b), delta = max - min;
        if (delta > 0) hue = wrap(max === r ? 60 * ((g - b) / delta)
            : max === g ? 60 * ((b - r) / delta + 2) : 60 * ((r - g) / delta + 4));
        weights = [delta, min, 1 - max];
        vertices = [vertex(hue), vertex(hue, 120), vertex(hue, 240)];
    }
    function position() {
        return [0, 1].map(axis => vertices.reduce((sum, v, i) => sum + v[axis] * weights[i], 0));
    }
    function draw() {
        if (disposed) return;
        const width = wheel.getBoundingClientRect().width;
        if (!width) return; // ResizeObserver redraws when a hidden tab becomes visible.
        const size = Math.max(1, Math.ceil(width * (window.devicePixelRatio || 1)));
        if (renderedSize !== size || renderedHue !== hue) {
            if (canvas.width !== size) { canvas.width = size; canvas.height = size; }
            const image = ctx.createImageData(size, size), pixels = image.data, rgb = pureHue(hue);
            // Barycentric coordinates are affine, so step across each scanline.
            const start = weightsAt([0.5 / size, 0.5 / size], vertices);
            const nextX = weightsAt([1.5 / size, 0.5 / size], vertices);
            const nextY = weightsAt([0.5 / size, 1.5 / size], vertices);
            const dx = nextX.map((v, i) => v - start[i]), dy = nextY.map((v, i) => v - start[i]);
            for (let y = 0; y < size; y++) {
                let h = start[0] + y * dy[0], w = start[1] + y * dy[1], k = start[2] + y * dy[2];
                for (let x = 0; x < size; x++, h += dx[0], w += dx[1], k += dx[2]) {
                    const coverage = clamp(Math.min(h, w, k) * (1.5 * radius) * size + 0.5);
                    if (coverage <= 0) continue;
                    const index = (y * size + x) * 4;
                    for (let c = 0; c < 3; c++) pixels[index + c] = byte(rgb[c] / 255 * h + w);
                    pixels[index + 3] = Math.round(coverage * 255);
                }
            }
            ctx.putImageData(image, 0, 0);
            renderedSize = size; renderedHue = hue;
        }
        const h = vertex(hue, 0, 0.45), p = position();
        hueMarker.style.left = `${h[0] * 100}%`; hueMarker.style.top = `${h[1] * 100}%`;
        colorMarker.style.left = `${p[0] * 100}%`; colorMarker.style.top = `${p[1] * 100}%`;
        const hex = '#' + rgba.map(v => v.toString(16).padStart(2, '0')).join('').toUpperCase();
        ring.setAttribute('aria-valuenow', hue.toFixed(1));
        ring.setAttribute('aria-valuetext', `${hue.toFixed(1)} degrees`);
        canvas.setAttribute('aria-valuenow', ((1 - weights[2]) * 100).toFixed(1));
        canvas.setAttribute('aria-valuetext', hex);
        const channels = channelValues().map(v => Math.round(v * 100) / 100);
        for (const [key, value] of Object.entries({ r: channels[0], g: channels[1], b: channels[2], a: rgba[3], alpha: rgba[3], hex }))
            if (document.activeElement !== fields[key]) fields[key].value = value;
    }
    function send() {
        if (frame) { cancelAnimationFrame(frame); frame = 0; }
        if (disposed) return;
        // Send in event order; do not queue behind a previous server response.
        // The revision guard prevents an older Blazor render overwriting a newer drag.
        callback.invokeMethodAsync('SelectColor', ...rgba, revision).catch(error => {
            if (!disposed) console.error('Colour picker callback failed', error);
        });
    }
    function changed(immediate = false) {
        revision++;
        draw();
        if (immediate) send();
        else if (!frame) frame = requestAnimationFrame(send);
    }
    function select(p, immediate = false) {
        if (drag.mode === 'hue') {
            hue = wrap(Math.atan2(p[1] - 0.5, p[0] - 0.5) * 180 / Math.PI + 90);
            vertices = [vertex(hue), vertex(hue, 120), vertex(hue, 240)];
        } else weights = closestWeights(p, vertices);
        const rgb = pureHue(hue);
        rgba = [...rgb.map(v => byte(v / 255 * weights[0] + weights[1])), rgba[3]];
        changed(immediate);
    }
    function pointer(e) {
        const box = wheel.getBoundingClientRect();
        return [(e.clientX - box.left) / box.width, (e.clientY - box.top) / box.height];
    }
    const on = (element, event, handler) => element.addEventListener(event, handler, { signal: events.signal });
    on(wheel, 'pointerdown', e => {
        if (e.button !== 0 || drag) return;
        const p = pointer(e), d = Math.hypot(p[0] - 0.5, p[1] - 0.5);
        const mode = d >= 0.41 && d <= 0.49 ? 'hue' : inside(weightsAt(p, vertices)) ? 'triangle' : null;
        if (!mode) return;
        drag = { id: e.pointerId, mode };
        (mode === 'hue' ? ring : canvas).focus({ preventScroll: true });
        wheel.setPointerCapture(e.pointerId); e.preventDefault(); select(p);
    });
    on(wheel, 'pointermove', e => { if (drag?.id === e.pointerId) { e.preventDefault(); select(pointer(e)); } });
    on(wheel, 'pointerup', e => {
        if (drag?.id !== e.pointerId) return;
        select(pointer(e), true); drag = null;
        wheel.releasePointerCapture(e.pointerId);
    });
    on(wheel, 'pointercancel', () => { drag = null; send(); });
    on(wheel, 'lostpointercapture', () => { if (drag) { drag = null; send(); } });
    on(wheel, 'keydown', e => {
        if (!['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].includes(e.key)) return;
        e.preventDefault();
        if (e.target === ring) {
            hue = wrap(hue + (e.key === 'ArrowLeft' || e.key === 'ArrowDown' ? -1 : 1) * (e.shiftKey ? 10 : 1));
            vertices = [vertex(hue), vertex(hue, 120), vertex(hue, 240)];
        } else {
            const p = position(), step = e.shiftKey ? 0.025 : 0.005;
            p[0] += e.key === 'ArrowLeft' ? -step : e.key === 'ArrowRight' ? step : 0;
            p[1] += e.key === 'ArrowUp' ? -step : e.key === 'ArrowDown' ? step : 0;
            weights = closestWeights(p, vertices);
        }
        rgba = [...pureHue(hue).map(v => byte(v / 255 * weights[0] + weights[1])), rgba[3]];
        changed(true);
    });
    on(fields.alpha, 'input', () => { rgba[3] = Number(fields.alpha.value); changed(true); });
    on(fields.format, 'change', () => {
        format = fields.format.value;
        const labels = format === 'RGB' ? ['R', 'G', 'B'] : format === 'HSV' ? ['H°', 'S%', 'V%'] : ['H°', 'S%', 'L%'];
        ['r', 'g', 'b'].forEach((key, i) => {
            root.querySelector(`[data-label="${key}"]`).textContent = labels[i];
            fields[key].max = format === 'RGB' ? 255 : i === 0 ? 360 : 100;
            fields[key].step = format === 'RGB' ? 1 : 0.1;
        });
        draw();
    });
    for (const [index, key] of ['r', 'g', 'b', 'a'].entries()) {
        const commit = () => {
            const value = Number(fields[key].value);
            if (fields[key].value !== '' && Number.isFinite(value) && value >= 0 && value <= Number(fields[key].max)) {
                if (index === 3 || format === 'RGB') rgba[index] = Math.round(value);
                else {
                    const values = channelValues(); values[index] = value;
                    const s = clamp(values[1] / 100), v = clamp(values[2] / 100);
                    const chroma = format === 'HSV' ? v * s : (1 - Math.abs(2 * v - 1)) * s;
                    const white = format === 'HSV' ? v - chroma : v - chroma / 2;
                    hue = wrap(values[0]);
                    rgba = [...pureHue(hue).map(c => byte(c / 255 * chroma + white)), rgba[3]];
                }
                if (index < 3) decode(rgba);
                changed(true);
            }
            fields[key].value = index === 3 ? rgba[3] : Math.round(channelValues()[index] * 100) / 100;
        };
        on(fields[key], 'change', commit);
        on(fields[key], 'keydown', e => { if (e.key === 'Enter') { e.preventDefault(); commit(); } });
    }
    const commitHex = () => {
        const text = fields.hex.value.trim().replace(/^#/, '');
        if (/^(?:[0-9a-f]{6}|[0-9a-f]{8})$/i.test(text)) {
            const color = text.match(/../g).map(v => parseInt(v, 16));
            if (color.length === 3) color.push(rgba[3]);
            rgba = color; decode(rgba); changed(true);
        }
        fields.hex.value = '#' + rgba.map(v => v.toString(16).padStart(2, '0')).join('').toUpperCase();
    };
    on(fields.hex, 'change', commitHex);
    on(fields.hex, 'keydown', e => { if (e.key === 'Enter') { e.preventDefault(); commitHex(); } });
    on(window, 'resize', draw);
    const observer = new ResizeObserver(draw);
    observer.observe(wheel);
    decode(rgba); draw();
    return {
        setColor(color, acknowledgedRevision) {
            if (disposed || acknowledgedRevision < revision || color.every((v, i) => v === rgba[i])) return;
            const rgbChanged = color.slice(0, 3).some((v, i) => v !== rgba[i]);
            rgba = [...color];
            if (rgbChanged) decode(rgba);
            draw();
        },
        dispose() {
            disposed = true;
            if (frame) cancelAnimationFrame(frame);
            observer.disconnect(); events.abort();
        }
    };
}
