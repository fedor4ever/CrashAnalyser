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
using SymbianParserLib.BaseStructures;
using SymbianParserLib.Elements;
using SymbianParserLib.Elements.SubFields;

namespace SymbianParserLib.ValueStores
{
    public class ValueStore
    {
        #region Enumerations
        internal enum TValueStoreType
        {
            EValueStoreTypeProperty = 0,
            EValueStoreTypeMethod,
            EValueStoreTypeStoreInternally
        }
        #endregion

        #region Constructors
        public ValueStore()
        {
        }
        #endregion

        #region API
        public void SetTargetProperty( object aPropertyObject, string aPropertyName )
        {
            iValueStore = aPropertyObject;
            iValueStoreType = TValueStoreType.EValueStoreTypeProperty;
            iValueStoreMemberName = aPropertyName;
            //
        }

        public void SetTargetMethod( object aMethodObject, string aMethodName, params TValueStoreMethodArguments[] aArgs )
        {
            iValueStore = aMethodObject;
            iValueStoreType = TValueStoreType.EValueStoreTypeMethod;
            iValueStoreMemberName = aMethodName;
            //
            iMethodArgumentSpecifier = aArgs;
        }
        #endregion

        #region Internal API
        internal void SetValue( ParserFieldFormatSpecifier aFieldFormatSpecifier, ParserFieldFormatValue aFieldFormatValue )
        {
            CheckForValidValueStore();
            //
            if ( iValueStoreType == TValueStoreType.EValueStoreTypeProperty )
            {
                // Store value to user-supplied property within object
                Binder binder = null;
                Type typeInfo = iValueStore.GetType();
                PropertyInfo propInfo = typeInfo.GetProperty( iValueStoreMemberName, PropertyBindingFlags );
                if ( propInfo == null )
                {
                    throw new MissingMemberException( "A property called: \"" + iValueStoreMemberName + "\" was not found within object: " + iValueStore.ToString() );
                }
                //
                Type propType = propInfo.PropertyType;
                object[] args = PrepareMethodArgument( aFieldFormatValue.Value, propType );
                typeInfo.InvokeMember( iValueStoreMemberName, PropertyBindingFlags, binder, iValueStore, args );
            }
            else if ( iValueStoreType == TValueStoreType.EValueStoreTypeMethod )
            {
                object[] args = null;
                //
                try
                {
                    // Store value to user-supplied method
                    Binder binder = null;

                    // Build arguments
                    Type valueTypeInfo = aFieldFormatValue.Value.GetType();
                    TValueStoreMethodArguments[] argumentSpecification = BuildMethodArgumentSpecification( valueTypeInfo, iValueStore, iValueStoreMemberName );
                    args = BuildCustomFunctionArguments( argumentSpecification, aFieldFormatSpecifier, aFieldFormatValue );
                    
                    // Sanity check number of arguments for method implementation actually agrees with our run-time generated
                    // object array of parameters.
                    Type valueStoreTypeInfo = iValueStore.GetType();
                    MethodInfo methodInfo = valueStoreTypeInfo.GetMethod( iValueStoreMemberName, MethodBindingFlags );
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    if ( args.Length != methodParams.Length )
                    {
                        throw new MissingMethodException( "Argument specification doesn't align with method implementation" );
                    }
                    else
                    {
                        valueStoreTypeInfo.InvokeMember( iValueStoreMemberName, MethodBindingFlags, binder, iValueStore, args );
                    }
                }
                catch ( Exception exception )
                {
                    if ( exception is TargetInvocationException || 
                         exception is MissingMethodException || 
                         exception is MissingMemberException || 
                         exception is AmbiguousMatchException )
                    {
                        StringBuilder funcDetails = new StringBuilder();
                        funcDetails.Append( iValueStoreMemberName + "( " );
                        //
                        int count = ( args != null ) ? args.Length : 0;
                        for ( int i = 0; i < count; i++ )
                        {
                            object arg = args[ i ];
                            string argTypeName = ( arg != null ) ? arg.GetType().ToString() : "null";
                            funcDetails.Append( argTypeName );
                            //
                            if ( i < count - 1 )
                            {
                                funcDetails.Append( ", " );
                            }
                        }
                        //
                        funcDetails.Append( " )" );
                        System.Diagnostics.Debug.WriteLine( "Failed to invoke method: " + funcDetails.ToString() );
                    }
                    else
                    {
                        throw exception;
                    }
                }
            }
            else if ( iValueStoreType == TValueStoreType.EValueStoreTypeStoreInternally )
            {
                // Store it in the value store and the client can extract it via the public properties...
                iValueStore = aFieldFormatValue.Value;
            }
        }
        #endregion

