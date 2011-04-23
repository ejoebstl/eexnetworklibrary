using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using System.Xml;
using System.Net;

namespace eExNLML.IO
{
    /// <summary>
    /// This class builds the base for all configuration writers
    /// </summary>
    public abstract class HandlerConfigurationWriter
    {
        TrafficHandler hHandler;
        const string XMLConfigurationProperty = "configurationProperty";

        /// <summary>
        /// Gets the traffic handler which is associated with this configuration writer
        /// </summary>
        public TrafficHandler TrafficHandlerToSave
        {
            get { return hHandler; }
        }

        /// <summary>
        /// Creates a new instance of this class associated with the given traffic handler
        /// </summary>
        /// <param name="h">The traffic handler to associate with this configuration writer</param>
        public HandlerConfigurationWriter(TrafficHandler h)
        {
            this.hHandler = h;
        }
        
        /// <summary>
        /// Writes the configuration to the given XmlWriter
        /// </summary>
        /// <param name="xmw">The XmlWriter to write the configuration to</param>
        /// <param name="eEnviornment">The environment to associate with the given configuration</param>
        public void SaveConfiguration(XmlWriter xmw, IEnvironment eEnviornment)
        {
            List<NameValueItem> lNvi = new List<NameValueItem>();
            AddConfiguration(lNvi, eEnviornment);
            NameValueItem[] nviItems = lNvi.ToArray();

            WriteNameValueItem(xmw, nviItems);
        }

        private void WriteNameValueItem(XmlWriter xmw, NameValueItem[] nviItems)
        {
            foreach (NameValueItem nvi in nviItems)
            {
                xmw.WriteStartElement(XMLConfigurationProperty);
                xmw.WriteStartAttribute("name");
                xmw.WriteString(nvi.Name);
                xmw.WriteEndAttribute();
                xmw.WriteString(nvi.Value);
                WriteNameValueItem(xmw, nvi.ChildItems);
                xmw.WriteEndElement();
            }
        }

        /// <summary>
        /// This method must be overriden by all derived classes. It has to add it's own configuration items to the given list.
        /// </summary>
        /// <param name="lNameValueItems">The list to add all configuration items to</param>
        /// <param name="eEnviornment">The environment</param>
        protected abstract void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment);

        /// <summary>
        /// Converts a IP address to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="ipa">The IP address which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, IPAddress ipa)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, ipa);
        }

        /// <summary>
        /// Converts an array of IP addresses to name value items with the given name
        /// </summary>
        /// <param name="strName">The name of the name value items</param>
        /// <param name="aripa">The IP addresses which should be converted to the valuees of the name value items</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, IPAddress[] aripa)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, aripa);
        }

        /// <summary>
        /// Converts a subnetmask to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="smMask">The subnetmask which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, Subnetmask smMask)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, smMask);
        }

        /// <summary>
        /// Converts an array of subnetmasks to name value items with the given name
        /// </summary>
        /// <param name="strName">The name of the name value items</param>
        /// <param name="arMasks">The subnetmasks which should be converted to the valuees of the name value items</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, Subnetmask[] arMasks)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, arMasks);
        }

        /// <summary>
        /// Converts a MAC address to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="maAddress">The MAC address which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, MACAddress maAddress)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, maAddress);
        }

        /// <summary>
        /// Converts an array of MAC addresses to name value items with the given name
        /// </summary>
        /// <param name="strName">The name of the name value items</param>
        /// <param name="arAddresses">The MAC addresses which should be converted to the valuees of the name value items</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, MACAddress[] arAddresses)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, arAddresses);
        }

        /// <summary>
        /// Converts an integer to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="iValue">The integer which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, int iValue)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, iValue);
        }

        /// <summary>
        /// Converts a double to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="dValue">The double which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, double dValue)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, dValue);
        }

        /// <summary>
        /// Converts a string to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="strValue">The string which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, string strValue)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, strValue);
        }

        /// <summary>
        /// Converts a bool to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="bValue">The bool which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        protected NameValueItem[] ConvertToNameValueItems(string strName, bool bValue)
        {
            return ConfigurationParser.ConvertToNameValueItems(strName, bValue);
        }
    }
}
