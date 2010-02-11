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
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Versions
{
    internal class CXmlVersion : CXmlNode
	{
		#region Constructors
        public CXmlVersion()
            : this( 1 )
		{
		}

        public CXmlVersion( int aNumber )
            : base( Constants.Version )
        {
            iNumber = aNumber;
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteString( this.ToString() );
        }
        #endregion

        #region Properties
        public int Number
        {
            get { return iNumber; }
            set { iNumber = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Number.ToString();
        }
        #endregion

        #region Data members
        private int iNumber = 1;
		#endregion
	}
}
