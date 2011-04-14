using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace eExNetworkLibrary.DNS
{
    /// <summary>
    /// This class represents a DNS frame
    /// </summary>
    public class DNSFrame : Frame
    {
        public static string DefaultFrameType { get { return FrameTypes.DNS; } }

        private short sIdentifier; // 2 byte;
        private bool bQRFlag;
        private DNSOptCode dnsOperationCode; // 4 bit
        private bool bAAFlag;
        private bool bTCFlag;
        private bool bRDFlag;
        private bool bRAFlag;
        // 3 bit zero
        private DNSResponseCode dnsResponseCode;// 4 bits

        private List<DNSQuestion> lQuestions;
        private List<DNSResourceRecord> lAnswers;
        private List<DNSResourceRecord> lAuthorotive;
        private List<DNSResourceRecord> lAdditional;

        #region List Aseccors

        /// <summary>
        /// Clears all question records from this instance
        /// </summary>
        public void ClearQuestions()
        {
            lQuestions.Clear();
        }

        /// <summary>
        /// Adds a question record to this instance
        /// </summary>
        /// <param name="item">The question record to add</param>
        public void AddQuestion(DNSQuestion item)
        {
            lQuestions.Add(item);
        }

        /// <summary>
        /// Returns all question records of this instance
        /// </summary>
        /// <returns>All question records of this instance</returns>
        public DNSQuestion[] GetQuestions()
        {
            return lQuestions.ToArray();
        }

        /// <summary>
        /// Checks whether a specific question record is contained in this instance
        /// </summary>
        /// <param name="item">The question record to search for</param>
        /// <returns>A bool indication whether a specific question record is contained in this instance</returns>
        public bool ContainsQuestion(DNSQuestion item)
        {
            return lQuestions.Contains(item);
        }

        /// <summary>
        /// Removes a question record from this instance
        /// </summary>
        /// <param name="item">The record to remove</param>
        public void RemoveQuestion(DNSQuestion item)
        {
            lQuestions.Remove(item);
        }

        /// <summary>
        /// Clears all answer records from this instance
        /// </summary>
        public void ClearAnswers()
        {
            lAnswers.Clear();
        }

        /// <summary>
        /// Adds a answer record to this instance
        /// </summary>
        /// <param name="item">The answer record to add</param>
        public void AddAnswer(DNSResourceRecord item)
        {
            lAnswers.Add(item);
        }

        /// <summary>
        /// Returns all answer records of this instance
        /// </summary>
        /// <returns>All answer records of this instance</returns>
        public DNSResourceRecord[] GetAnswers()
        {
            return lAnswers.ToArray();
        }

        /// <summary>
        /// Checks whether a specific answer record is contained in this instance
        /// </summary>
        /// <param name="item">The answer record to search for</param>
        /// <returns>A bool indication whether a specific answer record is contained in this instance</returns>
        public bool ContainsAnswer(DNSResourceRecord item)
        {
            return lAnswers.Contains(item);
        }

        /// <summary>
        /// Removes a answer record from this instance
        /// </summary>
        /// <param name="item">The record to remove</param>
        public void RemoveAnswer(DNSResourceRecord item)
        {
            lAnswers.Remove(item);
        }

        /// <summary>
        /// Clears all authorative records from this instance
        /// </summary>
        public void ClearAuthorotives()
        {
            lAuthorotive.Clear();
        }

        /// <summary>
        /// Adds a authorotive record to this instance
        /// </summary>
        /// <param name="item">The authorotive record to add</param>
        public void AddAuthorotive(DNSResourceRecord item)
        {
            lAuthorotive.Add(item);
        }

        /// <summary>
        /// Returns all authorative records of this instance
        /// </summary>
        /// <returns>All authorative records of this instance</returns>
        public DNSResourceRecord[] GetAuthorotives()
        {
            return lAuthorotive.ToArray();
        }

        /// <summary>
        /// Checks whether a specific authorative record is contained in this instance
        /// </summary>
        /// <param name="item">The authorative record to search for</param>
        /// <returns>A bool indication whether a specific authorative record is contained in this instance</returns>
        public bool ContainsAuthorotive(DNSResourceRecord item)
        {
            return lAuthorotive.Contains(item);
        }

        /// <summary>
        /// Removes a authorotive record from this instance
        /// </summary>
        /// <param name="item">The record to remove</param>
        public void RemoveAuthorotive(DNSResourceRecord item)
        {
            lAuthorotive.Remove(item);
        }

        /// <summary>
        /// Clears all additional records from this instance
        /// </summary>
        public void ClearAdditionals()
        {
            lAdditional.Clear();
        }

        /// <summary>
        /// Adds a additional record to this instance
        /// </summary>
        /// <param name="item">The additional record to add</param>
        public void AddAdditional(DNSResourceRecord item)
        {
            lAdditional.Add(item);
        }

        /// <summary>
        /// Returns all additional records of this instance
        /// </summary>
        /// <returns>All additional records of this instance</returns>
        public DNSResourceRecord[] GetAdditionals()
        {
            return lAdditional.ToArray();
        }

        /// <summary>
        /// Checks whether a specific additional record is contained in this instance
        /// </summary>
        /// <param name="item">The additional record to search for</param>
        /// <returns>A bool indication whether a specific additional record is contained in this instance</returns>
        public bool ContainsAdditional(DNSResourceRecord item)
        {
            return lAdditional.Contains(item);
        }

        /// <summary>
        /// Removes a additional record from this instance
        /// </summary>
        /// <param name="item">The record to remove</param>
        public void RemoveAdditional(DNSResourceRecord item)
        {
            lAdditional.Remove(item);
        }

        #endregion

        #region Props

        /// <summary>
        /// Gets or sets the DNS response code
        /// </summary>
        public DNSResponseCode ResponseCode
        {
            get { return dnsResponseCode; }
            set { dnsResponseCode = value; }
        }

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        public short Identifier
        {
            get { return sIdentifier; }
            set { sIdentifier = value; }
        }

        /// <summary>
        /// Gets or sets the recoursion allowed flag
        /// </summary>
        public bool RAFlag
        {
            get { return bRAFlag; }
            set { bRAFlag = value; }
        }

        /// <summary>
        /// Gets or sets the QR flag
        /// </summary>
        public bool QRFlag
        {
            get { return bQRFlag; }
            set { bQRFlag = value; }
        }

        /// <summary>
        /// Gets or sets the authorotive answer flag
        /// </summary>
        public bool AAFlag
        {
            get { return bAAFlag; }
            set { bAAFlag = value; }
        }

        /// <summary>
        /// Gets or sets the truncated response flag
        /// </summary>
        public bool TCFlag
        {
            get { return bTCFlag; }
            set { bTCFlag = value; }
        }

        /// <summary>
        /// Gets or sets the recourson desired flag
        /// </summary>
        public bool RDFlag
        {
            get { return bRDFlag; }
            set { bRDFlag = value; }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DNSFrame()
        {
            lQuestions = new List<DNSQuestion>();
            lAnswers = new List<DNSResourceRecord>();
            lAuthorotive = new List<DNSResourceRecord>();
            lAdditional = new List<DNSResourceRecord>();
            dnsOperationCode = DNSOptCode.Query;
            dnsResponseCode = DNSResponseCode.NoError;
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public DNSFrame(byte[] bData)
        {
            sIdentifier = (short)((bData[0] << 8) + bData[1]);
            bQRFlag = (bData[2] & 0x80) > 0;
            dnsOperationCode = (DNSOptCode)((bData[2] >> 3) & 0x0F);
            bAAFlag = (bData[2] & 0x04) > 0;
            bTCFlag = (bData[2] & 0x02) > 0;
            bRDFlag = (bData[2] & 0x01) > 0;
            bRAFlag = (bData[3] & 0x80) > 0;
            dnsResponseCode = (DNSResponseCode)((bData[3]) & 0x0F);

            int iQCount = (bData[4] << 8) + bData[5];
            int iACount = (bData[6] << 8) + bData[7];
            int iAuthCount = (bData[8] << 8) + bData[9];
            int iAddCount = (bData[10] << 8) + bData[11];

            lQuestions = new List<DNSQuestion>();
            lAnswers = new List<DNSResourceRecord>();
            lAuthorotive = new List<DNSResourceRecord>();
            lAdditional = new List<DNSResourceRecord>();

            int iIndex = 12;

            while (lQuestions.Count < iQCount)
            {
                DNSQuestion qLast = new DNSQuestion(bData, ref iIndex);
                lQuestions.Add(qLast);
            }
            while (lAnswers.Count < iACount)
            {
                DNSResourceRecord rLast = new DNSResourceRecord(bData, ref iIndex);
                lAnswers.Add(rLast);
            }
            while (lAuthorotive.Count < iAuthCount)
            {
                DNSResourceRecord rLast = new DNSResourceRecord(bData, ref iIndex);
                lAuthorotive.Add(rLast);
            }
            while (lAdditional.Count < iAddCount)
            {
                DNSResourceRecord rLast = new DNSResourceRecord(bData, ref iIndex);
                lAdditional.Add(rLast);
            }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return DNSFrame.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get
            {
                Dictionary<string, int> dictCompression = new Dictionary<string,int>();
                MemoryStream ms = new MemoryStream();
                byte[] bData = new byte[12];
                bData[0] = (byte)((sIdentifier >> 8) & 0xFF);
                bData[1] = (byte)((sIdentifier) & 0xFF);
                bData[2] |= (byte)(bQRFlag ? 0x80 : 0);
                bData[2] |= (byte)(bAAFlag ? 0x04 : 0);
                bData[2] |= (byte)(bTCFlag ? 0x02 : 0);
                bData[2] |= (byte)(bRDFlag ? 0x01 : 0);
                bData[2] |= (byte)(((int)dnsOperationCode & 0x0F) << 3);
                bData[3] |= (byte)(bRAFlag ? 0x80 : 0);
                bData[3] |= (byte)((int)dnsResponseCode & 0x0F);


                bData[4] = (byte)((lQuestions.Count >> 8) & 0xFF);
                bData[5] = (byte)((lQuestions.Count) & 0xFF);
                bData[6] = (byte)((lAnswers.Count >> 8) & 0xFF);
                bData[7] = (byte)((lAnswers.Count) & 0xFF);
                bData[8] = (byte)((lAuthorotive.Count >> 8) & 0xFF);
                bData[9] = (byte)((lAuthorotive.Count) & 0xFF);
                bData[10] = (byte)((lAdditional.Count >> 8) & 0xFF);
                bData[11] = (byte)((lAdditional.Count) & 0xFF);

                ms.Write(bData, 0, 12);

                int iIndex = 12;


                foreach (DNSQuestion q in lQuestions)
                {
                    bData = q.GetCompressedBytes(dictCompression, iIndex);
                    ms.Write(bData, 0, bData.Length);
                    iIndex += bData.Length;
                }
                foreach (DNSResourceRecord r in lAnswers)
                {
                    bData = r.GetCompressedBytes(dictCompression, iIndex);
                    ms.Write(bData, 0, bData.Length);
                    iIndex += bData.Length;
                }
                foreach (DNSResourceRecord r in lAuthorotive)
                {
                    bData = r.GetCompressedBytes(dictCompression, iIndex);
                    ms.Write(bData, 0, bData.Length);
                    iIndex += bData.Length;
                }
                foreach (DNSResourceRecord r in lAdditional)
                {
                    bData = r.GetCompressedBytes(dictCompression, iIndex);
                    ms.Write(bData, 0, bData.Length);
                    iIndex += bData.Length;
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                return FrameBytes.Length;
            }
        }

        /// <summary>
        /// Creates a new identical instance of this frame
        /// </summary>
        /// <returns>A new identical instance of this frame</returns>
        public override Frame Clone()
        {
            return new DNSFrame(this.FrameBytes);
        }
    }

    /// <summary>
    /// An enumeration for DNS resource types
    /// Fore more information see http://www.dns.net/dnsrd/rr.html
    /// </summary>
    public enum DNSResourceType
    {  
        /// <summary>
        /// IPv4 Address of a single host
        /// </summary>
        A = 1,
        /// <summary>
        /// Authoritative name server
        /// </summary>
        NS = 2,
        /// <summary>
        /// Mail destination
        /// </summary>
        MD = 3,
        /// <summary>
        /// Mail forwarder
        /// </summary>
        MF = 4,
        /// <summary>
        /// Canonical name for a DNS alias
        /// </summary>
        CNAME = 5,
        /// <summary>
        /// Start of authority
        /// </summary>
        SOA = 6,
        /// <summary>
        /// Mailbox
        /// </summary>
        MB = 7,
        /// <summary>
        /// Mail group member
        /// </summary>
        MG = 8,
        /// <summary>
        /// Mail rename domain name
        /// </summary>
        MR = 9,
        /// <summary>
        /// Null record
        /// </summary>
        NULL = 10,
        /// <summary>
        /// Well-known service
        /// </summary>
        WKS = 11,
        /// <summary>
        /// Domain name pointer
        /// </summary>
        PTR = 12,
        /// <summary>
        /// Host Information
        /// </summary>
        HINFO = 13,
        /// <summary>
        /// Mailbox or mailing list information
        /// </summary>
        MINFO = 14,
        /// <summary>
        /// Mail Exchanger
        /// </summary>
        MX = 15,
        /// <summary>
        /// Text string
        /// </summary>
        TXT = 16,
        /// <summary>
        /// IPv6 address record
        /// </summary>
        AAAA = 28,
        /// <summary>
        /// Certificate record
        /// </summary>
        CERT = 37,
        /// <summary>
        /// All available information
        /// </summary>
        All = 255
    }

    /// <summary>
    /// An enumeration for DNS resource classes
    /// </summary>
    public enum DNSResourceClass
    {
        /// <summary>
        /// DNS resource class IN, the internet
        /// </summary>
        Internet = 1,
        /// <summary>
        /// DNS resource class CS
        /// </summary>
        CSNETClass = 2,
        /// <summary>
        /// DNS resource class CH
        /// </summary>
        CHAOS = 3,
        /// <summary>
        /// DNS resource class HS
        /// </summary>
        Hesiod = 4,
        /// <summary>
        /// Any DNS resource class
        /// </summary>
        Any = 255
    }

    /// <summary>
    /// An enumeration for DNS option codes
    /// </summary>
    public enum DNSOptCode
    {
        /// <summary>
        /// A query
        /// </summary>
        Query = 0,
        /// <summary>
        /// An incerse query
        /// </summary>
        IQuery = 1,
        /// <summary>
        /// Status 
        /// </summary>
        Status = 2,
        /// <summary>
        /// Reserverd
        /// </summary>
        Reserved = 3,
        /// <summary>
        /// Notify
        /// </summary>
        Notify = 4,
        /// <summary>
        /// Update
        /// </summary>
        Update = 5
    }

    /// <summary>
    /// AN enumeration for DNS response codes
    /// </summary>
    public enum DNSResponseCode
    {
        /// <summary>
        /// No error occoured
        /// </summary>
        NoError = 0,
        /// <summary>
        /// There was a format error
        /// </summary>
        FormatError = 1,
        /// <summary>
        /// There was a server failure
        /// </summary>
        ServerFailure = 2,
        /// <summary>
        /// There was a name error
        /// </summary>
        NameError = 3,
        /// <summary>
        /// The requested function was not implemented
        /// </summary>
        NotImplemented = 4,
        /// <summary>
        /// The requested function was refused
        /// </summary>
        Refused = 5,
        /// <summary>
        /// 
        /// </summary>
        YXDomain = 6,
        /// <summary>
        /// 
        /// </summary>
        YXRRSet = 7,
        /// <summary>
        /// 
        /// </summary>
        NXRRSet = 8,
        /// <summary>
        /// 
        /// </summary>
        NotAutorotive = 9,
        /// <summary>
        /// 
        /// </summary>
        NotZone = 10
    }
}
