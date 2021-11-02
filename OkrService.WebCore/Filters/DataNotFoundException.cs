using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace OKRService.WebCore.Filters
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException(string message) : base(message)
        {

        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected DataNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
