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
using System.Xml;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm.Registers.EmbeddedTrace;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Utilities;
using SymbianETMLib.Common.Exception;
using SymbianETMLib.Common.Config;
using SymbianETMLib.ETB.Buffer;

namespace SymbianETMLib.ETB.Config
{
    public class ETBConfig : ETConfigBase
    {
        #region Constructors
        public ETBConfig( ETBBuffer aBuffer )
            : base( aBuffer )
        {
        }

        public ETBConfig( ETBBuffer aBuffer, string aETBXmlFile )
            : this( aBuffer )
        {
            bool loaded = ExtractFromXml( aETBXmlFile );
            if ( !loaded )
            {
                throw new ETMException( "ERROR: XML input data is corrupt or invalid" );
            }
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint RegisterETBRamWritePointer
        {
            get { return iRegistersETB[ TArmRegisterType.EArmReg_ETB_RamWritePointer ].Value; }
            set { iRegistersETB[ TArmRegisterType.EArmReg_ETB_RamWritePointer ].Value = value; }
        }
        #endregion

        #region Internal methods
        protected new ETBBuffer Buffer
        {
            get { return base.Buffer as ETBBuffer; }
        }

        private bool ExtractFromXml( string aFileName )
        {
            bool success = false;
            //
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.IgnoreComments = true;
                settings.CheckCharacters = true;
                settings.IgnoreWhitespace = true;
                //
                using ( XmlReader reader = XmlReader.Create( aFileName, settings ) )
                {
                    XmlDocument document = new XmlDocument();
                    document.Load( reader );
                    XmlElement root = document.DocumentElement;
                    if ( root == null || root.Name != "etb" )
                    {
                        throw new ETMException( "ERROR - document root expected to be \'etb\'" );
                    }
                    else
                    {
                        foreach ( XmlNode node in root )
                        {
                            string nodeName = node.Name.Trim().ToUpper();
                            if ( nodeName == "REGISTERS" )
                            {
                                ExtractXmlRegisters( node );
                            }
                            else if ( nodeName == "EXCEPTION_VECTORS" )
                            {
                                ExtractXmlExceptionVectors( node );
                            }
                            else if ( nodeName == "RAW_DATA" )
                            {
                                ExtractXmlRawData( node );
                            }
                            else if ( nodeName == "THREAD_TABLE" )
                            {
                                ExtractXmlThreadTable( node );
                            }
                        }
                    }
                }

                // Primed okay?
                success = true;

                // Re-order data based upon write pointer value
                if ( RegisterETBRamWritePointer != 0 )
                {
                    Buffer.Reorder( RegisterETBRamWritePointer );
                }
            }
            catch ( System.Exception )
            {
            }
            //
            return success;
        }

        private void ExtractXmlRegisters( XmlNode aNode )
        {
            foreach ( XmlNode node in aNode.ChildNodes )
            {
                string nodeName = node.Name.Trim().ToUpper();
                if ( nodeName == "REGISTER" && node.Attributes.Count == 2 )
                {
                    XmlAttributeCollection attributes = node.Attributes;
                    //
                    XmlAttribute attribName = attributes[ "name" ];
                    XmlAttribute attribValue = attributes[ "value" ];
                    //
                    if ( attribName != null && !string.IsNullOrEmpty( attribName.Value.Trim() ) &&
                         attribValue != null && !string.IsNullOrEmpty( attribValue.Value.Trim() )
                        )
                    {
                        string name = attribName.Value.Trim().ToUpper();
                        uint value = uint.Parse( attribValue.Value, System.Globalization.NumberStyles.HexNumber );
                        //
                        TArmRegisterType regType = ETMTextToEnumConverter.ToRegisterType( name );
                        switch ( regType )
                        {
                        default:
                            break;
                        case TArmRegisterType.EArmReg_ETM_Id:
                            base.RegisterETMId = value;
                            break;
                        case TArmRegisterType.EArmReg_ETM_Control:
                            base.RegisterETMControl = value;
                            break;
                        case TArmRegisterType.EArmReg_ETB_RamWritePointer:
                            RegisterETBRamWritePointer = value;
                            break;
                        }
                    }
                }
            }
        }

        private void ExtractXmlExceptionVectors( XmlNode aNode )
        {
            foreach ( XmlNode node in aNode.ChildNodes )
            {
                string nodeName = node.Name.Trim().ToUpper();
                if ( nodeName == "REGISTER" && node.Attributes.Count == 2 )
                {
                    XmlAttributeCollection attributes = node.Attributes;
                    //
                    XmlAttribute attribName = attributes[ "name" ];
                    XmlAttribute attribValue = attributes[ "value" ];
                    //
                    if ( attribName != null && !string.IsNullOrEmpty( attribName.Value.Trim() ) &&
                         attribValue != null && !string.IsNullOrEmpty( attribValue.Value.Trim() )
                        )
                    {
                        string name = attribName.Value.Trim().ToUpper();
                        uint value = uint.Parse( attribValue.Value, System.Globalization.NumberStyles.HexNumber );
                        //
                        TArmExceptionVector exceptionVectorType = ETMTextToEnumConverter.ToExceptionVector( name );
                        SetExceptionVector( exceptionVectorType, value );
                    }
                }
            }
        }

        private void ExtractXmlThreadTable( XmlNode aNode )
        {
            foreach ( XmlNode node in aNode.ChildNodes )
            {
                string nodeName = node.Name.Trim().ToUpper();
                if ( nodeName == "THREAD" && node.Attributes.Count == 3 )
                {
                    XmlAttributeCollection attributes = node.Attributes;
                    //
                    XmlAttribute attribName = attributes[ "name" ];
                    XmlAttribute attribAddress = attributes[ "address" ];
                    XmlAttribute attribId = attributes[ "id" ];
                    //
                    if ( attribName != null && !string.IsNullOrEmpty( attribName.Value.Trim() ) &&
                         attribAddress != null && !string.IsNullOrEmpty( attribAddress.Value.Trim() )
                        )
                    {
                        string name = attribName.Value.Trim();
                        uint value = uint.Parse( attribAddress.Value, System.Globalization.NumberStyles.HexNumber );

                        // Add the missing colon back in.
                        if ( name.Contains( ":" ) )
                        {
                            name = name.Replace( ":", "::" );
                        }
                        AddContextIdMapping( value, name );
                    }
                }
            }
        }

        private void ExtractXmlRawData( XmlNode aNode )
        {
            List<string> lines = new List<string>();
            //
            foreach ( XmlNode node in aNode.ChildNodes )
            {
                string nodeName = node.Name.Trim().ToUpper();
                if ( nodeName == "DATA" )
                {
                    string line = node.InnerText;
                    lines.Add( line );
                }
                else
                {
                    break;
                }
            }

            // Convert strings to bytes
            List<byte> data = new List<byte>();
            foreach ( string line in lines )
            {
                int len = line.Length;
                if ( len % 2 != 0 )
                {
                    throw new ETMException( "ERROR: Raw data is corrupt - invalid line length" );
                }

                for ( int i = 0; i < len; i += 2 )
                {
                    string byteString = line.Substring( i, 2 );
                    byte b = System.Convert.ToByte( byteString, 16 );
                    data.Add( b );
                }
            }

            // Save entire data
            base.Buffer.AddRange( data.ToArray() );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private ArmETBRegisterCollection iRegistersETB = new ArmETBRegisterCollection();
        #endregion
    }
}
