using EmployeeManagement.Client.Components.UI;

namespace EmployeeManagement.Client.Services;

public sealed record ToastMessage(Guid Id, Tone Tone, string Text);

// Short confirmations shown in the corner of the page (for example "Jane Doe was added.").
// Pages call Show; the ToastHost in the layout displays and dismisses them.
public sealed class ToastService
{
    private readonly List<ToastMessage> messages = [];

    public event Action? Changed;

    public IReadOnlyList<ToastMessage> Messages => messages;

    public void Show(Tone tone, string text)
    {
        messages.Add(new ToastMessage(Guid.NewGuid(), tone, text));
        Changed?.Invoke();
    }

    public void Dismiss(Guid id)
    {
        if (messages.RemoveAll(message => message.Id == id) > 0)
        {
            Changed?.Invoke();
        }
    }
}