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
	public class CIEvent : CIElement
    {
        #region Enumerations
        public enum TSpecificType
        {
            /// <summary>
            /// Specific type unknown - nothing can be inferred from the 'type' or 'value' fields
            /// included as properties of this object.
            /// </summary>
            ETypeUnknown = 0,

            /// <summary>
            /// The event relates to a key press and therefore the 'value' field includes a
            /// text-encoded represenation of the keyboard scan code.
            /// </summary>
            ETypeKey
        }
        #endregion

        #region Constructors
        public CIEvent( CIContainer aContainer )
            : base( aContainer )
		{
		}
        #endregion

        #region API
        #endregion

        #region Properties
        public string TypeName
        {
            get { return iTypeName; }
            set { iTypeName = value; }
        }

        public object Value
        {
            get { return iValue; }
            set { iValue = value; }
        }

        public TSpecificType Type
        {
            get { return iType; }
            set { iType = value; }
        }
        #endregion

        #region Operators
        public static implicit operator CIDBRow( CIEvent aEvent )
        {
            CIDBRow row = new CIDBRow();

            // To ensure that the register and cells are correctly associated
            row.Element = aEvent;
            row.Add( new CIDBCell( aEvent.TypeName ) );

            string value = string.Empty;
            if ( aEvent.Value != null )
            {
                value = aEvent.Value.ToString();
            }
            row.Add( new CIDBCell( value ) );
            //
            return row;
        }
        #endregion

        #region Internal methods
        #endregion

        #region From CIElement
        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            try
            {
                if ( iType == TSpecificType.ETypeKey && ( iValue is int ) )
                {
                    // Replace generic scan/key code with key name
                    int keyCode = (int) iValue;
                    string keyName = keyCode.ToString();
                    //
                    if ( aParams.DebugEngine.KeyBindings.IsKeyBindingTableAvailable )
                    {
                        keyName = aParams.DebugEngine.KeyBindings[ keyCode ];
                    }
                    //
                    System.Diagnostics.Debug.Assert( keyName != null );
                    iValue = keyName;
                }
                else
                {
                    // No operation required.
                }
            }
            finally
            {
                base.OnFinalize( aParams );
            }
        }
        #endregion

        #region Data members
        private string iTypeName = string.Empty;
        private object iValue = string.Empty;
        private TSpecificType iType = TSpecificType.ETypeUnknown;
        #endregion
    }
}
