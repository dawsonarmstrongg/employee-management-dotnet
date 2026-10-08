using System.Net;
using Bunit;
using EmployeeManagement.Client.Components;
using EmployeeManagement.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Tests.Components;

// R-19 (plus R-03/R-04 on single tests): Blazor add-employee form collects every field separately, shows
// client-side validation messages, and surfaces API duplicate-email (409) and invalid-phone
// errors next to the relevant field.
[Trait("Requirement", "R-19")]
public class AddEmployeeFormTests : BunitContext
{
    public AddEmployeeFormTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose; // The form imports small JS modules for focus/scroll; not under test here.
    }

    private FakeHttpMessageHandler UseFakeApi(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new FakeHttpMessageHandler(responder);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(new EmployeeApiClient(http));
        return handler;
    }

    [Fact]
    public void Render_ShowsEveryFieldSeparately_NotTheCombinedTableFormat()
    {
        UseFakeApi(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var cut = Render<AddEmployeeForm>();

        Assert.NotNull(cut.Find("#firstName"));
        Assert.NotNull(cut.Find("#lastName"));
        Assert.NotNull(cut.Find("#email"));
        Assert.NotNull(cut.Find("#dateOfBirth"));
        Assert.NotNull(cut.Find("#phone"));
        Assert.NotNull(cut.Find("#address1"));
        Assert.NotNull(cut.Find("#address2"));
        Assert.NotNull(cut.Find("#city"));
        Assert.NotNull(cut.Find("#state"));
        Assert.NotNull(cut.Find("#zip"));
    }

    [Fact]
    public void Submit_EmptyForm_ShowsRequiredMessagesAndSendsNoRequest()
    {
        var handler = UseFakeApi(_ => throw new InvalidOperationException("No HTTP call should happen for an invalid form."));

        var cut = Render<AddEmployeeForm>();
        cut.Find("form").Submit();

        Assert.Contains("First name is required.", cut.Markup);
        Assert.Contains("Last name is required.", cut.Markup);
        Assert.Contains("Email address is required.", cut.Markup);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    [Trait("Requirement", "R-04")]
    public void PhoneField_InvalidFormatAfterBlur_ShowsValidationMessage()
    {
        UseFakeApi(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var cut = Render<AddEmployeeForm>();

        var phone = cut.Find("#phone");
        phone.Input("555123"); // Incomplete; the component waits for blur or a complete number.
        phone.Blur();

        Assert.Contains("Phone number must be in the format", cut.Markup);
    }

    [Fact]
    [Trait("Requirement", "R-04")]
    public void PhoneField_CompleteValidNumber_ShowsNoValidationMessage()
    {
        UseFakeApi(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var cut = Render<AddEmployeeForm>();

        cut.Find("#phone").Input("5551234567");

        Assert.DoesNotContain("Phone number must be in the format", cut.Markup);
    }

    [Fact]
    [Trait("Requirement", "R-03")]
    public void Submit_ApiReturnsDuplicateEmailConflict_ShowsMessageUnderEmailField()
    {
        const string json = """{"title":"Email address already in use.","detail":"Another employee already uses the email address 'dup@example.com'.","status":409}""";
        UseFakeApi(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/problem+json"),
        });

        var cut = Render<AddEmployeeForm>();
        FillValidForm(cut, email: "dup@example.com");
        cut.Find("form").Submit();

        Assert.Contains("already uses the email address", cut.Markup);
    }

    [Fact]
    public void Submit_ServerUnreachable_ShowsGeneralErrorMessage()
    {
        UseFakeApi(_ => throw new HttpRequestException("simulated network failure"));

        var cut = Render<AddEmployeeForm>();
        FillValidForm(cut);
        cut.Find("form").Submit();

        Assert.Contains("Could not reach the server", cut.Markup);
    }

    [Fact]
    public void Submit_ValidForm_InvokesOnCreatedWithServerResponse()
    {
        const string json = """
            {"id":42,"firstName":"Pat","lastName":"Lee","email":"pat.lee@example.com","phoneNumber":"(555)-123-4567",
             "dateOfBirth":"1990-01-01","address":{"address1":"1 Test Way","address2":null,"city":"Austin","state":"TX","zip":"73301"}}
            """;
        UseFakeApi(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });

        EmployeeManagement.Contracts.Employees.EmployeeDto? created = null;
        var cut = Render<AddEmployeeForm>(p => p.Add(
            f => f.OnCreated,
            Microsoft.AspNetCore.Components.EventCallback.Factory.Create<EmployeeManagement.Contracts.Employees.EmployeeDto>(
                this, e => created = e)));
        FillValidForm(cut);

        cut.Find("form").Submit();

        Assert.NotNull(created);
        Assert.Equal(42, created!.Id);
    }

    private static void FillValidForm(IRenderedComponent<AddEmployeeForm> cut, string email = "pat.lee@example.com")
    {
        cut.Find("#firstName").Change("Pat");
        cut.Find("#lastName").Change("Lee");
        cut.Find("#email").Change(email);
        cut.Find("#dateOfBirth").Change("1990-01-01");
        cut.Find("#phone").Input("5551234567");
        cut.Find("#address1").Change("1 Test Way");
        cut.Find("#city").Change("Austin");
        cut.Find("#state").Input("TX");
        cut.Find("#zip").Change("73301");
    }
}
