using Application.Account.Models;
using Application.General;
using Application.General.Entities;
using Application.General.Models;
using Application.Resident.Models;
using Functions;
using Repository;
using System;
using System.Collections.Generic;
using System.Data;

namespace Application.Resident
{
    public class ResidentService
    {
        private Context context;
        private LogService ls;

        public PreloadModel GetPreload(string IdAccount, string IdPerson)
        {
            PreloadModel plReturn = new PreloadModel();
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            context = new Context();

            stQuery = "select IDGENDER as VALUE, GENDER as LABEL from tbgender;";

            try
            {
                var retDTGender = context.RunCommandDT(stQuery);
                if (retDTGender.Rows.Count > 0)
                {
                    plReturn.GENDER = queryUtils.DataTableToList<GenericParamModel>(retDTGender);
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - GetPreload (37)", e.Message, stQuery);
            }

            stQuery = "select IDTYPERESIDENT as VALUE, TYPERESIDENT as LABEL from tbtyperesident;";

            try
            {
                var retDTTypeResident = context.RunCommandDT(stQuery);
                if (retDTTypeResident.Rows.Count > 0)
                {
                    plReturn.TYPERESIDENT = queryUtils.DataTableToList<GenericParamModel>(retDTTypeResident);
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - GetPreload (52)", e.Message, stQuery);
            }

            stQuery = "SELECT" +
                    "	 tbperson.IDPERSON AS VALUE" +
                    "	,tbperson.NAME AS LABEL" +
                    " FROM tbperson" +
                    " JOIN tbresident ON tbresident.IDPERSON = tbperson.IDPERSON" +
                    " WHERE" +
                    "		tbperson.IDROLE		  = 4" +
                    "	AND tbperson.ACTIVE		  = true" +
                    "	AND tbresident.ANSWERABLE = true" +
                    "	AND tbresident.IDACCOUNT  = " + IdAccount + ";";

            try
            {
                var retDTSponsor = context.RunCommandDT(stQuery);
                if (retDTSponsor.Rows.Count > 0)
                {
                    plReturn.SPONSOR = queryUtils.DataTableToList<GenericParamModel>(retDTSponsor);
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - GetPreload (76)", e.Message, stQuery);
            }

            stQuery = "SELECT IDUNITY" +
                    "	,IDTYPEUNITY" +
                    "	,UNITYNAME" +
                    "	,IDUNITYSTATE" +
                    "	,IDACCOUNT" +
                    " FROM tbunity" +
                    " WHERE IDACCOUNT = " + IdAccount +
                    " ORDER BY UNITYNAME;";

            try
            {
                var retDTUnity = context.RunCommandDT(stQuery);
                if (retDTUnity.Rows.Count > 0)
                {
                    plReturn.UNITY = queryUtils.DataTableToList<UnityModel>(retDTUnity);
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - GetPreload (99)", e.Message, stQuery);
            }

            stQuery = "SELECT IDVEHICLE" +
                    "	,LICENSEPLATE" +
                    "	,MODEL" +
                    "	,MANUFACTURER" +
                    "	,COLOR" +
                    "	,COMMENTS" +
                    "	,IDACCOUNT" +
                    "	,ACTIVE" +
                    " FROM tbvehicle" +
                    " WHERE" +
                    "       IDACCOUNT = " + IdAccount +
                    "   AND ACTIVE    = " + "true" +
                    " ORDER BY Model;";

            try
            {
                var retDTVehicle = context.RunCommandDT(stQuery);
                if (retDTVehicle.Rows.Count > 0)
                {
                    plReturn.VEHICLE = queryUtils.DataTableToList<VehicleModel>(retDTVehicle);
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - GetPreload (126)", e.Message, stQuery);
            }

            context.Dispose();

            return plReturn;
        }

        public GenericReturnModel AddEditResident(ResidentModel Resident, string IdPerson, string IdAdmin)
        {
            QueryUtils queryUtils = new QueryUtils();
            Resident = queryUtils.SQLHandler(Resident);

            if (Resident.IDPERSON == 0)
                return AddResident(Resident, IdPerson, IdAdmin);
            else
                return EditResident(Resident, IdPerson);
            
        }

        private GenericReturnModel AddResident(ResidentModel Resident, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            Utils utils = new Utils();
            string stQuery;

            string username = utils.RemoveAccents(Resident.NAME.Replace(" ", ".").ToLower());
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
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - Add Resident (182)", e.Message, stQuery);
                }
            }

            stQuery = " INSERT INTO tbperson (" +
                    "	 NAME" +
                    "	,USERNAME" +
                    "	,PASSWORD" +
                    "	,IDROLE" +
                    "	,IDTYPEPERSON" +
                    "   ,IDADMIN" +
                    "	,ACTIVE" +
                    "	)" +
                    " VALUES (" +
                    "	 " + queryUtils.InsertSingleQuotes(Resident.NAME) +
                    "	," + queryUtils.InsertSingleQuotes(username) +
                    "   ," + queryUtils.InsertSingleQuotes(Resident.CPF) +
                    "	," + "4" +
                    "	," + "1" +
                    "   ," + IdAdmin +
                    "	," + Resident.ACTIVE +
                    "	);";

            context = new Context();
            Resident.IDPERSON = 0;

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDPERSON;");

                if (retDB.HasRows)
                    Resident.IDPERSON = int.Parse(queryUtils.ReturnId(retDB, "IDPERSON"));

                statusReturn.ID = Resident.IDPERSON;
            }
            catch (Exception e)
            {
                Resident.IDPERSON = 0;

                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - Add Resident (221)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                statusReturn.STATUSCODE = 500;
            }

            context.Dispose();

            if (Resident.IDPERSON != 0)
            {
                List<string> querys = new List<string>();

                stQuery = "INSERT INTO tbnaturalperson (" +
                        "	 IDPERSON" +
                        "	,CPF" +
                        "	,RG" +
                        "	,BIRTHDATE" +
                        "	,IDGENDER" +
                        "	)" +
                        " VALUES (" +
                        "	 " + Resident.IDPERSON +
                        "	," + queryUtils.InsertSingleQuotes(Resident.CPF) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.RG) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.BIRTHDATE) +
                        "	," + Resident.IDGENDER +
                        "	);";

                querys.Add(stQuery);

                stQuery = "INSERT INTO tbcontact (" +
                        "	 IDTYPECONTACT" +
                        "	,EMAIL" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "1" +
                        "	," + queryUtils.InsertSingleQuotes(Resident.EMAIL) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                #region tel // Nem eu sei o que eu fiz aqui
                string auxStQueryTel1;
                string auxStQueryTel2;
                string auxStQueryTel3;
                string auxStQueryTelPre1 = "	,TEL";
                string auxStQueryTelPre2 = "	,TEL";
                string auxStQueryTelPre3 = "	,TEL";

                Resident.TELONE = utils.RemoveLeadingZero(utils.ExtractNumbers(Resident.TELONE));
                Resident.TELTWO = utils.RemoveLeadingZero(utils.ExtractNumbers(Resident.TELTWO));
                Resident.TELTHREE = utils.RemoveLeadingZero(utils.ExtractNumbers(Resident.TELTHREE));

                if (Resident.TELONE.Length > 9)
                {
                    auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(Resident.TELONE.Substring(0, 2)) +
                                     "	," + queryUtils.InsertSingleQuotes(Resident.TELONE.Substring(2));

                    auxStQueryTelPre1 = "	,DDDTEL" +
                                        "	,TEL";
                }
                else
                    auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(Resident.TELONE);

                stQuery = "INSERT INTO tbcontact (" +
                        "	IDTYPECONTACT" +
                        auxStQueryTelPre1 +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "2" +
                                 auxStQueryTel1 +
                        "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                if (Resident.TELTWO.Length > 9)
                {
                    auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTWO.Substring(0, 2)) +
                                     "	," + queryUtils.InsertSingleQuotes(Resident.TELTWO.Substring(2));

                    auxStQueryTelPre2 = "	,DDDTEL" +
                                        "	,TEL";
                }
                else
                    auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTWO);

