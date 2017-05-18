using Networking;
using Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Core.Authentication;
using Core.Data;

namespace Core
{
    public class LandingProcessor : PooledProcessor
    {
        protected static readonly string ConnectionTagName = "LandingPage";

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

        public override void ProcessorAttach(Connection con)
        {
            lock (con)
                con.SetMessageProcessorTag(ConnectionTagName, new LandingStateData());

            if (con.SentHeader)
            {
                SendUserFileMessage(con, "login/logon.data");
                con.SentHeader = true;
            }

            // send them the hello
            SendUserFileMessage(con, "login/get_name.data");
        }

        protected override void ProcessConnection(Connection user)
        {
            base.ProcessConnection(user);

            if (!user.HasPendingInbound())
                return;

            LandingStateData data = GetConStateData(user);

            string msg = user.PopInboundMessage();
            while (msg != string.Empty)
            {
                bool keepGoing = false;
                if (Handlers.ContainsKey(data.LoginState))
                    keepGoing = Handlers[data.LoginState](user, msg);
                else
                {
                    LogCache.Log(LogCache.BasicLog, "Unable to process handler for " + data.LoginState.ToString());
                    data.LoginState = LandingStateData.LoginStates.Unknown;
                    keepGoing = Handlers[data.LoginState](user, msg);
                }

                if (keepGoing)
                    msg = user.PopInboundMessage();
                else
                    msg = string.Empty;
            }
        }

        protected bool HandleUnknown(Connection user, string message)
        {
            LandingStateData data = GetConStateData(user);

            string name = message.ToLowerInvariant();

            if (data.NeedUserCreate)
            {
                if (!ValidUserName(name))
                    SendUserFileMessage(user, "login/invalid_name.data");
                else
                {
                    // check if name exists?
                    if (AuthenticaitonDB.UserExists(name))
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
            LandingStateData data = GetConStateData(user);

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
                if (!AuthenticaitonDB.CreateUser(data.UserName, data.Password, string.Empty))
                {
                    SendUserFileMessage(user, "login/create_error.data");
                    data.LoginState = LandingStateData.LoginStates.Unknown;
                    return true;
                }
            }
            data.LoginState = LandingStateData.LoginStates.GotPassword;

            // process the new user or the login
            string authFlags = string.Empty;

            if (!AuthenticaitonDB.AuthenticateUser(data.UserName,data.Password,out authFlags))
            {
                SendUserFileMessage(user, "login/login_error.data");
                data.LoginState = LandingStateData.LoginStates.Unknown;
                return true;
            }

            data.LoginState = LandingStateData.LoginStates.Authenticated;

            // send them to the next handler
            AuthenticationComplete?.Invoke(this, user);
            return user.MessageProcessor == this;
        }

        protected void GotConUsername(Connection user, string username)
        {
            LandingStateData data = GetConStateData(user);
            data.UserName = username;
            data.LoginState = LandingStateData.LoginStates.GotName;

            if (data.NeedUserCreate)
                SendUserFileMessage(user, "login/create_password.data");
            else
                SendUserFileMessage(user,"login/login_password.data");
        }

        protected void SendUserFileMessage(Connection user, string path)
        {
            user.SendOutboundMessage(FileTools.GetFileContents(Paths.DataPath, path, true));
        }

        protected bool ValidUserName(string name)
        {
            return name.Trim().Length >= 2 && name != "new";
        }

        protected bool ValidPassword(string pass)
        {
            return pass.Trim().Length >= 3;
        }

        protected LandingStateData GetConStateData(Connection con)
        {
            LandingStateData d = con.GetMesssageProcessorTag<LandingStateData>();
            if (d == null)
            {
                d = con.GetMesssageProcessorTag(ConnectionTagName) as LandingStateData;
                if (d == null)
                {
                    d = new LandingStateData();
                    con.SetMessageProcessorTag(ConnectionTagName, d);
                }
                else
                    con.SetMessageProcessorTag(ConnectionTagName);
            }

            return d;
        }
    }

}
