// This source file is part of the eEx Network Library Management Layer (NLML)
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary;

namespace eExNLML.IO
{
    public static class ConfigurationParser
    {
        /// <summary>
        /// Converts an array of name value items to an array of ip addresses
        /// </summary>
        /// <param name="strString">The name value item which should be converted</param>
        /// <returns>An array of ip addresses</returns>
        public static IPAddress[] ConvertToIPAddress(NameValueItem[] strString)
        {
            IPAddress[] ipa = new IPAddress[strString.Length];

            for (int iC1 = 0; iC1 < strString.Length; iC1++)
            {
                ipa[iC1] = IPAddress.Parse(strString[iC1].Value);
            }

            return ipa;
        }

        /// <summary>
        /// Converts an array of name value items to an array of subnetmasks
        /// </summary>
        /// <param name="strString">The name value item which should be converted</param>
        /// <returns>An array of subnetmasks</returns>
        public static Subnetmask[] ConvertToSubnetmask(NameValueItem[] strString)
        {
            Subnetmask[] sam = new Subnetmask[strString.Length];

            for (int iC1 = 0; iC1 < strString.Length; iC1++)
            {
                sam[iC1] = Subnetmask.Parse(strString[iC1].Value);
            }

            return sam;
        }

        /// <summary>
        /// Converts an array of name value items to an array of MAC addresses
        /// </summary>
        /// <param name="strString">The name value item which should be converted</param>
        /// <returns>An array of MAC addresses</returns>
        public static MACAddress[] ConvertToMACAddress(NameValueItem[] strString)
        {
            MACAddress[] mac = new MACAddress[strString.Length];

            for (int iC1 = 0; iC1 < strString.Length; iC1++)
            {
                mac[iC1] = MACAddress.Parse(strString[iC1].Value);
            }

            return mac;
        }

        /// <summary>
        /// Converts an array of name value items to an array of integers
        /// </summary>
        /// <param name="strString">The name value item which should be converted</param>
        /// <returns>An array of integers</returns>
        public static int[] ConvertToInt(NameValueItem[] strString)
        {
            int[] values = new int[strString.Length];

            for (int iC1 = 0; iC1 < strString.Length; iC1++)
            {
                values[iC1] = Int32.Parse(strString[iC1].Value);
            }

            return values;
        }

        /// <summary>
        /// Converts an array of name value items to an array of doubles
        /// </summary>
        /// <param name="strString">The name value item which should be converted</param>
        /// <returns>An array of doubles</returns>
        public static double[] ConvertToDouble(NameValueItem[] strString)
        {
            double[] values = new double[strString.Length];

            for (int iC1 = 0; iC1 < strString.Length; iC1++)
            {
                values[iC1] = Double.Parse(strString[iC1].Value);
            }

            return values;
        }

        /// <summary>
        /// Converts an array of name value items to an array of strings
        /// </summary>
        /// <param name="strString">The name value item which should be converted</param>
        /// <returns>An array of strings</returns>
        public static string[] ConvertToString(NameValueItem[] strString)
        {
            string[] values = new string[strString.Length];

            for (int iC1 = 0; iC1 < strString.Length; iC1++)
            {
                values[iC1] = strString[iC1].Value;
            }

            return values;
        }

        /// <summary>
        /// Converts an array of name value items to an array of bools
        /// </summary>
        /// <param name="strString">The name value item which should be converted</param>
        /// <returns>An array of bools</returns>
        public static bool[] ConvertToBools(NameValueItem[] strString)
        {
            bool[] values = new bool[strString.Length];

            for (int iC1 = 0; iC1 < strString.Length; iC1++)
            {
                values[iC1] = Boolean.Parse(strString[iC1].Value);
            }

            return values;
        }

        /// <summary>
        /// Converts a IP address to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="ipa">The IP address which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, IPAddress ipa)
        {
            return new NameValueItem[] { new NameValueItem(strName, ipa.ToString()) };
        }

        /// <summary>
        /// Converts an array of IP addresses to name value items with the given name
        /// </summary>
        /// <param name="strName">The name of the name value items</param>
        /// <param name="aripa">The IP addresses which should be converted to the valuees of the name value items</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, IPAddress[] aripa)
        {
            NameValueItem[] nvi = new NameValueItem[aripa.Length];

            for (int iC1 = 0; iC1 < nvi.Length; iC1++)
            {
                nvi[iC1] = new NameValueItem(strName, aripa[iC1].ToString());
            }

            return nvi;
        }

        /// <summary>
        /// Converts a subnetmask to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="smMask">The subnetmask which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, Subnetmask smMask)
        {
            return new NameValueItem[] { new NameValueItem(strName, smMask.ToString()) };
        }

        /// <summary>
        /// Converts an array of subnetmasks to name value items with the given name
        /// </summary>
        /// <param name="strName">The name of the name value items</param>
        /// <param name="arMasks">The subnetmasks which should be converted to the valuees of the name value items</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, Subnetmask[] arMasks)
        {
            NameValueItem[] nvi = new NameValueItem[arMasks.Length];

            for (int iC1 = 0; iC1 < nvi.Length; iC1++)
            {
                nvi[iC1] = new NameValueItem(strName, arMasks[iC1].ToString());
            }

            return nvi;
        }

        /// <summary>
        /// Converts a MAC address to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="maAddress">The MAC address which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, MACAddress maAddress)
        {
            return new NameValueItem[] { new NameValueItem(strName, maAddress.ToString()) };
        }

        /// <summary>
        /// Converts an array of MAC addresses to name value items with the given name
        /// </summary>
        /// <param name="strName">The name of the name value items</param>
        /// <param name="arAddresses">The MAC addresses which should be converted to the valuees of the name value items</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, MACAddress[] arAddresses)
        {
            NameValueItem[] nvi = new NameValueItem[arAddresses.Length];

            for (int iC1 = 0; iC1 < nvi.Length; iC1++)
            {
                nvi[iC1] = new NameValueItem(strName, arAddresses[iC1].ToString());
            }

            return nvi;
        }

        /// <summary>
        /// Converts an integer to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="iValue">The integer which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, int iValue)
        {
            return new NameValueItem[] { new NameValueItem(strName, iValue.ToString()) };
        }

        /// <summary>
        /// Converts a double to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="dValue">The double which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, double dValue)
        {
            return new NameValueItem[] { new NameValueItem(strName, dValue.ToString()) };
        }

        /// <summary>
        /// Converts a string to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="strValue">The string which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, string strValue)
        {
            return new NameValueItem[] { new NameValueItem(strName, strValue) };
        }

        /// <summary>
        /// Converts a bool to a name value item with the given name
        /// </summary>
        /// <param name="strName">The name of the name value item</param>
        /// <param name="bValue">The bool which should be converted to the value of the name value item</param>
        /// <returns>An array of name value items which represents the given parameters</returns>
        public static NameValueItem[] ConvertToNameValueItems(string strName, bool bValue)
        {
            return new NameValueItem[] { new NameValueItem(strName, bValue.ToString()) };
        }
    }
}
