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
using System.IO;
using System.Xml;

namespace SymbianTree
{
	public class SymDocument : SymNodeAddAsChild
	{
		#region Constructors
		public SymDocument()
            : this( Granularity )
		{
		}

        public SymDocument( int aGranularity )
            : this( aGranularity, null )
        {
        }

        public SymDocument( object aData )
            : this( Granularity, aData )
		{
		}
        
        public SymDocument( int aGranularity, object aData )
            : base( aData )
        {
            iCurrentNode = this;
            CreateChildrenListNow( aGranularity );
        }
        #endregion

		#region API
        public void SerializeToXml( string aFileName )
        {
            if ( File.Exists( aFileName ) )
            {
                File.Delete( aFileName );
            }

            using ( XmlTextWriter writer = new XmlTextWriter( aFileName, System.Text.Encoding.UTF8 ) )
            {
                writer.Formatting = Formatting.Indented;
                //
                try
                {
                    base.Serialize( writer );
                }
                catch ( XmlException )
                {
                }
            }
        }

		public void MakeParentCurrent()
		{
			System.Diagnostics.Debug.Assert( CurrentNode.HasParent );
			CurrentNode = CurrentNode.Parent;
		}
		#endregion

		#region Properties
		public SymNode CurrentNode
		{
			get { return iCurrentNode; }
			set
            {
                iPreviousCurrentNode = iCurrentNode;
                iCurrentNode = value;
            }
		}

        public SymNode PreviousCurrentNode
		{
            get { return iPreviousCurrentNode; }
		}
		#endregion

		#region Data members
		private SymNode iCurrentNode;
		private SymNode iPreviousCurrentNode;
		#endregion
	}
}
