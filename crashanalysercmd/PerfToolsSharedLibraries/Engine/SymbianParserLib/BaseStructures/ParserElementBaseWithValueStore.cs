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
using System.ComponentModel;
using SymbianParserLib.Enums;
using SymbianParserLib.ValueStores;
using SymbianParserLib.Elements.SubFields;

namespace SymbianParserLib.BaseStructures
{
    public abstract class ParserElementBaseWithValueStore : ParserElementBase
    {
        #region Constructors
        protected ParserElementBaseWithValueStore()
            : this( string.Empty )
        {
        }

        protected ParserElementBaseWithValueStore( string aName )
            : base( aName )
        {
        }
        #endregion

        #region Abstract API
        internal abstract void SetTargetProperty( object aPropertyObject, string aPropertyName, int aIndex );

        internal virtual void SetValue( ParserFieldFormatSpecifier aFormat, ParserFieldFormatValue aValue )
        {
            ValueStore vs = GetValueStore( this );
            if ( vs == null )
            {
                // Make a new "store internally" value store
                iValueStore = new ValueStore();
                vs = iValueStore;
            }
            //
            vs.SetValue( aFormat, aValue );
        }
        #endregion

        #region API
        public virtual void SetTargetObject()
        {
            iValueStore = new ValueStore();
        }

        public virtual void SetTargetProperty( object aPropertyObject, string aPropertyName )
        {
            SetTargetProperty( aPropertyObject, aPropertyName, KGloballyApplicable );
        }

        public virtual void SetTargetProperties( object aPropertyObjects, params string[] aPropertyNames )
        {
            if ( aPropertyObjects == null )
            {
                throw new ArgumentException( "Property object cannot be null" );
            }

            int count = aPropertyNames.Length;
            if ( count == 0 )
            {
                throw new ArgumentException( "Property name array must not be empty" );
            }

            for ( int i = 0; i < count; i++ )
            {
                string propName = aPropertyNames[ i ];
                if ( propName.Length == 0 )
                {
                    throw new ArgumentException( "Property name is invalid" );
                }

                SetTargetProperty( aPropertyObjects, propName, i );
            }
        }

        public virtual void SetTargetProperties( object[] aPropertyObjects, params string[] aPropertyNames )
        {
            if ( aPropertyObjects.Length == 0 || aPropertyNames.Length == 0 || aPropertyObjects.Length != aPropertyNames.Length )
            {
                throw new ArgumentException( "Property object/names are invalid" );
            }
            //
            int count = aPropertyNames.Length;
            for ( int i = 0; i < count; i++ )
            {
                object obj = aPropertyObjects[ i ];
                if ( obj == null )
                {
                    throw new ArgumentException( "Property object cannot be null" );
                }

                string propName = aPropertyNames[ i ];
                if ( propName.Length == 0 )
                {
                    throw new ArgumentException( "Property name is invalid" );
                }

                SetTargetProperty( obj, propName, i );
            }
        }

        public virtual void SetTargetMethod( object aMethodObject, string aMethodName )
        {
            SetTargetMethod( aMethodObject, aMethodName, TValueStoreMethodArguments.EValueStoreMethodArgumentCalculateAtRuntime );
        }

        public virtual void SetTargetMethod( object aMethodObject, string aMethodName, params TValueStoreMethodArguments[] aMethodArgs )
        {
            // Check that the args are okay.
            foreach ( TValueStoreMethodArguments argType in aMethodArgs )
            {
                if ( argType == TValueStoreMethodArguments.EValueStoreMethodArgumentCalculateAtRuntime )
                {
                    if ( aMethodArgs.Length != 1 )
                    {
                        throw new ArgumentException( "Method arguments must contain only a single entry when using \'calculate at runtime\' approach" );
                    }
                }
            }

            iValueStore = new ValueStore();
            iValueStore.SetTargetMethod( aMethodObject, aMethodName, aMethodArgs );
        }
        #endregion

        #region Properties
        public bool IsInt
        {
            get
            {
                bool ret = ( iValueStore != null ) && iValueStore.IsInt;
                return ret;
            }
        }

        public bool IsUint
        {
            get
            {
                bool ret = ( iValueStore != null ) && iValueStore.IsUint;
                return ret;
            }
        }

        public bool IsString
        {
            get
            {
                bool ret = ( iValueStore != null ) && iValueStore.IsString;
                return ret;
            }
        }

        public int AsInt
        {
            get
            {
                int ret = 0;
                //
                if ( IsInt )
                {
                    ret = iValueStore.AsInt;
                }
                //
                return ret;
            }
        }

        public uint AsUint
        {
            get
            {
                uint ret = 0;
                //
                if ( IsUint )
                {
                    ret = iValueStore.AsUint;
                }
                //
                return ret;
            }
        }

        public string AsString
        {
            get
            {
                string ret = string.Empty;
                //
                if ( IsString )
                {
                    ret = iValueStore.AsString;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal static methods
        private static ValueStore GetValueStore( ParserElementBase aElement )
        {
            ValueStore ret = null;
            //
            if ( aElement is ParserElementBaseWithValueStore )
            {
                ParserElementBaseWithValueStore element = (ParserElementBaseWithValueStore) aElement;
                if ( element.iValueStore != null )
                {
                    ret = element.iValueStore;
                }
                else if ( element.Parent != null )
                {
                    ret = GetValueStore( element.Parent );
                }
            }
            //
            return ret;
        }
        #endregion

        #region Internal constant
        protected const int KGloballyApplicable = -1;
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.Name;
        }
        #endregion

        #region Data members
        protected ValueStore iValueStore = null;
        #endregion
    }
}
