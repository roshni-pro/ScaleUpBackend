using IdentityServer4;
using IdentityServer4.Models;

namespace ScaleUP.Services.IdentityAPI
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
             new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };
        }

        public static IEnumerable<ApiResource> GetAllApiResources()
        {
            return new List<ApiResource>
        {
           new ApiResource
            {
                Name = "crmApi",
                DisplayName = "API #1",
                Description = "Allow the application to access API #1 on your behalf",
                Scopes = new List<string> {"crmApi"},
                ApiSecrets = new List<Secret> {new Secret("secret".Sha256())}, // change me!
                UserClaims = new List<string> {"role"}

            }
        };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new[]
            {
            new ApiScope("crmApi", "Access to API #1"),
        };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
        {
            new Client
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "crmApi" }
            },

              new Client
            {
                  ClientId = "mvc",
                  ClientName = "MVC Client",
                  AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                  RedirectUris = {"https://localhost:44315/signin-oidc"},
                  PostLogoutRedirectUris = {"https://localhost:44315/signout-callback-oidc"},
                  RequireConsent = false,
                    AllowOfflineAccess = false,
        AlwaysSendClientClaims = false,

        AlwaysIncludeUserClaimsInIdToken = true,
        ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                  AllowedScopes =new List<string>
                  {
                      IdentityServerConstants.StandardScopes.OpenId,
                      IdentityServerConstants.StandardScopes.Profile,
                      IdentityServerConstants.StandardScopes.Email,
                      "role",
                      "crmApi"
            }
        },
    };


        }
    }
}
