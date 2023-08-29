using System.Data;

namespace Groove.CSFE.Supplemental.Services
{
    public interface IDbConnections : IDisposable
    {
        IDbConnection Supplemental { get; }
        IDbConnection CsMaster { get; }
        IDbConnection CsPortal { get; }
    }
}
