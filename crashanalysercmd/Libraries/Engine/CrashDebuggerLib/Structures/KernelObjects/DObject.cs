/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.Collections.Generic;
using System.Text;

namespace CrashDebuggerLib.Structures.KernelObjects
{
    public class DObject : DBase
    {
        #region Enumerations
        public enum TObjectType
        {
	        EThread=0,
	        EProcess,
	        EChunk,
	        ELibrary,
	        ESemaphore,
	        EMutex,
	        ETimer,
	        EServer,
	        ESession,
	        ELogicalDevice,
	        EPhysicalDevice,
	        ELogicalChannel,
	        EChangeNotifier,
	        EUndertaker,
	        EMsgQueue,	
	        EPropertyRef,
	        ECondVar,
	        ENumObjectTypes,	// number of DObject-derived types
	        EObjectTypeAny=-1
        }
        #endregion

        #region Constructors
        public DObject( CrashDebuggerInfo aCrashDebugger, TObjectType aType )
            : base( aCrashDebugger )
        {
            iType = aType;
        }
        #endregion

        #region API
        public static string AsClassName( TObjectType aType )
        {
            string ret = "Unknown";
            //
            switch ( aType )
            {
                case DObject.TObjectType.EThread:
                    ret = "DThread";
                    break;
                case DObject.TObjectType.EProcess:
                    ret = "DProcess";
                    break;
                case DObject.TObjectType.EChunk:
                    ret = "DChunk";
                    break;
                case DObject.TObjectType.ELibrary:
                    ret = "DLibrary";
                    break;
                case DObject.TObjectType.ESemaphore:
                    ret = "DSemaphore";
                    break;
                case DObject.TObjectType.EMutex:
                    ret = "DMutex";
                    break;
                case DObject.TObjectType.ETimer:
                    ret = "DTimer";
                    break;
                case DObject.TObjectType.EServer:
                    ret = "DServer";
                    break;
                case DObject.TObjectType.ESession:
                    ret = "DSession";
                    break;
                case DObject.TObjectType.ELogicalDevice:
                    ret = "DLogicalDevice";
                    break;
                case DObject.TObjectType.EPhysicalDevice:
                    ret = "DPhysicalDevice";
                    break;
                case DObject.TObjectType.ELogicalChannel:
                    ret = "DLogicalChannel";
                    break;
                case DObject.TObjectType.EChangeNotifier:
                    ret = "DChangeNotifier";
                    break;
                case DObject.TObjectType.EUndertaker:
                    ret = "DUndertaker";
                    break;
                case DObject.TObjectType.EMsgQueue:
                    ret = "DMsgQueue";
                    break;
                case DObject.TObjectType.EPropertyRef:
                    ret = "DPropertyRef";
                    break;
                case DObject.TObjectType.ECondVar:
                    ret = "DCondVar";
                    break;
                default:
                    break;
            }

            return ret;
        }

        public static string AsTypeDescription( TObjectType aType )
        {
            string ret = "Unknown";
            //
            switch ( aType )
            {
                case DObject.TObjectType.EThread:
                    ret = "Thread";
                    break;
                case DObject.TObjectType.EProcess:
                    ret = "Process";
                    break;
                case DObject.TObjectType.EChunk:
                    ret = "Chunk";
                    break;
                case DObject.TObjectType.ELibrary:
                    ret = "Library";
                    break;
                case DObject.TObjectType.ESemaphore:
                    ret = "Semaphore";
                    break;
                case DObject.TObjectType.EMutex:
                    ret = "Mutex";
                    break;
                case DObject.TObjectType.ETimer:
                    ret = "Timer";
                    break;
                case DObject.TObjectType.EServer:
                    ret = "Server";
                    break;
                case DObject.TObjectType.ESession:
                    ret = "Session";
                    break;
                case DObject.TObjectType.ELogicalDevice:
                    ret = "Logical Device";
                    break;
                case DObject.TObjectType.EPhysicalDevice:
                    ret = "Physical Device";
                    break;
                case DObject.TObjectType.ELogicalChannel:
                    ret = "Logical Channel";
                    break;
                case DObject.TObjectType.EChangeNotifier:
                    ret = "Change Notifier";
                    break;
                case DObject.TObjectType.EUndertaker:
                    ret = "Undertaker";
                    break;
                case DObject.TObjectType.EMsgQueue:
                    ret = "Message Queue";
                    break;
                case DObject.TObjectType.EPropertyRef:
                    ret = "Property Ref";
                    break;
                case DObject.TObjectType.ECondVar:
                    ret = "Condition Variable";
                    break;
                default:
                    break;
            }

            return ret;
        }
        #endregion

        #region Properties
        public TObjectType Type
        {
            get { return iType; }
        }

        public uint VTable
        {
            get { return iVTable; }
            set { iVTable = value; }
        }

        public int AccessCount
        {
            get { return iAccessCount; }
            set { iAccessCount = value; }
        }

        public uint OwnerAddress
        {
            get { return iOwnerAddress; }
            set { iOwnerAddress = value; }
        }

        public DObject Owner
        {
            get
            {
                DObject ret = CrashDebugger.ObjectByAddress( OwnerAddress );
                return ret;
            }
        }

        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public string ClassName
        {
            get
            {
                string ret = AsClassName( iType );
                return ret;
            }
        }

        public string TypeDescription
        {
            get
            {
                string ret = AsTypeDescription( iType );
                return ret;
            }
        }

        public bool Tagged
        {
            get { return iTagged; }
            set { iTagged = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From DBase
        public override string ToClipboard()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( TypeDescription.ToUpper() + " [" + Name + "] " + base.ToClipboard() );
            ret.AppendFormat( " VTable: 0x{0:x8} Owner: 0x{1:x8},  AccessCount: {2}", VTable, OwnerAddress, AccessCount );
            //
            return ret.ToString();
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Data members
        private readonly TObjectType iType;
        private uint iVTable = 0;
        private int iAccessCount = 0;
        private uint iOwnerAddress = 0;
        private string iName = string.Empty;
        private bool iTagged = true;
        #endregion
    }
}
