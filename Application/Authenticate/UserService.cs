using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Authenticate.Models;
using Application.General;
using Application.General.Entities;
using Application.General.Models;
using Functions;
using Microsoft.IdentityModel.Tokens;
using Repository;

namespace Application.Authenticate
{
    public interface IUserService
    {
        User Authenticate(AuthenticateModel am, int type);
    }
    public class UserService : IUserService
    {
        private LogService ls;
        private Context context;
        public User Authenticate(AuthenticateModel am, int type)
        {
            QueryUtils queryUtils = new QueryUtils();
            am = queryUtils.SQLHandler(am);
            User user = new User();
            ls = new LogService();
            
            string stQuery = "SELECT tbperson.IDPERSON" +
                                "	,tbperson.NAME" +
                                "	,tbrole.ROLE" +
                                "	,tbperson.PASSWORD" +
                                "   ,tbperson.IDADMIN" +
                                " FROM tbperson" +
                                " JOIN tbrole on tbrole.IDROLE = tbperson.IDROLE" +
                                " WHERE " +
                                "		tbperson.USERNAME = " + queryUtils.InsertSingleQuotes(am.USERNAME) +
                                "	AND tbperson.PASSWORD = " + queryUtils.InsertSingleQuotes(am.PASSWORD) + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);

                if (retDT.Rows.Count > 0)
                {
                    if (retDT.Rows[0]["PASSWORD"] != null && retDT.Rows[0]["PASSWORD"].ToString() == am.PASSWORD)
                    {
                        user = queryUtils.DataTableToObject<User>(retDT);
                    }
                    else
                    {
                        user = null;
                    }
                } else
                {
                    user = null;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(0, "Erro na Authenticate (67)", e.Message, stQuery);
                user = null;
            }

            context.Dispose();

            if (user == null)
                return null;

