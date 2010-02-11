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
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Container;
using SymbianStructuresLib.Uids;

namespace CrashItemLib.Crash.Messages
{
    #region Attributes
    [CIDBAttributeColumn( "Type", 0 )]
    [CIDBAttributeColumn( "Overview", 1, true )]
    #endregion
    public class CIMessageDictionary : CIElementList<CIMessage>
	{
		#region Constructors
        [CIElementAttributeMandatory()]
        public CIMessageDictionary( CIContainer aContainer )
            : base( aContainer )
		{
            base.AddSupportedChildType( typeof( CrashItemLib.Crash.Messages.CIMessage ) );
		}

        internal CIMessageDictionary( CIElementList<CIMessage> aList )
            : this( aList.Container )
        {
            base.AddRange( aList.ToArray() );
        }
		#endregion

        #region API
        public void AddRange( CIMessageDictionary aFrom )
        {
            foreach ( CIMessage msg in aFrom )
            {
                if ( !this.Contains( msg ) )
                {
                    base.Add( msg );
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region From CIElement
        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            base.OnFinalize( aParams );
            //
            BuildIndex();
        }

        public override void PrepareRows()
        {
            DataBindingModel.ClearRows();

            // Our data binding model is based upon the object, rather
            // than any key-value-pair properties.
            foreach ( CIMessage msg in this )
            {
                DataBindingModel.Add( msg );
            }
        }
        #endregion

        #region Internal methods
        private void BuildIndex()
        {
            CIElementList<CIMessage> messages = Container.ChildrenByType<CIMessage>( TChildSearchType.EEntireHierarchy );
            foreach ( CIMessage message in messages )
            {
                if ( !base.Contains( message.Id ) )
                {
                    base.Add( message );
                }
            }
        }
        #endregion

        #region Data members
        #endregion
    }
}
