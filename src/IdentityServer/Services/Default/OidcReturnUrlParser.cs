// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Duende.IdentityServer.Configuration;

namespace Duende.IdentityServer.Services
{
    internal class OidcReturnUrlParser : IReturnUrlParser
    {
        private readonly IdentityServerOptions _options;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IUserSession _userSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;

        public OidcReturnUrlParser(
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IUserSession userSession,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OidcReturnUrlParser> logger,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
        {
            _options = options;
            _validator = validator;
            _userSession = userSession;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
        {
            if (IsValidReturnUrl(returnUrl))
            {
                var parameters = returnUrl.ReadQueryStringAsNameValueCollection();
                if (_authorizationParametersMessageStore != null)
                {
                    var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                    var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                    parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();
                }

                var user = await _userSession.GetUserAsync();
                var result = await _validator.ValidateAsync(parameters, user);
                if (!result.IsError)
                {
                    _logger.LogTrace("AuthorizationRequest being returned");
                    return new AuthorizationRequest(result.ValidatedRequest);
                }
            }

            _logger.LogTrace("No AuthorizationRequest being returned");
            return null;
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            if (_options.UserInteraction.AllowOriginInReturnUrl && returnUrl != null)
            {
                if (!Uri.TryCreate(returnUrl, UriKind.RelativeOrAbsolute, out _))
                {
                    _logger.LogTrace("returnUrl is not valid");
                    return false;
                }

                var origin = _httpContextAccessor.HttpContext.GetIdentityServerOrigin();
                if (returnUrl.StartsWith(origin, StringComparison.OrdinalIgnoreCase) == true)
                {
                    returnUrl = returnUrl.Substring(origin.Length);
                }
            }
            
            if (returnUrl.IsLocalUrl())
            {
                {
                    var index = returnUrl.IndexOf('?');
                    if (index >= 0)
                    {
                        returnUrl = returnUrl.Substring(0, index);
                    }
                }
                {
                    var index = returnUrl.IndexOf('#');
                    if (index >= 0)
                    {
                        returnUrl = returnUrl.Substring(0, index);
                    }
                }

                if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                    returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
                {
                    _logger.LogTrace("returnUrl is valid");
                    return true;
                }
            }

            _logger.LogTrace("returnUrl is not valid");
            return false;
        }
    }
}
