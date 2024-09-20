using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace ScaleUP.Services.IdentityAPI.Extensions
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (context.UserName == "amit22" && context.Password == "Amit@1234")
            {
                context.Result = new GrantValidationResult(context.UserName, GrantType.ResourceOwnerPassword);
            }

            return Task.CompletedTask;
        }
    }
}
