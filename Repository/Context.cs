using MySql.Data.MySqlClient;
using System.Data;

namespace Repository
{
    public class Context
    {
        private string connString = "Server=localhost;Database=prdb;Uid=sistema;Pwd=gvt@645;Pooling=True;";
        
        private MySqlConnection myConnection;

        public Context()
        {
            myConnection = new MySqlConnection(connString);
            myConnection.Open();
        }

        // insert , update ou delete
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Tratamento de query realizado anteriormente.")]
        public void RunCommand(string strQuery)
        {
            MySqlCommand comando = new MySqlCommand(strQuery, myConnection);
            comando.ExecuteNonQuery();

        }

        // select
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Tratamento de query realizado anteriormente.")]
        public DataTable RunCommandDT(string strQuery)
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter(strQuery, myConnection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return ds.Tables[0];
        }

        // insert , update ou delete com retorno
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Tratamento de query realizado anteriormente.")]
        public MySqlDataReader RunCommandRetID(string strQuery)
        {
            var comando = new MySqlCommand(strQuery, myConnection);
            return comando.ExecuteReader();
        }

        public void Dispose()
        {
            if (myConnection.State == ConnectionState.Open)
            {
                myConnection.Dispose();
                myConnection.Close();
            }
        }
    }
}
