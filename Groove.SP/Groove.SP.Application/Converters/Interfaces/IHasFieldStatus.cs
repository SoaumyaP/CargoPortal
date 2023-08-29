using System.Collections.Generic;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Converters.Interfaces
{
    public interface IHasFieldStatus
    {
        Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        bool IsPropertyDirty(string name);
    }
}