using Microsoft.AspNetCore.Components.Forms;

namespace BlazorSample;

public class CustomFieldClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, 
        in FieldIdentifier fieldIdentifier)
    {
        return editContext.IsValid(fieldIdentifier) ? 
            "validField" : "invalidField";
    }
}
