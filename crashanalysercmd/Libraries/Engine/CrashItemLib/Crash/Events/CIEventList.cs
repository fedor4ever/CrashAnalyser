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
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Utils;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Events
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CIEventList : CIElement, IEnumerable<CIEvent>
    {
        #region Constructors
        [CIElementAttributeMandatory()]
        public CIEventList( CIContainer aContainer )
            : base( aContainer )
        {
            // Restrict children to events
            base.AddSupportedChildType( typeof( CIEvent ) );
            base.AddSupportedChildType( typeof( CrashItemLib.Crash.Messages.CIMessage ) );
        }
        #endregion

        #region From IEnumerable<CIEvent>
        public new IEnumerator<CIEvent> GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CIEvent )
                {
                    CIEvent reg = (CIEvent) element;
                    yield return reg;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CIEvent )
                {
                    CIEvent reg = (CIEvent) element;
                    yield return reg;
                }
            }
        }
        #endregion

        #region From CIElementBase
        public override void PrepareRows()
        {
            DataBindingModel.ClearRows();

            // Our data binding model is based upon the event object, rather
            // than any key-value-pair properties.
            foreach ( CIEvent e in this )
            {
                DataBindingModel.Add( e );
            }
        }
        #endregion
    }
}
