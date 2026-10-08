// Wires a native <dialog> to its Blazor component. Closing always goes through .NET so the parent
// component can confirm first (for example "Discard this employee?").
const states = new WeakMap();

export function init(dialog, dotNet) {
    const state = { dotNet, opener: null, closingFromDotNet: false, pointerDownOutside: false };

    state.onKeyDown = event => {
        // An open control inside the dialog (such as the state list) handles Escape first.
        if (event.key !== 'Escape' || event.defaultPrevented || !dialog.open) {
            return;
        }
        event.preventDefault();
        requestClose(dialog, state);
    };

    // Other close requests the browser sends (for example a device back gesture).
    state.onCancel = event => {
        event.preventDefault();
        requestClose(dialog, state);
    };

    state.onClose = () => {
        if (state.closingFromDotNet) {
            state.closingFromDotNet = false;
            return;
        }
        state.dotNet.invokeMethodAsync('NotifyClosedByBrowser');
    };

    // A click on the backdrop targets the dialog itself, outside its box. Both the press and the release
    // must be outside, so selecting text and releasing over the backdrop does not close it.
    state.onPointerDown = event => {
        state.pointerDownOutside = event.target === dialog && isOutside(dialog, event);
    };

    state.onClick = event => {
        const outside = state.pointerDownOutside && event.target === dialog && isOutside(dialog, event);
        state.pointerDownOutside = false;
        if (outside) {
            requestClose(dialog, state);
        }
    };

    dialog.addEventListener('keydown', state.onKeyDown);
    dialog.addEventListener('cancel', state.onCancel);
    dialog.addEventListener('close', state.onClose);
    dialog.addEventListener('pointerdown', state.onPointerDown);
    dialog.addEventListener('click', state.onClick);
    states.set(dialog, state);
}

export function show(dialog) {
    if (dialog.open) {
        return;
    }
    const state = states.get(dialog);
    if (state) {
        state.opener = document.activeElement;
    }
    dialog.showModal();
}

export function hide(dialog) {
    const state = states.get(dialog);
    if (!dialog.open) {
        return;
    }
    if (state) {
        state.closingFromDotNet = true;
    }
    dialog.close();

    // Browsers return focus to the opener themselves; this covers the ones that don't, an opener that
    // was removed while the dialog was open, and dialogs opened by a backdrop click (which leaves focus
    // on <body>). In that case focus goes back into the dialog underneath, or to the page heading.
    const active = document.activeElement;
    if (!active || active === document.body) {
        const opener = state?.opener;
        if (opener && opener !== document.body && opener.isConnected && typeof opener.focus === 'function') {
            opener.focus();
        } else {
            const underneath = document.querySelector('dialog[open]');
            const target = underneath?.querySelector('[autofocus], input, select, textarea, button')
                ?? document.querySelector('main h1');
            target?.focus();
        }
    }
    if (state) {
        state.opener = null;
    }
}

export function dispose(dialog) {
    const state = states.get(dialog);
    if (!state) {
        return;
    }
    dialog.removeEventListener('keydown', state.onKeyDown);
    dialog.removeEventListener('cancel', state.onCancel);
    dialog.removeEventListener('close', state.onClose);
    dialog.removeEventListener('pointerdown', state.onPointerDown);
    dialog.removeEventListener('click', state.onClick);
    states.delete(dialog);
    if (dialog.open) {
        state.closingFromDotNet = true;
        dialog.close();
    }
}

function requestClose(dialog, state) {
    commitPendingInput(dialog);
    state.dotNet.invokeMethodAsync('RequestClose');
}

// Blazor text inputs update their value on "change", which normally fires on blur. Escape and backdrop
// clicks can happen while the cursor is still in a field, so send that change first; otherwise text the
// user just typed would not count as unsaved.
function commitPendingInput(dialog) {
    const active = document.activeElement;
    if (active && dialog.contains(active) && active.matches('input, textarea, select')) {
        active.dispatchEvent(new Event('change', { bubbles: true }));
    }
}

function isOutside(dialog, event) {
    const box = dialog.getBoundingClientRect();
    return event.clientX < box.left || event.clientX > box.right || event.clientY < box.top || event.clientY > box.bottom;
}