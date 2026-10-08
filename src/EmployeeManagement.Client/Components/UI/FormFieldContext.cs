namespace EmployeeManagement.Client.Components.UI;

// Passed to a FormField's input so it can point aria-describedby at the field's help and error text.
public sealed record FormFieldContext(string DescribedBy);