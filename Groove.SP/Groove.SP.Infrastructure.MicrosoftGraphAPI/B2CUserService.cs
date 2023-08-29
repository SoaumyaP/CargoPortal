using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Groove.SP.Infrastructure.MicrosoftGraphAPI.Config;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace Groove.SP.Infrastructure.MicrosoftGraphAPI
{
    public class B2CUserService : IB2CUserService
    {
        #region Fields
        private readonly MicrosoftGraphSettings _graphSettings;
        private readonly GraphServiceClient _graphClient;
        #endregion

        #region ctors
        public B2CUserService(IOptions<MicrosoftGraphSettings> graphSettings,
            IServiceProvider serviceProvider)
        {
            _graphSettings = graphSettings.Value;

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(_graphSettings.ClientId)
            .WithTenantId(_graphSettings.TenantId)
            .WithClientSecret(_graphSettings.ClientSecret)
            .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            _graphClient = new GraphServiceClient(authProvider);
        }
        #endregion

        #region Utils
        public string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
        #endregion

        public async Task<dynamic> GetUserBySignInEmailAsync(string userEmailAddress)
        {
            var users = await FindUsersByEmailAsync(userEmailAddress);

            if (users != null && users.Count > 1)
            {
                throw new Exception($"Multiple users found for email {userEmailAddress}");
            }

            return users.FirstOrDefault();
        }

        public async Task CreateUserAsync(
            string displayName,
            string email,
            string title,
            string department,
            string companyName
            )
        {
            var newUser = new UserModel
            {
                DisplayName = displayName,
                JobTitle = title,
                Department = department,
                CompanyName = companyName,
                PasswordProfile = new PasswordProfile
                {
                    Password = GenerateRandomPassword(),
                    ForceChangePasswordNextSignIn = false
                },
                PasswordPolicies = "DisablePasswordExpiration",
                AccountEnabled = true,
                Identities = new List<ObjectIdentity>()
                {
                    new ObjectIdentity
                    {
                        SignInType = "emailAddress",
                        Issuer = $"{_graphSettings.Tenant}",
                        IssuerAssignedId = email
                    }
                }
            };

            await _graphClient.Users
                .Request()
                .AddAsync(newUser);
        }

        public async Task RemoveUserAsync(string userEmailAddress)
        {
            var users = await FindUsersByEmailAsync(userEmailAddress);
            if (users != null && users.Count > 1)
            {
                throw new Exception($"Multiple users found for email {userEmailAddress}");
            }

            // Remove by user id on Azure B2C
            await _graphClient.Users[users.FirstOrDefault().Id].Request().DeleteAsync();         
        }

        private Task<IGraphServiceUsersCollectionPage> FindUsersByEmailAsync(string userEmail)
        {
            var result = _graphClient.Users
                         .Request()
                         .Filter($"identities/any(c:c/issuerAssignedId eq '{WebUtility.UrlEncode(userEmail)}' and c/issuer eq '{_graphSettings.Tenant}')")
                         .Select(e => new
                         {
                             e.Id,
                             e.DisplayName
                         })
                         .GetAsync();
            return result;
        }
    }
}
