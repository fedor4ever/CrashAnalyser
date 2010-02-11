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
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.CodeSegments;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.DataBuffer;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Processes;

namespace CrashItemLib.Crash.CodeSegs
{
	public class CICodeSegList : CIElement, IEnumerable<CICodeSeg>
    {
        #region Constructors
        public CICodeSegList( CIProcess aProcess )
            : base( aProcess.Container, aProcess )
		{
            base.AddSupportedChildType( typeof( CICodeSeg ) );
            base.AddSupportedChildType( typeof( CrashItemLib.Crash.Messages.CIMessage ) );
        }
 
        public CICodeSegList( CIProcess aProcess, IEnumerable<CICodeSeg> aEnumerable )
            : this( aProcess )
		{
            foreach ( CICodeSeg cs in aEnumerable )
            {
                AddChild( cs );
            }
        }
        #endregion

        #region API
        public bool Contains( string aDeviceBinaryFileName )
        {
            bool ret = false;
            //
            string deviceBinaryFileName = aDeviceBinaryFileName.ToUpper();
            foreach ( CICodeSeg codeSeg in this )
            {
                string codeSegName = Path.GetFileNameWithoutExtension( codeSeg.Name ).ToUpper();
                if ( deviceBinaryFileName.Contains( codeSegName ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }


        internal void DiscardImplicitCodeSegments()
        {
            int count = base.Count;
            for ( int i = count - 1; i >= 0; i-- )
            {
                CICodeSeg cs = this[ i ] as CICodeSeg;
                if ( cs != null && !cs.IsExplicit )
                {
                    base.Trace( string.Format( "[CICodeSegList] DiscardImplicitCodeSegments() - dicarding: {0}", cs ) );
                    base.RemoveChild( cs );
                }
            }
        }
        #endregion

        #region Properties
        public CIProcess OwningProcess
        {
            get { return (CIProcess) base.Parent; }
        }

        public CICodeSeg this[ uint aAddress ]
        {
            get
            {
                CICodeSeg ret = null;
                //
                foreach ( CICodeSeg cs in this )
                {
                    if ( cs.Contains( aAddress ) )
                    {
                        ret = cs;
                        break;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Operators
        public static implicit operator CodeSegDefinitionCollection( CICodeSegList aList )
        {
            return aList.iCollection;
        }
        #endregion

        #region From IEnumerable<CICodeSeg>
        public new IEnumerator<CICodeSeg> GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CICodeSeg )
                {
                    CICodeSeg ret = (CICodeSeg) element;
                    yield return ret;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CICodeSeg )
                {
                    CICodeSeg ret = (CICodeSeg) element;
                    yield return ret;
                }
            }
        }
        #endregion

        #region From CIElement
        protected override void OnElementAddedToSelf( CIElement aElement )
        {
            System.Diagnostics.Debug.Assert( aElement is CICodeSeg );
            CICodeSeg codeSeg = (CICodeSeg) aElement;
            iCollection.Add( codeSeg );
            //
            base.OnElementAddedToSelf( aElement );
        }
        #endregion

        #region Data members
        private CodeSegDefinitionCollection iCollection = new CodeSegDefinitionCollection();
        #endregion
    }
}
