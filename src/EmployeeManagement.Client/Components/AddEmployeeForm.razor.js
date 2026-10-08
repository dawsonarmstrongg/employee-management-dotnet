// Focuses the first field Blazor marked invalid. Runs after the render that added aria-invalid;
// waits one frame and tries again in case the browser hasn't painted that render yet.
export function focusFirstInvalid(container) {
    const find = () => container?.querySelector('[aria-invalid="true"]');
    const field = find();
    if (field) {
        field.focus();
        return;
    }
    requestAnimationFrame(() => find()?.focus());
}