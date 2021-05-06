// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Duende.IdentityServer.Models
{
    /// <summary>
    /// Models general storage for an external authentication provider/handler scheme
    /// </summary>
    public class IdentityProvider
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public IdentityProvider()
        {
        }
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="other"></param>
        public IdentityProvider(IdentityProvider other)
        {
            Scheme = other.Scheme;
            DisplayName = other.DisplayName;
            Enabled = other.Enabled;
            Type = other.Type;
            Properties = new Dictionary<string, string>(other.Properties);
        }

        /// <summary>
        /// Scheme name for the provider.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Display name for the provider.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Flag that indicates if the provider should be used.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Protocol type of the provider.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Protocol specific properties for the provider.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Properties indexer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string this[string name]
        {
            get
            {
                Properties.TryGetValue(name, out var result);
                return result;
            }
            set
            {
                Properties[name] = value;
            }
        }
    }
}
