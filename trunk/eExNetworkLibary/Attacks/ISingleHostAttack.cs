using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.Attacks
{
    /// <summary>
    /// Provides an interface for all kinds of attacks against single hosts.
    /// 
    /// When implementing such an attack, please use this interface for enhanced functionality, such as network map integration. 
    /// </summary>
    public interface ISingleHostAttack
    {
        /// <summary>
        /// Adds the given IPAddress to the attacks victim list, so it will be attacked.
        /// </summary>
        /// <param name="ipaVictim">The IPAddress to attack</param>
        void Attack(IPAddress ipaVictim);

        /// <summary>
        /// Removes the given IPAddress from the victim list, so attacking will be stopped immediately. 
        /// </summary>
        /// <param name="ipaVictim">The IPAddress to remove from the victim list</param>
        void StopAttack(IPAddress ipaVictim);

        /// <summary>
        /// Stops all attacks to all victims immediately and removes all victims from the victim list.
        /// </summary>
        void StopAttacks();

        /// <summary>
        /// Pauses the attack until ResumeAttack() is called.
        /// </summary>
        void PauseAttack();

        /// <summary>
        /// Resumes the attack which was suspended when PauseAttack() was called.
        /// </summary>
        void ResumeAttack();

        /// <summary>
        /// Returns the name of this attack.
        /// </summary>
        string Name { get; }
    }
}
