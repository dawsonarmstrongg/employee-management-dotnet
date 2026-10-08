// On small screens the navigation is an off-canvas panel. Close it when focus moves outside it
// (for example tabbing past the last link) and when the window grows to the rail or full layout.
const smallScreen = window.matchMedia('(max-width: 767.98px)');

export function init(nav, dotNet) {
    const isOpen = () => nav.classList.contains('is-open');

    const onFocusOut = event => {
        if (isOpen() && smallScreen.matches && event.relatedTarget && !nav.contains(event.relatedTarget)) {
            dotNet.invokeMethodAsync('Collapse');
        }
    };

    const onScreenChange = () => {
        if (!smallScreen.matches && isOpen()) {
            dotNet.invokeMethodAsync('Collapse');
        }
    };

    nav.addEventListener('focusout', onFocusOut);
    smallScreen.addEventListener('change', onScreenChange);
    nav._zlNavCleanup = () => {
        nav.removeEventListener('focusout', onFocusOut);
        smallScreen.removeEventListener('change', onScreenChange);
    };
}

export function dispose(nav) {
    nav?._zlNavCleanup?.();
    delete nav?._zlNavCleanup;
}