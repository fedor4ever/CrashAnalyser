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
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
    public class CIDBAttributeCell : Attribute
    {
        #region Enumerations
        public enum TOptions
        {
            ENone = 0,
            EAutoExpand
        }
        #endregion

        #region Constructors
        public CIDBAttributeCell( TOptions aOptions )
        {
            if ( aOptions == TOptions.ENone )
            {
                throw new ArgumentException( "Options cannot be \'none\'" );
            }

            iOptions = aOptions;
        }

        public CIDBAttributeCell( string aCaption, int aOrder )
            : this( aCaption, aOrder, string.Empty )
        {
        }

        public CIDBAttributeCell( string aCaption, int aOrder, object aDefaultValue )
            : this( aCaption, aOrder, string.Empty, aDefaultValue )
        {
        }

        public CIDBAttributeCell( string aCaption, int aOrder, string aFormat )
            : this( aCaption, aOrder, aFormat, null )
        {
        }

        public CIDBAttributeCell( string aCaption, int aOrder, string aFormat, object aDefaultValue )
        {
            iCaption = aCaption;
            iOrder = aOrder;
            iFormat = aFormat;
            iDefaultValue = aDefaultValue;
        }

        public CIDBAttributeCell( string aCaption, int aOrder, Color aForeColor )
            : this( aCaption, aOrder )
        {
            iForeColor = aForeColor;
        }

        public CIDBAttributeCell( string aCaption, int aOrder, Color aForeColor, Color aBackColor )
            : this( aCaption, aOrder, aForeColor )
        {
            iBackColor = aBackColor;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string Caption
        {
            get { return iCaption; }
        }

        public string Format
        {
            get { return iFormat; }
        }

        public int Order
        {
            get { return iOrder; }
        }

        public Color ForeColor
        {
            get { return iForeColor; }
        }

        public Color BackColor
        {
            get { return iBackColor; }
        }

        public object DefaultValue
        {
            get { return iDefaultValue; }
        }

        public TOptions Options
        {
            get { return iOptions; }
        }
        #endregion

        #region Data members
        private TOptions iOptions = TOptions.ENone;
        private readonly string iCaption;
        private readonly int iOrder;
        private readonly Color iForeColor;
        private readonly Color iBackColor;
        private readonly string iFormat;
        private readonly object iDefaultValue = null;
        #endregion
	}

    [AttributeUsage( AttributeTargets.Class, AllowMultiple=true ) ]
    public class CIDBAttributeColumn : Attribute
    {
        #region Constructors
        public CIDBAttributeColumn( string aCaption, int aOrder )
        {
            iCaption = aCaption;
            iOrder = aOrder;
        }

        public CIDBAttributeColumn( string aCaption, int aOrder, int aWidth )
            : this( aCaption, aOrder )
        {
            iWidth = aWidth;

            // Apply width information
            iWidthSet = true;
        }

        public CIDBAttributeColumn( string aCaption, int aOrder, bool aTakesUpSlack )
            : this( aCaption, aOrder, CIDBColumn.KDefaultWidth )
        {
            iTakesUpSlack = aTakesUpSlack;

            // Don't apply width information
            iWidthSet = false;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string Caption
        {
            get { return iCaption; }
        }

        public int Order
        {
            get { return iOrder; }
        }

        public bool WidthSet
        {
            get
            {
                return iWidthSet;
            }
        }

        public int Width
        {
            get { return iWidth; }
        }

        public bool TakesUpSlack
        {
            get { return iTakesUpSlack; }
        }
        #endregion

        #region Data members
        private readonly string iCaption;
        private readonly int iOrder;
        private readonly int iWidth;
        private readonly bool iWidthSet;
        private readonly bool iTakesUpSlack;
        #endregion
    }
}
