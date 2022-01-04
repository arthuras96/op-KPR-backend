using Application.Account.Models;
using Application.General;
using Application.General.Entities;
using Application.General.Models;
using Functions;
using Repository;
using System;
using System.Collections.Generic;
using System.Data;


namespace Application.Account
{
    public class AccountService
    {
        private Context context;
        private LogService ls;
        
        public GenericReturnModel AddEditAccount(AccountModel account, string IdPerson, string IdAdmin)
        {
            QueryUtils queryUtils = new QueryUtils();
            account = queryUtils.SQLHandler(account);

            if (account.IDPERSON == 0)
                return AddAccount(account, IdPerson, IdAdmin);
            else
                return EditAccount(account, IdPerson);
        }

        private GenericReturnModel AddAccount(AccountModel account, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();

            QueryUtils queryUtils = new QueryUtils();
            Utils utils = new Utils();

            ls = new LogService();

            string stQuery;

            string username = utils.RemoveAccents(account.NAME.Replace(" ", ".").Replace("-", ".").ToLower());
            int contUsername = 1;
            bool controlUsername = true;

            while (controlUsername)
            {
                stQuery = "select USERNAME from tbperson" +
                    " where USERNAME = " + queryUtils.InsertSingleQuotes(username);

                context = new Context();

                try
                {
                    var retDT = context.RunCommandDT(stQuery);

                    if (retDT.Rows.Count > 0)
                    {
                        contUsername++;
                        if (contUsername > 2)
                            username = username.Replace((contUsername - 1).ToString(), contUsername.ToString());
                        else
                            username = username + contUsername.ToString();
                    }
                    else
                    {
                        controlUsername = false;
                    }
                }
                catch (Exception e)
                {
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - Add Account (70)", e.Message, stQuery);
                }
            }

            stQuery = " INSERT INTO tbperson (" +
                    "	 NAME" +
                    "	,USERNAME" +
                    "	,PASSWORD" +
                    "	,IDROLE" +
                    "	,IDADMIN" +
                    "	,IDTYPEPERSON" +
                    "	,ACTIVE" +
                    "	)" +
                    " VALUES (" +
                    "	 " + queryUtils.InsertSingleQuotes(account.NAME) +
                    "	," + queryUtils.InsertSingleQuotes(username) +
                    "   ," + queryUtils.InsertSingleQuotes("123@mudar") +
                    "	," + "5" +
                    "   ," + IdAdmin +
                    "	," + "2" +
                    "	," + account.ACTIVE +
                    "	);";

            context = new Context();

            account.IDPERSON = 0;

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDPERSON;");

                if (retDB.HasRows)
                    account.IDPERSON = int.Parse(queryUtils.ReturnId(retDB, "IDPERSON"));
                
                statusReturn.ID = account.IDPERSON;
            }
            catch(Exception e)
            {
                account.IDPERSON = 0;

                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - Add Account (110)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                statusReturn.STATUSCODE = 500;
            }

            context.Dispose();