            if(type == 0)
            {
                user.PERMISSION = new List<int>();

                stQuery = "SELECT IDPERMISSION" +
                    " FROM tbuserprofilepermission" +
                    " join tbperson on tbperson.IDUSERPROFILE = tbuserprofilepermission.IDUSERPROFILE" +
                    " WHERE tbperson.IDPERSON = " + user.IDPERSON + ";";       
                
                context = new Context();

                try
                {
                    var retDT = context.RunCommandDT(stQuery);

                    if (retDT.Rows.Count > 0)
                    {
                        
                        foreach (DataRow reader in retDT.Rows)
                        {
                            if (reader != null)
                            {
                                user.PERMISSION.Add(Convert.ToInt32(reader[0]));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ls.AddLogError(0, "Erro na Authenticate (102)", e.Message, stQuery);
                }
            }

            if(type == 1)
            {
                stQuery = "select IDACCOUNT from tbresident where IDPERSON = " + user.IDPERSON + ";";
                context = new Context();

                try
                {
                    var retDT = context.RunCommandDT(stQuery);

                    if (retDT.Rows.Count > 0)
                    {
                        user.IDACCOUNT = new List<int>();
                        foreach(DataRow reader in retDT.Rows)
                        {
                            if(reader != null)
                            {
                                user.IDACCOUNT.Add(Convert.ToInt32(reader[0]));
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    ls.AddLogError(0, "Erro na Authenticate (67)", e.Message, stQuery);
                }

                context.Dispose();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.IDPERSON.ToString()),
                    new Claim(ClaimTypes.Role, user.ROLE)
                }),
                Expires = DateTime.UtcNow.AddHours(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.TOKEN = tokenHandler.WriteToken(token);

            user.PASSWORD = null;

            return user;
        }

        public GenericReturnModel AddEditAdmin(UserRegisterModel user, string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            user = queryUtils.SQLHandler(user);

            if (user.IDPERSON == 0)
                return AddAdmin(user, IdPerson);
            else
                return EditAdmin(user, IdPerson);
        }

        public GenericReturnModel AddAdmin(UserRegisterModel user, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            bool control = true;
            ls = new LogService();

            user.IDPERSON = 0;
            user.IDUSERPROFILE = 0;

            string stQuery;
            List<string> querys = new List<string>();

            stQuery = "select username from tbperson where username = " + queryUtils.InsertSingleQuotes(user.USERNAME) +";";
            context = new Context();

            try
            {
                var retDB = context.RunCommandDT(stQuery);
                if (retDB.Rows.Count > 0)
                {
                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Este nome de usuário está em uso.";
                    statusReturn.STATUSCODE = 409;
                    control = false;
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (164)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha no processo. Por favor, tente novamente.";
                statusReturn.STATUSCODE = 500;
                control = false;
            }

            context.Dispose();

            stQuery = "select * from tbcontact where IDTYPECONTACT = 1 AND EMAIL = " + queryUtils.InsertSingleQuotes(user.EMAIL) + ";";
            context = new Context();

            try
            {
                var retDB = context.RunCommandDT(stQuery);
                if (retDB.Rows.Count > 0)
                {
                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Este e-mail está em uso.";
                    statusReturn.STATUSCODE = 409;
                    control = false;
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (190)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha no processo. Por favor, tente novamente.";
                statusReturn.STATUSCODE = 500;
                control = false;
            }

            context.Dispose();


            stQuery = "INSERT INTO tbperson (" +
                        "	 NAME" +
                        "	,IDROLE" +
                        "	,IDUSERPROFILE" +
                        "	,USERNAME" +
                        "	,PASSWORD" +
                        "	,IDTYPEPERSON" +
                        "	,IDADMIN" +
                        "	,ACTIVE" +
                        "	)" +
                        " VALUES (" +
                        "	 " + queryUtils.InsertSingleQuotes(user.NAME) +
                        "	," + "2" +
                        "	," + user.IDUSERPROFILE +
                        "	," + queryUtils.InsertSingleQuotes(user.USERNAME) +
                        "	," + queryUtils.InsertSingleQuotes(user.PASSWORD) +
                        "	," + "1" +
                        "	," + "1" +
                        "	," + user.ACTIVE +
                        "	);";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDPERSON;");
                if (retDB.HasRows)
                    user.IDPERSON = int.Parse(queryUtils.ReturnId(retDB, "IDPERSON"));

            }
            catch(Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (234)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                statusReturn.STATUSCODE = 500;
                control = false;
            }

            context.Dispose();

            if (control)
            {
                stQuery = "INSERT INTO tbuserprofile (" +
                        "	 USERPROFILE" +
                        "	,DESCRIPTION" +
                        "	,IDADMIN" +
                        "	)" +
                        " VALUES (" +
                        "	 'Administrador'" +
                        "	,'Acesso total ao sistema.'" +
                        "	," + user.IDPERSON +
                        "	);";

                context = new Context();

                try
                {
                    var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDUSERPROFILE;");
                    if (retDB.HasRows)
                        user.IDUSERPROFILE = int.Parse(queryUtils.ReturnId(retDB, "IDUSERPROFILE"));

                }
                catch (Exception e)
                {
                    control = false;

                    ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (271)", e.Message, stQuery);

                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Falha ao inserir perfil de administrador no banco de dados.";
                    statusReturn.STATUSCODE = 500;
                }

                context.Dispose();
            }

            if (control)
            {
                stQuery = "SELECT IDPERMISSION as value FROM tbpermission;";

                context = new Context();

                var retDBPermission = context.RunCommandDT(stQuery);

                if (retDBPermission.Rows.Count > 0)
                {
                    foreach (DataRow reader in retDBPermission.Rows)
                    {
                        if (reader[0] == null) { }
                        else
                        {
                            stQuery = "INSERT INTO tbuserprofilepermission (" +
                                    "	IDUSERPROFILE" +
                                    "	,IDPERMISSION" +
                                    "	)" +
                                    " VALUES (" +
                                    "	 " + user.IDUSERPROFILE +
                                    "	," + reader[0].ToString() +
                                    "	);";

                            querys.Add(stQuery);
                        }
                    }

                    if (querys.Count == 0) 
                    {
                        control = false;

                        statusReturn.ID = 0;
                        statusReturn.MESSAGE = "Não existem permissões a serem liberadas. Contate o administrador do sistema.";
                        statusReturn.STATUSCODE = 500;
                    }
                }
                else
                {
                    control = false;
                    
                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Não existem permissões a serem liberadas. Contate o administrador do sistema.";
                    statusReturn.STATUSCODE = 500;
                }

                context.Dispose();
            }

            if (control)
            {
                stQuery = "INSERT INTO tbcontact (" +
                        "	 IDTYPECONTACT" +
                        "	,EMAIL" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "1" +
                        "	," + queryUtils.InsertSingleQuotes(user.EMAIL) +
                        "	," + queryUtils.InsertSingleQuotes(user.IDPERSON.ToString()) +
                        "	);";

                querys.Add(stQuery);

                stQuery = "update tbperson set IDADMIN = " + user.IDPERSON + ", IDUSERPROFILE = " + user.IDUSERPROFILE + " where IDPERSON = " + user.IDPERSON + ";";
                
                querys.Add(stQuery);

                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "Administrador cadastrado com sucesso!";
                    statusReturn.STATUSCODE = 201;
                    ls.AddLogSystem(int.Parse(IdPerson), user.IDPERSON, "Inseriu novo administrador.", IdLogSystemList.REGISTERUSER, "");
                }
                else
                {
                    control = false;

                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                    statusReturn.STATUSCODE = 500;
                }
            }

            if (!control)
            {
                context = new Context();

                if (user.IDPERSON != 0)
                {
                    try { 
                        stQuery = "DELETE FROM tbperson WHERE IDPERSON = " + user.IDPERSON + ";";
                        context.RunCommand(stQuery);
                    } 
                    catch(Exception e)
                    {
                        ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (379)", e.Message, stQuery);
                    }
                    try
                    {
                        stQuery = "DELETE FROM tbcontact WHERE IDPERSON = " + user.IDPERSON + ";";
                        context.RunCommand(stQuery);
                    }
                    catch (Exception e)
                    {
                        ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (388)", e.Message, stQuery);
                    }
                }

                if(user.IDUSERPROFILE != 0)
                {
                    try
                    {
                        stQuery = "DELETE FROM tbuserprofile where IDUSERPROFILE = " + user.IDUSERPROFILE + ";";
                        context.RunCommand(stQuery);
                    }
                    catch (Exception e)
                    {
                        ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (401)", e.Message, stQuery);
                    }

                    try
                    {
                        stQuery = "DELETE FROM tbuserprofilepermission where IDUSERPROFILE = " + user.IDUSERPROFILE + ";";
                        context.RunCommand(stQuery);
                    }
                    catch (Exception e)
                    {
                        ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddAdmin (411)", e.Message, stQuery);
                    }
                }

                context.Dispose();
            }

            if (control && user.SENDACTIVEEMAIL == true)
            {
                Email sender = new Email();
                sender.EmailSender("", "arthur.alencar@xpsat.com.br", "", "", "Teste", "<html><body>Vai chegar?</body></html>");
            }

            return statusReturn;
        }

        public GenericReturnModel EditAdmin(UserRegisterModel user, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;
            List<string> querys = new List<string>();

            stQuery = "select * from tbcontact where IDTYPECONTACT = 1 AND EMAIL = " + queryUtils.InsertSingleQuotes(user.EMAIL) + ";";
            context = new Context();

            try
            {
                var retDB = context.RunCommandDT(stQuery);
                if (retDB.Rows.Count > 0)
                {
                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Este e-mail está em uso.";
                    statusReturn.STATUSCODE = 409;
                    context.Dispose();
                    return statusReturn;
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - EditAdmin (455)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha no processo. Por favor, tente novamente.";
                statusReturn.STATUSCODE = 500;
                context.Dispose();
                return statusReturn;
            }

            context.Dispose();

            stQuery = "UPDATE tbperson" +
                        " SET " +
                        "	 NAME = " + queryUtils.InsertSingleQuotes(user.NAME) +
                        "	,ACTIVE = " + user.ACTIVE +
                        " WHERE IDPERSON = " + user.IDPERSON + ";";

            querys.Add(stQuery);

            stQuery = "DELETE FROM tbcontact" +
              "	 WHERE IDPERSON = " + queryUtils.InsertSingleQuotes(user.IDPERSON.ToString()) + ";";

            querys.Add(stQuery);

            stQuery = "INSERT INTO tbcontact (" +
                        "	 IDTYPECONTACT" +
                        "	,EMAIL" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "1" +
                        "	," + queryUtils.InsertSingleQuotes(user.EMAIL) +
                        "	," + queryUtils.InsertSingleQuotes(user.IDPERSON.ToString()) +
                        "	);";

            querys.Add(stQuery);

            string stringAdmin = "";
            try 
            {
                var adminBefore = GetListAdmins(IdPerson);
                int indexBefore = adminBefore.FindIndex(x => x.IDPERSON == user.IDPERSON);
                stringAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(adminBefore[indexBefore]);
            } 
            catch (Exception e) { }

            ContextTransaction ct = new ContextTransaction();

            if (ct.RunTransaction(querys))
            {
                statusReturn.MESSAGE = "Administrador atualizado com sucesso!";
                statusReturn.STATUSCODE = 201;
                ls.AddLogSystem(int.Parse(IdPerson), user.IDPERSON, "Atualizadou administrador.", IdLogSystemList.EDITUSER, stringAdmin);
            }
            else
            {
                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao atualizar no banco de dados.";
                statusReturn.STATUSCODE = 500;
            }

            return statusReturn;
        }

        public List<UserRegisterModel> GetListAdmins(string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            List<UserRegisterModel> admins = new List<UserRegisterModel>();

            stQuery = "SELECT" +
                    "	 tbperson.IDPERSON" +
                    "	,tbperson.NAME" +
                    "	,tbperson.IDUSERPROFILE" +
                    "	,tbperson.USERNAME" +
                    "	,tbperson.PASSWORD" +
                    "	,tbperson.IDTYPEPERSON" +
                    "	,tbperson.IDADMIN" +
                    "	,tbperson.ACTIVE" +
                    "   ,tbuserprofile.USERPROFILE" +
                    " FROM tbperson" +
                    " JOIN tbuserprofile on tbuserprofile.IDUSERPROFILE = tbperson.IDUSERPROFILE" +
                    " WHERE IDROLE = 2;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                admins = queryUtils.DataTableToList<UserRegisterModel>(retDT);

                for(int index = 0; index < admins.Count; index++)
                {
                    context.Dispose();
                    context = new Context();

                    stQuery = "SELECT EMAIL" +
                                " FROM tbcontact" +
                                " WHERE " +
                                "		IDTYPECONTACT = 1" +
                                "	AND IDPERSON = " + admins[index].IDPERSON + ";";

                    var retDTEmail = context.RunCommandDT(stQuery);
                    
                    if(retDTEmail.Rows.Count > 0)
                        if (retDTEmail.Rows[0][0] != null) 
                        {
                            admins[index].EMAIL = retDTEmail.Rows[0][0].ToString();
                        }
                }
            }
            catch(Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - GetListAdmins (455)", e.Message, stQuery);
            }

            context.Dispose();

            return admins;
        }

        public List<UserProfileModel> GetListProfile(string IdPerson, string IdAdmin)
        {
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            List<UserProfileModel> profiles = new List<UserProfileModel>();

            stQuery = "SELECT" +
                    "	 IDUSERPROFILE" +
                    "	,USERPROFILE" +
                    "	,DESCRIPTION" +
                    " FROM tbuserprofile" +
                    " WHERE" +
                    "       IDADMIN = " + IdAdmin +
                    "   AND USERPROFILE <> " + queryUtils.InsertSingleQuotes("Administrador") + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                profiles = queryUtils.DataTableToList<UserProfileModel>(retDT);
                
                if(profiles.Count > 0)
                {
                    for(int i = 0; i < profiles.Count; i++)
                    {
                        context.Dispose();

                        stQuery = "SELECT IDPERMISSION" +
                                    " FROM tbuserprofilepermission" +
                                    " WHERE IDUSERPROFILE = " + profiles[i].IDUSERPROFILE + ";";

                        context = new Context();

                        var retDTPermission = context.RunCommandDT(stQuery);

                        if(retDTPermission.Rows.Count > 0)
                        {
                            profiles[i].PERMISSION = new List<int>();

                            foreach(DataRow reader in retDTPermission.Rows)
                            {
                                if(reader != null && !string.IsNullOrEmpty(reader[0].ToString()))
                                {
                                    profiles[i].PERMISSION.Add(Convert.ToInt32(reader[0]));
                                }
                            }
                        }

                    }

                    stQuery = "";
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - GetListProfile (513)", e.Message, stQuery);
            }

            context.Dispose();

            return profiles;
        }

        public List<PermissionModel> GetListPermission(string IdPerson)
        {
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            List<PermissionModel> permissions = new List<PermissionModel>();

            stQuery = "SELECT IDPERMISSION" +
                    "	,PERMISSION" +
                    "	,DESCRIPTION" +
                    " FROM tbpermission;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                permissions = queryUtils.DataTableToList<PermissionModel>(retDT);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - GetListPermission (624)", e.Message, stQuery);
            }

            context.Dispose();

            return permissions;
        }

        public GenericReturnModel AddEditUserProfile(UserProfileModel profile, string IdPerson, string IdAdmin)
        {
            QueryUtils queryUtils = new QueryUtils();
            profile = queryUtils.SQLHandler(profile);

            if (profile.IDUSERPROFILE == 0)
                return AddUserProfile(profile, IdPerson, IdAdmin);
            else
                return EditUserProfile(profile, IdPerson, IdAdmin);
        }

        private GenericReturnModel AddUserProfile(UserProfileModel profile, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;

            stQuery = "INSERT INTO tbuserprofile (" +
                    "	 USERPROFILE" +
                    "	,DESCRIPTION" +
                    "	,IDADMIN" +
                    "	)" +
                    " VALUES (" +
                    "	 " + queryUtils.InsertSingleQuotes(profile.USERPROFILE) + 
                    "	," + queryUtils.InsertSingleQuotes(profile.DESCRIPTION) +
                    "	," + IdAdmin +
                    "	);";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDUSERPROFILE;");
                if (retDB.HasRows)
                    profile.IDUSERPROFILE = int.Parse(queryUtils.ReturnId(retDB, "IDUSERPROFILE"));
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddUserProfile (707)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                statusReturn.STATUSCODE = 500;
                context.Dispose();
                return statusReturn;
            }

            context.Dispose();

            if (profile.PERMISSION.Count > 0)
            {
                List<string> querys = new List<string>();

                foreach (int perm in profile.PERMISSION)
                {
                    stQuery = "INSERT INTO tbuserprofilepermission (" +
                            "	 IDUSERPROFILE" +
                            "	,IDPERMISSION" +
                            "	)" +
                            " VALUES (" +
                            "	 " + profile.IDUSERPROFILE +
                            "	," + perm.ToString() +
                            "	);";

                    querys.Add(stQuery);
                }

                ContextTransaction contextTransaction = new ContextTransaction();

                if (!contextTransaction.RunTransaction(querys))
                {
                    stQuery = "DELETE FROM tbuserprofile where IDUSERPROFILE = " + profile.IDUSERPROFILE + ";";

                    context = new Context();

                    try
                    {
                        context.RunCommand(stQuery);
                        statusReturn.ID = 0;
                        statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                        statusReturn.STATUSCODE = 500;
                    }
                    catch (Exception e)
                    {
                        ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddUserProfile (752)", e.Message, stQuery);
                        ls.AddLogSystem(int.Parse(IdPerson), int.Parse(IdAdmin), "Inseriu novo perfil de usuário.", IdLogSystemList.REGISTERUSERPROFILE, "");
                        statusReturn.ID = 0;
                        statusReturn.MESSAGE = "Perfil adicionado sem permissões.";
                        statusReturn.STATUSCODE = 201;
                    }

                    context.Dispose();
                } 
                else
                {
                    statusReturn.MESSAGE = "Perfil adicionado com sucesso.";
                    statusReturn.STATUSCODE = 201;
                    ls.AddLogSystem(int.Parse(IdPerson), int.Parse(IdAdmin), "Inseriu novo perfil de usuário.", IdLogSystemList.REGISTERUSERPROFILE, "");
                }
            }

            return statusReturn;
        }

        private GenericReturnModel EditUserProfile(UserProfileModel profile, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            List<string> querys = new List<string>();
            string stQuery;

            stQuery = "UPDATE tbuserprofile" +
                        " SET" +
                        "	 USERPROFILE = " + queryUtils.InsertSingleQuotes(profile.USERPROFILE) +
                        "	,DESCRIPTION = " + queryUtils.InsertSingleQuotes(profile.DESCRIPTION) +
                        " WHERE " +
                        "       IDADMIN = " + IdAdmin +
                        "   AND IDUSERPROFILE = " + profile.IDUSERPROFILE.ToString() + ";";

            querys.Add(stQuery);

            stQuery = "DELETE FROM tbuserprofilepermission" +
                        " WHERE IDUSERPROFILE = " + profile.IDUSERPROFILE.ToString() + ";";

            querys.Add(stQuery);

            if (profile.PERMISSION.Count > 0)
            {
                foreach (int perm in profile.PERMISSION)
                {
                    stQuery = "INSERT INTO tbuserprofilepermission (" +
                            "	 IDUSERPROFILE" +
                            "	,IDPERMISSION" +
                            "	)" +
                            " VALUES (" +
                            "	 " + profile.IDUSERPROFILE +
                            "	," + perm.ToString() +
                            "	);";

                    querys.Add(stQuery);
                }
            }

            string stringUserProfile = "";

            try 
            {
                var userProfileBefore = GetListProfile(IdPerson, IdAdmin);
                int indexBefore = userProfileBefore.FindIndex(x => x.IDUSERPROFILE == profile.IDUSERPROFILE);
                stringUserProfile = Newtonsoft.Json.JsonConvert.SerializeObject(userProfileBefore[indexBefore]);
            } 
            catch (Exception e) { }

            ContextTransaction contextTransaction = new ContextTransaction();

            if (!contextTransaction.RunTransaction(querys))
            {
                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao atualizar no banco de dados.";
                statusReturn.STATUSCODE = 500;
            }
            else
            {
                statusReturn.MESSAGE = "Perfil atualizado com sucesso.";
                statusReturn.STATUSCODE = 201;
                ls.AddLogSystem(int.Parse(IdPerson), int.Parse(IdAdmin), "Atualizou perfil de usuário.", IdLogSystemList.EDITUSERPROFILE, stringUserProfile);
            }

            return statusReturn;
        }

        public GenericReturnModel AddEditUser(UserRegisterModel user, string IdPerson, string IdAdmin)
        {
            QueryUtils queryUtils = new QueryUtils();
            user = queryUtils.SQLHandler(user);

            if (user.IDPERSON == 0)
                return AddUser(user, IdPerson, IdAdmin);
            else
                return EditUser(user, IdPerson, IdAdmin);
        }

        private GenericReturnModel AddUser(UserRegisterModel user, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();

            ls = new LogService();

            user.IDPERSON = 0;

            string stQuery;

            stQuery = "select username from tbperson where username = " + queryUtils.InsertSingleQuotes(user.USERNAME) + ";";

            context = new Context();

            try
            {
                var retDB = context.RunCommandDT(stQuery);
                if (retDB.Rows.Count > 0)
                {
                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Este nome de usuário está em uso.";
                    statusReturn.STATUSCODE = 409;
                    return statusReturn;
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddUser (886)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha no processo. Por favor, tente novamente.";
                statusReturn.STATUSCODE = 500;
                context.Dispose();
                return statusReturn;
            }

            context.Dispose();

            stQuery = "select * from tbcontact where IDTYPECONTACT = 1 AND EMAIL = " + queryUtils.InsertSingleQuotes(user.EMAIL) + ";";
            context = new Context();

            try
            {
                var retDB = context.RunCommandDT(stQuery);
                if (retDB.Rows.Count > 0)
                {
                    statusReturn.ID = 0;
                    statusReturn.MESSAGE = "Este e-mail está em uso.";
                    statusReturn.STATUSCODE = 409;
                    return statusReturn;
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddUser (914)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha no processo. Por favor, tente novamente.";
                statusReturn.STATUSCODE = 500;
                context.Dispose();
                return statusReturn;

            }

            context.Dispose();

            stQuery = "INSERT INTO tbperson (" +
                        "	 NAME" +
                        "	,IDROLE" +
                        "	,IDUSERPROFILE" +
                        "	,USERNAME" +
                        "	,PASSWORD" +
                        "	,IDTYPEPERSON" +
                        "	,IDADMIN" +
                        "	,ACTIVE" +
                        "	)" +
                        " VALUES (" +
                        "	 " + queryUtils.InsertSingleQuotes(user.NAME) +
                        "	," + "3" +
                        "	," + user.IDUSERPROFILE +
                        "	," + queryUtils.InsertSingleQuotes(user.USERNAME) +
                        "	," + queryUtils.InsertSingleQuotes(user.PASSWORD) +
                        "	," + "1" +
                        "	," + IdAdmin +
                        "	," + user.ACTIVE +
                        "	);";

            context = new Context();

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDPERSON;");
                if (retDB.HasRows)
                    user.IDPERSON = int.Parse(queryUtils.ReturnId(retDB, "IDPERSON"));

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddUser (958)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                statusReturn.STATUSCODE = 500;
                context.Dispose();
                return statusReturn;
            }

            context.Dispose();

            stQuery = "INSERT INTO tbcontact (" +
                        "	 IDTYPECONTACT" +
                        "	,EMAIL" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "1" +
                        "	," + queryUtils.InsertSingleQuotes(user.EMAIL) +
                        "	," + queryUtils.InsertSingleQuotes(user.IDPERSON.ToString()) +
                        "	);";

            context = new Context();

            try
            {
                context.RunCommand(stQuery);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddUser (988)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
                statusReturn.STATUSCODE = 500; 
            }

            context.Dispose();

            if(statusReturn.STATUSCODE == 500)
            {
                context = new Context();

                try
                {
                    stQuery = "DELETE FROM tbperson WHERE IDPERSON = " + user.IDPERSON + ";";
                    context.RunCommand(stQuery);
                }
                catch (Exception e)
                {
                    ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - AddUser (1008)", e.Message, stQuery);
                }

                context.Dispose();
            }
            else
            {
                statusReturn.MESSAGE = "Usuário cadastrado com sucesso!";
                statusReturn.STATUSCODE = 201;
                ls.AddLogSystem(int.Parse(IdPerson), user.IDPERSON, "Inseriu novo usuário.", IdLogSystemList.REGISTERUSER, "");
            }

            return statusReturn;
        }

        public GenericReturnModel EditUser(UserRegisterModel user, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();

            string stQuery;
            List<string> querys = new List<string>();

            stQuery = "select IDPERSON from tbcontact where IDTYPECONTACT = 1 AND EMAIL = " + queryUtils.InsertSingleQuotes(user.EMAIL) + ";";
            context = new Context();

            try
            {
                var retDB = context.RunCommandDT(stQuery);
                if (retDB.Rows.Count > 0)
                {
                    if (Convert.ToInt32(retDB.Rows[0][0]) != user.IDPERSON) 
                    {
                        statusReturn.ID = 0;
                        statusReturn.MESSAGE = "Este e-mail está em uso.";
                        statusReturn.STATUSCODE = 409;
                        context.Dispose();
                        return statusReturn;
                    }
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - EditUser (1048)", e.Message, stQuery);

                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha no processo. Por favor, tente novamente.";
                statusReturn.STATUSCODE = 500;
                context.Dispose();
                return statusReturn;
            }

            context.Dispose();

            stQuery = "UPDATE tbperson" +
                        " SET " +
                        "	 NAME = " + queryUtils.InsertSingleQuotes(user.NAME) +
                        "	,IDUSERPROFILE = " + user.IDUSERPROFILE +
                        "	,ACTIVE = " + user.ACTIVE +
                        " WHERE IDPERSON = " + user.IDPERSON + ";";

            querys.Add(stQuery);

            stQuery = "DELETE FROM tbcontact" +
              "	 WHERE IDPERSON = " + queryUtils.InsertSingleQuotes(user.IDPERSON.ToString()) + ";";

            querys.Add(stQuery);

            stQuery = "INSERT INTO tbcontact (" +
                        "	 IDTYPECONTACT" +
                        "	,EMAIL" +
                        "	,IDPERSON" +
                        "	)" +
                        " VALUES (" +
                        "	 " + "1" +
                        "	," + queryUtils.InsertSingleQuotes(user.EMAIL) +
                        "	," + queryUtils.InsertSingleQuotes(user.IDPERSON.ToString()) +
                        "	);";

            querys.Add(stQuery);

            string stringUser = "";
            try
            {
                var userBefore = GetListUsers(IdPerson, IdAdmin);
                int indexBefore = userBefore.FindIndex(x => x.IDPERSON == user.IDPERSON);
                stringUser = Newtonsoft.Json.JsonConvert.SerializeObject(userBefore[indexBefore]);
            }
            catch (Exception e) { }

            ContextTransaction ct = new ContextTransaction();

            if (ct.RunTransaction(querys))
            {
                statusReturn.MESSAGE = "Usuário atualizado com sucesso!";
                statusReturn.STATUSCODE = 201;
                ls.AddLogSystem(int.Parse(IdPerson), user.IDPERSON, "Atualizadou usuário.", IdLogSystemList.EDITUSER, stringUser);
            }
            else
            {
                statusReturn.ID = 0;
                statusReturn.MESSAGE = "Falha ao atualizar no banco de dados.";
                statusReturn.STATUSCODE = 500;
            }

            return statusReturn;
        }

        public List<UserRegisterModel> GetListUsers(string IdPerson, string IdAdmin)
        {
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            List<UserRegisterModel> users = new List<UserRegisterModel>();

            stQuery = "SELECT" +
                    "	 tbperson.IDPERSON" +
                    "	,tbperson.NAME" +
                    "	,tbperson.IDUSERPROFILE" +
                    "	,tbperson.USERNAME" +
                    "	,tbperson.PASSWORD" +
                    "	,tbperson.IDTYPEPERSON" +
                    "	,tbperson.IDADMIN" +
                    "	,tbperson.ACTIVE" +
                    "   ,tbuserprofile.USERPROFILE" +
                    " FROM tbperson" +
                    " JOIN tbuserprofile on tbuserprofile.IDUSERPROFILE = tbperson.IDUSERPROFILE" +
                    " WHERE " +
                    "       tbperson.IDPERSON <> " + IdPerson +
                    "   AND tbperson.IDADMIN = " + IdAdmin + 
                    "   AND tbperson.IDPERSON <> " + IdAdmin + ";";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                users = queryUtils.DataTableToList<UserRegisterModel>(retDT);

                for (int index = 0; index < users.Count; index++)
                {
                    context.Dispose();
                    context = new Context();

                    stQuery = "SELECT EMAIL" +
                                " FROM tbcontact" +
                                " WHERE " +
                                "		IDTYPECONTACT = 1" +
                                "	AND IDPERSON = " + users[index].IDPERSON + ";";

                    var retDTEmail = context.RunCommandDT(stQuery);

                    if (retDTEmail.Rows.Count > 0)
                        if (retDTEmail.Rows[0][0] != null)
                        {
                            users[index].EMAIL = retDTEmail.Rows[0][0].ToString();
                        }
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no User Service - GetListUsers (1164)", e.Message, stQuery);
            }

            context.Dispose();

            return users;
        }
    }
}
