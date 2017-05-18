﻿using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class PooledProcessor : IMessageProcessor
    {
        protected List<Connection> ActiveConnections = new List<Connection>();

        protected int MaxConnections = -1;

        protected bool DestoryOnEmpty = true;

        public event EventHandler ProcessSetup = null;

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

        public virtual void ProcessAccept(Connection con)
        {
        }

        public virtual void ProcessDisconnect(Connection con)
        {
            ProcessorDetatch(con);
        }

        public virtual void ProcessInbound(string message, Connection con)
        {
            throw new NotImplementedException();
        }

        public virtual void ProcessorAttach(Connection con)
        {
            lock (ActiveConnections)
                ActiveConnections.Add(con);
        }

        public virtual void ProcessorDetatch(Connection con)
        {
            lock (ActiveConnections)
                ActiveConnections.Remove(con);
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

        protected virtual void ProcessConnection(Connection con)
        {

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
        private static Dictionary<string, ProcessorPoolData> ProcessorPools = new Dictionary<string, Core.ProcessorPoolData>();

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

        public static IMessageProcessor GetProcessor(string name, Connection con)
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
