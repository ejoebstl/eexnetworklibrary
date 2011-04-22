using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.HTTP;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.Actions
{
    public abstract class ImageAction : HTTPStreamModifierAction
    {
        protected ImageAction()
        {
            arCodecs = ImageCodecInfo.GetImageEncoders();
        }

        ImageCodecInfo[] arCodecs;

        public override bool IsMatch(HTTPMessage httpMessage)
        {
            bool bResult = false;

            if (httpMessage.Headers.Contains("Content-Type"))
            {
                bResult = httpMessage.Headers["Content-Type"][0].Value.ToLower().Contains("image");
            }

            bool bBaseResult = base.IsMatch(httpMessage);
            return bResult && bBaseResult;
        }

        public override HTTPMessage ApplyAction(HTTPMessage httpMessage)
        {
            ImageFormat imgFormat = GetImageFormat(httpMessage.Headers["Content-Type"][0].Value.ToLower());
            if (imgFormat == null)
            {
                throw new InvalidOperationException("Unknown image format " + httpMessage.Headers["Content-Type"][0].Value);
            }


            Bitmap img = new Bitmap(new MemoryStream(httpMessage.Payload));

            img = ModifyImage(img);

            MemoryStream msSave = new MemoryStream();
            img.Save(msSave, imgFormat);
            httpMessage.Payload = msSave.ToArray();

            img.Dispose();
            msSave.Dispose();

            return httpMessage;
        }

        protected abstract Bitmap ModifyImage(Bitmap bmp);

        private ImageFormat GetImageFormat(string strMime)
        {
            foreach (ImageCodecInfo imgCodec in arCodecs)
            {
                if (strMime.Contains(imgCodec.MimeType))
                {
                    return new ImageFormat(imgCodec.FormatID);
                }
            }
            return null;
        }
    }
}
