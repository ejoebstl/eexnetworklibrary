using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This class represents the base for a simulator filled with simulator chain items. 
    /// </summary>
    public abstract class Simulator : TrafficHandler, ITrafficSimulatorChainItem
    {
        private ITrafficSimulatorChainItem tsmiRoot;

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public Simulator()
        {
            this.tsmiRoot = AddSimulatorChainItems();
        }

        /// <summary>
        /// This is a method which is responsible to add simulator items in a derived class. 
        /// These items should be chained together and will then be handled like the items of a linked list. 
        /// Do not forget to link the Next propertie of the last item to this class. 
        /// The class Simulator will call the Start method of each given item. 
        /// <example><code>
        /// //Code snippet from WAN Emulator
        /// protected ITrafficSimulatorChainItem AddSimulatorChainItems();
        /// {
        ///     // create traffic simulator chain items
        ///     scSpeedConstrainer = new SpeedConstrainer();
        ///     pdPacketDropper = new PacketDropper();
        ///     pdPacketDuplicator = new PacketDuplicator();
        ///     pdPacketReorderer = new PacketReorderer();
        ///     bfByteFlipper = new ByteFlipper();
        ///     tjTimeJitter = new DelayJitter();
        /// 
        ///     // link the items together like they were a linked list
        ///     scSpeedConstrainer.Next = pdPacketDropper;
        ///     pdPacketDropper.Next = pdPacketDuplicator;
        ///     pdPacketDuplicator.Next = pdPacketReorderer;
        ///     pdPacketReorderer.Next = bfByteFlipper;
        ///     bfByteFlipper.Next = tjTimeJitter;
        ///     
        ///     // assign the simulator itself as the last item of the chain
        ///     tjTimeJitter.Next = this;
        /// 
        ///     // return the first item of the chain
        ///     return scSpeedConstrainer;
        /// }
        /// </code></example>
        /// </summary>
        /// <returns>The root of a linked chain of Traffic Simulator Modify Items</returns>
        protected abstract ITrafficSimulatorChainItem AddSimulatorChainItems();
  
        /// <summary>
        /// Pushes the frame forward to the first item in this simulators item chain
        /// </summary>
        /// <param name="fInputFrame">The frame to push</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            tsmiRoot.Push(fInputFrame);
        }

        /// <summary>
        /// Pushes the frame to the next given traffic handler.
        /// </summary>
        /// <param name="f">The frame to push</param>
        public void Push(Frame f)
        {
            NotifyNext(f);
        }

        /// <summary>
        /// Stops all simulation items and the handler itself.
        /// </summary>
        public override void Stop()
        {
            StopRecourse(tsmiRoot);
            base.Stop();
        }

        /// <summary>
        /// Does nothing. 
        /// </summary>
        public override void Cleanup()
        {

        }

        /// <summary>
        /// Starts all simulation items.
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (tsmiRoot == null)
            {
                this.tsmiRoot = this;
            }
            else
            {
                StartRecourse(tsmiRoot);
            }
        }

        private void StopRecourse(ITrafficSimulatorChainItem tsmi)
        {
            if (tsmi != null && tsmi != this)
            {
                tsmi.Stop();
                StopRecourse(tsmi.Next);
            }
        }

        private void StartRecourse(ITrafficSimulatorChainItem tsmi)
        {
            if (tsmi != null && tsmi != this)
            {
                tsmi.Start();
                StartRecourse(tsmi.Next);
            }
        }

        /// <summary>
        /// returns null. 
        /// </summary>
        public ITrafficSimulatorChainItem Next
        {
            get { return null; }
        }
    }

    /// <summary>
    /// This class provides a base for all traffic simulator items.
    /// </summary>
    public abstract class TrafficSimulatorModificationItem : ITrafficSimulatorChainItem
    {
        /// <summary>
        /// The next item in the simulator chain.
        /// </summary>
        protected ITrafficSimulatorChainItem tscNext;

        /// <summary>
        /// Gets or sets the next item in the simulator chain.
        /// </summary>
        public ITrafficSimulatorChainItem Next
        {
            get { return tscNext; }
            set
            {
                tscNext = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Applies the effect of this simulator chain item to the given frame.
        /// </summary>
        /// <param name="f">The input frame</param>
        public abstract void Push(Frame f);

        /// <summary>
        /// Starts this simulator item.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops this simulator item. 
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Is invoked when a property of this simulator item is changed.
        /// </summary>
        public event EventHandler PropertyChanged;

        /// <summary>
        /// Invokes the PropertyChanged event
        /// </summary>
        protected void InvokePropertyChanged()
        {
            InvokeExternalAsync(PropertyChanged, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes a delegate with the given params on an external target
        /// </summary>
        /// <param name="d">The delegate to invoke</param>
        /// <param name="param">The invokation params</param>
        protected void InvokeExternal(Delegate d, object param)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).Invoke(dDelgate, new object[] { this, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(this, param);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes a delegate with the given params asynchronously on an external target
        /// </summary>
        /// <param name="d">The delegate to invoke</param>
        /// <param name="param">The invokation params</param>
        protected void InvokeExternalAsync(Delegate d, object param)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { this, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(this, param);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Provides the basic interface which all simulator items must implement.
    /// </summary>
    public interface ITrafficSimulatorChainItem
    { 
        /// <summary>
        /// Gets the next item in the simulator chain
        /// </summary>
        ITrafficSimulatorChainItem Next { get;}
        /// <summary>
        /// Pushes a frame to this simulator item
        /// </summary>
        /// <param name="f">The frame to push</param>
        void Push(Frame f);
        /// <summary>
        /// Starts this simulator item
        /// </summary>
        void Start();
        /// <summary>
        /// Stops this simulator item
        /// </summary>
        void Stop();
    }
}
