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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace SymbianUtils.Graphics
{
	public static class ScreenUtils
	{
        public static Color ColorAtPixel( Point aPos )
        {
            Bitmap bitmap = new Bitmap( 1, 1, PixelFormat.Format32bppArgb );
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage( bitmap );
            graphics.CopyFromScreen( aPos.X, aPos.Y, 0, 0, new Size( 1, 1 ) );
            Color ret = bitmap.GetPixel( 0, 0 );
            return ret;
        }
    }
}
