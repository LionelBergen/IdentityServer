using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duende.IdentityServer.Storage.Models;

/// <summary>
/// Represents a persisted Pushed Authorization Request.
/// </summary>
public class PushedAuthorizationRequest
{
    public string RequestUri { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string Parameters { get; set; }
}
