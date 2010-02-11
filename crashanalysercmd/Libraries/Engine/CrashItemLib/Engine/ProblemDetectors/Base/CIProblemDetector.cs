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
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Summarisable;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Engine.ProblemDetectors
{
    internal abstract class CIProblemDetector
    {
        #region Constructors
        protected CIProblemDetector()
		{
        }
		#endregion

        #region API
        public abstract void Check( CIContainer aContainer );

        public virtual long Priority
        {
            get { return long.MinValue; }
        }
        #endregion

        #region Internal methods
        protected string CreateIdentifierText( CISummarisableEntity aEntry )
        {
            StringBuilder ret = new StringBuilder();
            //
            if ( aEntry.IsAvailable( CISummarisableEntity.TElement.EElementThread ) )
            {
                ret.AppendFormat( LibResources.CIProblemDetector_Msg_Thread, aEntry.Thread.FullName );
            }
            else if ( aEntry.IsAvailable( CISummarisableEntity.TElement.EElementStack ) )
            {
                ret.AppendFormat( LibResources.CIProblemDetector_Msg_Stack, ArmRegisterBankUtils.BankAsStringLong( aEntry.Stack.Type ) );
            }
            //
            return ret.ToString();
        }

        protected void CreateMessage( CIContainer aContainer, CIElement aAssociatedElement, string aTitle )
        {
            CreateMessage( aContainer, aAssociatedElement, aTitle, string.Empty );
        }

        protected void CreateMessage( CIContainer aContainer, CIElement aAssociatedElement, string aTitle, string aDescription )
        {
            CIMessage message = CIMessage.NewMessage( aContainer );
            message.Title = aTitle;
            message.Description = aDescription;
            //
            aAssociatedElement.AddChild( message );
        }

        protected void CreateWarning( CIContainer aContainer, CIElement aAssociatedElement, string aTitle )
        {
            CreateWarning( aContainer, aAssociatedElement, aTitle, string.Empty );
        }

        protected void CreateWarning( CIContainer aContainer, CIElement aAssociatedElement, string aTitle, string aDescription )
        {
            CIMessageWarning message = new CIMessageWarning( aContainer, aTitle );
            message.Description = aDescription;
            //
            aAssociatedElement.AddChild( message );
        }

        protected void CreateError( CIContainer aContainer, CIElement aAssociatedElement, string aTitle )
        {
            CreateError( aContainer, aAssociatedElement, aTitle, string.Empty );
        }

        protected void CreateError( CIContainer aContainer, CIElement aAssociatedElement, string aTitle, string aDescription )
        {
            CIMessageError message = new CIMessageError( aContainer, aTitle );
            message.Description = aDescription;
            //
            aAssociatedElement.AddChild( message );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
		#endregion
    }
}
