/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
* All rights reserved.
* This component and the accompanying materials are made available
* under the terms of "Eclipse Public License v1.0"
* which accompanies this distribution, and is available
* at the URL "http://www.eclipse.org/legal/epl-v10.html".
*
* Initial Contributors:
* Nokia Corporation - initial contribution.
*
* Contributors:
* 
* Description:
*
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace SymbianUtils.Colour
{
    public static class ColourUtils
    {
        public static Color Lighten( Color aColour )
        {
            Color ret = ControlPaint.Light( aColour );
            return ret;
        }

        public static Color LightenMore( Color aColour )
        {
            Color ret = ControlPaint.LightLight( aColour );
            return ret;
        }

        public static Color Darken( Color aColour )
        {
            Color ret = ControlPaint.Dark( aColour );
            return ret;
        }

        public static Color Lighten( Color aColour, float aPercentage )
        {
            int r = aColour.R;
            int g = aColour.G;
            int b = aColour.B;
            //
            int amount = (int) ( 255.0f * aPercentage );
            //
            r = Math.Min( r + amount, 255 );
            g = Math.Min( g + amount, 255 );
            b = Math.Min( b + amount, 255 );
            //
            Color ret = Color.FromArgb( r, g, b );
            return ret;
        }

        public static Color Darken( Color aColour, float aPercentage )
        {
            int r = aColour.R;
            int g = aColour.G;
            int b = aColour.B;
            //
            int amount = (int) ( 255.0f * aPercentage );
            //
            r = Math.Max( r - amount, 0 );
            g = Math.Max( g - amount, 0 );
            b = Math.Max( b - amount, 0 );
            //
            Color ret = Color.FromArgb( r, g, b );
            return ret;
        }
    }
}
