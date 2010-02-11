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
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using SymbianUtils;
using CrashItemLib;

namespace CrashItemLib.PluginAPI
{
    public abstract class CFFSourceAndConfidence : CFFSource, IComparable<CFFSourceAndConfidence>
	{
        #region Constructors
        protected CFFSourceAndConfidence( FileInfo aFile )
            : base( aFile )
		{
            Level = int.MinValue;
		}
        #endregion
        
        #region API
        public void SetCertain()
        {
            Level = int.MaxValue;
        }
        #endregion

        #region Properties
        public int Level
        {
            get { return iLevel; }
            set { iLevel = value; }
        }

        public bool IsSupported
        {
            get { return !IsUnsupported && MasterFileName != string.Empty && Reader != null; }
        }

        public bool IsCertain
        {
            get { return Level == int.MaxValue; }
        }

        public bool IsUnsupported
        {
            get
            {
                bool ret = false;

                // The source cannot be processed if:
                //
                // a) Type is not supported
                // b) The confidence level is "no confidence whatsoever" or
                // c) There is no reader
                //
                if ( Level == int.MinValue )
                {
                    ret = true; // (b)
                }
                else if ( Reader == null )
                {
                    ret = true; // (c)
                }
                else if ( OpType == TReaderOperationType.EReaderOpTypeNotSupported )
                {
                    ret = true; // (a)
                }

                return ret;
            }
        }
        #endregion

        #region From IComparable<CFFConfidenceLevel>
        public int CompareTo( CFFSourceAndConfidence aOther )
        {
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = Level.CompareTo( aOther.Level );
            }
            //
            return ret;
        }
        #endregion

		#region Data members
        private int iLevel = int.MinValue;
		#endregion
    }
}
