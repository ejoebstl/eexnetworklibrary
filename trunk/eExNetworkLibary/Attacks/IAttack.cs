using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Attacks
{
    public interface IAttack
    {      
        /// <summary>
        /// Returns the name of this attack
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Pauses the attack until ResumeAttack() is called.
        /// </summary>
        void PauseAttack();

        /// <summary>
        /// Resumes the attack which was suspended ba a previous call to PauseAttack().
        /// </summary>
        void ResumeAttack();
    }
}
