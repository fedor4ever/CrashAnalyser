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
using CrashItemLib.Crash.Messages;
using CrashAnalyserEngine.Plugins;
using CrashItemLib.Crash.Container;

namespace CAPCrashAnalysis.CommandLine
{
	internal class CACmdLineMessageList : IEnumerable<CACmdLineMessage>
	{
        #region Constructors
        public CACmdLineMessageList()
		{
		}
        #endregion

		#region API
        public void Clear()
        {
            iMessages.Clear();
        }

        public void ClearErrorsAndWarnings()
        {
            for( int i=iMessages.Count; i>=0; i-- )
            {
                CACmdLineMessage msg = iMessages[ i ];
                //
                bool remove = true;
                switch( msg.Type )
                {
                default:
                case CACmdLineMessage.TType.ETypeError:
                case CACmdLineMessage.TType.ETypeMessage:
                case CACmdLineMessage.TType.ETypeWarning:
                    break;
                case CACmdLineMessage.TType.ETypeDiagnostic:
                    remove = false;
                    break;
                }
                //
                if ( remove )
                {
                    iMessages.RemoveAt( i );
                }
            }
        }

        public void AddError( string aTitle, string aDescription )
        {
            Add( aTitle, aDescription, CACmdLineMessage.TType.ETypeError );
        }

        public void AddWarning( string aTitle, string aDescription )
        {
            Add( aTitle, aDescription, CACmdLineMessage.TType.ETypeWarning );
        }

        public void AddMessage( string aTitle, string aDescription )
        {
            Add( aTitle, aDescription, CACmdLineMessage.TType.ETypeMessage );
        }

        public void AddDiagnostic( string aTitle, string aDescription )
        {
            Add( aTitle, aDescription, CACmdLineMessage.TType.ETypeDiagnostic );
        }

        public void Add( CACmdLineMessage aMessage )
        {
            iMessages.Add( aMessage );
        }

        public void AddRange( IEnumerable<CACmdLineMessage> aMessages )
        {
            foreach ( CACmdLineMessage msg in aMessages )
            {
                Add( msg );
            }
        }

        public void AddRange( IEnumerable<CACmdLineMessage> aMessages, CACmdLineMessage.TType aOnlyOfType )
        {
            foreach ( CACmdLineMessage msg in aMessages )
            {
                if ( msg.Type == aOnlyOfType )
                {
                    Add( msg );
                }
            }
        }

        public void CopyMessagesToContainer( CIContainer aContainer )
        {
            CopyMessagesToContainer( iMessages, aContainer );
        }

        public static void CopyMessagesToContainer( IEnumerable<CACmdLineMessage> aMessages, CIContainer aContainer )
        {
            foreach ( CACmdLineMessage msg in aMessages )
            {
                msg.CopyToContainer( aContainer );
            }
        }

        public CACmdLineMessage[] ToArray()
        {
            return iMessages.ToArray();
        }
        #endregion

		#region Properties
        public bool IsEmtpy
        {
            get
            {
                return iMessages.Count == 0;
            }
        }

        public int Count
        {
            get { return iMessages.Count; }
        }
        #endregion

        #region Internal methods
        private int CountByType( CACmdLineMessage.TType aType )
        {
            int count = 0;
            //
            iMessages.ForEach( delegate( CACmdLineMessage aMessage )
            {
                if ( aMessage.Type == CACmdLineMessage.TType.ETypeError )
                {
                    ++count;
                }
            }
            );
            //
            return count;
        }

        private void Add( string aTitle, string aDescription, CACmdLineMessage.TType aType )
        {
            CACmdLineMessage msg = new CACmdLineMessage( aType, aTitle, aDescription );
            Add( msg );
        }
        #endregion

        #region From IEnumerable<CACmdLineMessage>
        public IEnumerator<CACmdLineMessage> GetEnumerator()
        {
            foreach ( CACmdLineMessage msg in iMessages )
            {
                yield return msg;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CACmdLineMessage msg in iMessages )
            {
                yield return msg;
            }
        }
        #endregion

        #region Data members
        private List<CACmdLineMessage> iMessages = new List<CACmdLineMessage>();
        #endregion
	}
}
