using Azure.Core;
using Azure.Identity;
using System;

namespace TaskManager.Infrastructure.Services
{
    public static class AzureCredentials
    {
        public static TokenCredential GetCredentials()
        {
            if (IsDevelopment)
            {
                return new AzureCliCredential();
            }

            return new ManagedIdentityCredential();
        }

        private static bool IsDevelopment => "Development".Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            StringComparison.InvariantCultureIgnoreCase
        );
    }
}
