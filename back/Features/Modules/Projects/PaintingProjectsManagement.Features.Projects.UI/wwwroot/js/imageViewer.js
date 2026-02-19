// Image Viewer JavaScript Module for Color Zones Dialog
// Handles zoom, pan, pixel magnifier, and color picking with Touch support

let container = null;
let imageElement = null;
let magnifierElement = null;
let dotNetHelper = null;
let canvas = null;
let ctx = null;

// State
let state = {
    scale: 1,
    x: 0,
    y: 0,
    isDragging: false,
    lastX: 0,
    lastY: 0,
    initialPinchDistance: 0,
    initialScale: 1
};

export function initializeImageViewer(containerId, imageElementId, magnifierElementId, dotNetRef) {
    if (containerId) container = document.getElementById(containerId);
    if (imageElementId) imageElement = document.getElementById(imageElementId);
    if (magnifierElementId) magnifierElement = document.getElementById(magnifierElementId);
    dotNetHelper = dotNetRef;
    
    // Create a canvas for pixel color extraction
    canvas = document.createElement('canvas');
    ctx = canvas.getContext('2d');

    if (container) {
        setupEventListeners();
    }
}

export function resetImageViewer() {
    state = {
        scale: 1,
        x: 0,
        y: 0,
        isDragging: false,
        lastX: 0,
        lastY: 0,
        initialPinchDistance: 0,
        initialScale: 1
    };
    updateTransform();
}

function setupEventListeners() {
    // Wheel Zoom
    container.addEventListener('wheel', handleWheel, { passive: false });

    // Pointer Events (Mouse, Touch, Pen)
    container.addEventListener('pointerdown', handlePointerDown);
    container.addEventListener('pointermove', handlePointerMove);
    container.addEventListener('pointerup', handlePointerUp);
    container.addEventListener('pointercancel', handlePointerUp);
    container.addEventListener('pointerleave', handlePointerUp);

    // Touch Events for Pinch Zoom (Pointer events don't handle pinch easily)
    container.addEventListener('touchstart', handleTouchStart, { passive: false });
    container.addEventListener('touchmove', handleTouchMove, { passive: false });
    container.addEventListener('touchend', handleTouchEnd);
}

function updateTransform() {
    if (!imageElement) return;
    imageElement.style.transform = `translate(${state.x}px, ${state.y}px) scale(${state.scale})`;
}

function handleWheel(e) {
    e.preventDefault();
    
    const delta = e.deltaY > 0 ? 0.9 : 1.1;
    const newScale = Math.max(0.5, Math.min(5.0, state.scale * delta));
    
    // Zoom towards cursor
    const rect = container.getBoundingClientRect();
    const mouseX = e.clientX - rect.left;
    const mouseY = e.clientY - rect.top;

    state.x = mouseX - (mouseX - state.x) * (newScale / state.scale);
    state.y = mouseY - (mouseY - state.y) * (newScale / state.scale);
    state.scale = newScale;

    updateTransform();
    
    // Update magnifier if visible
    if (magnifierElement.style.display !== 'none') {
        updateMagnifier(e.clientX, e.clientY);
    }
}

let activePointers = new Map();

function handlePointerDown(e) {
    activePointers.set(e.pointerId, { x: e.clientX, y: e.clientY });
    
    // Right click or Touch -> Pan
    // Left click -> Potential Select
    if (e.button === 2 || e.pointerType === 'touch') {
        state.isDragging = true;
        state.lastX = e.clientX;
        state.lastY = e.clientY;
        container.setPointerCapture(e.pointerId);
    }
}

function handlePointerMove(e) {
    const isDown = activePointers.has(e.pointerId);
    
    // Allow mouse hover to update magnifier
    if (!isDown && e.pointerType !== 'mouse') return;

    if (state.isDragging && isDown && activePointers.size === 1) {
        // Pan
        const dx = e.clientX - state.lastX;
        const dy = e.clientY - state.lastY;
        
        state.x += dx;
        state.y += dy;
        state.lastX = e.clientX;
        state.lastY = e.clientY;
        
        updateTransform();
    }

    // Update Magnifier
    updateMagnifier(e.clientX, e.clientY);
}

