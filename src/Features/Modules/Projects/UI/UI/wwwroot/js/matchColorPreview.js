const previewInstances = new Map();

export function initializeMatchColorPreview(rootId) {
    const root = document.getElementById(rootId);
    if (!root || previewInstances.has(rootId)) {
        return;
    }

    const preview = document.createElement('div');
    preview.className = 'match-hover-preview';
    preview.style.display = 'none';

    const reference = createPreviewColumn('Reference');
    const paint = createPreviewColumn('Paint');
    preview.append(reference.column, paint.column);
    document.body.appendChild(preview);

    const getCard = target => {
        if (!(target instanceof Element)) {
            return null;
        }

        const card = target.closest('[data-match-preview="true"]');
        return card && root.contains(card) ? card : null;
    };

    const updatePreview = (card, clientX, clientY) => {
        const referenceColor = card.dataset.referenceColor ?? '';
        const paintColor = card.dataset.paintColor ?? '';
        if (!CSS.supports('color', referenceColor) || !CSS.supports('color', paintColor)) {
            preview.style.display = 'none';
            return;
        }

        reference.swatch.style.backgroundColor = referenceColor;
        reference.name.textContent = card.dataset.referenceName ?? 'Reference';
        reference.hex.textContent = referenceColor.toUpperCase();
        paint.swatch.style.backgroundColor = paintColor;
        paint.name.textContent = card.dataset.paintName ?? 'Paint';
        paint.hex.textContent = paintColor.toUpperCase();

        preview.style.display = 'flex';
        const offset = 18;
        const rect = preview.getBoundingClientRect();
        const left = clientX + offset + rect.width <= window.innerWidth
            ? clientX + offset
            : clientX - rect.width - offset;
        const top = clientY + offset + rect.height <= window.innerHeight
            ? clientY + offset
            : clientY - rect.height - offset;
        preview.style.left = `${Math.max(0, left)}px`;
        preview.style.top = `${Math.max(0, top)}px`;
    };

    const onPointerOver = event => {
        const card = getCard(event.target);
        if (card) {
            updatePreview(card, event.clientX, event.clientY);
        }
    };

    const onPointerMove = event => {
        const card = getCard(event.target);
        if (card) {
            updatePreview(card, event.clientX, event.clientY);
        }
    };

    const onPointerOut = event => {
        const card = getCard(event.target);
        if (card && (!event.relatedTarget || !card.contains(event.relatedTarget))) {
            preview.style.display = 'none';
        }
    };

    root.addEventListener('pointerover', onPointerOver);
    root.addEventListener('pointermove', onPointerMove);
    root.addEventListener('pointerout', onPointerOut);
    previewInstances.set(rootId, { root, preview, onPointerOver, onPointerMove, onPointerOut });
}

export function disposeMatchColorPreview(rootId) {
    const instance = previewInstances.get(rootId);
    if (!instance) {
        return;
    }

    instance.root.removeEventListener('pointerover', instance.onPointerOver);
    instance.root.removeEventListener('pointermove', instance.onPointerMove);
    instance.root.removeEventListener('pointerout', instance.onPointerOut);
    instance.preview.remove();
    previewInstances.delete(rootId);
}

function createPreviewColumn(title) {
    const column = document.createElement('div');
    column.className = 'match-hover-preview-column';

    const name = document.createElement('span');
    name.className = 'match-hover-preview-label';
    name.textContent = title;

    const swatch = document.createElement('div');
    swatch.className = 'match-hover-preview-swatch';

    const hex = document.createElement('span');
    hex.className = 'match-hover-preview-hex';

    column.append(name, swatch, hex);
    return { column, name, swatch, hex };
}
