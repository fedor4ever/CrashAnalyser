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
using System.Text;
using System.Collections.Generic;
using SymbianUtils;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianStructuresLib.CodeSegments;

namespace SymbianStructuresLib.Debug.Code
{
    public class CodeCollectionList : IEnumerable<CodeCollection>
    {
        #region Constructors
        public CodeCollectionList()
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( iCollections )
            {
                iCollections.Clear();
            }
        }

        public void Add( CodeCollection aCollection )
        {
            lock ( iCollections )
            {
                if ( !Contains( aCollection ) )
                {
                    aCollection.ParentList = this;
                    iCollections.Add( aCollection );
                }
                else
                {
                    throw new ArgumentException( string.Format( "Collection \'{0}\' already exists", aCollection.FileName.FileNameInHost ) );
                }
            }
        }

        public void Remove( CodeCollection aCollection )
        {
            Predicate<CodeCollection> predicate = delegate( CodeCollection collection )
            {
                return collection.Equals( aCollection );
            };
            //
            lock ( iCollections )
            {
                iCollections.RemoveAll( predicate );
            }
        }

        public void RemoveUntagged()
        {
            Predicate<CodeCollection> predicate = delegate( CodeCollection collection )
            {
                return collection.Tagged == false;
            };
            //
            lock ( iCollections )
            {
                iCollections.RemoveAll( predicate );
            }
        }

        public bool Contains( uint aAddress )
        {
            Predicate<CodeCollection> predicate = delegate( CodeCollection collection )
            {
                return collection.Contains( aAddress );
            };
            //
            bool ret = false;
            //
            lock ( iCollections )
            {
                CodeCollection found = iCollections.Find( predicate );
                if ( found != null )
                {
                    // Implement last-access optimisation
                    MakeHeadOfMRU( found );
                    ret = true;
                }
            }
            //
            return ret;
        }

        public bool Contains( CodeCollection aCollection )
        {
            Predicate<CodeCollection> predicate = delegate( CodeCollection collection )
            {
                return collection.Equals( aCollection );
            };
            //
            bool ret = false;
            //
            lock ( iCollections )
            {
                ret = iCollections.Find( predicate ) != null;
            }
            //
            return ret;
        }

        public bool GetInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions )
        {
            Predicate<CodeCollection> predicate = delegate( CodeCollection collection )
            {
                return collection.Contains( aAddress );
            };
            //
            bool ret = false;
            //
            lock ( iCollections )
            {
                CodeCollection found = iCollections.Find( predicate );
                if ( found != null )
                {
                    // Implement last-access optimisation
                    ret = found.GetInstructions( aAddress, aInstructionSet, aCount, out aInstructions );
                }
                else
                {
                    aInstructions = new IArmInstruction[ 0 ];
                }
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock ( iCollections )
                {
                    return iCollections.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get { return Count == 0; } 
        }

        public bool Tagged
        {
            set
            {
                lock ( this )
                {
                    Action<CodeCollection> action = delegate( CodeCollection collection )
                    {
                        collection.Tagged = value;
                    };
                    iCollections.ForEach( action );
                }
            }
        }

        public object Tag
        {
            get { return iTag; }
            set
            {
                iTag = value;
            }
        }

        public CodeCollection this[ int aIndex ]
        {
            get
            {
                lock ( iCollections )
                {
                    return iCollections[ aIndex ];
                }
            }
        }

        public CodeCollection this[ PlatformFileName aFileName ]
        {
            get
            {
                Predicate<CodeCollection> predicate = delegate( CodeCollection collection )
                {
                    bool same = collection.FileName.Equals( aFileName );
                    return same;
                };
                //
                CodeCollection ret = null;
                //
                lock ( iCollections )
                {
                    ret = iCollections.Find( predicate );
                }
                //
                return ret;
            }
        }

        public CodeCollection this[ CodeSegDefinition aCodeSegment ]
        {
            get
            {
                Predicate<CodeCollection> predicate = delegate( CodeCollection collection )
                {
                    bool same = collection.IsMatchingCodeSegment( aCodeSegment );
                    return same;
                };
                //
                CodeCollection ret = null;
                //
                lock ( iCollections )
                {
                    ret = iCollections.Find( predicate );
                }
                //
                return ret;
            }
        }
        #endregion

        #region From IEnumerable<CodeCollection>
        IEnumerator<CodeCollection> IEnumerable<CodeCollection>.GetEnumerator()
        {
            foreach ( CodeCollection col in iCollections )
            {
                yield return col;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CodeCollection col in iCollections )
            {
                yield return col;
            }
        }
        #endregion

        #region Internal methods
        private void MakeHeadOfMRU( CodeCollection aCollection )
        {
            lock ( iCollections )
            {
                // Implement last-access optimisation
                int pos = iCollections.IndexOf( aCollection );
                iCollections.RemoveAt( pos );
                iCollections.Insert( 0, aCollection );
            }
        }
        #endregion

        #region Data members
        private object iTag = null;
        private List<CodeCollection> iCollections = new List<CodeCollection>();
        #endregion
    }
}