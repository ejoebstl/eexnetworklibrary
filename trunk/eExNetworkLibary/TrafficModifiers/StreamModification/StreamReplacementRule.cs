using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification
{
    public class StreamReplacementRule
    {
        byte[] bDataToFind;
        byte[] bDataToReplace;

        public byte[] DataToFind
        {
            get { return bDataToFind; }
        }

        public byte[] DataToReplace
        {
            get { return bDataToReplace; }
        }

        public StreamReplacementRule(byte[] bDataToFind, byte[] bDataToReplace)
        {
            this.bDataToReplace = bDataToReplace;
            this.bDataToFind = bDataToFind;
        }
    }

}
