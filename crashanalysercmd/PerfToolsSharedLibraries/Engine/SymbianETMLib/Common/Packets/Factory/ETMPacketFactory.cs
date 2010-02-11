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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using SymbianUtils.BasicTypes;
using SymbianETMLib.Common.Types;

namespace SymbianETMLib.Common.Packets.Factory
{
    public static class ETMPacketFactory
    {
        public static ETMPcktBase Create( SymByte aByte )
        {
            ETMPcktBase ret = null;
            //
            if ( iPackets.Count == 0 )
            {
                CreatePackets();
            }
            //
            ret = FindPacket( aByte );
            //
            return ret;
        }

        #region Internal methods
        private static void CreatePackets()
        {
            Type pluginType = typeof( ETMPcktBase );
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            foreach ( Type type in types )
            {
                if ( !type.IsAbstract && pluginType.IsAssignableFrom( type ) )
                {
                    ConstructorInfo ctor = type.GetConstructor( System.Type.EmptyTypes );
                    if ( ctor != null )
                    {
                        object instance = Activator.CreateInstance( type, null );
                        if ( instance != null )
                        {
                            iPackets.Add( (ETMPcktBase) instance );
                        }
                    }
                }
            }

            // Sort the packets into priority order
            Comparison<ETMPcktBase> comparer = delegate( ETMPcktBase aLeft, ETMPcktBase aRight )
            {
                return ( aLeft.Priority.CompareTo( aRight.Priority ) * -1 );
            };
            iPackets.Sort( comparer );
        }

        private static ETMPcktBase FindPacket( SymByte aByte )
        {
            ETMPcktBase ret = new ETMPcktUnknown();
            //
            int count = iPackets.Count;
            for ( int i = 0; i < count; i++ )
            {
                ETMPcktBase packet = iPackets[ i ];
                if ( packet.Matches( aByte ) )
                {
                    ret = packet;
                    ret.RawByte = aByte;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private static List<ETMPcktBase> iPackets = new List<ETMPcktBase>();
        #endregion
    }
}
