using System;
using System.Collections.Generic;

namespace Totem.Infrastructure
{
    public class CaseInsensitiveDictionary<T> : Dictionary<string, T>
    { 
        public CaseInsensitiveDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}
