using System.ComponentModel;

namespace Demo1;

public class ApplicationMessages
{
    public class AuthenticationPolicies : ILocalizedResource
    {
        public enum Errors
        {
            [Description("You tried to login with the forbidden tenant")] TriedToLoginWithForbiddenTenant,
            [Description("You tried to login with the forbidden username")] TriedToLoginWithForbiddenUsername,
            [Description("The password must have at least 3 characteres")] PasswordMustHave3Characters,
        }
    }
}