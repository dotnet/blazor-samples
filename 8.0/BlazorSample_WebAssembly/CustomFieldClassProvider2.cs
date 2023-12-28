using Microsoft.AspNetCore.Components.Forms;

namespace BlazorSample;

public class CustomFieldClassProvider2 : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext,
        in FieldIdentifier fieldIdentifier)
    {
        if (fieldIdentifier.FieldName == "Name")
        {
            return editContext.IsValid(fieldIdentifier) ?
                "validField" : "invalidField";
        }

        return string.Empty;
    }
}