        #region Properties
        public bool IsInt
        {
            get
            {
                bool ret = IsDynamicAndOfType( typeof( int ) );
                return ret;
            }
        }

        public bool IsUint
        {
            get
            {
                bool ret = IsDynamicAndOfType( typeof( uint ) );
                return ret;
            }
        }

        public bool IsString
        {
            get
            {
                bool ret = IsDynamicAndOfType( typeof( string ) );
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
                    ret = (int) iValueStore;
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
                    ret = (uint) iValueStore;
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
                    ret = (string) iValueStore;
                }
                //
                return ret;
            }
        }

        internal TValueStoreType ValueStoreType
        {
            get
            {
                return iValueStoreType;
            }
        }

        internal BindingFlags MethodBindingFlags
        {
            get
            {
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod;
                return bindingFlags;
            }
        }

        internal BindingFlags PropertyBindingFlags
        {
            get
            {
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty;
                return bindingFlags;
            }
        }
        #endregion

        #region Internal methods
        private bool IsDynamicAndOfType( Type aExpectedType )
        {
            bool ret = false;
            //
            object vs = iValueStore;
            TValueStoreType vsType = ValueStoreType;
            if ( vs != null && vsType == TValueStoreType.EValueStoreTypeStoreInternally )
            {
                Type typeInfo = vs.GetType();
                ret = ( typeInfo == aExpectedType );
            }
            //
            return ret;
        }

        private void CheckForValidValueStore()
        {
            if ( iValueStoreType == TValueStoreType.EValueStoreTypeProperty ||
                 iValueStoreType == TValueStoreType.EValueStoreTypeMethod )
            {
                if ( iValueStore == null )
                {
                    throw new Exception( "Missing value store" );
                }
            }
        }

        private object[] PrepareMethodArgument( object aValue, Type aExpectedPropertyType )
        {
            // If we're calling a property where the expected property type doesn't match the value
            // we have been asked to use (for example, a property expects an enumerator, but we are given
            // a uint or int) then we must convert from the value type to the expected property type.
            object[] ret = { aValue };
            Type valueType = aValue.GetType();
            //
            if ( aExpectedPropertyType != valueType )
            {
                // Get a type converter to perform the operation. Note that some of the built-in type
                // converters are very "dumb" in that they only convert from strings to numbers (and vice
                // versa). This doesn't help us when we need to convert from e.g. a uint to a long.
                TypeConverter conv = TypeDescriptor.GetConverter( aExpectedPropertyType );
                if ( conv == null )
                {
                    throw new NotSupportedException( "No type converter exists to convert between " + valueType.ToString() + " and " + aExpectedPropertyType.ToString() );
                }
                else if ( conv.CanConvertFrom( valueType ) )
                {
                    object converted = conv.ConvertFrom( aValue );
                    ret = new object[] { converted };
                }
                else
                {
                    // Might be one of the built-in type converters that only works
                    // with strings. Convert the value to a string and try once more
                    string asString = aValue.ToString();
                    try
                    {
                        object converted = conv.ConvertFrom( asString );
                        ret = new object[] { converted };
                    }
                    catch ( NotSupportedException )
                    {
                        throw new NotSupportedException( "No type converter exists to convert between " + valueType.ToString() + " and " + aExpectedPropertyType.ToString() );
                    }
                }
            }
            //
            return ret;
        }

        private TValueStoreMethodArguments[] BuildMethodArgumentSpecification( Type aValueTypeInfo, object aObject, string aMethodName )
        {
            List<TValueStoreMethodArguments> args = new List<TValueStoreMethodArguments>();
            //
            if ( iMethodArgumentSpecifier == null )
            {
                iMethodArgumentSpecifier = new TValueStoreMethodArguments[] { TValueStoreMethodArguments.EValueStoreMethodArgumentCalculateAtRuntime };
            }
            //
            bool done = false;
            int count = iMethodArgumentSpecifier.Length;
            for( int i=0; i<count && !done; i++ )
            {
                TValueStoreMethodArguments argType = iMethodArgumentSpecifier[ i ];
                switch ( argType )
                {
                default:
                    args.Add( argType );
                    break;
                case TValueStoreMethodArguments.EValueStoreMethodArgumentCalculateAtRuntime:
                    args.Clear();
                    BuildRuntimeGeneratedArgumentList( aValueTypeInfo, aObject, aMethodName, ref args );
                    done = true;
                    break;
                }
            }
            //
            return args.ToArray();
        }

