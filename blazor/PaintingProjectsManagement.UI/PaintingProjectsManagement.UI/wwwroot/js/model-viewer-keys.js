export function attach(dotnetRef) {
    function onKeyDown(e) {
        if (e.key === 'ArrowLeft') { e.preventDefault(); dotnetRef.invokeMethodAsync('JsPrev'); }
        else if (e.key === 'ArrowRight') { e.preventDefault(); dotnetRef.invokeMethodAsync('JsNext'); }
    }
    document.addEventListener('keydown', onKeyDown);
    return {
        dispose: () => document.removeEventListener('keydown', onKeyDown)
    };
}