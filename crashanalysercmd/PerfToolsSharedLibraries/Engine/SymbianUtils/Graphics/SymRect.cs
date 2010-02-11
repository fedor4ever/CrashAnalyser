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
using System.Drawing;

namespace SymbianUtils.Graphics
{
	public class SymRect
	{
        #region Constructors
		public SymRect()
		{
		}

        public SymRect( Rectangle aRect )
        {
            iRectangle = aRect;
        }

        public SymRect( Point aTopleft, Size aSize )
        {
            iRectangle = new Rectangle( aTopleft, aSize );
        }
        #endregion

        #region API
        public void Inflate( int aDx, int aDy )
        {
            iRectangle.Inflate( aDx, aDy );
        }

        public void Offset( int aDx, int aDy )
        {
            iRectangle.Offset( aDx, aDy );
        }

        public void Offset( Size aSize )
        {
            iRectangle.Offset( aSize.Width, aSize.Height );
        }

        public void Offset( Point aPosition )
        {
            iRectangle.Offset( aPosition.X, aPosition.Y );
        }

        public void HalfOffset( Size aSize )
        {
            Offset( aSize.Width / 2, aSize.Height / 2 );
        }

        public void HalfOffset( Point aPosition )
        {
            Offset( aPosition.X / 2, aPosition.Y / 2 );
        }
        #endregion

        #region Properties
        public Rectangle Rectangle
        {
            get { return iRectangle; }
            set { iRectangle = value; }
        }

        public Size Size
        {
            get { return iRectangle.Size; }
            set { iRectangle.Size = value; }
        }

        public Point Location
        {
            get { return iRectangle.Location; }
            set { iRectangle.Location = value; }
        }

        public Point TopLeft
        {
            get { return iRectangle.Location; }
        }

        public Point TopRight
        {
            get { return new Point( iRectangle.Right, iRectangle.Top ); }
        }

        public Point BottomLeft
        {
            get { return new Point( iRectangle.Left, iRectangle.Bottom ); }
        }

        public Point BottomRight
        {
            get { return new Point( iRectangle.Right, iRectangle.Bottom ); }
        }

        public int Width
        {
            get { return iRectangle.Width; }
        }

        public int Height
        {
            get { return iRectangle.Height; }
        }

        public int Top
        {
            get { return iRectangle.Top; }
        }

        public int Bottom
        {
            get { return iRectangle.Bottom; }
        }

        public int Left
        {
            get { return iRectangle.Left; }
        }

        public int Right
        {
            get { return iRectangle.Right; }
        }
        #endregion

        #region Operators
        public static implicit operator Rectangle( SymRect aRect )
        {
            return aRect.Rectangle;
        }

        public static implicit operator SymRect( Rectangle aRect )
        {
            return new SymRect( aRect );
        }
        #endregion

        #region Data members
        private Rectangle iRectangle = new Rectangle();
        #endregion
	}
}
