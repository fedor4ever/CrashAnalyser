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
    internal class CXmlVersionExtended : CXmlNode
	{
		#region Constructors
        public CXmlVersionExtended()
            : base( Constants.Version_Extended )
		{
		}

        public CXmlVersionExtended( Version aVersion )
            : this( aVersion.Major, aVersion.Minor )
        {
            // Version supports 4 levels of versining information:
            //
            //      major.minor.build.revision
            //
            // This object only supports two:
            //
            //      major.minor
            //
            // However, "this.minor" can encapsulate both Version.minor and Version.build.
            //
            iMinor = ( aVersion.Minor * 10 ) + aVersion.Build;
        }
        
        public CXmlVersionExtended( int aMajor )
            : this( aMajor, 0 )
        {
        }

        public CXmlVersionExtended( int aMajor, int aMinor )
            : this()
        {
            iMajor = aMajor;
            iMinor = aMinor;
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteElementString( Constants.Version_Extended_Major, Major.ToString() );
            aParameters.Writer.WriteElementString( Constants.Version_Extended_Minor, Minor.ToString( "d2" ) );
        }
        #endregion

        #region Properties
        public int Major
        {
            get { return iMajor; }
            set { iMajor = value; }
        }

        public int Minor
        {
            get { return iMinor; }
            set { iMinor = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "{0}.{1:d2}", Major, Minor );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private int iMajor = 1;
        private int iMinor = 0;
		#endregion
	}
}
