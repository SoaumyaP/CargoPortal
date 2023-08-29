using Dapper;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.CSFE.Supplemental.Test
{
    internal class ConvertDataToInsert
    {
        const string constr = "Server=tcp:global-ltl-db.database.windows.net,1433;Initial Catalog=supplementalDb;Persist Security Info=False;User ID=ltl-admin;Password=Abc@1234;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [Test]
        public void ConvertToStatement()
        {
            SqlConnection conn = new SqlConnection(constr);
            conn.Open();

            string sql = "select * from ArticleMaster where id in (select articleid from invtransactions) order by id";
            var queryResult = conn.Query<dynamic>(sql);

            string json = System.Text.Json.JsonSerializer.Serialize(queryResult);

            conn.Close();
        }
    }
}
