// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Stores;

namespace IdentityServerHost.Pages.Portal;

public class ThirdPartyInitiatedLoginLink
{
    public string? LinkText { get; set; }
    public string? InitiateLoginUri { get; set; }
}


public class ClientRepository
{
    public async Task<IEnumerable<ThirdPartyInitiatedLoginLink>> GetClientsWithLoginUris(string? filter = null)
    {
        var query = Config.Clients
            .Where(c => c.InitiateLoginUri != null);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(x => x.ClientId.Contains(filter) || x.ClientName.Contains(filter));
        }

        var result = query.Select(c => new ThirdPartyInitiatedLoginLink
        {
            LinkText = string.IsNullOrWhiteSpace(c.ClientName) ? c.ClientId : c.ClientName,
            InitiateLoginUri = c.InitiateLoginUri
        });

        return result.ToList();
    }
}
