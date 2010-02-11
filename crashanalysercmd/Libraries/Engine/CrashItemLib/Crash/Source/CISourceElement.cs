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
using System.Collections;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Source
{
    public class CISourceElement : CIElement, IEnumerable<FileInfo>
    {
        #region Constructors
        public CISourceElement( CIContainer aContainer, CISource aSource )
            : base( aContainer )
        {
            iSource = aSource;
        }
        #endregion

        #region API
        public void InputDataClear()
        {
            iInputData.Clear();
        }

        public void InputDataAdd( byte[] aRawData )
        {
            iInputData.AddRange( aRawData );
        }
        #endregion

        #region Properties
        public byte[] InputData
        {
            get { return iInputData.ToArray(); }
        }

        public string MasterFileName
        {
            get { return iSource.MasterFileName; }
        }

        public FileInfo[] AllFiles
        {
            get { return iSource.AllFiles; }
        }

        public bool IsLineNumberAvailable
        {
            get { return iSource.IsLineNumberAvailable; }
        }

        public long LineNumber
        {
            get { return iSource.LineNumber; }
        }
        #endregion

        #region Operators
        public static implicit operator CISource( CISourceElement aSource )
        {
            return aSource.iSource;
        }
        #endregion

        #region From IEnumerable<FileInfo>
        public new IEnumerator<FileInfo> GetEnumerator()
        {
            return iSource.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return iSource.GetEnumerator();
        }
        #endregion

        #region Data members
        private readonly CISource iSource;
        private List<byte> iInputData = new List<byte>();
        #endregion
    }
}
