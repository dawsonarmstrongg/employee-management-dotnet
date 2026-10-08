// Blazor can't conditionally cancel a keydown at the moment it happens, so this listener stops Enter
// from submitting the form while an option is highlighted, stops arrow keys from moving the caret, and
// marks Escape as handled so an enclosing drawer doesn't close too. Blazor still receives the event and
// does the selection. It also opens the list upward when there isn't room below the field.
const listHeight = 240; // max-height of .state-options (15rem)

export function init(input) {
    const picker = input.closest('.state-picker');
    const place = () => updatePlacement(input, picker);

    const onKeyDown = e => {
        if (e.key === 'ArrowDown') {
            place();
        }
        if (input.getAttribute('aria-expanded') !== 'true') {
            return;
        }
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp' || e.key === 'Escape') {
            e.preventDefault();
        } else if (e.key === 'Enter' && input.getAttribute('aria-activedescendant')) {
            e.preventDefault();
        }
    };

    input.addEventListener('keydown', onKeyDown);
    input.addEventListener('focus', place);
    input.addEventListener('click', place);
    input.addEventListener('input', place);
    input._usState = { onKeyDown, place };
}

export function scrollIntoView(id) {
    document.getElementById(id)?.scrollIntoView({ block: 'nearest' });
}

export function dispose(input) {
    const handlers = input?._usState;
    if (!handlers) {
        return;
    }
    input.removeEventListener('keydown', handlers.onKeyDown);
    input.removeEventListener('focus', handlers.place);
    input.removeEventListener('click', handlers.place);
    input.removeEventListener('input', handlers.place);
    delete input._usState;
}

function updatePlacement(input, picker) {
    if (!picker) {
        return;
    }
    const field = input.getBoundingClientRect();
    const area = scrollArea(input)?.getBoundingClientRect();
    const top = Math.max(area?.top ?? 0, 0);
    const bottom = Math.min(area?.bottom ?? window.innerHeight, window.innerHeight);
    const below = bottom - field.bottom;
    const above = field.top - top;
    picker.dataset.placement = below < listHeight && above > below ? 'top' : 'bottom';
}

// The nearest scrolling container (for example the drawer body), which would clip the list.
function scrollArea(element) {
    for (let node = element.parentElement; node && node !== document.body; node = node.parentElement) {
        const overflowY = getComputedStyle(node).overflowY;
        if (overflowY === 'auto' || overflowY === 'scroll') {
            return node;
        }
    }
    return null;
}