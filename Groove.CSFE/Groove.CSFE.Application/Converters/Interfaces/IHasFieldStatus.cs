using System.Collections.Generic;
using Groove.CSFE.Core;

namespace Groove.CSFE.Application.Converters.Interfaces
{
    public interface IHasFieldStatus
    {
        Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        bool IsPropertyDirty(string name);
    }
}