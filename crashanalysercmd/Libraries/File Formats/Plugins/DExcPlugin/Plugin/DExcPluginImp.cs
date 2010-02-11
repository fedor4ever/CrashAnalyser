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
using System.Collections.Generic;
using CrashItemLib.PluginAPI;
using DExcPlugin.Descriptor;
using DExcPlugin.Reader;

namespace DExcPlugin.Plugin
{
	public class DExcPluginImp : CFFPlugin
	{
		#region Constructors
        public DExcPluginImp( CFFDataProvider aDataProvider )
		:   base( aDataProvider )
		{
        }
        #endregion

        #region From CFFEngine
        public override CFFSourceAndConfidence GetConfidence( FileInfo aFile, CFFFileList aOtherFiles )
        {
            DExcDescriptor ret = new DExcDescriptor( aFile );
            //
            if ( ret.Exists )
            {
                string extension = ret.Extension.ToLower();
                //
                if ( ret.IsTraceExtension )
                {
                    ret.Level = int.MaxValue / 2;
                    ret.OpType = CFFSource.TReaderOperationType.EReaderOpTypeTrace;
                }

                // If confidence indicates we can handle the file, then make a reader
                if ( ret.OpType != CFFSource.TReaderOperationType.EReaderOpTypeNotSupported )
                {
                    ret.Reader = new DExcReader( this, ret );

                    // Remove any stack file if present
                    string stackFile = ret.StackFileName;
                    if ( ret.StackFileExists )
                    {
                        aOtherFiles.Remove( stackFile );
                    }
                }
            }
            //
            return ret;
        }

        public override void GetSupportedFileTypes( List<CFFFileSpecification> aFileTypes )
        {
            aFileTypes.Add( CFFFileSpecification.TraceFiles() );
            aFileTypes.Add( CFFFileSpecification.AllFiles() );
        }

        public override string Name
        {
            get
            {
                return "D_EXC Plugin";
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region Data members
		#endregion
	}
}
