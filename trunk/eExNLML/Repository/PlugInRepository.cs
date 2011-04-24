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
using System.IO;
using System.Xml;
using System.Reflection;
using System.Net;
using System.Xml.Schema;

namespace eExNLML.Repository
{
    public class PlugInRepository
    {
        Uri uriRepository;
        ICredentials credAuthentication; 
        string XMLComment;
        const string XMLPluginList = "pluginList";
        const string XMLError = "error";
        const string XMLPlugIn = "plugin";
        const string XMLRequest = "request";

        public PlugInRepository(Uri uriRepository)
            : this(uriRepository, null)
        {

        }

        public PlugInRepository(Uri uriRepository, ICredentials credAuthentication)
        {
            this.uriRepository = uriRepository;
            this.credAuthentication = credAuthentication;
            this.XMLComment = @"eEx NLML Plug-In Request, saved by NLML Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public PlugInDescription GetPlugIn(string strPlugInKey)
        {
            PlugInDescription[] arPlugIns = GetPlugIns(new string[] { strPlugInKey });
            if (arPlugIns.Length > 0)
            {
                return arPlugIns[0];
            }
            return null;
        }

        public PlugInDescription[] GetPlugIns(string[] strPlugInKeys)
        {
            PlugInDescription[] arPlugIns = new PlugInDescription[0];

            HttpWebRequest webRequest = CreateWebRequest(uriRepository);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/xml";

            Stream sStream = webRequest.GetRequestStream();

            try
            {
                WritePlugInRequest(strPlugInKeys, sStream);

                webRequest.ContentLength = sStream.Length;
            }
            finally
            {
                sStream.Close();
            }

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            try
            {
                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException("The server response included an error: " + (int)webResponse.StatusCode + " " + webResponse.StatusCode.ToString());
                }

                Stream sResponseStream = webResponse.GetResponseStream();

                arPlugIns = ReadPlugInsFromStream(sResponseStream);

                sResponseStream.Close();
            }
            finally
            {
                webResponse.Close();
            }

            return arPlugIns;
        }

        private HttpWebRequest CreateWebRequest(Uri strRequestTarget)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(strRequestTarget);

            if (credAuthentication != null)
            {
                webRequest.Credentials = credAuthentication;
            }

            webRequest.UserAgent = "eEx NLML Plugin System";
            return webRequest;
        }

        public PlugInDescription[] GetPlugIns()
        {
            return GetPlugIns(new string[0]);
        }

        public PlugInFile[] DownloadPlugIn(PlugInDescription plDescription)
        {
            List<PlugInFile> plFile = new List<PlugInFile>();

            foreach (string strFile in plDescription.Files)
            {
                HttpWebRequest webRequest = CreateWebRequest(new Uri(uriRepository, strFile));

                webRequest.Method = "GET";

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                try
                {
                    string strContentDispositionHeader = webResponse.GetResponseHeader("Content-Disposition");
                    string strFileName = "";
                    string strContent = ""; 

                    if (webResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException("The server response included an error: " + (int)webResponse.StatusCode + " " + webResponse.StatusCode.ToString());
                    }

                    if (strContentDispositionHeader != null && strContentDispositionHeader.Contains("filename="))
                    {
                        strFileName = strContentDispositionHeader.Substring(strContentDispositionHeader.IndexOf("filename=") + "filename=".Length);
                    }
                    else
                    {
                        throw new Exception("The name of the file to download was not given by the server (There was no Content-Disposition header).");
                    }

                    Stream sContent = webResponse.GetResponseStream();

                    StreamReader sReader = new StreamReader(sContent, Encoding.GetEncoding(webResponse.ContentEncoding));

                    strContent = sReader.ReadToEnd();

                    sReader.Close();
                    sContent.Close();

                    plFile.Add(new PlugInFile(strFileName, strContent));
                }
                finally
                {
                    webResponse.Close();
                }
            }

            return plFile.ToArray();
        }

