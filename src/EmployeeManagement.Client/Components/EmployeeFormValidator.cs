using EmployeeManagement.Contracts.Employees;
using EmployeeManagement.Contracts.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace EmployeeManagement.Client.Components;

// Validates the add-employee form with the same rules and code the API uses (Contracts), and shows
// errors returned by the API (400/409) next to the matching fields. Both use the same error keys,
// such as "Email" or "Address.Zip". Used instead of <DataAnnotationsValidator />, which does not
// validate the nested Address.
public sealed class EmployeeFormValidator : ComponentBase, IDisposable
{
    private EditContext? editContext;
    private ValidationMessageStore? messages;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    private EmployeeRequest Model => (EmployeeRequest)editContext!.Model;

    protected override void OnParametersSet()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(EmployeeFormValidator)} must be placed inside an EditForm.");
        }

        // The form creates a new EditContext when it resets, so re-subscribe when it changes.
        if (ReferenceEquals(CurrentEditContext, editContext))
        {
            return;
        }

        Unsubscribe();
        editContext = CurrentEditContext;
        messages = new ValidationMessageStore(editContext);
        editContext.OnValidationRequested += HandleValidationRequested;
        editContext.OnFieldChanged += HandleFieldChanged;
    }

    // Shows errors that came back from the API.
    public void DisplayErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        messages!.Clear();
        AddMessages(errors, onlyField: null);
        editContext!.NotifyValidationStateChanged();
    }

    // On submit: check every field.
    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        messages!.Clear();
        AddMessages(Validate(), onlyField: null);
        editContext!.NotifyValidationStateChanged();
    }

    // While typing: re-check only the field that changed, so untouched fields don't show errors yet.
    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        messages!.Clear(e.FieldIdentifier);
        AddMessages(Validate(), e.FieldIdentifier);
        editContext!.NotifyValidationStateChanged();
    }

    private Dictionary<string, string[]> Validate() =>
        EmployeeRequestValidator.Validate(EmployeeRequestNormalizer.Normalize(Model));

    private void AddMessages(IReadOnlyDictionary<string, string[]> errors, FieldIdentifier? onlyField)
    {
        foreach (var (key, fieldMessages) in errors)
        {
            var field = ToFieldIdentifier(key);
            if (onlyField is null || field.Equals(onlyField.Value))
            {
                messages!.Add(field, fieldMessages);
            }
        }
    }

    // "Email" -> (request, "Email"); "Address.Zip" -> (request.Address, "Zip").
    // The (object, property) pair is how Blazor matches a message to an input and its <ValidationMessage>.
    private FieldIdentifier ToFieldIdentifier(string key)
    {
        const string addressPrefix = nameof(EmployeeRequest.Address) + ".";
        if (key.StartsWith(addressPrefix, StringComparison.OrdinalIgnoreCase) && Model.Address is not null)
        {
            return new FieldIdentifier(Model.Address, key[addressPrefix.Length..]);
        }

        return new FieldIdentifier(Model, key);
    }

    private void Unsubscribe()
    {
        if (editContext is not null)
        {
            editContext.OnValidationRequested -= HandleValidationRequested;
            editContext.OnFieldChanged -= HandleFieldChanged;
            messages?.Clear();
        }
    }

    public void Dispose() => Unsubscribe();
}