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
using CrashItemLib.Crash.Container;

namespace CrashAnalyserServerExe.Engine
{
	internal class CACmdLineMessage
    {
        #region Enumerations
        public enum TType
        {
            ETypeMessage = 0,
            ETypeWarning,
            ETypeError,
            ETypeDiagnostic
        }
        #endregion

        #region Constructors
        public CACmdLineMessage( TType aType, string aTitle, string aDescription )
		{
            iType = aType;
            iTitle = aTitle;
            iDescription = aDescription;
		}
        #endregion

		#region API
        public void CopyToContainer( CIContainer aContainer )
        {
            // Diagnostics never appear in the crash item itself.
            if ( Type != TType.ETypeDiagnostic )
            {
                CIMessage msg = null;
                //
                if ( Type == TType.ETypeMessage )
                {
                    msg = CIMessage.NewMessage( aContainer );
                }
                else if ( Type == TType.ETypeWarning )
                {
                    msg = new CIMessageWarning( aContainer );
                }
                else if ( Type == TType.ETypeError )
                {
                    msg = new CIMessageError( aContainer );
                }

                // Build details & add to container
                msg.Title = this.Title;
                msg.Description = this.Description;
                aContainer.Messages.Add( msg );
            }
        }
        #endregion

		#region Properties
        public TType Type
        {
            get { return iType; }
        }

        public string Title
        {
            get { return iTitle; }
        }

        public string Description
        {
            get { return iDescription; }
        }
        #endregion

        #region Data members
        private readonly TType iType;
        private readonly string iTitle;
        private readonly string iDescription;
        #endregion
	}
}