function handlePointerUp(e) {
    const startPos = activePointers.get(e.pointerId);
    activePointers.delete(e.pointerId);
    
    if (state.isDragging) {
        state.isDragging = false;
        container.releasePointerCapture(e.pointerId);
    }

    if (e.type === 'pointerleave' && magnifierElement) {
        magnifierElement.style.display = 'none';
    }

    // Check for Click (Tap)
    if (startPos) {
        const dist = Math.hypot(e.clientX - startPos.x, e.clientY - startPos.y);
        if (dist < 10) { // Threshold for click vs drag
            if (e.button === 0 || (e.pointerType === 'touch' && activePointers.size === 0)) {
                selectColor(e.clientX, e.clientY);
            }
        }
    }
}

// Pinch Zoom Handling
let initialPinchDistance = null;
let initialScale = null;

function handleTouchStart(e) {
    if (e.touches.length === 2) {
        e.preventDefault();
        initialPinchDistance = getPinchDistance(e);
        initialScale = state.scale;
    }
}

function handleTouchMove(e) {
    if (e.touches.length === 2 && initialPinchDistance) {
        e.preventDefault();
        const currentDistance = getPinchDistance(e);
        const scaleFactor = currentDistance / initialPinchDistance;
        
        const newScale = Math.max(0.5, Math.min(5.0, initialScale * scaleFactor));
        
        // Zoom towards center of pinch
        const center = getPinchCenter(e);
        const rect = container.getBoundingClientRect();
        const centerX = center.x - rect.left;
        const centerY = center.y - rect.top;

        state.x = centerX - (centerX - state.x) * (newScale / state.scale);
        state.y = centerY - (centerY - state.y) * (newScale / state.scale);
        state.scale = newScale;
        
        updateTransform();
    }
}

function handleTouchEnd(e) {
    if (e.touches.length < 2) {
        initialPinchDistance = null;
    }
}

function getPinchDistance(e) {
    return Math.hypot(
        e.touches[0].clientX - e.touches[1].clientX,
        e.touches[0].clientY - e.touches[1].clientY
    );
}

function getPinchCenter(e) {
    return {
        x: (e.touches[0].clientX + e.touches[1].clientX) / 2,
        y: (e.touches[0].clientY + e.touches[1].clientY) / 2
    };
}

function updateMagnifier(clientX, clientY) {
    if (!imageElement || !magnifierElement) return;

    const rect = imageElement.getBoundingClientRect();
    
    // Calculate image coordinates
    // Note: rect includes transform, so we reverse it to get local coords
    // Actually, simpler: (clientX - rect.left) / scale is wrong if we use rect of transformed element?
    // No, getBoundingClientRect returns the visual rect.
    // But we need coordinates relative to the unscaled image (0..naturalWidth)
    
    // Correct math:
    // The image is at (state.x, state.y) relative to container top-left.
    // Container top-left is at containerRect.left, containerRect.top.
    const containerRect = container.getBoundingClientRect();
    const relativeX = clientX - containerRect.left - state.x;
    const relativeY = clientY - containerRect.top - state.y;
    
    const localX = relativeX / state.scale;
    const localY = relativeY / state.scale;

    // Check bounds
    if (localX >= 0 && localX < imageElement.naturalWidth && localY >= 0 && localY < imageElement.naturalHeight) {
        showPixelMagnifier(imageElement, magnifierElement, clientX, clientY, localX, localY);
    } else {
        magnifierElement.style.display = 'none';
    }
}

