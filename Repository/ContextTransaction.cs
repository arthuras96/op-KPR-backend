using Functions;
using MySql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class ContextTransaction
    {
        private MySqlConnection connection;
        private string connString = "Server=localhost;Database=prdb;Uid=sistema;Pwd=gvt@645;Pooling=True;";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Tratamento de query realizado anteriormente.")]
        public bool RunTransaction(List<string> Transactions)
        {
            connection = new MySqlConnection(connString);

            //SqlCommand command = connection.CreateCommand();
            MySqlTransaction transaction = null;

            try
            {
                // BeginTransaction() Requires Open Connection
                connection.Open();
                transaction = connection.BeginTransaction();

                foreach (string query in Transactions)
                {
                    MySqlCommand command = new MySqlCommand(query, connection, transaction);
                    //command.CommandText = query;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();

                return true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                var ss = ex.Message;

                //Email es = new Email();
                //es.EmailSender("arthuralencarsilva@gmail.com", "arthuralencarsilva@gmail.com", "", "", "Erro - Transaction",
                //    "Mensagem de erro (501 - catch Transaction):<br><br>" + ex);

                return false;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