        private void BuildRuntimeGeneratedArgumentList( Type aValueTypeInfo, object aObject, string aMethodName, ref List<TValueStoreMethodArguments> aArgs )
        {
            Type typeInfo = aObject.GetType();
            MethodInfo methodInfo = typeInfo.GetMethod( aMethodName, MethodBindingFlags );
            if ( methodInfo == null )
            {
                throw new MissingMemberException( "Method: " + aMethodName + " was not found within object: " + aObject.ToString() );
            }
            ParameterInfo[] methodParams = methodInfo.GetParameters();

            // Check if the parameters include an explicit request for a ParserFieldName object
            // If so, we'll not send the "argument name as string" since the client has
            // explicitly requested it as an object...
            bool requestingArgNameAsObject = false;
            foreach ( ParameterInfo info in methodParams )
            {
                if ( info.ParameterType == typeof( ParserFieldName ) )
                {
                    requestingArgNameAsObject = true;
                    break;
                }
            }

            // Second pass to build real list...
            foreach ( ParameterInfo info in methodParams )
            {
                Type paramType = info.ParameterType;
                //
                if ( paramType == typeof( ParserFieldName ) )
                {
                    aArgs.Add( TValueStoreMethodArguments.EValueStoreMethodArgumentNameAsObject );
                }
                else if ( paramType == typeof( ParserField ) )
                {
                    aArgs.Add( TValueStoreMethodArguments.EValueStoreMethodArgumentField );
                }
                else if ( paramType == typeof( ParserLine ) )
                {
                    aArgs.Add( TValueStoreMethodArguments.EValueStoreMethodArgumentLine );
                }
                else if ( paramType == typeof( ParserParagraph ) )
                {
                    aArgs.Add( TValueStoreMethodArguments.EValueStoreMethodArgumentParagraph );
                }
                else if ( paramType == typeof( string ) && !requestingArgNameAsObject )
                {
                    // Best guess - if the first argument of the method is actually
                    // a string-based "value" argument (rather than field name argument) then we'll
                    // pass the parameter out of order - hence the "name as object" approach used
                    // above, which let's us accurately infer type...
                    aArgs.Add( TValueStoreMethodArguments.EValueStoreMethodArgumentNameAsString );
                }
                else if ( paramType == aValueTypeInfo )
                {
                    aArgs.Add( TValueStoreMethodArguments.EValueStoreMethodArgumentValue );
                }
            }
        }

        private object[] BuildCustomFunctionArguments( TValueStoreMethodArguments[] aArgumentSpecification, ParserFieldFormatSpecifier aFieldFormatSpecifier, ParserFieldFormatValue aFieldFormatValue )
        {

            List<object> args = new List<object>();
            //
            foreach ( TValueStoreMethodArguments argType in aArgumentSpecification )
            {
                if ( argType == TValueStoreMethodArguments.EValueStoreMethodArgumentNameAsString )
                {
                    args.Add( aFieldFormatSpecifier.Name.ToString() );
                }
                else if ( argType == TValueStoreMethodArguments.EValueStoreMethodArgumentNameAsObject )
                {
                    args.Add( aFieldFormatSpecifier.Name );
                }
                else if ( argType == TValueStoreMethodArguments.EValueStoreMethodArgumentValue )
                {
                    args.Add( aFieldFormatValue.Value );
                }
                else if ( argType == TValueStoreMethodArguments.EValueStoreMethodArgumentParagraph )
                {
                    ParserParagraph para = null;
                    //
                    if ( aFieldFormatSpecifier.Field.Parent != null && aFieldFormatSpecifier.Field.Parent is ParserLine )
                    {
                        ParserLine line = (ParserLine) aFieldFormatSpecifier.Field.Parent;
                        if ( line.Parent != null && line.Parent is ParserParagraph )
                        {
                            para = (ParserParagraph) line.Parent;
                        }
                    }
                    //
                    args.Add( para );
                }
                else if ( argType == TValueStoreMethodArguments.EValueStoreMethodArgumentLine )
                {
                    ParserLine line = null;
                    //
                    if ( aFieldFormatSpecifier.Field.Parent != null && aFieldFormatSpecifier.Field.Parent is ParserLine )
                    {
                        line = (ParserLine) aFieldFormatSpecifier.Field.Parent;
                    }
                    //
                    args.Add( line );
                }
                else if ( argType == TValueStoreMethodArguments.EValueStoreMethodArgumentField )
                {
                    args.Add( aFieldFormatSpecifier.Field );
                }
                else
                {
                    System.Diagnostics.Debug.Assert( false, "Argument specification contains unresolved runtime reference" );
                }
            }
            //
            return args.ToArray();
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        // Common members
        private object iValueStore = null;
        private string iValueStoreMemberName = string.Empty;
        private TValueStoreType iValueStoreType = TValueStoreType.EValueStoreTypeStoreInternally;

        // Method-specific members
        private TValueStoreMethodArguments[] iMethodArgumentSpecifier = null;
        #endregion
    }
}