function showPixelMagnifier(img, magnifier, clientX, clientY, localX, localY) {
    if (!canvas || !ctx) return;
    
    const magnifierSize = 150;
    const pixelSize = 5; 
    
    // Set canvas size to match image
    if (canvas.width !== img.naturalWidth || canvas.height !== img.naturalHeight) {
        canvas.width = img.naturalWidth;
        canvas.height = img.naturalHeight;
        ctx.drawImage(img, 0, 0);
    }
    
    // Calculate the area to magnify
    const startX = Math.max(0, Math.floor(localX) - 2);
    const startY = Math.max(0, Math.floor(localY) - 2);
    const endX = Math.min(img.naturalWidth, Math.floor(localX) + 3);
    const endY = Math.min(img.naturalHeight, Math.floor(localY) + 3);
    
    // Create magnified view
    const magnifiedCanvas = document.createElement('canvas');
    magnifiedCanvas.width = magnifierSize;
    magnifiedCanvas.height = magnifierSize;
    const magnifiedCtx = magnifiedCanvas.getContext('2d');
    
    // Draw the 5x5 pixel area magnified
    const sourceSize = endX - startX;
    const sourceSizeY = endY - startY;
    
    magnifiedCtx.imageSmoothingEnabled = false;
    magnifiedCtx.drawImage(
        canvas,
        startX, startY, sourceSize, sourceSizeY,
        0, 0, magnifierSize, magnifierSize
    );
    
    // Draw grid lines
    magnifiedCtx.strokeStyle = '#000';
    magnifiedCtx.lineWidth = 1;
    const gridSize = magnifierSize / pixelSize;
    for (let i = 0; i <= pixelSize; i++) {
        const pos = i * gridSize;
        magnifiedCtx.beginPath();
        magnifiedCtx.moveTo(pos, 0);
        magnifiedCtx.lineTo(pos, magnifierSize);
        magnifiedCtx.stroke();
        
        magnifiedCtx.beginPath();
        magnifiedCtx.moveTo(0, pos);
        magnifiedCtx.lineTo(magnifierSize, pos);
        magnifiedCtx.stroke();
    }
    
    // Highlight center pixel
    const centerX = Math.floor(pixelSize / 2) * gridSize;
    const centerY = Math.floor(pixelSize / 2) * gridSize;
    magnifiedCtx.strokeStyle = '#ff0000';
    magnifiedCtx.lineWidth = 2;
    magnifiedCtx.strokeRect(centerX, centerY, gridSize, gridSize);
    
    // Set magnifier content
    try {
        magnifier.style.backgroundImage = `url(${magnifiedCanvas.toDataURL()})`;
        magnifier.style.backgroundSize = 'cover';
        magnifier.style.backgroundPosition = 'center';
        magnifier.style.display = 'block';
    } catch (e) {
        magnifier.style.display = 'none';
    }
    
    // Position magnifier
    const offset = 20;
    let targetX = clientX + offset;
    let targetY = clientY + offset;
    
    if (targetX + magnifierSize > window.innerWidth) {
        targetX = clientX - magnifierSize - offset;
    }
    if (targetY + magnifierSize > window.innerHeight) {
        targetY = clientY - magnifierSize - offset;
    }
    
    const containerRect = container.getBoundingClientRect();
    const relativeLeft = targetX - containerRect.left;
    const relativeTop = targetY - containerRect.top;
    
    magnifier.style.left = `${relativeLeft}px`;
    magnifier.style.top = `${relativeTop}px`;
}

function selectColor(clientX, clientY) {
    if (!imageElement || !canvas || !ctx) return;
    
    const containerRect = container.getBoundingClientRect();
    const relativeX = clientX - containerRect.left - state.x;
    const relativeY = clientY - containerRect.top - state.y;
    
    const localX = Math.floor(relativeX / state.scale);
    const localY = Math.floor(relativeY / state.scale);
    
    if (localX < 0 || localX >= imageElement.naturalWidth || localY < 0 || localY >= imageElement.naturalHeight) {
        return;
    }
    
    try {
        // Ensure canvas is up to date
        if (canvas.width !== imageElement.naturalWidth || canvas.height !== imageElement.naturalHeight) {
            canvas.width = imageElement.naturalWidth;
            canvas.height = imageElement.naturalHeight;
            ctx.drawImage(imageElement, 0, 0);
        }

        const imageData = ctx.getImageData(localX, localY, 1, 1);
        const [r, g, b] = imageData.data;
        
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('SetSelectedColor', r, g, b);
        }
    } catch (e) {
        console.error("Failed to get pixel color", e);
    }
}

