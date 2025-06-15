using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace App_CCP.TagHelpers
{
    [HtmlTargetElement("required-label", Attributes = "asp-for")]
    public class RequiredLabelTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public required ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "label";
            output.Attributes.SetAttribute("class", "form-label");

            var displayName = For.Metadata.DisplayName ?? For.Name;
            var isRequired = For.Metadata.IsRequired;

            var labelHtml = displayName;
            if (isRequired)
            {
                labelHtml += " <span class=\"text-danger\">*</span>";
            }

            output.Content.SetHtmlContent(labelHtml);
        }
    }
}
