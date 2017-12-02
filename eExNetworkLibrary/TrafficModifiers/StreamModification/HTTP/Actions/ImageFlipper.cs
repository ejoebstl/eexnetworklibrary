// This source file is part of the eEx Network Library
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
using System.Drawing;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.Actions
{
    public class ImageFlipper : ImageAction
    {
        public RotateFlipType RotateFlipType
        {
            get;
            set;
        }

        public ImageFlipper()
        {
            RotateFlipType = RotateFlipType.Rotate180FlipNone;
        }

        protected override Bitmap ModifyImage(Bitmap bmp)
        {
            bmp.RotateFlip(RotateFlipType);
            return bmp;
        }

        public override string Name
        {
            get { return "Image Flipper"; }
        }

        public override string GetLongDescription()
        {
            return RotateFlipType.ToString();
        }

        public override string GetShortDescription()
        {
            return RotateFlipType.ToString();
        }

        public override object Clone()
        {
            ImageFlipper imgFlipClone = new ImageFlipper();
            imgFlipClone.RotateFlipType = this.RotateFlipType;
            CloneChildsTo(imgFlipClone);
            return imgFlipClone;
        }
    }
}
