using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.Actions;
using eExNLML.IO;

namespace eExNLML.SubPlugInDefinitions
{
    public class ImageFlipperDefinition : Extensibility.HTTPModifierActionDefinition
    {
        public ImageFlipperDefinition()
        {
            Name = "Image Flipper";
            Description = "Flips images contained in a HTTP message.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_http_flipper";
            Version = new Version(0, 9);
        }

        public override eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.HTTPStreamModifierAction Create()
        {
            return new ImageFlipper();
        }

        public override eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.HTTPStreamModifierAction Create(eExNLML.IO.NameValueItem nviConfigurationRoot)
        {
            ImageFlipper imgFlip = (ImageFlipper)Create();
            imgFlip.RotateFlipType = (System.Drawing.RotateFlipType)ConfigurationParser.ConvertToInt(nviConfigurationRoot["imageOperation"])[0];
            return imgFlip;
        }

        public override eExNLML.IO.NameValueItem[] GetConfiguration(eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.HTTPStreamModifierAction htCondition)
        {
            ImageFlipper imgFlip = (ImageFlipper)htCondition;
            return new NameValueItem[] { ConfigurationParser.ConvertToNameValueItems("imageOperation", (int)imgFlip.RotateFlipType)[0] };
        }
    }
}
