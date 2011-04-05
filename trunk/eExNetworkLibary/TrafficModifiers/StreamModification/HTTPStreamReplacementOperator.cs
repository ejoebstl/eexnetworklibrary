using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Sockets;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification
{
    public class HTTPStreamReplacementOperator : HTTPStreamOperator
    {

        public HTTPStreamReplacementOperator(NetworkStream sAlice, NetworkStream sBob)
            : base(sAlice, sBob)
        {
        }

        protected override eExNetworkLibrary.HTTP.HTTPRequest ModifyRequest(eExNetworkLibrary.HTTP.HTTPRequest httpRequest)
        {

            return httpRequest;
        }

        protected override eExNetworkLibrary.HTTP.HTTPResponse ModifyResponse(eExNetworkLibrary.HTTP.HTTPResponse httpResponse)
        {
            byte[] bPayload = httpResponse.Payload;

            string strTransferEncoding;
            if (httpResponse.Headers.Contains("Transfer-Encoding"))
            {
                strTransferEncoding = httpResponse.Headers["Transfer-Encoding"][0].Value.ToLower();
                if (strTransferEncoding == "chunked")
                {
                    bPayload = CompressionHelper.DecompressChunked(bPayload);
                    httpResponse.Headers.Remove("Transfer-Encoding");
                    httpResponse.Headers.Add(new HTTP.HTTPHeader("Content-Length", bPayload.Length.ToString()));
                }
            }

            if (httpResponse.Headers.Contains("Content-Type") && httpResponse.Headers.Contains("Content-Length"))
            {
                string strContentType = httpResponse.Headers["Content-Type"][0].Value.ToLower();
                string strContentEncoding = null;

                if (httpResponse.Headers.Contains("Content-Encoding"))
                {
                    strContentEncoding = httpResponse.Headers["Content-Encoding"][0].Value.ToLower();
                    if (strContentEncoding == "gzip" || strContentEncoding == "x-gzip")
                    {
                        bPayload = CompressionHelper.DecompressGZip(bPayload);
                    }
                    else if (strContentEncoding == "deflate")
                    {
                        bPayload = CompressionHelper.DecompressDeflate(bPayload);
                    }
                    else if (strContentEncoding == "chunked")
                    {
                        bPayload = CompressionHelper.DecompressChunked(bPayload);
                        strContentEncoding = "none";
                    }
                } 

                if (strContentType.Contains("text"))
                {
                    //bPayload = ASCIIEncoding.ASCII.GetBytes(ASCIIEncoding.ASCII.GetString(bPayload).Replace("e", "E"));
                }
                else if (strContentType.Contains("image/png"))
                {
                    System.Drawing.Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(bPayload));
                    img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                    System.IO.MemoryStream msOut = new System.IO.MemoryStream();
                    img.Save(msOut, System.Drawing.Imaging.ImageFormat.Png);
                    bPayload = msOut.ToArray();
                    msOut.Close();
                    img.Dispose();
                }
                else if (strContentType.Contains("image/jpeg"))
                {
                    System.Drawing.Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(bPayload));
                    img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                    System.IO.MemoryStream msOut = new System.IO.MemoryStream();
                    img.Save(msOut, System.Drawing.Imaging.ImageFormat.Jpeg);
                    bPayload = msOut.ToArray();
                    msOut.Close();
                    img.Dispose();
                }

                if (strContentEncoding != null)
                {
                    if (strContentEncoding == "gzip" || strContentEncoding == "x-gzip")
                    {
                        bPayload = CompressionHelper.CompressGZip(bPayload);
                    }
                    else if (strContentEncoding == "deflate")
                    {
                        bPayload = CompressionHelper.CompressDeflate(bPayload);
                    }
                }
                httpResponse.Headers["Content-Length"][0].Value = bPayload.Length.ToString();
                httpResponse.Payload = bPayload;
            }
            return httpResponse;
        }

        public override string Description
        {
            get { return "Edit HTTP Message"; }
        }
    }
}