        private PlugInDescription[] ReadPlugInsFromStream(Stream sStream)
        {
            string XMLName = "name";
            string XMLDescription = "description";
            string XMLAuthor = "author";
            string XMLWebLink = "weblink";
            string XMLVersion = "version";
            string XMLType = "type";
            string XMLFile = "file";
            string XMLKey = "key";

            string strName = "";
            string strKey = "";
            string strDescription = "";
            string strAuthor = "";
            string strWebLink = "";
            string strVersion = "";
            string strType = "";

            List<string> lFiles = new List<string>();
            List<PlugInDescription> lPlugInDescriptions = new List<PlugInDescription>();

            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            xmlSettings.CloseInput = true;
            xmlSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlSettings.IgnoreComments = true;
            xmlSettings.IgnoreWhitespace = true;
            xmlSettings.ProhibitDtd = true;

            Stream sSchema = File.Open(Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "NLMLPluginDescription.xsd"), FileMode.Open);
            XmlSchema xmlSchema = XmlSchema.Read(sSchema, new ValidationEventHandler(XmlSchema_Validate));
            sSchema.Close();

            xmlSettings.Schemas.Add(xmlSchema);
            xmlSettings.ValidationType = ValidationType.Schema;

            XmlReader xmlReader = XmlReader.Create(sStream, xmlSettings);

            try
            {
                while (!xmlReader.EOF)
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlReader.Name == XMLPlugIn)
                        {
                            strName = "";
                            strDescription = "";
                            strAuthor = "";
                            strWebLink = "";
                            strVersion = "";
                            strType = "";

                            strKey = xmlReader.GetAttribute(XMLKey);

                            lFiles.Clear();
                        }
                        if (xmlReader.Name == XMLName)
                            strName = xmlReader.Value;

                        if (xmlReader.Name == XMLDescription)
                            strDescription = xmlReader.Value;

                        if (xmlReader.Name == XMLAuthor)
                            strAuthor = xmlReader.Value;

                        if (xmlReader.Name == XMLWebLink)
                            strWebLink = xmlReader.Value;

                        if (xmlReader.Name == XMLVersion)
                            strVersion = xmlReader.Value;

                        if (xmlReader.Name == XMLType)
                            strType = xmlReader.Value;

                        if (xmlReader.Name == XMLFile)
                            lFiles.Add(xmlReader.Value);

                        if (xmlReader.Name == XMLError)
                        {
                            string strError = xmlReader.GetAttribute("error");
                            string strMessage = xmlReader.Value;

                            throw new Exception("The NLML repository server responed with an error. " + strError + ": " + strMessage);
                        }
                    }
                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        lPlugInDescriptions.Add(
                            new PlugInDescription(strAuthor, 
                                strDescription, 
                                strName, 
                                strKey, 
                                strType, 
                                strWebLink, 
                                strVersion, 
                                lFiles.ToArray()));
                    }
                }
            }
            finally
            {
                xmlReader.Close();
            }

            return lPlugInDescriptions.ToArray();
        }

        private void XmlSchema_Validate(object sender, ValidationEventArgs args)
        {

        }

        private void WritePlugInRequest(string[] strPlugInsToRequest, Stream sStream)
        {
            string XMLRequestedPlugIn = "requestedPlugIn";

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.CloseOutput = false;
            xmlSettings.Indent = true;
            xmlSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlSettings.Encoding = System.Text.UnicodeEncoding.Unicode;
            xmlSettings.OmitXmlDeclaration = false;
            XmlWriter wWriter = XmlWriter.Create(sStream);

            try
            {
                wWriter.WriteStartDocument();
                wWriter.WriteComment(XMLComment);

                wWriter.WriteStartElement(XMLRequest);
                foreach (string strPlugInToRequest in strPlugInsToRequest)
                {
                    wWriter.WriteStartElement(XMLRequestedPlugIn);
                    wWriter.WriteValue(strPlugInToRequest);
                    wWriter.WriteEndElement();
                }
                wWriter.WriteEndElement();

                wWriter.WriteEndDocument();
            }
            finally
            {
                wWriter.Close();
            }
        }
    }
}
