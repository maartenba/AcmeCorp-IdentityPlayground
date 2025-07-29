using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AcmeCorp.IdentityServer;

[HtmlTargetElement("passkey-submit")]
public class PasskeySubmitTagHelper : TagHelper
{
    [HtmlAttributeName("operation")]
    public PasskeyOperation Operation { get; set; }

    [HtmlAttributeName("name")]
    public string Name { get; set; } = null!;
    
    [HtmlAttributeName("email-name")]
    public string? EmailName { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Button is the main element we want to create, capture all attributes etc.
        var buttonAttributes = output.Attributes.Where(it => it.Name != "operation" && it.Name != "name" && it.Name != "email-name").ToList();
        var buttonContent = (await output.GetChildContentAsync(NullHtmlEncoder.Default))
            .GetContent(NullHtmlEncoder.Default);
        
        // Create the button
        using var htmlWriter = new StringWriter();
        htmlWriter.Write("<button type=\"submit\" name=\"__passkeySubmit\" ");
        foreach (var buttonAttribute in buttonAttributes)
        {
            buttonAttribute.WriteTo(htmlWriter, NullHtmlEncoder.Default);
            htmlWriter.Write(" ");
        }
        htmlWriter.Write(">");
        if (!string.IsNullOrEmpty(buttonContent))
        {
            htmlWriter.Write(buttonContent);
        }
        htmlWriter.Write("</button>");
        htmlWriter.WriteLine();
        
        // Create the element
        htmlWriter.Write("<passkey-submit ");
        htmlWriter.Write($"operation=\"{Operation}\" ");
        htmlWriter.Write($"name=\"{Name}\" ");
        htmlWriter.Write($"email-name=\"{EmailName ?? ""}\" ");
        htmlWriter.Write(">");
        htmlWriter.Write("</passkey-submit>");
        
        // Emit the element
        output.TagName = null;
        output.Attributes.Clear();
        output.Content.Clear();
        output.Content.SetHtmlContent(htmlWriter.ToString());

        await base.ProcessAsync(context, output);
    }
}