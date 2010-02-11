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

namespace SymbianUtils
{
	public class DisposableObject : IDisposable
	{
		#region Constructors
		public DisposableObject()
		{
		}

		~DisposableObject()
		{
			// Not allowed to access managed resources from
			// within a C# destructor (in this context we are being
			// called by the GC and it will take care of disposing of
			// other managed resources for us).
			Cleanup( false, true );
		}
		#endregion

        #region Properties
        public bool HaveBeenDisposedOf
        {
            get { return iHaveBeenDisposedOf; }
        }
        #endregion

        #region API - Cleanup Framework
        protected virtual void CleanupManagedResources()
        {
        }

        protected virtual void CleanupUnmanagedResources()
        {
        }
        #endregion

        #region From IDisposable
        public void Dispose()
		{
			// In this situation, we are programatically being asked to
			// release all resources, including managed ones.
			Cleanup( true, true );

			// Take yourself off the Finalization queue 
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize( this );
		}
		#endregion

		#region Internal methods
		private void Cleanup( bool aReleaseManagedResources, bool aReleaseUnmanagedResources )
		{
            lock( this )
            {
                if ( iHaveBeenDisposedOf == false )
                {
                    try
                    {
                        if ( aReleaseManagedResources )
                        {
                            CleanupManagedResources();
                        }
                        if ( aReleaseUnmanagedResources )
                        {
                            CleanupUnmanagedResources();
                        }

                        iHaveBeenDisposedOf = true;
                    }
                    catch
                    {
                    }
                }
			}
		}
		#endregion

		#region Data members
		private bool iHaveBeenDisposedOf = false;
		#endregion
	}
}
