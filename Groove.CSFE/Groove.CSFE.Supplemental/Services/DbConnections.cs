using System.Data;
using System.Data.SqlClient;

namespace Groove.CSFE.Supplemental.Services
{
    public class DbConnections : IDbConnections
    {
        private readonly IConfiguration configs;
        IDbConnection _supplemental;
        public DbConnections(IConfiguration configs)
        {
            this.configs = configs;
        }
        public IDbConnection Supplemental
        {
            get
            {
                if (_supplemental == null)
                {
                    string constr = configs["ConnectionStrings:CSP_Supplemental_ConnctionString"];
                    _supplemental = new SqlConnection(constr);
                    _supplemental.Open();
                }
                return _supplemental;
            }
        }

        IDbConnection _csMaster;
        public IDbConnection CsMaster
        {
            get
            {
                if (_csMaster == null)
                {
                    string constr = configs["ConnectionStrings:CSMaster_ConnectionString"];
                    _csMaster = new SqlConnection(constr);
                    _csMaster.Open();
                }
                return _csMaster;
            }
        }

        IDbConnection _csPortal;
        public IDbConnection CsPortal
        {
            get
            {
                if (_csPortal == null)
                {
                    string constr = configs["ConnectionStrings:CSP_ConnctionString"];
                    _csPortal = new SqlConnection(constr);
                    _csPortal.Open();
                }
                return _csPortal;
            }
        }

        public void Dispose()
        {
            if (_supplemental != null && _supplemental.State == ConnectionState.Open)
                _supplemental.Close();
            if (_supplemental != null)
                _supplemental.Dispose();

            if (_csMaster != null && _csMaster.State == ConnectionState.Open)
                _csMaster.Close();
            if (_csMaster != null)
                _csMaster.Dispose();

            if (_csPortal != null && _supplemental.State == ConnectionState.Open)
                _csPortal.Close();
            if (_csPortal != null)
                _csPortal.Dispose();
        }
    }
}
