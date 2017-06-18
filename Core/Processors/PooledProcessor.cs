using Core.Data;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Core.Processors
{
    public class PooledProcessor : IMessageProcessor
    {
        protected List<Connection> ActiveConnections = new List<Connection>();

        protected int MaxConnections = -1;

        protected bool DestoryOnEmpty = true;

        public event EventHandler ProcessSetup = null;

        protected T GetConStateData<T>(Connection user) where T:class
        {
            T d = user.GetMesssageProcessorTag<T>();
            if (d == null)
            {
                string name = typeof(T).Name;

                d = user.GetMesssageProcessorTag(name) as T;
                if (d == null)
                {
                    d = Activator.CreateInstance<T>();
                    user.SetMessageProcessorTag(name, d);
                }
                else
                    user.SetMessageProcessorTag(name);
            }

            return d;
        }

        public virtual void Setup()
        {
            ProcessSetup?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool Empty()
        {
            lock (ActiveConnections)
                return ActiveConnections.Count == 0;
        }

        public virtual int Count()
        {
            lock (ActiveConnections)
                return ActiveConnections.Count;
        }

        public virtual bool Full()
        {
            if (MaxConnections < 0)
                return false;

            lock (ActiveConnections)
                return ActiveConnections.Count >= MaxConnections;
        }

        public virtual void ProcessAccept(Connection user)
        {
        }

        public virtual void ProcessDisconnect(Connection user)
        {
            ProcessorDetatch(user);
        }

        public virtual void ProcessInbound(string message, Connection user)
        {
        }

        public virtual void ProcessorAttach(Connection user)
        {
            lock (ActiveConnections)
                ActiveConnections.Add(user);
        }

        public virtual void ProcessorDetatch(Connection user)
        {
            lock (ActiveConnections)
                ActiveConnections.Remove(user);
        }

        /// <summary>
        /// Processes any inbound messages from all connected users.
        /// </summary>
        /// <returns>True if the processor is empty and should be deleted</returns>
        public virtual bool ProcessAllConnections()
        {
            Connection[] users = new Connection[0];
            lock (ActiveConnections)
                users = ActiveConnections.ToArray();

            if (users.Length == 0 && DestoryOnEmpty)
                return true;

            foreach (var user in users)
                ProcessConnection(user);

            return false;
        }

        protected virtual void ProcessConnection(Connection user)
        { 
            if (!user.HasPendingInbound())
                return;

            string msg = user.PopInboundMessage();
            while (msg != string.Empty)
            {
                if (ProcessUserMessage(user, msg))
                    msg = user.PopInboundMessage();
                else
                    msg = string.Empty;
            }
        }

        protected virtual bool ProcessUserMessage(Connection user, string msg)
        {
            return false;
        }

        protected virtual void SendUserFileMessage(Connection user, string path)
        {
            MsgUtils.SendUserFileMessage(user, path);
        }

        protected virtual void SendUserFileMessage(Connection user, string path, Dictionary<string,string> repacements)
        {
            MsgUtils.SendUserFileMessage(user, path, repacements);
        }
        protected virtual void SendUserFileMessage(Connection user, string path, string key, string value)
        {
            MsgUtils.SendUserFileMessage(user, path, key, value);
        }
    }

    internal class ProcessorPoolData
    {
        public Type T = typeof(PooledProcessor);
        public bool BallanceAllocate = false;
        public int MaxProcessors = 1;

        public bool ThreadUpdates = false;

        public EventHandler SetupEvent = null;

        public List<PooledProcessor> Processors = new List<PooledProcessor>();
        public List<Thread> ProcessorThreads = new List<Thread>();
    }

    internal class ThreadedPoolProcessor
    {
        public ProcessorPoolData Pool = null;
        public PooledProcessor Processor = null;
        public Thread ProcessorThread = null;

        public bool First = false;

        public event EventHandler KillMe = null;

        private int cycles = 0;

        public int HospiceCycles = 1000;    // be empty this many cycles before we ask to be killed

        public void Start()
        {
            ProcessorThread = new Thread(new ThreadStart(Work));
            ProcessorThread.Start();
        }

        public void Work()
        {
            while(true)
            {
                bool wantToDie = false;
                lock (Processor)
                    wantToDie = Processor.ProcessAllConnections();

                if (!First && wantToDie)
                {
                    cycles++;
                    if (cycles > HospiceCycles)
                        break;
                }
                else
                    Thread.Sleep(20);

                cycles = 0;
            }

            KillMe?.Invoke(this, EventArgs.Empty);
        }
    }

    public static class ProcessorPool
    {
        private static Dictionary<string, ProcessorPoolData> ProcessorPools = new Dictionary<string, Core.Processors.ProcessorPoolData>();

        private static List<ProcessorPoolData> NonThreadedPools = new List<ProcessorPoolData>();
        private static List<ThreadedPoolProcessor> ProcessingThreads = new List<ThreadedPoolProcessor>();

        public static bool SetupProcessorPool(string name, Type t, int maxCount, bool ballance, bool useThreads, EventHandler setupEvent)
        {
            if (!t.IsSubclassOf(typeof(PooledProcessor)) || maxCount < 1 || ProcessorPools.ContainsKey(name)) 
                return false;

            ProcessorPoolData data = new ProcessorPoolData();
            data.T = t;
            data.MaxProcessors = maxCount;
            data.BallanceAllocate = ballance;
            data.ThreadUpdates = useThreads;
            data.SetupEvent = setupEvent;

            // always add the first one
            AddProcessorToPool(data, true);

            ProcessorPools.Add(name,data);
            if (!useThreads)
                NonThreadedPools.Add(data);

            return true;
        }

        private static PooledProcessor AddProcessorToPool(ProcessorPoolData pool, bool first)
        {
            var proc = Activator.CreateInstance(pool.T) as PooledProcessor;

            if (pool.SetupEvent != null)
                proc.ProcessSetup += pool.SetupEvent;

            lock (pool.Processors)
                pool.Processors.Add(proc);

            pool.SetupEvent?.Invoke(proc, EventArgs.Empty);

            if (pool.ThreadUpdates)
            {
                ThreadedPoolProcessor tp = new ThreadedPoolProcessor();
                tp.First = first;
                tp.Processor = proc;
                tp.Pool = pool;
                tp.KillMe += ThreadedPoolProcessorKillMe;
                tp.Start();
                lock (ProcessingThreads)
                    ProcessingThreads.Add(tp);
            }

            return proc;
        }

        private static void ThreadedPoolProcessorKillMe(object sender, EventArgs e)
        {
            ThreadedPoolProcessor tp = sender as ThreadedPoolProcessor;
            if (tp == null)
                return;

            lock (ProcessingThreads)
                ProcessingThreads.Remove(tp);

            lock(tp.Pool)
            {
                lock (tp.Pool.Processors)
                    tp.Pool.Processors.Remove(tp.Processor);
            }
        }

        public static IMessageProcessor GetProcessor(string name, Connection user)
        {
            lock(ProcessorPools)
            {
                if (!ProcessorPools.ContainsKey(name))
                    return null;

                var pool = ProcessorPools[name];

                lock (pool)
                {
                    if (!pool.BallanceAllocate)
                    {
                        foreach(var p in pool.Processors)
                        {
                            lock(p)
                            {
                                if (!p.Full())
                                    return p;
                            }
                        }

                        if (pool.Processors.Count == pool.MaxProcessors)
                            return null; // we are 100 % full

                        // spin up a new one
                        return AddProcessorToPool(pool, false);
                    }
                    else
                    {
                        // find the one with the smallest count
                        PooledProcessor smallP = null;
                        if (pool.Processors.Count < pool.MaxProcessors)
                        {
                            foreach (var p in pool.Processors)
                            {
                                lock (p)
                                {
                                    if (smallP == null || smallP.Count() > p.Count())
                                        smallP = p;
                                }
                            }
                            if ((smallP == null || smallP.Full()) && pool.Processors.Count == pool.MaxProcessors)
                                return null; // we are 100%full

                            return smallP;
                        }

                        return AddProcessorToPool(pool, false);
                    }
                }
            }
        }

        public static void UpdateProcessorsPools()
        {
            lock(NonThreadedPools)
            {
                foreach( var pool in NonThreadedPools)
                {
                    bool first = true;
                    foreach(var p in pool.Processors.ToArray())
                    {
                        if (p.ProcessAllConnections() && !first)
                            pool.Processors.Remove(p);
                        first = false;
                    }
                }
            }
        }
    }
}
