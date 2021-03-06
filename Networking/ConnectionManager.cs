﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Utilities;

namespace Networking
{
    public class ConnectionManager
    {
        protected IProtocol ProtcolProcessor = null;
      

        protected List<Connection> ActiveConnections = new List<Connection>();

        protected object DataLocker = new object();
        protected Thread ClientDataThread = null;

        protected int MaxConnections = 0;

        protected readonly int BufferSize = 8 * 1024;
        protected readonly int MaxReadCountPerCycle = 5;
        protected readonly int MaxWriteCountPerCycle = 5;

        public delegate IMessageProcessor GetMessageProcessorCallback(Connection c);
        protected GetMessageProcessorCallback DefaultProcessorFactory = null;

        public ConnectionManager(IProtocol protocol, GetMessageProcessorCallback factory, int maxCount)
        {
            ProtcolProcessor = protocol;
            DefaultProcessorFactory = factory;
            MaxConnections = maxCount;
        }

        public virtual bool Accept(TcpClient client)
        {
            if (ProtcolProcessor == null)
            {
                LogCache.Log(LogCache.BasicLog, "ProtcolProcessor is null, rejecting client");
                return false;
            }

            lock(ActiveConnections)
            {
                if (ActiveConnections.Count >= MaxConnections)
                    return false;

                var con = new Connection(client);
                con.SetMessageProcessor(DefaultProcessorFactory(con));

                ProtcolProcessor.AddConnection(con); // let the protocol processor setup any data needed for the connection

                ActiveConnections.Add(con);  // TO DO, add a message processor here

                if (con.MessageProcessor != null)
                    con.MessageProcessor.ProcessAccept(con);
            }

            lock(DataLocker)    // make sure our data read/write thread is running
            {
                if (ClientDataThread == null)
                {
                    ClientDataThread = new Thread(new ThreadStart(ClientDataWorker));
                    ClientDataThread.Start();
                }
            }

            return true;
        }

        protected void ClientDataWorker()
        {
            while (true)
            {
                Connection[] connections = new Connection[0];

                lock (ActiveConnections)
                {
                    if (ActiveConnections.Count == 0)
                    {
                        lock (DataLocker)
                            ClientDataThread = null;

                        return;
                    }
                    else
                        connections = ActiveConnections.ToArray();
                }

                bool oneHadPending = false;

                foreach(var con in connections)
                {
                    if (con.Socket == null || !con.Socket.Connected || con.MessageProcessor == null) // it's dead or noody wants them
                    {
                        if (con.Socket != null) 
                            con.Socket.Close();

                        ProtcolProcessor.RemoveConnection(con);
                        if (con.MessageProcessor != null)
                            con.MessageProcessor.ProcessDisconnect(con);

                        lock (ActiveConnections)
                            ActiveConnections.Remove(con);

                        con.OnDisconnect();
                    }
                    else
                    {
                        if (con.DataStream.DataAvailable)
                        {
                            int read = BufferSize;
                            int reads = 0;

                            while (read == BufferSize && reads < MaxReadCountPerCycle)
                            {
                                reads++;

                                byte[] buffer = new byte[BufferSize];
                                read = con.DataStream.Read(buffer, 0, BufferSize);
                                if (read != 0)
                                {
                                    byte[] packet = new byte[read];
                                    Array.Copy(buffer, packet, read);
                                    ProtcolProcessor.TranslateInbound(packet, con);
                                }
                            }
                            if (read == BufferSize)
                                oneHadPending = true;
                        }

                        if (con.HasOutboundData())
                        {
                            StringBuilder sb = new StringBuilder();

                            string[] messages = con.PopNOutboundMessages(MaxWriteCountPerCycle);
                            if (messages.Length == 0)
                                break;

                            foreach(var m in messages)
                                ProtcolProcessor.TranslateOutbound(m, sb, con);

                            string data = sb.ToString();
                            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
                            con.DataStream.Write(buffer, 0, buffer.Length);
                            con.DataStream.FlushAsync();

                            if (con.HasOutboundData())
                                oneHadPending = true;
                        }

                        if (con.MessageProcessor == null) // they are not handing anyone so kill them. They will be cleaned up on the next pass
                            con.Socket.Close();
                    }
                }

                if (oneHadPending)        
                    Thread.Sleep(1);    // only sleep long when there is nothing left to do
                else
                    Thread.Sleep(50);
            }
        }
    }
}
