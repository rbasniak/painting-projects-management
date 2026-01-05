// Image Viewer JavaScript Module for Color Zones Dialog
// Handles zoom, pan, pixel magnifier, and color picking

let imageElement = null;
let magnifierElement = null;
let dotNetHelper = null;
let canvas = null;
let ctx = null;

export function initializeImageViewer(imageElementId, magnifierElementId, dotNetRef) {
    if (imageElementId) {
        imageElement = document.getElementById(imageElementId);
    }
    if (magnifierElementId) {
        magnifierElement = document.getElementById(magnifierElementId);
    }
    dotNetHelper = dotNetRef;
    
    // Create a canvas for pixel color extraction
    canvas = document.createElement('canvas');
    ctx = canvas.getContext('2d');
}

export function updateImageTransform(imageElementId, scale, offsetX, offsetY) {
    if (!imageElementId) return;
    
    const img = document.getElementById(imageElementId);
    if (!img) {
        console.warn(`Image element with ID '${imageElementId}' not found for transform update`);
        return;
    }
    
    img.style.transform = `translate(${offsetX}px, ${offsetY}px) scale(${scale})`;
}

export function handleMouseMove(imageElementId, magnifierElementId, clientX, clientY, scale, offsetX, offsetY) {
    if (!imageElementId || !magnifierElementId) return;
    
    const img = document.getElementById(imageElementId);
    const magnifier = document.getElementById(magnifierElementId);
    
    if (!img || !magnifier) return;
    
    const rect = img.getBoundingClientRect();
    
    // Calculate local coordinates
    const localX = (clientX - rect.left - offsetX) / scale;
    const localY = (clientY - rect.top - offsetY) / scale;
    
    // Check if mouse is over the image
    if (localX >= 0 && localX < img.naturalWidth && localY >= 0 && localY < img.naturalHeight) {
        showPixelMagnifier(img, magnifier, clientX, clientY, localX, localY, scale);
    } else {
        magnifier.style.display = 'none';
    }
}

function showPixelMagnifier(img, magnifier, clientX, clientY, localX, localY, scale) {
    if (!canvas || !ctx) return;
    
    const magnifierSize = 150;
    const pixelSize = 5; // Show 5x5 pixels (30x30 pixels total)
    const zoomLevel = 10; // Magnify 10x
    
    // Set canvas size to match image
    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;
    
    // Draw image to canvas
    ctx.drawImage(img, 0, 0);
    
    // Calculate the area to magnify (5x5 pixels around cursor)
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
    magnifier.style.backgroundImage = `url(${magnifiedCanvas.toDataURL()})`;
    magnifier.style.backgroundSize = 'cover';
    magnifier.style.backgroundPosition = 'center';
    magnifier.style.display = 'block';
    
    // Position magnifier near cursor (avoid going off screen)
    const offset = 20;
    let left = clientX + offset;
    let top = clientY + offset;
    
    if (left + magnifierSize > window.innerWidth) {
        left = clientX - magnifierSize - offset;
    }
    if (top + magnifierSize > window.innerHeight) {
        top = clientY - magnifierSize - offset;
    }
    
    magnifier.style.left = `${left}px`;
    magnifier.style.top = `${top}px`;
}

export function getPixelColor(imageElementId, clientX, clientY, scale, offsetX, offsetY) {
    if (!imageElementId || !canvas || !ctx) return null;
    
    const img = document.getElementById(imageElementId);
    if (!img) return null;
    
    const rect = img.getBoundingClientRect();
    
    // Calculate image coordinates
    const localX = Math.floor((clientX - rect.left - offsetX) / scale);
    const localY = Math.floor((clientY - rect.top - offsetY) / scale);
    
    // Check bounds
    if (localX < 0 || localX >= img.naturalWidth || localY < 0 || localY >= img.naturalHeight) {
        return null;
    }
    
    // Set canvas size to match image
    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;
    
    // Draw image to canvas
    ctx.drawImage(img, 0, 0);
    
    // Get pixel color
    const imageData = ctx.getImageData(localX, localY, 1, 1);
    const [r, g, b] = imageData.data;
    
    return {
        r: r,
        g: g,
        b: b
    };
}

