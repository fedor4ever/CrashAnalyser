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
using System.IO;
using CrashItemLib.Engine;
using CrashXmlPlugin.FileFormat.Node;
using CrashXmlPlugin.FileFormat.Document;
using CrashXmlPlugin.FileFormat.Segment.Entries;
using SymbianUtils.PluginManager;

namespace CrashXmlPlugin.FileFormat.Segment
{
	internal class CXmlNodeSegmentTable : CXmlNode, IEnumerable<CXmlSegBase>
	{
		#region Constructors
        public CXmlNodeSegmentTable()
            : base( Constants.SegmentTable )
		{
            AddChildren();
        }
		#endregion

        #region From CXmlNode
        public override void XmlSerialize( CXmlDocumentSerializationParameters aParameters )
        {
            try
            {
                // Serialize data to string
                using ( CXmlDocumentSerializationParameters newParams = new CXmlDocumentSerializationParameters( aParameters, aParameters.Document, iSerializedData ) )
                {
                    base.XmlSerialize( newParams );

                    // The serialized XML data which is stored to this class' data member will
                    // be written later on by the segment dictionary.
                    newParams.Writer.Flush();

                    // Tidy up head and tail so that it matches the rest of the document.
                    string indent = aParameters.Writer.Settings.IndentChars;
                    string text = iSerializedData.ToString();
                    using ( StringReader reader = new StringReader( text ) )
                    {
                        iSerializedData.Length = 0;
                        string line = reader.ReadLine();
                        while ( line != null )
                        {
                            iSerializedData.AppendLine( indent + line );
                            line = reader.ReadLine();
                        }
                    }
                    iSerializedData.Insert( 0, System.Environment.NewLine );
                }
            }
            catch ( Exception )
            {
            }
        }
        #endregion

        #region Properties
        internal string SerializedData
        {
            get { return iSerializedData.ToString(); }
        }
        #endregion

        #region Internal methods
        private void AddChildren()
        {
            PluginManager<CXmlSegBase> plugins = new PluginManager<CXmlSegBase>( 1 );
            plugins.LoadFromCallingAssembly();
            
            // Sort them by priority
            SortedList<int, CXmlSegBase> list = new SortedList<int,CXmlSegBase>();
            foreach( CXmlSegBase seg in plugins )
            {
                list.Add( seg.SegmentPriority, seg );
            }

            // Now they are sorted, add them as children
            foreach ( KeyValuePair<int, CXmlSegBase> kvp in list )
            {
                base.Add( kvp.Value );
            }
        }
        #endregion

        #region From IEnumerable<CXmlSegBase>
        public IEnumerator<CXmlSegBase> GetEnumerator()
        {
            foreach ( SymbianTree.SymNode node in base.Children )
            {
                if ( node is CXmlSegBase )
                {
                    CXmlSegBase ret = (CXmlSegBase) node;
                    yield return ret;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( SymbianTree.SymNode node in base.Children )
            {
                if ( node is CXmlSegBase )
                {
                    CXmlSegBase ret = (CXmlSegBase) node;
                    yield return ret;
                }
            }
        }
        #endregion

        #region Data members
        private StringBuilder iSerializedData = new StringBuilder();
		#endregion
    }
}
