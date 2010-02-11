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
using System.Text;
using System.Collections.Generic;
using System.Drawing;

namespace CrashItemLib.Crash.Base.DataBinding
{
    public class CIDBCell
	{
		#region Constructors
        public CIDBCell()
        {
            iColors.Add( System.Drawing.Color.Black );
            iColors.Add( System.Drawing.Color.Transparent );
        }

        public CIDBCell( string aCaption )
            : this()
        {
            iCaption = aCaption;
 		}

        public CIDBCell( string aCaption, Color aForeColor )
            : this()
        {
            iCaption = aCaption;
            ForeColor = aForeColor;
 		}

        public CIDBCell( Color aForeColor, Color aBackColor )
            : this( string.Empty, aForeColor, aBackColor )
        {
        }

        public CIDBCell( string aCaption, Color aForeColor, Color aBackColor )
            : this( aCaption, aForeColor, aBackColor, string.Empty )
        {
        }

        public CIDBCell( string aCaption, Color aForeColor, Color aBackColor, string aFormat )
            : this()
        {
            Caption = aCaption;
            ForeColor = aForeColor;
            BackColor = aBackColor;
            Format = aFormat;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string Caption
        {
            get { return iCaption; }
            set { iCaption = value; }
        }

        public string Format
        {
            get { return iFormat; }
            set { iFormat = value; }
        }

        public Color ForeColor
        {
            get
            {
                bool set = IsSet( KColIndexFore );
                Color ret = Color( KColIndexFore );

                // Get color from row if we've not been explicitly set
                if ( !set && Row != null )
                {
                    ret = Row.ForeColor;
                }
                //
                return ret; 
            }
            set
            {
                SetColor( value, KColIndexFore );
            }
        }

        public Color BackColor
        {
            get
            {
                bool set = IsSet( KColIndexBack );
                Color ret = Color( KColIndexBack );

                // Get color from row if we've not been explicitly set
                if ( !set && Row != null )
                {
                    ret = Row.BackColor;
                }
                //
                return ret;
            }
            set 
            {
                SetColor( value, KColIndexBack );
            }
        }

        public CIDBRow Row
        {
            get { return iRow; }
            internal set { iRow = value; }
        }

        public CIElement Element
        {
            get { return Row.Element; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Caption;
        }
        #endregion

        #region Internal constants
        private const int KColIndexFore = 0;
        private const int KColIndexBack = 1;
        #endregion

        #region Internal methods
        private bool IsSet( int aIndex )
        {
            return iColorsSet[ aIndex ];
        }

        private Color Color( int aIndex )
        {
            return iColors[ aIndex ];
        }

        private void SetColor( Color aColor, int aIndex )
        {
            iColors[ aIndex ] = aColor;
            iColorsSet[ aIndex ] = true;
        }
        #endregion

        #region Data members
        private CIDBRow iRow = null;
        private string iCaption = string.Empty;
        private string iFormat = string.Empty;
        private bool[] iColorsSet = new bool[ 2 ] { false, false };
        private List<Color> iColors = new List<Color>( 2 );
        #endregion
	}
}
