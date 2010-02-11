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
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using CrashItemLib.PluginAPI;

namespace DExcPlugin.Descriptor
{
    internal class DExcDescriptor : CFFSourceAndConfidence
    {
        #region Constructors
        public DExcDescriptor( FileInfo aFile )
            : base( aFile )
		{
            if ( StackFileExists )
            {
                string stackFile = StackFileName;
                base.AddAdditionalFile( new FileInfo( stackFile ) );
            }
        }
        #endregion

        #region From CISource
        public override Version ImplementorVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                return version;
            }
        }

        public override string ImplementorName
        {
            get { return "D_EXC"; }
        }
        #endregion

        #region Constants
        public const string KFileExtensionStack = ".stk";
        #endregion

        #region Properties
        public string StackFileName
        {
            get
            {
                string path = Path.GetDirectoryName( base.MasterFileName );
                string fileName = Path.GetFileNameWithoutExtension( base.MasterFileName );
                //
                string ret = Path.Combine( path, fileName + KFileExtensionStack );
                return ret;
            }
        }

        public bool StackFileExists
        {
            get
            {
                string file = StackFileName;
                return ( File.Exists( file ) );
            }
        }
		#endregion

        #region Data members
		#endregion
	}
}
