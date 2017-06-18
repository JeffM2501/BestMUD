using Networking;
using Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Core.Databases.Authentication;
using Core.Data;

namespace Core.Processors
{
    public class LandingProcessor : PooledProcessor
    {
        public class LandingStateData
        {
            public enum LoginStates
            {
                Unknown,
                GotName,
                GotPassword,
                NewUser,
                Authenticated,
            }
            public LoginStates LoginState = LoginStates.Unknown;

            public bool NeedUserCreate = false;

            public string UserName = string.Empty;
            public string Password = string.Empty;
        }

        protected delegate bool StateHandler(Connection user, string message);

        protected Dictionary<LandingStateData.LoginStates, StateHandler> Handlers = new Dictionary<LandingStateData.LoginStates, StateHandler>();

        public event EventHandler<Connection> AuthenticationComplete = null;

        public LandingProcessor()
        {
            Handlers.Add(LandingStateData.LoginStates.Unknown, HandleUnknown);
            Handlers.Add(LandingStateData.LoginStates.GotName, HandleGotUsername);

            MaxConnections = 200;
            DestoryOnEmpty = true;
        }

        public override void ProcessorAttach(Connection user)
        {
            base.ProcessorAttach(user);

            GetConStateData<LandingStateData>(user);

            user.UserID = -1;
            user.AccessFlags.Clear();

            if (user.SentHeader)
            {
                SendUserFileMessage(user, "login/logon.data");
                user.SentHeader = true;
            }

            // send them the hello
            SendUserFileMessage(user, "login/get_name.data");
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            LandingStateData data = GetConStateData<LandingStateData>(user);

            if (Handlers.ContainsKey(data.LoginState))
                return Handlers[data.LoginState](user, msg);
            else
            {
                LogCache.Log(LogCache.BasicLog, "Unable to process landing handler for " + data.LoginState.ToString());
                data.LoginState = LandingStateData.LoginStates.Unknown;
                return Handlers[data.LoginState](user, msg);
            }
        }

        protected bool HandleUnknown(Connection user, string message)
        {
            LandingStateData data = GetConStateData<LandingStateData>(user);

            string name = message.ToLowerInvariant();

            if (data.NeedUserCreate)
            {
                if (name == "exit")
                {
                    SendUserFileMessage(user, "login/get_name.data");
                    return true;

                }
                if (!ValidUserName(name))
                    SendUserFileMessage(user, "login/invalid_name.data");
                else
                {
                    // check if name exists?
                    if (AuthenticaitonDB.Instance.UserExists(name))
                    {
                        SendUserFileMessage(user, "login/invalid_name.data");
                        SendUserFileMessage(user, "login/create_name.data");
                    }
                    else
                    {
                        data.UserName = name;
                        SendUserFileMessage(user, "login/create_password.data");
                        data.LoginState = LandingStateData.LoginStates.GotName;
                    }
                }
            }
            else
            {
                if (name == "new")
                {
                    data.NeedUserCreate = true;
                    SendUserFileMessage(user, "login/create_name.data");
                }
                else
                    GotConUsername(user, message);
            }

            return true;
        }

        protected bool HandleGotUsername(Connection user, string message)
        {
            LandingStateData data = GetConStateData<LandingStateData>(user);

            if (data.NeedUserCreate)
            {
                if (!ValidPassword(message))
                {
                    SendUserFileMessage(user, "login/invalid_password.data");
                    return true;
                } 
            }

            data.Password = message;
            if (data.NeedUserCreate)
            {
                if (!AuthenticaitonDB.Instance.CreateUser(data.UserName, data.Password, string.Empty))
                {
                    SendUserFileMessage(user, "login/create_error.data");
                    data.LoginState = LandingStateData.LoginStates.Unknown;
                    return true;
                }
            }
            data.LoginState = LandingStateData.LoginStates.GotPassword;

            // process the new user or the login
            string authFlags = string.Empty;
            int userID = -1;

            if (!AuthenticaitonDB.Instance.AuthenticateUser(data.UserName,data.Password,out authFlags, out userID))
            {
                SendUserFileMessage(user, "login/login_error.data");
                data.LoginState = LandingStateData.LoginStates.Unknown;
                return true;
            }

            data.LoginState = LandingStateData.LoginStates.Authenticated;
            user.UserID = userID;

            if (authFlags != string.Empty)
                user.AccessFlags.AddRange(authFlags.Split(";".ToCharArray()));

            if (data.NeedUserCreate)
                LogCache.Log(LogCache.BasicLog, "User Created:(" + user.UserID.ToString() + ")" + data.UserName);

            LogCache.Log(LogCache.NetworkLog, "User Authenticated:(" + user.UserID.ToString() + ")" + data.UserName);
            // send them to the next handler
            AuthenticationComplete?.Invoke(this, user);
            return user.MessageProcessor == this;
        }

        protected void GotConUsername(Connection user, string username)
        {
            LandingStateData data = GetConStateData<LandingStateData>(user);
            data.UserName = username;
            data.LoginState = LandingStateData.LoginStates.GotName;

            if (data.NeedUserCreate)
                SendUserFileMessage(user, "login/create_password.data");
            else
                SendUserFileMessage(user,"login/login_password.data");
        }

        protected bool ValidUserName(string name)
        {
            return name.Trim().Length >= 2 && name != "new";
        }

        protected bool ValidPassword(string pass)
        {
            return pass.Trim().Length >= 3;
        }
    }

}
