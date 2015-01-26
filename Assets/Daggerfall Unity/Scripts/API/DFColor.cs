// Project:         Daggerfall Tools For Unity
// Description:     Read data from Daggerfall's file formats into Unity3D.
// Copyright:       Copyright (C) 2009-2014 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    https://code.google.com/p/daggerfall-unity/

#region Using Statements
using System;
using System.Text;
#endregion

namespace DaggerfallConnect.Utility
{
    /// <summary>
    /// Stores colour data.
    /// </summary>
    public class DFColor
    {
        #region Fields

        public byte R;

        public byte G;

        public byte B;

        public byte A;

        #endregion

        #region Constructors

        /// <summary>
        /// Value constructor RGB.
        /// </summary>
        public DFColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = 0xff;
        }

        /// <summary>
        /// Value constructor RGBA.
        /// </summary>
        public DFColor(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        #endregion
    }
}