            if(account.IDPERSON != 0)
            {
                List<string> querys = new List<string>();

                stQuery = "INSERT INTO tbjuridicalperson (" +
                        "	IDPERSON" +
                        "	,CNPJ" +
                        "	,CONTACT" +
                        "	,DURESSPASSWORD" +
                        "	,HISTORICPERSIST" +
                        "	)" +
                        " VALUES (" +
                        "	 " + account.IDPERSON +
                        "	," + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.CNPJ)) +
                        "	," + queryUtils.InsertSingleQuotes(account.CONTACT) +
                        "	," + queryUtils.InsertSingleQuotes(account.DURESSPASSWORD) +
                        "	," + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.HISTORICPERSIST)) +
                        "	);";

                querys.Add(stQuery);

                stQuery = "INSERT INTO tbaddress (" +
                        "	 CEP" +
                        "	,ADDRESS" +
                        "	,NUMBER" +
                        "	,REFERENCE" +
                        "	,NEIGHBORHOOD" +
                        "	,CITY" +
                        "	,STATE" +
                        "	,COUNTRY" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.CEP)) +
                        "	," + queryUtils.InsertSingleQuotes(account.ADDRESS) +
                        "	," + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.NUMBER)) +
                        "	," + queryUtils.InsertSingleQuotes(account.REFERENCE) +
                        "	," + queryUtils.InsertSingleQuotes(account.NEIGHBORHOOD) +
                        "	," + queryUtils.InsertSingleQuotes(account.CITY) +
                        "	," + queryUtils.InsertSingleQuotes(account.STATE) +
                        "	," + queryUtils.InsertSingleQuotes(account.COUNTRY) +
                        "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                stQuery = "INSERT INTO tbcontact (" +
                        "	 IDTYPECONTACT" +
                        "	,EMAIL" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "1" +
                        "	," + queryUtils.InsertSingleQuotes(account.EMAIL) +
                        "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                string auxStQueryTel1;
                string auxStQueryTel2;
                string auxStQueryTel3;
                string auxStQueryTelPre1 = "	,TEL";
                string auxStQueryTelPre2 = "	,TEL";
                string auxStQueryTelPre3 = "	,TEL";

                account.TELONE = utils.RemoveLeadingZero(utils.ExtractNumbers(account.TELONE));
                account.TELTWO = utils.RemoveLeadingZero(utils.ExtractNumbers(account.TELTWO));
                account.TELTHREE = utils.RemoveLeadingZero(utils.ExtractNumbers(account.TELTHREE));

                if (account.TELONE.Length > 9)
                {
                    auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(account.TELONE.Substring(0, 2)) +
                                     "	," + queryUtils.InsertSingleQuotes(account.TELONE.Substring(2));

                    auxStQueryTelPre1 = "	,DDDTEL" +
                                        "	,TEL";
                }
                else
                    auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(account.TELONE);

                stQuery = "INSERT INTO tbcontact (" +
                        "	IDTYPECONTACT" +
                        auxStQueryTelPre1 +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "2" +
                                 auxStQueryTel1 +
                        "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                if (account.TELTWO.Length > 9)
                {
                    auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(account.TELTWO.Substring(0, 2)) +
                                     "	," + queryUtils.InsertSingleQuotes(account.TELTWO.Substring(2));

                    auxStQueryTelPre2 = "	,DDDTEL" +
                                        "	,TEL";
                }
                else
                    auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(account.TELTWO);

                stQuery = "INSERT INTO tbcontact (" +
                        "	IDTYPECONTACT" +
                        auxStQueryTelPre2 +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "2" +
                                 auxStQueryTel2 +
                        "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                if (account.TELTHREE.Length > 9)
                {
                    auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(account.TELTHREE.Substring(0, 2)) +
                                     "	," + queryUtils.InsertSingleQuotes(account.TELTHREE.Substring(2));

                    auxStQueryTelPre3 = "	,DDDTEL" +
                                        "	,TEL";
                }
                else
                    auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(account.TELTHREE);

                if(account.TELTHREE.Length != 0) { 
                    stQuery = "INSERT INTO tbcontact (" +
                            "	IDTYPECONTACT" +
                            auxStQueryTelPre3 +
                            "	,IDPERSON" +
                            "	)" +
                            " VALUES (" +
                            "	 " + "2" +
                                     auxStQueryTel3 +
                            "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                            "	);";

                    querys.Add(stQuery);
                }

                try
                {
                    ContextTransaction ct = new ContextTransaction();
                    
                    if(ct.RunTransaction(querys))
                    {
                        statusReturn.MESSAGE = "Conta inserida com sucesso!";
                        statusReturn.STATUSCODE = 201;
                        //ls.AddLogSystem(int.Parse(IdPerson), account.IDPERSON, "Inseriu nova conta.");
                        stQuery = "";
                    }
                    else
                    {
                        stQuery = "delete from tbperson where IDPERSON = " + statusReturn.ID.ToString();

                        statusReturn.ID = 0;
                        statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                        statusReturn.STATUSCODE = 500;                       
                    }
                }
                catch (Exception e)
                {
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - Add Account (287)", e.Message, stQuery);
                }

                if (stQuery != "")
                {
                    context = new Context();
                    context.RunCommand(stQuery);
                    context.Dispose();
                }
                else
                {
                    ls.AddLogSystem(int.Parse(IdPerson), account.IDPERSON, "Adicionou nova conta", IdLogSystemList.REGISTERACCOUNT, "");
                }
            }

            return statusReturn;
        }

        private GenericReturnModel EditAccount(AccountModel account, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();

            QueryUtils queryUtils = new QueryUtils();
            Utils utils = new Utils();

            ls = new LogService();

            string stQuery;

            List<string> querys = new List<string>();

            stQuery = "DELETE FROM tbjuridicalperson WHERE IDPERSON = " + account.IDPERSON.ToString() + ";";
            querys.Add(stQuery);

            stQuery = "DELETE FROM tbcontact WHERE IDPERSON = " + account.IDPERSON.ToString() + ";";
            querys.Add(stQuery);

            stQuery = "DELETE FROM tbaddress WHERE IDPERSON = " + account.IDPERSON.ToString() + ";";
            querys.Add(stQuery);

            stQuery = "UPDATE tbperson" +
                        " SET" +
                        "	 NAME = " + queryUtils.InsertSingleQuotes(account.NAME) +
                        "	,ACTIVE = " + account.ACTIVE +
                        " WHERE IDPERSON = " + account.IDPERSON + ";";
            querys.Add(stQuery);

            stQuery = "INSERT INTO tbjuridicalperson (" +
                        "	 IDPERSON" +
                        "	,CNPJ" +
                        "	,CONTACT" +
                        "	,DURESSPASSWORD" +
                        "	,HISTORICPERSIST" +
                        "	)" +
                        " VALUES (" +
                        "	 " + account.IDPERSON +
                        "	," + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.CNPJ)) +
                        "	," + queryUtils.InsertSingleQuotes(account.CONTACT) +
                        "	," + queryUtils.InsertSingleQuotes(account.DURESSPASSWORD) +
                        "	," + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.HISTORICPERSIST)) +
                        "	);";
            querys.Add(stQuery);

            stQuery = "INSERT INTO tbaddress (" +
                    "	 CEP" +
                    "	,ADDRESS" +
                    "	,NUMBER" +
                    "	,REFERENCE" +
                    "	,NEIGHBORHOOD" +
                    "	,CITY" +
                    "	,STATE" +
                    "	,COUNTRY" +
                    "	,IDPERSON" +
                    "	)" +
                    " VALUES (" +
                    "	 " + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.CEP)) +
                    "	," + queryUtils.InsertSingleQuotes(account.ADDRESS) +
                    "	," + utils.ExtractNumbers(queryUtils.InsertSingleQuotes(account.NUMBER)) +
                    "	," + queryUtils.InsertSingleQuotes(account.REFERENCE) +
                    "	," + queryUtils.InsertSingleQuotes(account.NEIGHBORHOOD) +
                    "	," + queryUtils.InsertSingleQuotes(account.CITY) +
                    "	," + queryUtils.InsertSingleQuotes(account.STATE) +
                    "	," + queryUtils.InsertSingleQuotes(account.COUNTRY) +
                    "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                    "	);";
            querys.Add(stQuery);

            stQuery = "INSERT INTO tbcontact (" +
                    "	 IDTYPECONTACT" +
                    "	,EMAIL" +
                    "	,IDPERSON" +
                    "	)" +
                    " VALUES (" +
                    "	 " + "1" +
                    "	," + queryUtils.InsertSingleQuotes(account.EMAIL) +
                    "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                    "	);";
            querys.Add(stQuery);

            string auxStQueryTel1;
            string auxStQueryTel2;
            string auxStQueryTel3;
            string auxStQueryTelPre1 = "	,TEL";
            string auxStQueryTelPre2 = "	,TEL";
            string auxStQueryTelPre3 = "	,TEL";

            account.TELONE = utils.RemoveLeadingZero(utils.ExtractNumbers(account.TELONE));
            account.TELTWO = utils.RemoveLeadingZero(utils.ExtractNumbers(account.TELTWO));
            account.TELTHREE = utils.RemoveLeadingZero(utils.ExtractNumbers(account.TELTHREE));

            if (account.TELONE.Length > 9)
            {
                auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(account.TELONE.Substring(0, 2)) +
                                 "	," + queryUtils.InsertSingleQuotes(account.TELONE.Substring(2));

                auxStQueryTelPre1 = "	,DDDTEL" +
                                    "	,TEL";
            }
            else
                auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(account.TELONE);

            stQuery = "INSERT INTO tbcontact (" +
                    "	IDTYPECONTACT" +
                    auxStQueryTelPre1 +
                    "	,IDPERSON" +
                    "	)" +
                    " VALUES (" +
                    "	 " + "2" +
                             auxStQueryTel1 +
                    "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                    "	);";
            querys.Add(stQuery);

            if (account.TELTWO.Length > 9)
            {
                auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(account.TELTWO.Substring(0, 2)) +
                                 "	," + queryUtils.InsertSingleQuotes(account.TELTWO.Substring(2));

                auxStQueryTelPre2 = "	,DDDTEL" +
                                    "	,TEL";
            }
            else
                auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(account.TELTWO);

            stQuery = "INSERT INTO tbcontact (" +
                    "	IDTYPECONTACT" +
                    auxStQueryTelPre2 +
                    "	,IDPERSON" +
                    "	)" +
                    " VALUES (" +
                    "	 " + "2" +
                             auxStQueryTel2 +
                    "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                    "	);";
            querys.Add(stQuery);

            if (account.TELTHREE.Length > 9)
            {
                auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(account.TELTHREE.Substring(0, 2)) +
                                 "	," + queryUtils.InsertSingleQuotes(account.TELTHREE.Substring(2));

                auxStQueryTelPre3 = "	,DDDTEL" +
                                    "	,TEL";
            }
            else
                auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(account.TELTHREE);

            if (account.TELTHREE.Length != 0)
            {
                stQuery = "INSERT INTO tbcontact (" +
                        "	IDTYPECONTACT" +
                        auxStQueryTelPre3 +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "2" +
                                 auxStQueryTel3 +
                        "	," + queryUtils.InsertSingleQuotes(account.IDPERSON.ToString()) +
                        "	);";
                querys.Add(stQuery);
            }

            try
            {
                string stringAccount = "";
                try
                {
                    stringAccount = Newtonsoft.Json.JsonConvert.SerializeObject(GetCompleteAccount(IdPerson, account.IDPERSON, "0"));
                } 
                catch (Exception e) { }

                ContextTransaction ct = new ContextTransaction();

                if(ct.RunTransaction(querys))
                { 
                    statusReturn.MESSAGE = "Conta alterada com sucesso!";
                    statusReturn.STATUSCODE = 201;
                    
                    ls.AddLogSystem(int.Parse(IdPerson), account.IDPERSON, "Atualizou dados cadastrais.", IdLogSystemList.EDITACCOUNT, stringAccount);
                }
                else
                {
                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Falha ao atualizar no banco de dados.";
                    statusReturn.STATUSCODE = 500;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - Edit Account (490)", e.Message, stQuery);
            }

            return statusReturn;
        }

        public List<AccountModel> GetAccounts(string IdPerson)
        {
            List<AccountModel> accountReturn = new List<AccountModel>();
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "SELECT" +
                    "	 tbperson.IDPERSON" +
                    "	,tbperson.NAME" +
                    "	,tbjuridicalperson.CNPJ" +
                    "	,tbjuridicalperson.CONTACT" +
                    "	,tbjuridicalperson.DURESSPASSWORD" +
                    "	,tbjuridicalperson.HISTORICPERSIST" +
                    "	,tbperson.ACTIVE" +
                    "	,tbaddress.CEP" +
                    "	,tbaddress.ADDRESS" +
                    "	,tbaddress.NUMBER" +
                    "	,tbaddress.REFERENCE" +
                    "	,tbaddress.NEIGHBORHOOD" +
                    "	,tbaddress.CITY" +
                    "	,tbaddress.STATE" +
                    "	,tbaddress.COUNTRY" +
                    " FROM tbperson" +
                    " JOIN tbjuridicalperson ON tbjuridicalperson.IDPERSON = tbperson.IDPERSON" +
                    " JOIN tbaddress ON tbaddress.IDPERSON = tbperson.IDPERSON" +
                    " WHERE tbperson.IDTYPEPERSON = 2;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    accountReturn = queryUtils.DataTableToList<AccountModel>(retDT);

                    for (int i = 0; i < accountReturn.Count; i++)
                        accountReturn[i] = ReturnContact(accountReturn[i], IdPerson);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetAccounts (540)", e.Message, stQuery);
            }

            context.Dispose();

            return accountReturn;
        }

        private AccountModel ReturnContact(AccountModel account, string IdPerson)
        {
            string stQuery;

            stQuery = "select" +
                    "    IDTYPECONTACT" +
                    "   ,EMAIL" +
                    "   ,DDDTEL" +
                    "   ,TEL" +
                    " from tbcontact" +
                    " where IdPerson = " + account.IDPERSON.ToString() +
                    " order by idcontact;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    foreach (DataRow reader in retDT.Rows)
                    {
                        int TypeContact = Convert.ToInt32(reader["IDTYPECONTACT"]);

                        if(TypeContact == 1)
                        {
                            if (reader["EMAIL"] == null) { account.EMAIL = ""; } else { account.EMAIL = reader["EMAIL"].ToString(); }
                        }
                        else if (TypeContact == 2)
                        {
                            if (string.IsNullOrEmpty(account.TELONE))
                            {
                                if (reader["DDDTEL"] == null)
                                {
                                    if (reader["TEL"] == null) { account.TELONE = ""; } else { account.TELONE = reader["TEL"].ToString(); }
                                } 
                                else
                                {
                                    if (reader["TEL"] == null) { account.TELONE = reader["DDDTEL"].ToString(); } else { account.TELONE = reader["DDDTEL"].ToString()+ reader["TEL"].ToString(); }
                                }
                            } 
                            else if (string.IsNullOrEmpty(account.TELTWO))
                            {
                                if (reader["DDDTEL"] == null)
                                {
                                    if (reader["TEL"] == null) { account.TELTWO = ""; } else { account.TELTWO = reader["TEL"].ToString(); }
                                }
                                else
                                {
                                    if (reader["TEL"] == null) { account.TELTWO = reader["DDDTEL"].ToString(); } else { account.TELTWO = reader["DDDTEL"].ToString() + reader["TEL"].ToString(); }
                                }
                            }
                            else if (string.IsNullOrEmpty(account.TELTHREE))
                            {
                                if (reader["DDDTEL"] == null)
                                {
                                    if (reader["TEL"] == null) { account.TELTHREE = ""; } else { account.TELTHREE = reader["TEL"].ToString(); }
                                }
                                else
                                {
                                    if (reader["TEL"] == null) { account.TELTHREE = reader["DDDTEL"].ToString(); } else { account.TELTHREE = reader["DDDTEL"].ToString() + reader["TEL"].ToString(); }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnContact (619)", e.Message, stQuery);
            }

            return account;
        }

        public List<GenericParamModel> GetAccountList(string IdPerson)
        {
            List<GenericParamModel> ParamReturn = new List<GenericParamModel>();
            string stQuery;

            stQuery = "select IDPERSON, NAME from tbperson where tbperson.IDTYPEPERSON = 2 and tbperson.ACTIVE = true;";

            context = new Context();

            try { 
                var retDT = context.RunCommandDT(stQuery);
                if(retDT.Rows.Count > 0)
                {
                    foreach (DataRow reader in retDT.Rows)
                    {
                        GenericParamModel paramAux = new GenericParamModel();
                        if (reader["IDPERSON"] == null) { paramAux.VALUE = ""; } else { paramAux.VALUE = reader["IDPERSON"].ToString(); }
                        if (reader["NAME"] == null) { paramAux.LABEL = ""; } else { paramAux.LABEL = reader["NAME"].ToString(); }

                        ParamReturn.Add(paramAux);
                    }
                }
            }
            catch(Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetAccountList (651)", e.Message, stQuery);
            }
            context.Dispose();

            return ParamReturn;
        }

        public AccountModel GetCompleteAccount(string IdPerson, int Id, string Parameters)
        {
            AccountModel AccountReturn = new AccountModel();
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            List<int> ListParams = new List<int>(Array.ConvertAll(Parameters.Split(','), int.Parse));

            stQuery = "SELECT " +
                    "    tbperson.IDPERSON" +
                    "	,tbperson.NAME" +
                    "	,tbjuridicalperson.CNPJ" +
                    "	,tbjuridicalperson.CONTACT" +
                    "	,tbjuridicalperson.DURESSPASSWORD" +
                    "	,tbjuridicalperson.HISTORICPERSIST" +
                    "	,tbperson.ACTIVE" +
                    "	,tbaddress.CEP" +
                    "	,tbaddress.ADDRESS" +
                    "	,tbaddress.NUMBER" +
                    "	,tbaddress.REFERENCE" +
                    "	,tbaddress.NEIGHBORHOOD" +
                    "	,tbaddress.CITY" +
                    "	,tbaddress.STATE" +
                    "	,tbaddress.COUNTRY" +
                    "	,tbtypeperson.TYPEPERSON" +
                    " FROM tbperson" +
                    " JOIN tbjuridicalperson ON tbjuridicalperson.IDPERSON = tbperson.IDPERSON" +
                    " JOIN tbaddress ON tbaddress.IDPERSON = tbperson.IDPERSON" +
                    " JOIN tbtypeperson on tbtypeperson.IDTYPEPERSON = tbperson.IDTYPEPERSON" +
                    " WHERE tbperson.IDTYPEPERSON = 2 and tbperson.IDPERSON = " + Id.ToString() +";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    // All returning have base select
                    // 0 - ALL
                    // 1 - Contact
                    // 2 - Annotations
                    // 3 - Zones
                    // 4 - Events
                    // 5 - Schedules
                    // 6 - Unitys
                    // 7 - Vehicles
                    // 8 - Devices
                    // 9 - CamsDGuard

                    AccountReturn = queryUtils.DataTableToObject<AccountModel>(retDT);

                    if(ListParams.IndexOf(1) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnContact(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(2) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnAnnotations(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(3) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnZones(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(4) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnEvents(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(5) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnSchedules(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(6) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnUnitys(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(7) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnVehicles(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(8) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnDevices(AccountReturn, IdPerson);

                    if (ListParams.IndexOf(9) != -1 || ListParams.IndexOf(0) != -1)
                        AccountReturn = ReturnCamsDGuard(AccountReturn, IdPerson);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetAccounts (727)", e.Message, stQuery);
            }

            context.Dispose();

            return AccountReturn;
        }

        public GenericReturnModel AddAnnotation(AnnotationModel Annotation, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            Annotation = queryUtils.SQLHandler(Annotation);

            string stQuery;

            stQuery = "INSERT INTO tbannotation (" +
                    "	 EXECUTANT" +
                    "	,AFFECTED" +
                    "	,ANNOTATION" +
                    "	)" +
                    " VALUES (" +
                    "	 " + IdPerson +
                    "	," + Annotation.AFFECTED +
                    "	," + queryUtils.InsertSingleQuotes(Annotation.ANNOTATION) +
                    ");";

            context = new Context();

            try
            {
                context.RunCommand(stQuery);
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Anotação criada com sucesso.";
            }
            catch(Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddAnotation (765)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();

            return statusReturn;
        }
    
        private AccountModel ReturnAnnotations(AccountModel account, string IdPerson)
        {
            ls = new LogService();
            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT " +
                    "	 tbperson.NAME as EXECUTANT" +
                    "	,tbannotation.ANNOTATION" +
                    "	,tbannotation.REGISTRATIONDATE" +
                    " FROM tbannotation" +
                    " INNER JOIN tbperson on tbannotation.EXECUTANT = tbperson.IDPERSON" +
                    " WHERE tbannotation.AFFECTED = " + account.IDPERSON +
                    " ORDER BY tbannotation.REGISTRATIONDATE desc;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    account.ANNOTATIONS = queryUtils.DataTableToList<AnnotationModel>(retDT);
                }
            }
            catch(Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnAnnotations (804)", e.Message, stQuery);
            }

            context.Dispose();
            return account;
        }

        public GenericReturnModel AddEditZone(ZoneModel Zone, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            Zone = queryUtils.SQLHandler(Zone);

            if (Zone.IDZONE == 0)
                return AddZone(Zone, IdPerson);
            else
                return EditZone(Zone, IdPerson);
        }

        private GenericReturnModel AddZone(ZoneModel Zone, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "INSERT INTO tbzone (" +
                    "	 IDPERSON" +
                    "	,ZONE" +
                    "	,ACTIVE" +
                    "	,ISRESTRICTED" +
                    "	)" +
                    " VALUES (" +
                    "	 " + Zone.IDPERSON +
                    "	," + queryUtils.InsertSingleQuotes(Zone.ZONE) +
                    "	," + Zone.ACTIVE.ToString() +
                    "	," + Zone.ISRESTRICTED.ToString() +
                    ");";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + "  SELECT LAST_INSERT_ID() as IDZONE;");

                if (retDB.HasRows)
                    statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "IDZONE"));

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Zona criada com sucesso.";
                ls.AddLogSystem(int.Parse(IdPerson), Zone.IDPERSON, "Inseriu nova zona.", IdLogSystemList.REGISTERZONE, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddZone (858)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();

            return statusReturn;
        }

        private GenericReturnModel EditZone(ZoneModel Zone, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "UPDATE tbzone" +
                    " SET " +
                    "	 ZONE = " + queryUtils.InsertSingleQuotes(Zone.ZONE) +
                    "	,ACTIVE = " + Zone.ACTIVE.ToString() +
                    "	,ISRESTRICTED = " + Zone.ISRESTRICTED.ToString() + 
                    " WHERE IdZone = " + Zone.IDZONE + ";";

            context = new Context();

            try
            {
                string stringZone = "";
                try 
                {
                    var zoneBefore = GetCompleteAccount(IdPerson, Zone.IDPERSON, "3").ZONES;
                    int indexBefore = zoneBefore.FindIndex(x => x.IDZONE == Zone.IDZONE);
                    stringZone = Newtonsoft.Json.JsonConvert.SerializeObject(zoneBefore[indexBefore]);
                } 
                catch (Exception e) { }

                var retDB = context.RunCommandRetID(stQuery);
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Zona atualizada com sucesso.";

                ls.AddLogSystem(int.Parse(IdPerson), Zone.IDPERSON, "Atualizou zona.", IdLogSystemList.EDITZONE, stringZone);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - EditZone (894)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();

            return statusReturn;
        }
        
        private AccountModel ReturnZones(AccountModel account, string IdPerson)
        {
            ls = new LogService();
            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT " +
                    "    IDZONE" +
                    "	,IDPERSON" +
                    "	,ZONE" +
                    "	,ACTIVE" +
                    "	,ISRESTRICTED" +
                    " FROM tbzone" +
                    " WHERE IDPERSON = " + account.IDPERSON + 
                    " ORDER by idzone" + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    account.ZONES = queryUtils.DataTableToList<ZoneModel>(retDT);
                }
                else
                {
                    account.ZONES = new List<ZoneModel>();
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnZones (934)", e.Message, stQuery);
            }

            context.Dispose();


            return account;
        }

        public GenericReturnModel AddEditEvent(EventModel Event, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            Event = queryUtils.SQLHandler(Event);

            Event.INSTRUCTIONS = Event.INSTRUCTIONS.Replace("\n", "<br />");

            if (Event.IDEVENT == 0)
                return AddEvent(Event, IdPerson);
            else
                return EditEvent(Event, IdPerson);
        }

        public GenericReturnModel AddEvent(EventModel Event, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "INSERT INTO tbevent (" +
                    "	 NAME" +
                    "	,IDPRIORITY" +
                    "	,MANUALOCCURRENCE" +
                    "	,MANUALCREATION" +
                    "	,RECORDIMAGES" +
                    "	,IDZONE" +
                    "	,ACTIVE" +
                    "	,IDPERSON" +
                    "	,INSTRUCTIONS" +
                    "	)" +
                    " VALUES (" +
                    "	 " + queryUtils.InsertSingleQuotes(Event.NAME) +
                    "	," + Event.IDPRIORITY.ToString() +
                    "	," + Event.MANUALOCCURRENCE.ToString() +
                    "	," + "true" +
                    "	," + Event.RECORDIMAGES.ToString() +
                    "	," + Event.IDZONE.ToString() +
                    "	," + Event.ACTIVE.ToString() +
                    "	," + Event.IDPERSON.ToString() +
                    "	," + queryUtils.InsertSingleQuotes(Event.INSTRUCTIONS) +
                    "	);";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDEVENT;");

                if (retDB.HasRows)
                    statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "IDEVENT"));

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Evento inserido com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), Event.IDPERSON, "Inseriu novo evento.", IdLogSystemList.REGISTEREVENT, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddEvent (1002)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        public GenericReturnModel EditEvent(EventModel Event, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            
            string stQuery;

            stQuery = "UPDATE tbevent" +
                    " SET " +
                    "	 NAME = " + queryUtils.InsertSingleQuotes(Event.NAME) +
                    "	,IDPRIORITY = " + Event.IDPRIORITY.ToString() +
                    "	,MANUALOCCURRENCE = " + Event.MANUALOCCURRENCE.ToString() +
                    "	,RECORDIMAGES = " + Event.RECORDIMAGES.ToString() + 
                    "	,IDZONE = " + Event.IDZONE.ToString() +
                    "	,ACTIVE = " + Event.ACTIVE.ToString() +
                    "	,INSTRUCTIONS = " + queryUtils.InsertSingleQuotes(Event.INSTRUCTIONS) +
                    " WHERE IDEVENT = " + Event.IDEVENT.ToString() + ";";

            string stringEvent = "";
            try
            {
                var eventBefore = GetCompleteAccount(IdPerson, Event.IDPERSON, "4").EVENTS;
                int indexBefore = eventBefore.FindIndex(x => x.IDEVENT == Event.IDEVENT);
                stringEvent = Newtonsoft.Json.JsonConvert.SerializeObject(eventBefore[indexBefore]);
            }
            catch (Exception e) { }

            context = new Context();

            try
            {
                context.RunCommand(stQuery);
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Evento alterado com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), Event.IDPERSON, "Editou evento " + Event.IDEVENT.ToString() + ".", IdLogSystemList.EDITEVENT, stringEvent);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - EditEvent (1041)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private AccountModel ReturnEvents(AccountModel account, string IdPerson)
        {
            ls = new LogService();

            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT" +
                    "	 tbevent.IDEVENT" +
                    "	,tbevent.NAME" +
                    "	,tbevent.IDPRIORITY" +
                    "	,tbpriority.PRIORITY" +
                    "	,tbevent.IDPERSON" +
                    "	,tbevent.MANUALOCCURRENCE" +
                    "	,tbevent.MANUALCREATION" +
                    "	,tbevent.RECORDIMAGES" +
                    "	,tbevent.IDZONE" +
                    "	,tbevent.ACTIVE" +
                    "	,tbevent.INSTRUCTIONS" +
                    " FROM tbevent" +
                    " JOIN tbpriority on tbpriority.IDPRIORITY = tbevent.IDPRIORITY" +
                    " WHERE IDPERSON = " + account.IDPERSON;

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    account.EVENTS = queryUtils.DataTableToList<EventModel>(retDT);
                }
                else
                {
                    account.EVENTS = new List<EventModel>();
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnEvents (1236)", e.Message, stQuery);
            }

            context.Dispose();
            return account;
        }

        public GenericReturnModel AddEditSchedule(ScheduleModel schedule, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            schedule = queryUtils.SQLHandler(schedule);
            if (schedule.IDSCHEDULE == 0)
                return AddSchedule(schedule, IdPerson);
            else
                return EditSchedule(schedule, IdPerson);
        }

        private GenericReturnModel AddSchedule(ScheduleModel schedule, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            GenericReturnModel statusReturn = new GenericReturnModel();
            string stQuery;
            ls = new LogService();

            stQuery = "INSERT INTO tbschedule (" +
                    "	 IDPERSON" +
                    "	,NAME" +
                    "	,DESCRIPTION" +
                    "	,BOOLSUNDAY" +
                    "	,BOOLMONDAY" +
                    "	,BOOLTUESDAY" +
                    "	,BOOLWEDNESDAY" +
                    "	,BOOLTHURSDAY" +
                    "	,BOOLFRIDAY" +
                    "	,BOOLSATURDAY" +
                    "	,SUNDAYSTART" +
                    "	,SUNDAYEND" +
                    "	,MONDAYSTART" +
                    "	,MONDAYEND" +
                    "	,TUESDAYSTART" +
                    "	,TUESDAYEND" +
                    "	,WEDNESDAYSTART" +
                    "	,WEDNESDAYEND" +
                    "	,THURSDAYSTART" +
                    "	,THURSDAYEND" +
                    "	,FRIDAYSTART" +
                    "	,FRIDAYEND" +
                    "	,SATURDAYSTART" +
                    "	,SATURDAYEND" +
                    "	)" +
                    " VALUES (" +
                    "	 " + schedule.IDPERSON +
                    "	," + queryUtils.InsertSingleQuotes(schedule.NAME) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.DESCRIPTION) +
                    "	," + schedule.BOOLSUNDAY + 
                    "	," + schedule.BOOLMONDAY +
                    "	," + schedule.BOOLTUESDAY +
                    "	," + schedule.BOOLWEDNESDAY +
                    "	," + schedule.BOOLTHURSDAY +
                    "	," + schedule.BOOLFRIDAY +
                    "	," + schedule.BOOLSATURDAY +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMESUNDAY.START) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMESUNDAY.END) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMEMONDAY.START) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMEMONDAY.END) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMETUESDAY.START) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMETUESDAY.END) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMEWEDNESDAY.START) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMEWEDNESDAY.END) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMETHURSDAY.START) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMETHURSDAY.END) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMEFRIDAY.START) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMEFRIDAY.END) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMESATURDAY.START) +
                    "	," + queryUtils.InsertSingleQuotes(schedule.TIMESATURDAY.END) +
                    "	);";

            context = new Context();

            try
            {
                context.RunCommand(stQuery);

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Horário inserido com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), schedule.IDPERSON, "Inseriu novo horário.", IdLogSystemList.REGISTERSCHEDULE, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddSchedule (1176)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private GenericReturnModel EditSchedule(ScheduleModel schedule, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            GenericReturnModel statusReturn = new GenericReturnModel();
            string stQuery;
            ls = new LogService();

            stQuery = "UPDATE tbschedule" +
                        " SET " +
                        "	 NAME = " + queryUtils.InsertSingleQuotes(schedule.NAME) +
                        "	,DESCRIPTION = " + queryUtils.InsertSingleQuotes(schedule.DESCRIPTION) +
                        "	,BOOLSUNDAY = " + schedule.BOOLSUNDAY.ToString() +
                        "	,BOOLMONDAY = " + schedule.BOOLMONDAY.ToString() +
                        "	,BOOLTUESDAY = " + schedule.BOOLTUESDAY.ToString() +
                        "	,BOOLWEDNESDAY = " + schedule.BOOLWEDNESDAY.ToString() +
                        "	,BOOLTHURSDAY = " + schedule.BOOLTHURSDAY.ToString() +
                        "	,BOOLFRIDAY = " + schedule.BOOLFRIDAY.ToString() +
                        "	,BOOLSATURDAY = " + schedule.BOOLSATURDAY.ToString() +
                        "	,SUNDAYSTART = " + queryUtils.InsertSingleQuotes(schedule.TIMESUNDAY.START) +
                        "	,SUNDAYEND = " + queryUtils.InsertSingleQuotes(schedule.TIMESUNDAY.END) +
                        "	,MONDAYSTART = " + queryUtils.InsertSingleQuotes(schedule.TIMEMONDAY.START) +
                        "	,MONDAYEND = " + queryUtils.InsertSingleQuotes(schedule.TIMEMONDAY.END) +
                        "	,TUESDAYSTART = " + queryUtils.InsertSingleQuotes(schedule.TIMETUESDAY.START) +
                        "	,TUESDAYEND = " + queryUtils.InsertSingleQuotes(schedule.TIMETUESDAY.END) +
                        "	,WEDNESDAYSTART = " + queryUtils.InsertSingleQuotes(schedule.TIMEWEDNESDAY.START) +
                        "	,WEDNESDAYEND = " + queryUtils.InsertSingleQuotes(schedule.TIMEWEDNESDAY.END) +
                        "	,THURSDAYSTART = " + queryUtils.InsertSingleQuotes(schedule.TIMETHURSDAY.START) +
                        "	,THURSDAYEND = " + queryUtils.InsertSingleQuotes(schedule.TIMETHURSDAY.END) +
                        "	,FRIDAYSTART = " + queryUtils.InsertSingleQuotes(schedule.TIMEFRIDAY.START) +
                        "	,FRIDAYEND = " + queryUtils.InsertSingleQuotes(schedule.TIMEFRIDAY.END) +
                        "	,SATURDAYSTART = " + queryUtils.InsertSingleQuotes(schedule.TIMESATURDAY.START) +
                        "	,SATURDAYEND = " + queryUtils.InsertSingleQuotes(schedule.TIMESATURDAY.END) +
                        " WHERE IDSCHEDULE = " + schedule.IDSCHEDULE;

            string stringSchedule = "";
            try
            {
                var scheduleBefore = GetCompleteAccount(IdPerson, schedule.IDPERSON, "5").SCHEDULES;
                int indexBefore = scheduleBefore.FindIndex(x => x.IDSCHEDULE == schedule.IDSCHEDULE);
                stringSchedule = Newtonsoft.Json.JsonConvert.SerializeObject(scheduleBefore[indexBefore]);
            }
            catch (Exception e) { }

            context = new Context();

            try
            {
                context.RunCommand(stQuery);

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Horário atualizado com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), schedule.IDPERSON, "Atualizou horário do " + schedule.IDSCHEDULE + ".", IdLogSystemList.EDITSCHEDULE, stringSchedule);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - EditSchedule (1231)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private AccountModel ReturnSchedules(AccountModel account, string IdPerson)
        {
            ls = new LogService();
            List<ScheduleModel> schedules = new List<ScheduleModel>();
            string stQuery;

            stQuery = "SELECT" +
                    "	 IDSCHEDULE" +
                    "	,IDPERSON" +
                    "	,NAME" +
                    "	,DESCRIPTION" +
                    "	,BOOLSUNDAY" +
                    "	,BOOLMONDAY" +
                    "	,BOOLTUESDAY" +
                    "	,BOOLWEDNESDAY" +
                    "	,BOOLTHURSDAY" +
                    "	,BOOLFRIDAY" +
                    "	,BOOLSATURDAY" +
                    "	,SUNDAYSTART" +
                    "	,SUNDAYEND" +
                    "	,MONDAYSTART" +
                    "	,MONDAYEND" +
                    "	,TUESDAYSTART" +
                    "	,TUESDAYEND" +
                    "	,WEDNESDAYSTART" +
                    "	,WEDNESDAYEND" +
                    "	,THURSDAYSTART" +
                    "	,THURSDAYEND" +
                    "	,FRIDAYSTART" +
                    "	,FRIDAYEND" +
                    "	,SATURDAYSTART" +
                    "	,SATURDAYEND" +
                    " FROM tbschedule" +
                    " WHERE IDPERSON = " + account.IDPERSON;

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    foreach (DataRow reader in retDT.Rows)
                    {
                        ScheduleModel auxSchedule = new ScheduleModel();
                        if (reader["IDSCHEDULE"] == null) { auxSchedule.IDSCHEDULE = 0; } else { auxSchedule.IDSCHEDULE = Convert.ToInt32(reader["IDSCHEDULE"]); }
                        if (reader["IDPERSON"] == null) { auxSchedule.IDPERSON = 0; } else { auxSchedule.IDPERSON = Convert.ToInt32(reader["IDPERSON"]); }
                        if (reader["NAME"] == null) { auxSchedule.NAME = ""; } else { auxSchedule.NAME = reader["NAME"].ToString(); }
                        if (reader["DESCRIPTION"] == null) { auxSchedule.DESCRIPTION = ""; } else { auxSchedule.DESCRIPTION = reader["DESCRIPTION"].ToString(); }
                        if (reader["BOOLSUNDAY"] == null) { } else { auxSchedule.BOOLSUNDAY = Convert.ToBoolean(reader["BOOLSUNDAY"]); }
                        if (reader["BOOLMONDAY"] == null) { } else { auxSchedule.BOOLMONDAY = Convert.ToBoolean(reader["BOOLMONDAY"]); }
                        if (reader["BOOLTUESDAY"] == null) { } else { auxSchedule.BOOLTUESDAY = Convert.ToBoolean(reader["BOOLTUESDAY"]); }
                        if (reader["BOOLWEDNESDAY"] == null) { } else { auxSchedule.BOOLWEDNESDAY = Convert.ToBoolean(reader["BOOLWEDNESDAY"]); }
                        if (reader["BOOLTHURSDAY"] == null) { } else { auxSchedule.BOOLTHURSDAY = Convert.ToBoolean(reader["BOOLTHURSDAY"]); }
                        if (reader["BOOLFRIDAY"] == null) { } else { auxSchedule.BOOLFRIDAY = Convert.ToBoolean(reader["BOOLFRIDAY"]); }
                        if (reader["BOOLSATURDAY"] == null) { } else { auxSchedule.BOOLSATURDAY = Convert.ToBoolean(reader["BOOLSATURDAY"]); }

                        auxSchedule.TIMESUNDAY = new TimeRangeModel();
                        if (reader["SUNDAYSTART"] == null) { auxSchedule.TIMESUNDAY.START = "00:00"; } else { auxSchedule.TIMESUNDAY.START = reader["SUNDAYSTART"].ToString().Substring(0, 5); }
                        if (reader["SUNDAYEND"] == null) { auxSchedule.TIMESUNDAY.END = "23:59"; } else { auxSchedule.TIMESUNDAY.END = reader["SUNDAYEND"].ToString().Substring(0, 5); }

                        auxSchedule.TIMEMONDAY = new TimeRangeModel();
                        if (reader["MONDAYSTART"] == null) { auxSchedule.TIMEMONDAY.START = "00:00"; } else { auxSchedule.TIMEMONDAY.START = reader["MONDAYSTART"].ToString().Substring(0, 5); }
                        if (reader["MONDAYEND"] == null) { auxSchedule.TIMEMONDAY.END = "23:59"; } else { auxSchedule.TIMEMONDAY.END = reader["MONDAYEND"].ToString().Substring(0, 5); }

                        auxSchedule.TIMETUESDAY = new TimeRangeModel();
                        if (reader["TUESDAYSTART"] == null) { auxSchedule.TIMETUESDAY.START = "00:00"; } else { auxSchedule.TIMETUESDAY.START = reader["TUESDAYSTART"].ToString().Substring(0, 5); }
                        if (reader["TUESDAYEND"] == null) { auxSchedule.TIMETUESDAY.END = "23:59"; } else { auxSchedule.TIMETUESDAY.END = reader["TUESDAYEND"].ToString().Substring(0, 5); }

                        auxSchedule.TIMEWEDNESDAY = new TimeRangeModel();
                        if (reader["WEDNESDAYSTART"] == null) { auxSchedule.TIMEWEDNESDAY.START = "00:00"; } else { auxSchedule.TIMEWEDNESDAY.START = reader["WEDNESDAYSTART"].ToString().Substring(0, 5); }
                        if (reader["WEDNESDAYEND"] == null) { auxSchedule.TIMEWEDNESDAY.END = "23:59"; } else { auxSchedule.TIMEWEDNESDAY.END = reader["WEDNESDAYEND"].ToString().Substring(0, 5); }

                        auxSchedule.TIMETHURSDAY = new TimeRangeModel();
                        if (reader["THURSDAYSTART"] == null) { auxSchedule.TIMETHURSDAY.START = "00:00"; } else { auxSchedule.TIMETHURSDAY.START = reader["THURSDAYSTART"].ToString().Substring(0, 5); }
                        if (reader["THURSDAYEND"] == null) { auxSchedule.TIMETHURSDAY.END = "23:59"; } else { auxSchedule.TIMETHURSDAY.END = reader["THURSDAYEND"].ToString().Substring(0, 5); }

                        auxSchedule.TIMEFRIDAY = new TimeRangeModel();
                        if (reader["FRIDAYSTART"] == null) { auxSchedule.TIMEFRIDAY.START = "00:00"; } else { auxSchedule.TIMEFRIDAY.START = reader["FRIDAYSTART"].ToString().Substring(0, 5); }
                        if (reader["FRIDAYEND"] == null) { auxSchedule.TIMEFRIDAY.END = "23:59"; } else { auxSchedule.TIMEFRIDAY.END = reader["FRIDAYEND"].ToString().Substring(0, 5); }

                        auxSchedule.TIMESATURDAY = new TimeRangeModel();
                        if (reader["SATURDAYSTART"] == null) { auxSchedule.TIMESATURDAY.START = "00:00"; } else { auxSchedule.TIMESATURDAY.START = reader["SATURDAYSTART"].ToString().Substring(0, 5); }
                        if (reader["SATURDAYEND"] == null) { auxSchedule.TIMESATURDAY.END = "23:59"; } else { auxSchedule.TIMESATURDAY.END = reader["SATURDAYEND"].ToString().Substring(0, 5); }

                        schedules.Add(auxSchedule);
                    }
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnSchedules (1411)", e.Message, stQuery);
            }

            context.Dispose();
            account.SCHEDULES = schedules;
            return account;
        }

        public List<GenericParamModel> GetTypeUnity(string IdPerson)
        {
            List<GenericParamModel> ParamReturn = new List<GenericParamModel>();
            QueryUtils queryUtils = new QueryUtils();
            string stQuery;

            stQuery = "select IDTYPEUNITY as VALUE, TYPEUNITY as LABEL from tbtypeunity;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                {
                    ParamReturn = queryUtils.DataTableToList<GenericParamModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetTypeUnity (1355)", e.Message, stQuery);
            }
            context.Dispose();

            return ParamReturn;
        }

        public List<GenericParamModel> GetUnityState(string IdPerson)
        {
            List<GenericParamModel> ParamReturn = new List<GenericParamModel>();
            QueryUtils queryUtils = new QueryUtils();
            string stQuery;

            stQuery = "select IDUNITYSTATE as VALUE, UNITYSTATE as LABEL from tbunitystate;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                {
                    ParamReturn = queryUtils.DataTableToList<GenericParamModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetUnityState (1383)", e.Message, stQuery);
            }
            context.Dispose();

            return ParamReturn;
        }

        public GenericReturnModel AddEditUnity(UnityModel Unity, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            Unity = queryUtils.SQLHandler(Unity);
            if (Unity.IDUNITY == 0)
                return AddUnity(Unity, IdPerson);
            else
                return EditUnity(Unity, IdPerson);
        }

        private GenericReturnModel AddUnity(UnityModel Unity, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "INSERT INTO tbunity (" +
                    "	IDTYPEUNITY" +
                    "	,UNITYNAME" +
                    "	,IDUNITYSTATE" +
                    "	,IDACCOUNT" +
                    "	)" +
                    " VALUES (" +
                    "	 " + Unity.IDTYPEUNITY +
                    "	," + queryUtils.InsertSingleQuotes(Unity.UNITYNAME) +
                    "	," + Unity.IDUNITYSTATE +
                    "	," + Unity.IDACCOUNT +
                    "	);";


            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDUNITY;");

                if (retDB.HasRows)
                    statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "IDUNITY"));

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Unidade inserida com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), Unity.IDACCOUNT, "Inseriu nova unidade - " + statusReturn.ID + ".", IdLogSystemList.REGISTERUNITY, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddUnity (1427)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private GenericReturnModel EditUnity(UnityModel Unity, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "UPDATE tbunity" +
                        " SET" +
                        "    IDTYPEUNITY = " + Unity.IDTYPEUNITY +
                        "	,UNITYNAME = " + queryUtils.InsertSingleQuotes(Unity.UNITYNAME) +
                        "	,IDUNITYSTATE = " + Unity.IDUNITYSTATE +
                        " WHERE IDUNITY = " + Unity.IDUNITY + ";";

            string stringUnity = "";
            try
            {
                var unityBefore = GetCompleteAccount(IdPerson, Unity.IDACCOUNT, "6").UNITYS;
                int indexBefore = unityBefore.FindIndex(x => x.IDUNITY == Unity.IDUNITY);
                stringUnity = Newtonsoft.Json.JsonConvert.SerializeObject(unityBefore[indexBefore]);
            }
            catch (Exception e) { }

            context = new Context();

            try
            {
                context.RunCommand(stQuery);

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Unidade atualizada com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), Unity.IDACCOUNT, "Atualizou unidade - " + Unity.IDUNITY + ".", IdLogSystemList.EDITUNITY, stringUnity);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - EditUnity (1479)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private AccountModel ReturnUnitys(AccountModel account, string IdPerson)
        {
            ls = new LogService();
            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT" +
                    "	 tbunity.IDUNITY" +
                    "	,tbunity.IDTYPEUNITY" +
                    "   ,tbtypeunity.TYPEUNITY" +
                    "	,tbunity.UNITYNAME" +
                    "	,tbunity.IDUNITYSTATE" +
                    "	,tbunitystate.UNITYSTATE" +
                    "	,tbunity.IDACCOUNT" +
                    " FROM tbunity" +
                    " JOIN tbtypeunity on tbtypeunity.IDTYPEUNITY = tbunity.IDTYPEUNITY" +
                    " JOIN tbunitystate on tbunitystate.IDUNITYSTATE = tbunity.IDUNITYSTATE" +
                    " WHERE tbunity.IDACCOUNT = " + account.IDPERSON +
                    " ORDER BY tbunity.UNITYNAME";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    account.UNITYS = queryUtils.DataTableToList<UnityModel>(retDT);
                }
                else
                {
                    account.UNITYS = new List<UnityModel>();
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnUnitys (1466)", e.Message, stQuery);
            }

            context.Dispose();


            return account;
        }

        public GenericReturnModel AddEditVehicle(VehicleModel Vehicle, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            Vehicle.LICENSEPLATE = Vehicle.LICENSEPLATE.ToUpper();
            Vehicle = queryUtils.SQLHandler(Vehicle);
            if (Vehicle.IDVEHICLE == 0)
                return AddVehicle(Vehicle, IdPerson);
            else
                return EditVehicle(Vehicle, IdPerson);
        }

        private GenericReturnModel AddVehicle(VehicleModel Vehicle, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "INSERT INTO tbvehicle (" +
                    "	 LICENSEPLATE" +
                    "	,MODEL" +
                    "	,MANUFACTURER" +
                    "	,COLOR" +
                    "	,COMMENTS" +
                    "	,IDACCOUNT" +
                    "	,ACTIVE" +
                    "	)" +
                    " VALUES (" +
                    "	 " + queryUtils.InsertSingleQuotes(Vehicle.LICENSEPLATE) +
                    "	," + queryUtils.InsertSingleQuotes(Vehicle.MODEL) +
                    "	," + queryUtils.InsertSingleQuotes(Vehicle.MANUFACTURER) +
                    "	," + queryUtils.InsertSingleQuotes(Vehicle.COLOR) +
                    "	," + queryUtils.InsertSingleQuotes(Vehicle.COMMENTS) +
                    "	," + Vehicle.IDACCOUNT +
                    "	," + Vehicle.ACTIVE +
                    "	);";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDVEHICLE;");

                if (retDB.HasRows)
                    statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "IDVEHICLE"));

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Veículo inserido com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), Vehicle.IDACCOUNT, "Inseriu novo veículo - " + statusReturn.ID + ".", IdLogSystemList.REGISTERVEHICLE, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddVehicle (1577)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private GenericReturnModel EditVehicle(VehicleModel Vehicle, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "UPDATE tbvehicle" +
                        " SET" +
                        "	 LICENSEPLATE = " + queryUtils.InsertSingleQuotes(Vehicle.LICENSEPLATE) +
                        "	,MODEL = " + queryUtils.InsertSingleQuotes(Vehicle.MODEL) +
                        "	,MANUFACTURER = " + queryUtils.InsertSingleQuotes(Vehicle.MANUFACTURER) +
                        "	,COLOR = " + queryUtils.InsertSingleQuotes(Vehicle.COLOR) +
                        "	,COMMENTS = " + queryUtils.InsertSingleQuotes(Vehicle.COMMENTS) +
                        "	,ACTIVE = " + Vehicle.ACTIVE +
                        " WHERE IDVEHICLE = " + Vehicle.IDVEHICLE + ";";

            string stringVehicle = "";
            try
            {
                var vehicleBefore = GetCompleteAccount(IdPerson, Vehicle.IDACCOUNT, "7").VEHICLES;
                int indexBefore = vehicleBefore.FindIndex(x => x.IDVEHICLE == Vehicle.IDVEHICLE);
                stringVehicle = Newtonsoft.Json.JsonConvert.SerializeObject(vehicleBefore[indexBefore]);
            }
            catch (Exception e) { }

            context = new Context();

            try
            {
                context.RunCommand(stQuery);

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Veículo atualizado com sucesso!";
                ls.AddLogSystem(int.Parse(IdPerson), Vehicle.IDACCOUNT, "Atualizou veículo - " + Vehicle.IDVEHICLE + ".", IdLogSystemList.EDITVEHICLE, stringVehicle);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - EditVehicle (1616)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private AccountModel ReturnVehicles(AccountModel account, string IdPerson)
        {
            ls = new LogService();
            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT" +
                    "	 IDVEHICLE" +
                    "	,LICENSEPLATE" +
                    "	,MODEL" +
                    "	,MANUFACTURER" +
                    "	,COLOR" +
                    "	,COMMENTS" +
                    "	,IDACCOUNT" +
                    "	,ACTIVE" +
                    " FROM tbvehicle" +
                    " WHERE IDACCOUNT = " + account.IDPERSON + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    account.VEHICLES = queryUtils.DataTableToList<VehicleModel>(retDT);
                }
                else
                {
                    account.VEHICLES = new List<VehicleModel>();
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnVehicles (1657)", e.Message, stQuery);
            }

            context.Dispose();

            return account;
        }

        public List<GenericParamModel> GetDeviceManufacturer(string IdPerson)
        {
            List<GenericParamModel> ParamReturn = new List<GenericParamModel>();
            QueryUtils queryUtils = new QueryUtils();
            string stQuery;

            stQuery = "select IDDEVICEMANUFACTURER as VALUE, DEVICEMANUFACTURER as LABEL from tbdevicemanufacturer;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                {
                    ParamReturn = queryUtils.DataTableToList<GenericParamModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetDeviceManufacturer (1693)", e.Message, stQuery);
            }
            context.Dispose();

            return ParamReturn;
        }

        public List<GenericParamModel> GetDeviceByManufacturer(string IdManufacturer, string IdPerson)
        {
            List<GenericParamModel> ParamReturn = new List<GenericParamModel>();
            QueryUtils queryUtils = new QueryUtils();
            string stQuery;

            stQuery = "select IDDEVICE as VALUE, DEVICE as LABEL from tbdevice where IDDEVICEMANUFACTURER = " + IdManufacturer + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                {
                    ParamReturn = queryUtils.DataTableToList<GenericParamModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetDeviceByManufacturer (1721)", e.Message, stQuery);
            }
            context.Dispose();

            return ParamReturn;
        }

        public GenericReturnModel AddEditDevice(DeviceModel Device, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            Device = queryUtils.SQLHandler(Device);

            if (Device.IDDEVICEACCOUNT == 0)
                return AddDevice(Device, IdPerson);
            else
                return EditDevice(Device, IdPerson);
        }

        private GenericReturnModel AddDevice(DeviceModel Device, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "INSERT INTO tbdeviceaccount (" +
                    "	 DEVICENAME" +
                    "   ,IDACCOUNT" +
                    "	,IDDEVICEMANUFACTURER" +
                    "	,IDDEVICE" +
                    "	,HOST" +
                    "	,PORT" +
                    "	,PORTSECONDARY" +
                    "	,USERNAME" +
                    "	,PASSWORD" +
                    "	,HOSTMONITORING" +
                    "	,PORTMONITORING" +
                    "	,ACTIVE" +
                    "	)" +
                    " VALUES (" +
                    "	 " + queryUtils.InsertSingleQuotes(Device.DEVICENAME) +
                    "	," + Device.IDACCOUNT +
                    "	," + Device.IDDEVICEMANUFACTURER +
                    "	," + Device.IDDEVICE +
                    "	," + queryUtils.InsertSingleQuotes(Device.HOST) +
                    "	," + queryUtils.InsertSingleQuotes(Device.PORT) +
                    "	," + queryUtils.InsertSingleQuotes(Device.PORTSECONDARY) +
                    "	," + queryUtils.InsertSingleQuotes(Device.USERNAME) +
                    "	," + queryUtils.InsertSingleQuotes(Device.PASSWORD) +
                    "	," + queryUtils.InsertSingleQuotes(Device.HOSTMONITORING) +
                    "	," + queryUtils.InsertSingleQuotes(Device.PORTMONITORING) +
                    "	," + Device.ACTIVE +
                    "	);";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDDEVICEACCOUNT;");

                if (retDB.HasRows)
                    statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "IDDEVICEACCOUNT"));

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Dispositivo criado com sucesso.";
                ls.AddLogSystem(int.Parse(IdPerson), Device.IDACCOUNT, "Inseriu novo dispositivo - " + statusReturn.ID + ".", IdLogSystemList.REGISTERDEVICE, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddDevice (1789)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();

            return statusReturn;
        }

        private GenericReturnModel EditDevice(DeviceModel Device, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "UPDATE tbdeviceaccount" +
                        " SET " +
                        "	 DEVICENAME = " + Device.DEVICENAME +
                        "	,IDDEVICEMANUFACTURER = " + Device.IDDEVICEMANUFACTURER +
                        "	,IDDEVICE = " + Device.IDDEVICE +
                        "	,HOST = " + Device.HOST +
                        "	,PORT = " + Device.PORT +
                        "	,PORTSECONDARY = " + Device.PORTSECONDARY +
                        "	,USERNAME = " + Device.USERNAME +
                        "	,PASSWORD = " + Device.PASSWORD +
                        "	,HOSTMONITORING = " + Device.HOSTMONITORING +
                        "	,PORTMONITORING = " + Device.PORTMONITORING +
                        "	,ACTIVE = " + Device.ACTIVE +
                        " WHERE IDDEVICEACCOUNT = " + Device.IDDEVICEACCOUNT + ";";

            string stringDevice = "";
            try
            {
                var deviceBefore = GetCompleteAccount(IdPerson, Device.IDACCOUNT, "8").DEVICES;
                int indexBefore = deviceBefore.FindIndex(x => x.IDDEVICE == Device.IDDEVICE);
                stringDevice = Newtonsoft.Json.JsonConvert.SerializeObject(deviceBefore[indexBefore]);
            }
            catch (Exception e) { }

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery);
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Dispositivo atualizado com sucesso.";
                ls.AddLogSystem(int.Parse(IdPerson), Device.IDACCOUNT, "Atualizou dispositivo " + Device.IDDEVICEACCOUNT + ".", IdLogSystemList.EDITDEVICE, stringDevice);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - EditDevice (1835)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();

            return statusReturn;
        }

        private AccountModel ReturnDevices(AccountModel account, string IdPerson)
        {
            ls = new LogService();
            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT " +
                    "	 tbdeviceaccount.IDDEVICEACCOUNT" +
                    "	,tbdeviceaccount.IDACCOUNT" +
                    "	,tbdeviceaccount.DEVICENAME" +
                    "	,tbdeviceaccount.IDDEVICEMANUFACTURER" +
                    "	,tbdeviceaccount.IDDEVICE" +
                    "	,tbdevice.DEVICE" +
                    "	,tbdeviceaccount.HOST" +
                    "	,tbdeviceaccount.PORT" +
                    "	,tbdeviceaccount.PORTSECONDARY" +
                    "	,tbdeviceaccount.USERNAME" +
                    "	,tbdeviceaccount.PASSWORD" +
                    "	,tbdeviceaccount.HOSTMONITORING" +
                    "	,tbdeviceaccount.PORTMONITORING" +
                    "	,tbdeviceaccount.ACTIVE" +
                    "	,tbdevicetype.DEVICETYPE" +
                    " FROM tbdeviceaccount" +
                    " JOIN tbdevice ON tbdevice.IDDEVICE = tbdeviceaccount.IDDEVICE" +
                    " JOIN tbdevicetype ON tbdevicetype.IDDEVICETYPE = tbdevice.IDDEVICETYPE" +
                    " WHERE tbdeviceaccount.IDACCOUNT = " + account.IDPERSON + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    account.DEVICES = queryUtils.DataTableToList<DeviceModel>(retDT);
                } 
                else
                {
                    account.DEVICES = new List<DeviceModel>();
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnDevices (1885)", e.Message, stQuery);
            }

            context.Dispose();

            return account;
        }

        public GenericReturnModel AddEditCamDGuard(CamDGuardModel CamDGuard, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            CamDGuard = queryUtils.SQLHandler(CamDGuard);

            if (CamDGuard.IDCAMDGUARD == 0)
                return AddCamDGuard(CamDGuard, IdPerson);
            else
                return EditCamDGuard(CamDGuard, IdPerson);
        }

        private GenericReturnModel AddCamDGuard(CamDGuardModel CamDGuard, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "INSERT INTO tbcamdguard (" +
                    "	 IDACCOUNT" +
                    "	,IDDEVICEACCOUNT" +
                    "	,CAMNUMBER" +
                    "	,LAYOUT" +
                    "	,CAMNAME" +
                    "	,IDZONE" +
                    "	,ACTIVEUSER" +
                    "	,ACTIVERESIDENT" +
                    "	)" +
                    " VALUES (" +
                    "	  " + CamDGuard.IDACCOUNT +
                    "	 ," + CamDGuard.IDDEVICEACCOUNT +
                    "	 ," + queryUtils.InsertSingleQuotes(CamDGuard.CAMNUMBER) +
                    "	 ," + queryUtils.InsertSingleQuotes(CamDGuard.LAYOUT) +
                    "	 ," + queryUtils.InsertSingleQuotes(CamDGuard.CAMNAME) +
                    "	 ," + CamDGuard.IDZONE +
                    "	 ," + CamDGuard.ACTIVEUSER +
                    "	 ," + CamDGuard.ACTIVERESIDENT +
                    "	);";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDCAMDGUARD;");

                if (retDB.HasRows)
                    statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "IDCAMDGUARD"));

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Câmera adicionada com sucesso.";
                ls.AddLogSystem(int.Parse(IdPerson), CamDGuard.IDACCOUNT, "Adiciounou câmera - " + statusReturn.ID + ".", IdLogSystemList.REGISTERCAMDGUARD, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - AddCamDGuard (1953)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();

            return statusReturn;
        }

        private GenericReturnModel EditCamDGuard(CamDGuardModel CamDGuard, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "UPDATE tbcamdguard" +
                    " SET " +
                    "	 IDDEVICEACCOUNT = " + CamDGuard.IDDEVICEACCOUNT +
                    "	,CAMNUMBER = " + queryUtils.InsertSingleQuotes(CamDGuard.CAMNUMBER) +
                    "	,LAYOUT = " + queryUtils.InsertSingleQuotes(CamDGuard.LAYOUT) +
                    "	,CAMNAME = " + queryUtils.InsertSingleQuotes(CamDGuard.CAMNAME) +
                    "	,IDZONE = " + CamDGuard.IDZONE +
                    "	,ACTIVEUSER = " + CamDGuard.ACTIVEUSER +
                    "	,ACTIVERESIDENT = " + CamDGuard.ACTIVERESIDENT +
                    " WHERE IDCAMDGUARD = " + CamDGuard.IDCAMDGUARD + ";";

            string stringCam = "";

            try
            {
                var userCamBefore = GetCompleteAccount(IdPerson, CamDGuard.IDACCOUNT, "9").CAMSDGUARD;
                int indexBefore = userCamBefore.FindIndex(x => x.IDCAMDGUARD == CamDGuard.IDCAMDGUARD);
                stringCam = Newtonsoft.Json.JsonConvert.SerializeObject(userCamBefore[indexBefore]);
            } 
            catch (Exception e) { }

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery);
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Câmera atualizada com sucesso.";
                ls.AddLogSystem(int.Parse(IdPerson), CamDGuard.IDACCOUNT, "Atualizou câmera " + CamDGuard.IDCAMDGUARD + ".", IdLogSystemList.EDITCAMDGUARD, stringCam);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - EditCamDGuard (1993)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
            }

            context.Dispose();

            return statusReturn;
        }

        private AccountModel ReturnCamsDGuard(AccountModel account, string IdPerson)
        {
            ls = new LogService();
            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT " +
                    "	 tbcamdguard.IDCAMDGUARD" +
                    "	,tbcamdguard.IDACCOUNT" +
                    "	,tbcamdguard.IDDEVICEACCOUNT" +
                    "	,tbcamdguard.CAMNUMBER" +
                    "	,tbcamdguard.LAYOUT" +
                    "	,tbcamdguard.CAMNAME" +
                    "	,tbcamdguard.IDZONE" +
                    "	,tbcamdguard.ACTIVEUSER" +
                    "	,tbcamdguard.ACTIVERESIDENT" +
                    "   ,tbdeviceaccount.HOST" +
                    "   ,tbdeviceaccount.PORT" +
                    "   ,tbdeviceaccount.USERNAME" +
                    "   ,tbdeviceaccount.PASSWORD" +
                    " FROM tbcamdguard" +
                    " JOIN tbdeviceaccount on tbdeviceaccount.IDDEVICEACCOUNT = tbcamdguard.IDDEVICEACCOUNT" +
                    " WHERE " +
                    "           tbdeviceaccount.ACTIVE = true" +
                    "       AND tbcamdguard.IDACCOUNT = " + account.IDPERSON + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    account.CAMSDGUARD = queryUtils.DataTableToList<CamDGuardModel>(retDT);
                }
                else
                {
                    account.CAMSDGUARD = new List<CamDGuardModel>();
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - ReturnCamsDGuard (2036)", e.Message, stQuery);
            }

            context.Dispose();

            return account;
        }

        public List<GenericParamModel> GetAccountsFromId(string ids, string IdPerson)
        {
            List<GenericParamModel> ParamReturn = new List<GenericParamModel>();
            QueryUtils queryUtils = new QueryUtils();
            string stQuery;

            stQuery = "select IDPERSON as VALUE, NAME as LABEL from tbperson where IDPERSON in (" + ids + ");";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                {
                    ParamReturn = queryUtils.DataTableToList<GenericParamModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetAccountsFromId (1693)", e.Message, stQuery);
            }
            context.Dispose();

            return ParamReturn;
        }

        public List<CamDGuardModel> GetCamsForResident(string IdAccount, string IdPerson)
        {
            ls = new LogService();
            QueryUtils queryUtils = new QueryUtils();
            List<CamDGuardModel> camReturn = new List<CamDGuardModel>();

            string stQuery;

            stQuery = "SELECT " +
                    "	 tbcamdguard.IDCAMDGUARD" +
                    "	,tbcamdguard.IDACCOUNT" +
                    "	,tbcamdguard.IDDEVICEACCOUNT" +
                    "	,tbcamdguard.CAMNUMBER" +
                    "	,tbcamdguard.LAYOUT" +
                    "	,tbcamdguard.CAMNAME" +
                    "	,tbcamdguard.IDZONE" +
                    "	,tbcamdguard.ACTIVEUSER" +
                    "	,tbcamdguard.ACTIVERESIDENT" +
                    "   ,tbdeviceaccount.HOST" +
                    "   ,tbdeviceaccount.PORT" +
                    "   ,tbdeviceaccount.USERNAME" +
                    "   ,tbdeviceaccount.PASSWORD" +
                    " FROM tbcamdguard" +
                    " JOIN tbdeviceaccount on tbdeviceaccount.IDDEVICEACCOUNT = tbcamdguard.IDDEVICEACCOUNT" +
                    " WHERE " +
                    "           tbdeviceaccount.ACTIVE = true" +
                    "       AND tbcamdguard.IDACCOUNT  = " + IdAccount +
                    "       AND tbcamdguard.ACTIVEUSER = true;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    camReturn = queryUtils.DataTableToList<CamDGuardModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetCamsForResident (2125)", e.Message, stQuery);
            }

            context.Dispose();

            return camReturn;
        }
    }
}
