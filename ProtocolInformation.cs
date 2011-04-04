using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace eExNetworkLibrary
{
    [Obsolete("Do not use this. Each traffic handler has to parse layer 7 data for itself", true)]
    class ProtocolInformation
    {
        private ConnectionType cConnectionType;
        private int iPort;
        private ConstructorInfo cFrameConstructor;
        private string strProtocolName;

        public ConnectionType ConnectionType
        {
            get { return cConnectionType; }
            set { cConnectionType = value; }
        }

        public int Port
        {
            get { return iPort; }
            set { iPort = value; }
        }

        public string ProtocolName
        {
            get { return strProtocolName; }
            set { strProtocolName = value; }
        }

        public ConstructorInfo FrameConstructor
        {
            get { return cFrameConstructor; }
        }

        public ProtocolInformation(ConnectionType cConnectionType, int iPort, Type cProtocolFrameType) : this(cConnectionType, iPort, cProtocolFrameType, "") { }

        public ProtocolInformation(ConnectionType cConnectionType, int iPort, Type cProtocolFrameType, string strProtocolName)
        {
            this.cConnectionType = cConnectionType;
            this.iPort = iPort;
            this.cFrameConstructor = cProtocolFrameType.GetConstructor(new Type[]{typeof(byte[])});
            this.strProtocolName = strProtocolName;
        }

        public Frame CreateFrame(byte[] bData)
        {
            return (Frame)cFrameConstructor.Invoke(new object[] { bData });
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ProtocolInformation))
            {
                ProtocolInformation p = (ProtocolInformation)obj;

                if (p.Port == this.Port && p.ConnectionType == this.ConnectionType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    enum ConnectionType
    {
        TCP = 0,
        UDP = 1
    }
}
