// Blazor can't conditionally cancel a keydown at the moment it happens, so this listener stops
// Enter from submitting the form while an option is highlighted, and stops arrow keys from
// moving the caret while the list is open. Blazor still receives the event and does the selection.
export function init(input) {
    const onKeyDown = e => {
        if (input.getAttribute('aria-expanded') !== 'true') {
            return;
        }
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
            e.preventDefault();
        } else if (e.key === 'Enter' && input.getAttribute('aria-activedescendant')) {
            e.preventDefault();
        }
    };
    input.addEventListener('keydown', onKeyDown);
    input._usStateKeyDown = onKeyDown;
}

export function scrollIntoView(id) {
    document.getElementById(id)?.scrollIntoView({ block: 'nearest' });
}

export function dispose(input) {
    if (input?._usStateKeyDown) {
        input.removeEventListener('keydown', input._usStateKeyDown);
        delete input._usStateKeyDown;
    }
}