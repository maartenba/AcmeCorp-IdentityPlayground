// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcmeCorp.IdentityServer.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
     
        public void OnGet(string returnUrl = null)
        {
            LocalRedirect("/Account/Login?ReturnUrl=" + returnUrl);
        }
    }
}