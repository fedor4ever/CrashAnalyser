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
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Threads;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Registers.Special
{
    public class CIRegisterFSR : CIRegister
	{
        #region Enumerations
        public enum TFaultType
        {
            EFaultTypeUnknown = -1,
            EFaultTypeVectorException = 00,
            EFaultTypeAlignmentFault = 01,
            EFaultTypeTerminalException = 02,
            EFaultTypeAlignmentFault2 = 03,
            EFaultTypeExternalAbortOnLinefetchForSectionTranslation = 04,
            EFaultTypeSectionTranslationFault = 05,
            EFaultTypeExternalAbortOnLineFetchForPageTranslation = 06,
            EFaultTypePageTranslationFault = 07,
            EFaultTypeExternalAbortOnNonLinefetchForSectionTranslation = 08,
            EFaultTypeDomainFaultOnSectionTranslation = 09,
            EFaultTypeExternalAbortOnNonLinefetchForPageTranslation = 10,
            EFaultTypeDomainFaultOnPageTranslation = 11,
            EFaultTypeExternalAbortOnFirstLevelTranslation = 12,
            EFaultTypePermissionFaultOnSection = 13,
            EFaultTypeExternalAbortOnSecondLevelTranslation = 14,
            EFaultTypePermissionFaultOnPage = 15
        }
        #endregion
        
        #region Constructors
        public CIRegisterFSR( CIRegisterList aCollection, uint aValue )
            : base( aCollection, TArmRegisterType.EArmReg_FSR, aValue )
        {
            PrepareMessage();
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TFaultType FaultType
        {
            get
            {
                uint fsr = System.Convert.ToUInt32( base.Value );
                uint fsrMask = 0xF; // Look at first 4 bits
                fsr = fsr & fsrMask;
                //
                TFaultType type = TFaultType.EFaultTypeUnknown;
                if ( fsr >= 0 && fsr < (uint) TFaultType.EFaultTypePermissionFaultOnPage )
                {
                    type = (TFaultType) fsr;
                }
                //
                return type;
            }
        }

        public string FaultDescription
        {
            get
            {
                string ret = "Unknown";
                //
                switch ( FaultType )
                {
                default:
                case TFaultType.EFaultTypeUnknown:
                    break;
                case TFaultType.EFaultTypeVectorException:
                    ret = "Vector exception";
                    break;
                case TFaultType.EFaultTypeAlignmentFault:
                    ret = "Alignment fault";
                    break;
                case TFaultType.EFaultTypeTerminalException:
                    ret = "Terminal exception";
                    break;
                case TFaultType.EFaultTypeAlignmentFault2:
                    ret = "Alignment fault";
                    break;
                case TFaultType.EFaultTypeExternalAbortOnLinefetchForSectionTranslation:
                    ret = "External abort on linefetch for section translation";
                    break;
                case TFaultType.EFaultTypeSectionTranslationFault:
                    ret = "Section translation fault (unmapped virtual address)";
                    break;
                case TFaultType.EFaultTypeExternalAbortOnLineFetchForPageTranslation:
                    ret = "External abort on linefetch for page translation";
                    break;
                case TFaultType.EFaultTypePageTranslationFault:
                    ret = "Page translation fault (unmapped virtual address)";
                    break;
                case TFaultType.EFaultTypeExternalAbortOnNonLinefetchForSectionTranslation:
                    ret = "External abort on non-linefetch for section translation";
                    break;
                case TFaultType.EFaultTypeDomainFaultOnSectionTranslation:
                    ret = "Domain fault on section translation (access to invalid domain)";
                    break;
                case TFaultType.EFaultTypeExternalAbortOnNonLinefetchForPageTranslation:
                    ret = "External abort on non-linefetch for page translation";
                    break;
                case TFaultType.EFaultTypeDomainFaultOnPageTranslation:
                    ret = "Domain fault on page translation (access to invalid domain)";
                    break;
                case TFaultType.EFaultTypeExternalAbortOnFirstLevelTranslation:
                    ret = "External abort on first level translation";
                    break;
                case TFaultType.EFaultTypePermissionFaultOnSection:
                    ret = "Permission fault on section (no permission to access virtual address)";
                    break;
                case TFaultType.EFaultTypeExternalAbortOnSecondLevelTranslation:
                    ret = "External abort on second level translation";
                    break;
                case TFaultType.EFaultTypePermissionFaultOnPage:
                    ret = "Permission fault on page (no permission to access virtual address)";
                    break;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Operators
        public static implicit operator TFaultType( CIRegisterFSR aReg )
        {
            return aReg.FaultType;
        }
        #endregion

        #region Internal methods
        private void PrepareMessage()
        {
            CIMessage message = CIMessage.NewMessage( Container );
            //
            message.Title = "Fault Status";
            message.AddLineFormatted( "The FSR (Fault Status Register) indicates that the processor encountered an fault of type [{0}].", FaultDescription );
            //
            base.AddChild( message );
        }
        #endregion

        #region Data members
        #endregion
    }
}
