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
using System.Drawing;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Container;
using System.ComponentModel;
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;

namespace CrashItemLib.Crash.Messages
{
    [CIDBAttributeColumn( "Type", 0 )]
    [CIDBAttributeColumn( "Overview", 1, true )]
    public class CIMessage : CIElement, IEnumerable<string>
    {
        #region Enumerations
        public enum TType
        {
            [Description( "Warning" )]
            ETypeWarning = 0,

            [Description( "Error" )]
            ETypeError,

            [Description( "Message" )]
            ETypeMessage,

            [Description( "Other" )]
            ETypeOther
        }
        #endregion

        #region Static constructors
        public static CIMessage NewMessage( CIContainer aContainer )
        {
            CIMessage ret = new CIMessage( aContainer );
            ret.Type = TType.ETypeMessage;
            return ret;
        }

        internal static CIMessage Null( CIContainer aContainer )
        {
            CIMessage ret = new CIMessage( aContainer );
            ret.Type = TType.ETypeOther;
            return ret;
        }
        #endregion

        #region Constructors
        protected CIMessage( CIContainer aContainer )
            : this( aContainer, string.Empty )
        {
        }

        protected CIMessage( CIContainer aContainer, string aTitle )
            : base( aContainer )
		{
            iTitle = aTitle;
		}
		#endregion

        #region API
        public override void Clear()
        {
            base.Clear();
            iLines.Clear();
        }

        public void SetLine( string aLine )
        {
            iLines.Clear();
            iLines.Add( aLine );
        }

        public void SetLineFormatted( string aFormat, params object[] aArgs )
        {
            string line = string.Format( aFormat, aArgs );
            SetLine( line );
        }

        public void AddLine( string aLine )
        {
            using ( StringReader reader = new StringReader( aLine ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    iLines.Add( line );
                    line = reader.ReadLine();
                }
            }
        }

        public void AddLineFormatted( string aFormat, params object[] aArgs )
        {
            string line = string.Format( aFormat, aArgs );
            AddLine( line );
        }

        public static string TypeToString( TType aType )
        {
            return SymbianUtils.Enum.EnumUtils.ToString( aType );
        }
        #endregion

        #region Properties
        public override string Name
        {
            get { return Title; }
            set { Title = value; }
        }

        [Base.DataBinding.CIDBAttributeCell( "Title", 1 )]
        public string Title
        {
            get { return iTitle; }
            set { iTitle = value; }
        }

        public string Description
        {
            get { return ToString(); }
            set 
            {
                List<string> lines = new List<string>();
                //
                using ( StringReader reader = new StringReader( value ) )
                {
                    string line = reader.ReadLine();
                    while ( line != null )
                    {
                        lines.Add( line );
                        line = reader.ReadLine();
                    }
                }
                //
                iLines = lines;
            }
        }

        public string FullText
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                //
                ret.AppendLine( Title );
                ret.Append( System.Environment.NewLine );
                ret.Append( Description );
                //
                return ret.ToString();
            }
        }

        public TType Type
        {
            get { return iType; }
            protected set { iType = value; }
        }

        [Base.DataBinding.CIDBAttributeCell( "Type", 0 )]
        public string TypeName
        {
            get { return TypeToString( Type ); }
        }

        public virtual Font Font
        {
            get { return iFont; }
            set { iFont = value; }
        }

        public virtual Color Color
        {
            get { return iColor; }
            set { iColor = value; }
        }

        public int LineCount
        {
            get { return iLines.Count; }
        }
        #endregion

        #region Operators
        public static implicit operator CIDBRow( CIMessage aMessage )
        {
            CIDBRow row = new CIDBRow();

            // To ensure that the register and cells are correctly associated
            row.Element = aMessage;

            row.Add( new CIDBCell( aMessage.TypeName ) );
            row.Add( new CIDBCell( aMessage.Title ) );
            //
            return row;
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            for ( int i = 0; i < iLines.Count; i++ )
            {
                ret.Append( iLines[ i ].Trim() );
                ret.Append( " " );
            }
            return ret.ToString();
        }
        #endregion

        #region From IEnumerable<string>
        public new IEnumerator<string> GetEnumerator()
        {
            foreach ( string s in iLines )
            {
                yield return s;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( string s in iLines )
            {
                yield return s;
            }
        }
        #endregion

        #region Data members
        private TType iType = TType.ETypeWarning;
        private Font iFont = new Font( "Tahoma", 8.25f );
        private Color iColor = Color.Black;
        private string iTitle = string.Empty;
        private List<string> iLines = new List<string>();
        #endregion
    }
}
