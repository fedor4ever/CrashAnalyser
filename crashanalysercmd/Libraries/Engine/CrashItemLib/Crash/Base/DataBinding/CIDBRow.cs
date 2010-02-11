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
using System.Drawing;

namespace CrashItemLib.Crash.Base.DataBinding
{
    public class CIDBRow : IEnumerable<CIDBCell>
	{
		#region Constructors
        public CIDBRow()
        {
        }

        public CIDBRow( params CIDBCell[] aCells )
        {
            iCells.AddRange( aCells );
        }

        public CIDBRow( Color aForeColor )
            : this( aForeColor, Color.Transparent )
        {
        }

        public CIDBRow( Color aForeColor, Color aBackColor )
        {
            iForeColor = aForeColor;
            iBackColor = aBackColor;
        }
		#endregion

        #region API
        public void Add( CIDBCell aCell )
        {
            aCell.Row = this;
            iCells.Add( aCell );
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iCells.Count; }
        }

        public CIDBCell this[ int aIndex ]
        {
            get { return iCells[ aIndex ]; }
        }

        public Color ForeColor
        {
            get { return iForeColor; }
            set { iForeColor = value; }
        }

        public Color BackColor
        {
            get { return iBackColor; }
            set { iBackColor = value; }
        }

        public CIElement Element
        {
            get
            {
                CIElement element = iElement;
                //
                if ( element == null && Model != null )
                {
                    element = Model.Element;
                }
                //
                return element;
            }
            set { iElement = value; }
        }

        internal CIDBModel Model
        {
            get { return iModel; }
            set
            { 
                iModel = value;

                // Try to ensure the element points to something
                if ( iElement == null )
                {
                    iElement = Model.Element;
                }
            }
        }
        #endregion

        #region From IEnumerable<CICell>
        public IEnumerator<CIDBCell> GetEnumerator()
        {
            foreach ( CIDBCell c in iCells )
            {
                yield return c;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIDBCell c in iCells )
            {
                yield return c;
            }
        }
        #endregion

        #region Data members
        private Color iForeColor = Color.Black;
        private Color iBackColor = Color.Transparent;
        private List<CIDBCell> iCells = new List<CIDBCell>();
        private CIDBModel iModel = null;
        private CIElement iElement = null;
        #endregion
    }
}
