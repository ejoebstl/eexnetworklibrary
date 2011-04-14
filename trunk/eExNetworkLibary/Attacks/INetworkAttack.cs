using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.Attacks
{    
    /// <summary>
    /// Provides an interface for all kinds of attacks against whole networks.
    /// 
    /// When implementing such an attack, please use this interface for enhanced functionality, such as network map integration. 
    /// </summary>
    public interface INetworkAttack : IAttack
    {        
        /// <summary>
        /// Adds the given IPAddress to the attacks victim list, so it will be attacked.
        /// </summary>
        /// <param name="ipaVictim">The network address of the network to attack</param>
        void Attack(IPAddress ipaVictim);

        /// <summary>
        /// Removes the given IPAddress from the victim list, so attacking will be stopped immediately. 
        /// </summary>
        /// <param name="ipaVictim">The network address of the network to remove from the victim list</param>
        void StopAttack(IPAddress ipaVictim);
    }
}
