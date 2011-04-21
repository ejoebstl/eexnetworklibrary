using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.HTTP;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP
{
    public abstract class HTTPStreamModifierAction : HTTPStreamModifierCondition
    {
        /// <summary>
        /// Applys an actions to this HTTP Message
        /// </summary>
        /// <param name="httpMessage">The HTTP message to edit</param>
        /// <returns>The edited HTTP message</returns>
        public abstract HTTPMessage ApplyAction(HTTPMessage httpMessage);
    }
}
