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
        const string XMLProtocolVersion = "protocolVersion";
        const int LocalVersion = 0;

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
                    byte[] bContent; 

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

                    BinaryReader sReader = new BinaryReader(sContent);

                    bContent = sReader.ReadBytes((int)sContent.Length);

                    sReader.Close();
                    sContent.Close();

                    plFile.Add(new PlugInFile(strFileName, bContent, webRequest.MediaType));
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
            string XMLSmallIcon = "smallIcon";
            string XMLLargeIcon = "largeIcon";
            string XMLRating = "rating";
            string XMLDownloads = "downloads";
            string XMLType = "type";
            string XMLFile = "file";
            string XMLKey = "key";
            string XMLDependency = "dependency";

            string strName = "";
            string strKey = "";
            string strDescription = "";
            string strAuthor = "";
            string strWebLink = "";
            string strVersion = "";
            string strType = "";
            string strSmallIcon = "";
            string strLargeIcon = "";

            string strResourceName = "";
            string strDllName = "";
            string strResourceLink = "";
            DependencyType tType;

            int iRating = 0;
            int iDownloads = 0;

            List<string> lFiles = new List<string>();
            List<PlugInDependency> lDependencies = new List<PlugInDependency>();
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

            //StreamReader strReader = new StreamReader(sStream);
            //string strOut = strReader.ReadToEnd();

            XmlReader xmlReader = XmlReader.Create(sStream, xmlSettings);


            try
            {
                while (xmlReader.Read())
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
                            strSmallIcon = "";
                            strLargeIcon = "";
                            iRating = 0;

                            strKey = xmlReader.GetAttribute(XMLKey);

                            lFiles.Clear();
                            lDependencies.Clear();
                        }
                        if (xmlReader.Name == XMLName)
                        {
                            xmlReader.Read();
                            strName = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLDescription)
                        {
                            xmlReader.Read();
                            strDescription = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLAuthor)
                        {
                            xmlReader.Read();
                            strAuthor = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLWebLink)
                        {
                            xmlReader.Read();
                            strWebLink = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLVersion)
                        {
                            xmlReader.Read();
                            strVersion = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLType)
                        {
                            xmlReader.Read();
                            strType = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLFile)
                        {
                            xmlReader.Read();
                            lFiles.Add(xmlReader.Value);
                        }
                        if (xmlReader.Name == XMLRating)
                        {
                            xmlReader.Read();
                            iRating = Int32.Parse(xmlReader.Value);
                        }
                        if (xmlReader.Name == XMLDownloads)
                        {
                            xmlReader.Read();
                            iDownloads = Int32.Parse(xmlReader.Value);
                        }
                        if (xmlReader.Name == XMLSmallIcon)
                        {
                            xmlReader.Read();
                            strSmallIcon = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLLargeIcon)
                        {
                            xmlReader.Read();
                            strLargeIcon = xmlReader.Value;
                        }
                        if (xmlReader.Name == XMLError)
                        {
                            string strError = xmlReader.GetAttribute("error");
                            xmlReader.Read();
                            xmlReader.Read();
                            string strMessage = xmlReader.Value;

                            throw new Exception("The NLML repository server responed with error " + strError + ": " + strMessage);
                        }
                        if (xmlReader.Name == XMLDependency)
                        {
                            if (xmlReader["type"] == "e")
                            {
                                tType = DependencyType.Extension;
                            }
                            else
                            {
                                tType = DependencyType.Library;
                            }

                            xmlReader.Read(); xmlReader.Read();

                            strResourceName = xmlReader.Value;

                            xmlReader.Read(); xmlReader.Read(); xmlReader.Read();

                            strDllName = xmlReader.Value;

                            xmlReader.Read(); xmlReader.Read(); xmlReader.Read();

                            strResourceLink = xmlReader.Value;


                            lDependencies.Add(new PlugInDependency(strResourceName, strDllName, strResourceLink, tType));
                        }

                        if (xmlReader.Name == XMLPluginList)
                        {
                            int iRemoteVersion = Int32.Parse(xmlReader.GetAttribute(XMLProtocolVersion));
                            if (iRemoteVersion != LocalVersion)
                            {
                                throw new InvalidOperationException("The protocol version indicated in the server response (" + iRemoteVersion.ToString()
                                    + ") is incompatible with the local version (" + LocalVersion.ToString() + ").");
                            }
                        }
                    }
                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        if (xmlReader.Name == XMLPlugIn)
                        {
                            lPlugInDescriptions.Add(
                                new PlugInDescription(strAuthor,
                                    strDescription,
                                    strName,
                                    strKey,
                                    strType,
                                    strWebLink,
                                    strVersion,
                                    iRating, 
                                    iDownloads,
                                    lFiles.ToArray(), 
                                    strLargeIcon,
                                    strSmallIcon,
                                    lDependencies.ToArray()));
                        }
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
                wWriter.WriteAttributeString(XMLProtocolVersion, LocalVersion.ToString());
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