                stQuery = "INSERT INTO tbcontact (" +
                        "	IDTYPECONTACT" +
                        auxStQueryTelPre2 +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "2" +
                                 auxStQueryTel2 +
                        "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                if (Resident.TELTHREE.Length > 9)
                {
                    auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTHREE.Substring(0, 2)) +
                                     "	," + queryUtils.InsertSingleQuotes(Resident.TELTHREE.Substring(2));

                    auxStQueryTelPre3 = "	,DDDTEL" +
                                        "	,TEL";
                }
                else
                    auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTHREE);

                if (Resident.TELTHREE.Length != 0)
                {
                    stQuery = "INSERT INTO tbcontact (" +
                            "	IDTYPECONTACT" +
                            auxStQueryTelPre3 +
                            "	,IDPERSON" +
                            "	)" +
                            " VALUES (" +
                            "	 " + "2" +
                                     auxStQueryTel3 +
                            "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                            "	);";

                    querys.Add(stQuery);
                }

                #endregion

                if(Resident.RESIDENTUNITY != null && Resident.RESIDENTUNITY.Count > 0)
                {
                    foreach(UnityModel unity in Resident.RESIDENTUNITY)
                    {
                        if (unity.IDUNITY != 0)
                        {
                            stQuery = "INSERT INTO tbpersonunity (" +
                                    "	 IDPERSON" +
                                    "	,IDACCOUNT" +
                                    "	,IDUNITY" +
                                    "	)" +
                                    " VALUES (" +
                                    "	 " + Resident.IDPERSON +
                                    "	," + Resident.IDACCOUNT +
                                    "	," + unity.IDUNITY +
                                    "	);";

                            querys.Add(stQuery);
                        }
                    }
                }

                if (Resident.RESIDENTVEHICLE != null && Resident.RESIDENTVEHICLE.Count > 0)
                {
                    foreach (VehicleModel vehicle in Resident.RESIDENTVEHICLE)
                    {
                        if (vehicle.IDVEHICLE != 0)
                        {
                            stQuery = "INSERT INTO tbpersonvehicle (" +
                                  "	 IDPERSON" +
                                  "	,IDACCOUNT" +
                                  "	,IDVEHICLE" +
                                  "	)" +
                                  " VALUES (" +
                                  "	 " + Resident.IDPERSON +
                                  "	," + Resident.IDACCOUNT +
                                  "	," + vehicle.IDVEHICLE +
                                  "	);";

                            querys.Add(stQuery);
                        }
                    }
                }

                if (Resident.ACCESSZONE != null && Resident.ACCESSZONE.Count > 0)
                {
                    foreach (PersonAccessZoneModel accessZone in Resident.ACCESSZONE)
                    {
                        if (accessZone.ACCESS)
                        { 
                            stQuery = "INSERT INTO tbpersonaccesszone (" +
                                    "	 IDPERSON" +
                                    "	,IDACCOUNT" +
                                    "	,IDZONE" +
                                    "	,IDSCHEDULE" +
                                    "	,ACCESS" +
                                    "	)" +
                                    " VALUES (" +
                                    "	 " + Resident.IDPERSON +
                                    "	," + Resident.IDACCOUNT +
                                    "	," + accessZone.IDZONE +
                                    "	," + accessZone.IDSCHEDULE +
                                    "	," + accessZone.ACCESS +
                                    "	);";

                            querys.Add(stQuery);
                        }
                    }
                }

                string sponsorTitle  = "";
                string sponsorInsert = "";
                if (!Resident.ANSWERABLE)
                {
                    sponsorTitle = "	,SPONSOR";
                    sponsorInsert = "	," + Resident.SPONSOR;
                }

                if(string.IsNullOrEmpty(Resident.ACCESSSTART))
                    Resident.ACCESSSTART = "null";
                else
                    Resident.ACCESSSTART = queryUtils.InsertSingleQuotes(Resident.ACCESSSTART);

                if (string.IsNullOrEmpty(Resident.ACCESSEND))
                    Resident.ACCESSEND = "null";
                else
                    Resident.ACCESSEND = queryUtils.InsertSingleQuotes(Resident.ACCESSEND);

                stQuery = "INSERT INTO tbresident (" +
                        "	 IDPERSON" +
                        "	,IDACCOUNT" +
                        "	,IDTYPERESIDENT" +
                        "	,ANSWERABLE" +
                        sponsorTitle +
                        "	,COMPANY" +
                        "	,DEPARTMENT" +
                        "	,NOTE" +
                        "	,ACCESSPERMISSION" +
                        "   ,ACCESSSTART" +
                        "   ,ACCESSEND" +
                        "	)" +
                        " VALUES (" +
                        "	 " + Resident.IDPERSON +
                        "	," + Resident.IDACCOUNT +
                        "	," + Resident.IDTYPERESIDENT +
                        "	," + Resident.ANSWERABLE +
                                 sponsorInsert +
                        "	," + queryUtils.InsertSingleQuotes(Resident.COMPANY) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.DEPARTMENT) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.NOTE) +
                        "	," + Resident.ACCESSPERMISSION +
                        "	," + Resident.ACCESSSTART +
                        "	," + Resident.ACCESSEND +
                        "	);";

                querys.Add(stQuery);

                try
                {
                    ContextTransaction ct = new ContextTransaction();

                    if (ct.RunTransaction(querys))
                    {
                        statusReturn.MESSAGE = "Pessoa inserida com sucesso!";
                        statusReturn.STATUSCODE = 201;
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
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - Add Resident (492)", e.Message, stQuery);
                }

                if (stQuery != "")
                {
                    context = new Context();
                    context.RunCommand(stQuery);
                    context.Dispose();
                }
                else
                {
                    ls.AddLogSystem(int.Parse(IdPerson), Resident.IDPERSON, "Adicionou nova pessoa no condominio " + Resident.IDACCOUNT + ".", IdLogSystemList.REGISTERPERSON, "");
                }
            }

            return statusReturn;
        }

        private GenericReturnModel EditResident(ResidentModel Resident, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;
            Utils utils = new Utils();

            List<string> querys = new List<string>();

            stQuery = "UPDATE tbperson" +
                        " SET " +
                        "	 NAME = " + queryUtils.InsertSingleQuotes(Resident.NAME) +
                        "	,ACTIVE = " + Resident.ACTIVE +
                        " WHERE IDPERSON = " + Resident.IDPERSON + ";";

            querys.Add(stQuery);

            stQuery = "DELETE" +
                        " FROM tbnaturalperson" +
                        " WHERE IDPERSON = " + Resident.IDPERSON + ";";

            querys.Add(stQuery);

            stQuery = "INSERT INTO tbnaturalperson (" +
                        "	 IDPERSON" +
                        "	,CPF" +
                        "	,RG" +
                        "	,BIRTHDATE" +
                        "	,IDGENDER" +
                        "	)" +
                        " VALUES (" +
                        "	 " + Resident.IDPERSON +
                        "	," + queryUtils.InsertSingleQuotes(Resident.CPF) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.RG) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.BIRTHDATE) +
                        "	," + Resident.IDGENDER +
                        "	);";

            querys.Add(stQuery);

            stQuery = "DELETE" +
            " FROM tbcontact" +
            " WHERE IDPERSON = " + Resident.IDPERSON + ";";


            querys.Add(stQuery);

            stQuery = "INSERT INTO tbcontact (" +
                        "	 IDTYPECONTACT" +
                        "	,EMAIL" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "1" +
                        "	," + queryUtils.InsertSingleQuotes(Resident.EMAIL) +
                        "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                        "	);";

            querys.Add(stQuery);

            #region tel
            string auxStQueryTel1;
            string auxStQueryTel2;
            string auxStQueryTel3;
            string auxStQueryTelPre1 = "	,TEL";
            string auxStQueryTelPre2 = "	,TEL";
            string auxStQueryTelPre3 = "	,TEL";

            Resident.TELONE = utils.RemoveLeadingZero(utils.ExtractNumbers(Resident.TELONE));
            Resident.TELTWO = utils.RemoveLeadingZero(utils.ExtractNumbers(Resident.TELTWO));
            Resident.TELTHREE = utils.RemoveLeadingZero(utils.ExtractNumbers(Resident.TELTHREE));

            if (Resident.TELONE.Length > 9)
            {
                auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(Resident.TELONE.Substring(0, 2)) +
                                 "	," + queryUtils.InsertSingleQuotes(Resident.TELONE.Substring(2));

                auxStQueryTelPre1 = "	,DDDTEL" +
                                    "	,TEL";
            }
            else
                auxStQueryTel1 = "	," + queryUtils.InsertSingleQuotes(Resident.TELONE);

            stQuery = "INSERT INTO tbcontact (" +
                    "	IDTYPECONTACT" +
                    auxStQueryTelPre1 +
                    "	,IDPERSON" +
                    "	)" +
                    " VALUES (" +
                    "	 " + "2" +
                             auxStQueryTel1 +
                    "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                    "	);";

            querys.Add(stQuery);

            if (Resident.TELTWO.Length > 9)
            {
                auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTWO.Substring(0, 2)) +
                                 "	," + queryUtils.InsertSingleQuotes(Resident.TELTWO.Substring(2));

                auxStQueryTelPre2 = "	,DDDTEL" +
                                    "	,TEL";
            }
            else
                auxStQueryTel2 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTWO);

            stQuery = "INSERT INTO tbcontact (" +
                    "	IDTYPECONTACT" +
                    auxStQueryTelPre2 +
                    "	,IDPERSON" +
                    "	)" +
                    " VALUES (" +
                    "	 " + "2" +
                             auxStQueryTel2 +
                    "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                    "	);";

            querys.Add(stQuery);

            if (Resident.TELTHREE.Length > 9)
            {
                auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTHREE.Substring(0, 2)) +
                                 "	," + queryUtils.InsertSingleQuotes(Resident.TELTHREE.Substring(2));

                auxStQueryTelPre3 = "	,DDDTEL" +
                                    "	,TEL";
            }
            else
                auxStQueryTel3 = "	," + queryUtils.InsertSingleQuotes(Resident.TELTHREE);

            if (Resident.TELTHREE.Length != 0)
            {
                stQuery = "INSERT INTO tbcontact (" +
                        "	IDTYPECONTACT" +
                        auxStQueryTelPre3 +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "2" +
                                 auxStQueryTel3 +
                        "	," + queryUtils.InsertSingleQuotes(Resident.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);
            }

            #endregion

            stQuery = "DELETE" +
                        " FROM tbpersonunity" +
                        " WHERE " +
                        "      IDPERSON = " + Resident.IDPERSON +
                        " AND IDACCOUNT = " + Resident.IDACCOUNT + ";";

            querys.Add(stQuery);

            if (Resident.RESIDENTUNITY != null && Resident.RESIDENTUNITY.Count > 0)
            {
                foreach (UnityModel unity in Resident.RESIDENTUNITY)
                {
                    if (unity.IDUNITY != 0)
                    {
                        stQuery = "INSERT INTO tbpersonunity (" +
                                "	 IDPERSON" +
                                "	,IDACCOUNT" +
                                "	,IDUNITY" +
                                "	)" +
                                " VALUES (" +
                                "	 " + Resident.IDPERSON +
                                "	," + Resident.IDACCOUNT +
                                "	," + unity.IDUNITY +
                                "	);";

                        querys.Add(stQuery);
                    }
                }
            }

            stQuery = "DELETE" +
                        " FROM tbpersonvehicle" +
                        " WHERE " +
                        "      IDPERSON = " + Resident.IDPERSON +
                        " AND IDACCOUNT = " + Resident.IDACCOUNT + ";";

            querys.Add(stQuery);

            if (Resident.RESIDENTVEHICLE != null && Resident.RESIDENTVEHICLE.Count > 0)
            {
                foreach (VehicleModel vehicle in Resident.RESIDENTVEHICLE)
                {
                    if (vehicle.IDVEHICLE != 0)
                    {
                        stQuery = "INSERT INTO tbpersonvehicle (" +
                              "	 IDPERSON" +
                              "	,IDACCOUNT" +
                              "	,IDVEHICLE" +
                              "	)" +
                              " VALUES (" +
                              "	 " + Resident.IDPERSON +
                              "	," + Resident.IDACCOUNT +
                              "	," + vehicle.IDVEHICLE +
                              "	);";

                        querys.Add(stQuery);
                    }
                }
            }

            stQuery = "DELETE" +
                        " FROM tbpersonaccesszone" +
                        " WHERE " +
                        "      IDPERSON = " + Resident.IDPERSON +
                        " AND IDACCOUNT = " + Resident.IDACCOUNT + ";";

            querys.Add(stQuery);

            if (Resident.ACCESSZONE != null && Resident.ACCESSZONE.Count > 0)
            {
                foreach (PersonAccessZoneModel accessZone in Resident.ACCESSZONE)
                {
                    if (accessZone.ACCESS)
                    {
                        stQuery = "INSERT INTO tbpersonaccesszone (" +
                                "	 IDPERSON" +
                                "	,IDACCOUNT" +
                                "	,IDZONE" +
                                "	,IDSCHEDULE" +
                                "	,ACCESS" +
                                "	)" +
                                " VALUES (" +
                                "	 " + Resident.IDPERSON +
                                "	," + Resident.IDACCOUNT +
                                "	," + accessZone.IDZONE +
                                "	," + accessZone.IDSCHEDULE +
                                "	," + accessZone.ACCESS +
                                "	);";

                        querys.Add(stQuery);
                    }
                }
            }

            string sponsorTitle = "";
            string sponsorInsert = "";
            if (!Resident.ANSWERABLE)
            {
                sponsorTitle = "	,SPONSOR";
                sponsorInsert = "	," + Resident.SPONSOR;
            }

            if (string.IsNullOrEmpty(Resident.ACCESSSTART))
                Resident.ACCESSSTART = "null";
            else
                Resident.ACCESSSTART = queryUtils.InsertSingleQuotes(Resident.ACCESSSTART);

            if (string.IsNullOrEmpty(Resident.ACCESSEND))
                Resident.ACCESSEND = "null";
            else
                Resident.ACCESSEND = queryUtils.InsertSingleQuotes(Resident.ACCESSEND);

            stQuery = "DELETE" +
                        " FROM tbresident" +
                        " WHERE " +
                        "      IDPERSON = " + Resident.IDPERSON +
                        " AND IDACCOUNT = " + Resident.IDACCOUNT + ";";

            querys.Add(stQuery);

            stQuery = "INSERT INTO tbresident (" +
                    "	 IDPERSON" +
                    "	,IDACCOUNT" +
                    "	,IDTYPERESIDENT" +
                    "	,ANSWERABLE" +
                    sponsorTitle +
                    "	,COMPANY" +
                    "	,DEPARTMENT" +
                    "	,NOTE" +
                    "	,ACCESSPERMISSION" +
                    "   ,ACCESSSTART" +
                    "   ,ACCESSEND" +
                    "	)" +
                    " VALUES (" +
                    "	 " + Resident.IDPERSON +
                    "	," + Resident.IDACCOUNT +
                    "	," + Resident.IDTYPERESIDENT +
                    "	," + Resident.ANSWERABLE +
                             sponsorInsert +
                    "	," + queryUtils.InsertSingleQuotes(Resident.COMPANY) +
                    "	," + queryUtils.InsertSingleQuotes(Resident.DEPARTMENT) +
                    "	," + queryUtils.InsertSingleQuotes(Resident.NOTE) +
                    "	," + Resident.ACCESSPERMISSION +
                    "	," + Resident.ACCESSSTART +
                    "	," + Resident.ACCESSEND +
                    "	);";

            querys.Add(stQuery);

            try
            {
                string stringResident = "";
                try { stringResident = Newtonsoft.Json.JsonConvert.SerializeObject(GetCompleteResident(Resident, IdPerson)); } catch (Exception e) { }

                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "Conta atualizada com sucesso!";
                    statusReturn.STATUSCODE = 201;
                    ls.AddLogSystem(int.Parse(IdPerson), Resident.IDPERSON, "Editou o residente.", IdLogSystemList.EDITPERSON, stringResident);
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
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - Edit Resident (828)", e.Message, stQuery);
            }

            return statusReturn;
        }

        public List<ResidentListModel> GetListResidents(string IdAccount, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            List<ResidentListModel> residents = new List<ResidentListModel>();

            stQuery = "SELECT" +
                    "	 tbperson.IDPERSON" +
                    "	,tbperson.NAME" +
                    "	,tbnaturalperson.CPF" +
                    "	,tbtyperesident.TYPERESIDENT" +
                    " FROM tbperson" +
                    " JOIN tbnaturalperson ON tbnaturalperson.IDPERSON = tbperson.IDPERSON" +
                    " JOIN tbresident ON tbresident.IDPERSON = tbperson.IDPERSON" +
                    " LEFT JOIN tbtyperesident ON tbtyperesident.IDTYPERESIDENT = tbresident.IDTYPERESIDENT" +
                    " WHERE " +
                    "		tbperson.IDROLE 	 = 4" +
                    "	AND tbresident.IDACCOUNT =  " + IdAccount + "" +
                    " ORDER BY tbperson.NAME;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    foreach (DataRow reader in retDT.Rows)
                    {
                        ResidentListModel auxResident = new ResidentListModel();
                        if (reader["IDPERSON"] == null) { auxResident.IDPERSON = 0; } else { auxResident.IDPERSON = Convert.ToInt32(reader["IDPERSON"]); }
                        if (reader["NAME"] == null) { auxResident.NAME = ""; } else { auxResident.NAME = reader["NAME"].ToString(); }
                        if (reader["CPF"] == null) { auxResident.CPF = ""; } else { auxResident.CPF = reader["CPF"].ToString(); }
                        if (reader["TYPERESIDENT"] == null) { auxResident.TYPERESIDENT = ""; } else { auxResident.TYPERESIDENT = reader["TYPERESIDENT"].ToString(); }

                        auxResident.UNITYNAME = "";
                        auxResident.TEL = "";

                        if(auxResident.IDPERSON != 0)
                        { 
                            stQuery = "SELECT" +
                                    "	 tbunity.UNITYNAME" +
                                    " FROM tbpersonunity" +
                                    " JOIN tbunity ON tbunity.IDUNITY = tbpersonunity.IDUNITY" +
                                    " WHERE " +
                                    "		tbpersonunity.IDPERSON 	= " + auxResident.IDPERSON +
                                    "	AND tbpersonunity.IDACCOUNT = " + IdAccount + ";";

                            var retDTUnity = context.RunCommandDT(stQuery);

                            if (retDTUnity.Rows.Count > 0)
                            {
                                foreach(DataRow readerUnity in retDTUnity.Rows)
                                {
                                    if (readerUnity["UNITYNAME"] == null) {} 
                                    else { 
                                        if(string.IsNullOrEmpty(auxResident.UNITYNAME))
                                            auxResident.UNITYNAME = readerUnity["UNITYNAME"].ToString(); 
                                        else
                                            auxResident.UNITYNAME += "\n" + readerUnity["UNITYNAME"].ToString();
                                    }
                                }
                            }

                            stQuery = "SELECT " +
                                    "	CONCAT (" +
                                    "		DDDTEL" +
                                    "		,' '" +
                                    "		,TEL" +
                                    "	) AS DDDTEL" +
                                    "	,TEL" +
                                    " FROM tbcontact" +
                                    " WHERE " +
                                    "		IDTYPECONTACT = 2" +
                                    "	AND IDPERSON      =  " + auxResident.IDPERSON +
                                    " LIMIT 1;";

                            var retDTTEL = context.RunCommandDT(stQuery);

                            if (retDTTEL.Rows.Count > 0)
                            {
                                foreach (DataRow readerTEL in retDTTEL.Rows)
                                {
                                    if (readerTEL["DDDTEL"] == null || readerTEL["DDDTEL"].ToString() == "")
                                    {
                                        if (readerTEL["TEL"] == null) { auxResident.TEL = ""; } else { auxResident.TEL = readerTEL["TEL"].ToString(); }
                                    }
                                    else
                                    {
                                        auxResident.TEL = readerTEL["DDDTEL"].ToString();
                                    }
                                }
                            }
                        } // end of the if
                        residents.Add(auxResident);
                    } // end of the foreach
                }

            }
            catch(Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - GetListResidents (614)", e.Message, stQuery);
            }

            context.Dispose();

            return residents;
        }

        public ResidentModel GetCompleteResident(ResidentModel Resident, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            stQuery = "SELECT" +
                    "	 tbresident.IDPERSON" +
                    "	,person.NAME" +
                    "	,person.ACTIVE" +
                    "	,person.USERNAME" +
                    "	,tbnaturalperson.CPF" +
                    "	,tbnaturalperson.RG" +
                    "	,tbnaturalperson.BIRTHDATE" +
                    "	,tbnaturalperson.IDGENDER" +
                    "	,tbresident.IDTYPERESIDENT" +
                    "	,tbresident.ANSWERABLE" +
                    "	,tbresident.SPONSOR" +
                    "	,tbresident.IDACCOUNT" +
                    "	,tbresident.COMPANY" +
                    "	,tbresident.DEPARTMENT" +
                    "	,tbresident.NOTE" +
                    "	,tbresident.ACCESSPERMISSION" +
                    "	,tbresident.ACCESSSTART" +
                    "	,tbresident.ACCESSEND" +
                    " FROM tbresident" +
                    " JOIN tbperson AS person ON person.IDPERSON = tbresident.IDPERSON" +
                    " JOIN tbnaturalperson ON tbnaturalperson.IDPERSON = tbresident.IDPERSON" +
                    " WHERE" +
                    "		tbresident.IDPERSON  = " + Resident.IDPERSON  +
                    "	AND tbresident.IDACCOUNT = " + Resident.IDACCOUNT + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    Resident = queryUtils.DataTableToObject<ResidentModel>(retDT);
                    context.Dispose();

                    Resident = ReturnContact(Resident, IdPerson);
                    Resident = ReturnUnity(Resident, IdPerson);
                    Resident = ReturnVehicles(Resident, IdPerson);
                    Resident = ReturnAccessZone(Resident, IdPerson);
                }
            }
            catch(Exception e)
            {
                context.Dispose();
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - ReturnResident (670)", e.Message, stQuery);
            }
            
            return Resident;
        }

        public ResidentModel GetCompleteResidentFromCPF(ResidentModel Resident, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            bool returnAccount = false;

            stQuery = "SELECT tbperson.IDPERSON, tbresident.IDACCOUNT" +
                        " FROM tbnaturalperson" +
                        " JOIN tbperson ON tbperson.IDPERSON = tbnaturalperson.IDPERSON" +
                        " JOIN tbresident ON tbresident.IDPERSON = tbnaturalperson.IDPERSON" +
                        " WHERE " +
                        "       tbperson.IDROLE = 4 " +
                        "   AND tbnaturalperson.CPF = " + queryUtils.InsertSingleQuotes(Resident.CPF) + ";";

            context = new Context();
            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    Resident.IDPERSON = Convert.ToInt32(retDT.Rows[0][0]);

                    foreach (DataRow reader in retDT.Rows)
                    {
                        int idaccount = 0;
                        if (reader["IDACCOUNT"] == null) { idaccount = 0; } else { idaccount = Convert.ToInt32(reader["IDACCOUNT"]); }

                        if (Resident.IDACCOUNT == idaccount)
                            returnAccount = true;
                    }
                }
                else
                {
                    context.Dispose();
                    return null;
                }
            }
            catch (Exception e)
            {
                context.Dispose();
                return null;
            }

            if (!returnAccount)
                stQuery = "SELECT" +
                        "	 tbperson.IDPERSON" +
                        "	,tbperson.NAME" +
                        "	,tbperson.ACTIVE" +
                        "	,tbperson.USERNAME" +
                        "	,tbnaturalperson.CPF" +
                        "	,tbnaturalperson.RG" +
                        "	,tbnaturalperson.BIRTHDATE" +
                        "	,tbnaturalperson.IDGENDER" +
                        " FROM tbperson" +
                        " JOIN tbnaturalperson ON tbnaturalperson.IDPERSON = tbperson.IDPERSON" +
                        " WHERE" +
                        "		tbperson.IDPERSON  = " + Resident.IDPERSON + ";";
            else
                return GetCompleteResident(Resident, IdPerson);

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    Resident = queryUtils.DataTableToObject<ResidentModel>(retDT);
                    context.Dispose();

                    Resident = ReturnContact(Resident, IdPerson);
                }
            }
            catch (Exception e)
            {
                context.Dispose();
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - ReturnResident (670)", e.Message, stQuery);
            }

            return Resident;
        }

        private ResidentModel ReturnContact(ResidentModel Resident, string IdPerson)
        {
            string stQuery;

            stQuery = "select" +
                    "    IDTYPECONTACT" +
                    "   ,EMAIL" +
                    "   ,DDDTEL" +
                    "   ,TEL" +
                    " from tbcontact" +
                    " where IdPerson = " + Resident.IDPERSON.ToString() +
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

                        if (TypeContact == 1)
                        {
                            if (reader["EMAIL"] == null) { Resident.EMAIL = ""; } else { Resident.EMAIL = reader["EMAIL"].ToString(); }
                        }
                        else if (TypeContact == 2)
                        {
                            if (string.IsNullOrEmpty(Resident.TELONE))
                            {
                                if (reader["DDDTEL"] == null)
                                {
                                    if (reader["TEL"] == null) { Resident.TELONE = ""; } else { Resident.TELONE = reader["TEL"].ToString(); }
                                }
                                else
                                {
                                    if (reader["TEL"] == null) { Resident.TELONE = reader["DDDTEL"].ToString(); } else { Resident.TELONE = reader["DDDTEL"].ToString() + reader["TEL"].ToString(); }
                                }
                            }
                            else if (string.IsNullOrEmpty(Resident.TELTWO))
                            {
                                if (reader["DDDTEL"] == null)
                                {
                                    if (reader["TEL"] == null) { Resident.TELTWO = ""; } else { Resident.TELTWO = reader["TEL"].ToString(); }
                                }
                                else
                                {
                                    if (reader["TEL"] == null) { Resident.TELTWO = reader["DDDTEL"].ToString(); } else { Resident.TELTWO = reader["DDDTEL"].ToString() + reader["TEL"].ToString(); }
                                }
                            }
                            else if (string.IsNullOrEmpty(Resident.TELTHREE))
                            {
                                if (reader["DDDTEL"] == null)
                                {
                                    if (reader["TEL"] == null) { Resident.TELTHREE = ""; } else { Resident.TELTHREE = reader["TEL"].ToString(); }
                                }
                                else
                                {
                                    if (reader["TEL"] == null) { Resident.TELTHREE = reader["DDDTEL"].ToString(); } else { Resident.TELTHREE = reader["DDDTEL"].ToString() + reader["TEL"].ToString(); }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - ReturnContact (747)", e.Message, stQuery);
            }

            if (string.IsNullOrEmpty(Resident.TELTWO))
                Resident.TELTWO = "";

            if (string.IsNullOrEmpty(Resident.TELTHREE))
                Resident.TELTHREE = "";

            context.Dispose();

            return Resident;
        }

        private ResidentModel ReturnUnity(ResidentModel Resident, string IdPerson)
        {
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "SELECT" +
                    "	 IDUNITY" +
                    " FROM tbpersonunity" +
                    " WHERE " +
                    "		IDPERSON  = " + Resident.IDPERSON +
                    "	AND IDACCOUNT = " + Resident.IDACCOUNT + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    Resident.RESIDENTUNITY = queryUtils.DataTableToList<UnityModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - ReturnUnitys (779)", e.Message, stQuery);
            }

            context.Dispose();

            return Resident;
        }

        private ResidentModel ReturnVehicles(ResidentModel Resident, string IdPerson)
        {
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "SELECT" +
                    "	 IDVEHICLE" +
                    " FROM tbpersonvehicle" +
                    " WHERE " +
                    "		IDPERSON  = " + Resident.IDPERSON +
                    "	AND IDACCOUNT = " + Resident.IDACCOUNT + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    Resident.RESIDENTVEHICLE = queryUtils.DataTableToList<VehicleModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - ReturnVehicles (811)", e.Message, stQuery);
            }

            context.Dispose();

            return Resident;
        }

        private ResidentModel ReturnAccessZone(ResidentModel Resident, string IdPerson)
        {
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "SELECT" +
                    "	 IDPERSON" +
                    "	,IDACCOUNT" +
                    "	,IDZONE" +
                    "	,IDSCHEDULE" +
                    "	,ACCESS" +
                    " FROM tbpersonaccesszone" +
                    " WHERE " +
                    "		IDPERSON  = " + Resident.IDPERSON +
                    "	AND IDACCOUNT = " + Resident.IDACCOUNT + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    Resident.ACCESSZONE = queryUtils.DataTableToList<PersonAccessZoneModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Resident Service - ReturnAccessZone (847)", e.Message, stQuery);
            }

            context.Dispose();

            return Resident;
        }

    }
}
