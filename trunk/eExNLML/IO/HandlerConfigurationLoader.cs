using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary;
using System.Xml;

namespace eExNLML.IO
{
    /// <summary>
    /// This class provides a base implementation for traffic handler configuration loaders
    /// </summary>
    public abstract class HandlerConfigurationLoader
    {
        TrafficHandler hHandler;
        const string XMLConfigurationProperty = "configurationProperty";

        /// <summary>
        /// Gets the traffic handler associated with this handler configuration loader
        /// </summary>
        public TrafficHandler TrafficHandlerToConfigure
        {
            get { return hHandler; }
        }

        /// <summary>
        /// Creates a new instance of this class associated with the given traffic handler
        /// </summary>
        /// <param name="h">The traffic handler to associate with this configruation loader</param>
        public HandlerConfigurationLoader(TrafficHandler h)
        {
            this.hHandler = h;
        }

        /// <summary>
        /// Loads the configuration from the given XmlReader
        /// </summary>
        /// <param name="xmw">The XmlReader to load the configuration for</param>
        /// <param name="eEnviornment">The environment to associate with the traffic handler</param>
        public void LoadConfiguration(XmlReader xmw, IEnvironment eEnviornment)
        {
            Dictionary<string, List<NameValueItem>> dictNameValues = new Dictionary<string, List<NameValueItem>>();
            Stack<NameValueItem> sItemStack = new Stack<NameValueItem>();
            NameValueItem nviCurrentItem = null;
            bool bBreak = false;

            string strName = null;
            string strValue = null;

            while (!bBreak)
            {
                switch (xmw.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xmw.Name == XMLConfigurationProperty)
                        {
                            if (strName != null)
                            {
                                nviCurrentItem = CreateNameValueItem(dictNameValues, sItemStack, nviCurrentItem, strName, strValue);
                            }

                            strName = xmw.GetAttribute("name");
                            strValue = ""; 
                        }
                        else
                        {
                            bBreak = true;
                        }
                        break;
                    case XmlNodeType.Text:
                        if (strName != null)
                        {
                            strValue = xmw.Value;

                            nviCurrentItem = CreateNameValueItem(dictNameValues, sItemStack, nviCurrentItem, strName, strValue);

                            strName = null;
                            strValue = null;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (xmw.Name == XMLConfigurationProperty)
                        {
                            if (strName != null)
                            {
                                //This is an empty item.
                                nviCurrentItem = CreateNameValueItem(dictNameValues, sItemStack, nviCurrentItem, strName, strValue);
                                strName = null;
                                strValue = null;
                            }
                            if (sItemStack.Count > 0)
                            {
                                nviCurrentItem = sItemStack.Pop();
                            }
                            else
                            {
                                nviCurrentItem = null;
                            }
                        }
                        break;
                }

                if (!bBreak && !xmw.Read())
                {
                    bBreak = true;
                }
            }

            Dictionary<string, NameValueItem[]> dictNameValueArray = new Dictionary<string, NameValueItem[]>();

            foreach (string str in dictNameValues.Keys)
            {
                dictNameValueArray.Add(str, dictNameValues[str].ToArray());
            }

            ParseConfiguration(dictNameValueArray, eEnviornment);
        }

        private static NameValueItem CreateNameValueItem(Dictionary<string, List<NameValueItem>> dictNameValues, Stack<NameValueItem> sItemStack, NameValueItem nviCurrentItem, string strName, string strValue)
        {
            if (nviCurrentItem == null)
            {
                nviCurrentItem = new NameValueItem(strName, strValue);

                if (!dictNameValues.ContainsKey(strName))
                {
                    dictNameValues.Add(strName, new List<NameValueItem>());
                }

                dictNameValues[strName].Add(nviCurrentItem);
            }
            else
            {
                NameValueItem nvi = new NameValueItem(strName, strValue);

                nviCurrentItem.AddChildItem(nvi);

                sItemStack.Push(nviCurrentItem);

                nviCurrentItem = nvi;
            }
            return nviCurrentItem;
        }

        /// <summary>
        /// This method must be overriden by any derived class. It must configure the given traffic handler according to the given name value configuration items.
        /// </summary>
        /// <param name="strNameValues">A dictionary filled with name value items which store the configuration to apply to your traffic handler</param>
        /// <param name="eEnviornment">The environment to associate with the traffic handler</param>
        protected abstract void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment);

        /// <summary>
        /// Converts an array of name value items to an array of ip addresses
        /// </summary>
        /// <param name="nviConfiguration">The name value item which should be converted</param>
        /// <returns>An array of ip addresses</returns>
        protected IPAddress[] ConvertToIPAddress(NameValueItem[] nviConfiguration)
        {
            return ConfigurationParser.ConvertToIPAddress(nviConfiguration);
        }

        /// <summary>
        /// Converts an array of name value items to an array of subnetmasks
        /// </summary>
        /// <param name="nviConfiguration">The name value item which should be converted</param>
        /// <returns>An array of subnetmasks</returns>
        protected Subnetmask[] ConvertToSubnetmask(NameValueItem[] nviConfiguration)
        {
            return ConfigurationParser.ConvertToSubnetmask(nviConfiguration);
        }

        /// <summary>
        /// Converts an array of name value items to an array of MAC addresses
        /// </summary>
        /// <param name="nviConfiguration">The name value item which should be converted</param>
        /// <returns>An array of MAC addresses</returns>
        protected MACAddress[] ConvertToMACAddress(NameValueItem[] nviConfiguration)
        {
            return ConfigurationParser.ConvertToMACAddress(nviConfiguration);
        }

        /// <summary>
        /// Converts an array of name value items to an array of integers
        /// </summary>
        /// <param name="nviConfiguration">The name value item which should be converted</param>
        /// <returns>An array of integers</returns>
        protected int[] ConvertToInt(NameValueItem[] nviConfiguration)
        {
            return ConfigurationParser.ConvertToInt(nviConfiguration);
        }

        /// <summary>
        /// Converts an array of name value items to an array of doubles
        /// </summary>
        /// <param name="nviConfiguration">The name value item which should be converted</param>
        /// <returns>An array of doubles</returns>
        protected double[] ConvertToDouble(NameValueItem[] nviConfiguration)
        {
            return ConfigurationParser.ConvertToDouble(nviConfiguration);
        }

        /// <summary>
        /// Converts an array of name value items to an array of strings
        /// </summary>
        /// <param name="nviConfiguration">The name value item which should be converted</param>
        /// <returns>An array of strings</returns>
        protected string[] ConvertToString(NameValueItem[] nviConfiguration)
        {
            return ConfigurationParser.ConvertToString(nviConfiguration);
        }

        /// <summary>
        /// Converts an array of name value items to an array of bools
        /// </summary>
        /// <param name="nviConfiguration">The name value item which should be converted</param>
        /// <returns>An array of bools</returns>
        protected bool[] ConvertToBools(NameValueItem[] nviConfiguration)
        {
            return ConfigurationParser.ConvertToBools(nviConfiguration);
        }
    }
}
