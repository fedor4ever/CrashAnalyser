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
using System.Collections.Generic;
using CrashItemLib.Crash.Container;
using CrashItemLib.Engine.Sources;
using MobileCrashLib.Structures;
using MobileCrashLib.Structures.Items;

namespace CrashAnalyserServerExe.Engine
{
    internal class CACmdLineFileSource : CACmdLineFSEntity, IEnumerable<CIContainer>
    {
        #region Internal enumerations
        public enum TState
        {
            EStateUninitialized = -1,
            EStateProcessedAndReadyToBeArchived = 0,
            EStateSkippedDueToMissingSymbols,
            EStateSkippedDueToNotBeingRecognized
        }
        #endregion

        #region Constructors
        public CACmdLineFileSource()
		{
		}
        #endregion

		#region API
        public OutputEntry AddOutput( CIContainer aContainer, string aFileName, TOutputStatus aStatus )
        {
            OutputEntry output = new OutputEntry( aContainer, aFileName, aStatus );
            iOutputs.Add( output );
            return output;
        }

        public void Add( CIContainer aContainer )
        {
            // If we're adding a container, then it must be because
            // there wasn't a source associated with this file (crash engine
            // could not understand input file).
            System.Diagnostics.Debug.Assert( iSource == null );

            if ( iContainers == null )
            {
                iContainers = new CIContainerCollection();
            }
            iContainers.Add( aContainer );
        }
        #endregion

		#region Properties
        public CIEngineSource Source
        {
            get { return iSource; }
            set { iSource = value; }
        }

        public int OutputCount
        {
            get { return iOutputs.Count; }
        }

        public OutputEntry this[ int aIndex ]
        {
            get { return iOutputs[ aIndex ]; }
        }

        public OutputEntry[] Outputs
        {
            get { return iOutputs.ToArray(); }
        }

        public int ContainerCount
        {
            get
            {
                int ret = 0;
                //
                if ( Source != null )
                {
                    ret = Source.ContainerCount;
                }
                else if ( iContainers != null )
                {
                    ret = iContainers.Count;
                }
                //
                return ret;
            }
        }

        public IEnumerable<CIContainer> Containers
        {
            get
            {
                CIContainerCollection ret = iContainers;
                //
                if ( Source != null )
                {
                    return Source;
                }
                else if ( ret == null )
                {
                    ret = new CIContainerCollection();
                }
                //
                return ret;
            }
        }

        public TState State
        {
            get { return iState; }
            set { iState = value; }
        }

        public TMobileCrashContentType ContentType
        {
            get { return iContentType; }
            set { iContentType = value; }
        }

        public MobileCrashItemUint32 RomId
        {
            get { return iRomId; }
            set { iRomId = value; }
        }

        #endregion

        #region Internal methods
        #endregion

        #region Output class
        public class OutputEntry : CACmdLineMessageList
        {
            #region Constructors
            internal OutputEntry( CIContainer aContainer, string aOutputFileName, TOutputStatus aXmlOutputStatus )
            {
                iContainer = aContainer;
                iXmlOutputStatus = aXmlOutputStatus;
                iOutputFileName = aOutputFileName;
            }
            #endregion

            #region Properties
            public CIContainer Container
            {
                get { return iContainer; }
            }

            public TOutputStatus Status
            {
                get
                {
                    // There are two different statuses. One is the container-level status, 
                    // i.e. whether the container refers to a 'real' crash or just a dummy (i.e. a 'failed' xml file).
                    //
                    // Then, there is the actual success associated with whether or not we could write
                    // the xml output. 
                    TOutputStatus ret = iXmlOutputStatus;
                    //
                    if ( ret == TOutputStatus.ESuccess )
                    {
                        // Check container level status then...
                        if ( Container.Status == CIContainer.TStatus.EStatusDefault )
                        {
                            ret = TOutputStatus.ESuccess;
                        }
                        else if ( Container.Status == CIContainer.TStatus.EStatusErrorContainer )
                        {
                            ret = TOutputStatus.EFailed;
                        }
                    }
                    //
                    return ret; 
                }
            }

            public string OutputFileName
            {
                get { return iOutputFileName; }
            }
            #endregion

            #region Data members
            private readonly TOutputStatus iXmlOutputStatus;
            private readonly CIContainer iContainer;
            private readonly string iOutputFileName;
            #endregion
        }
        #endregion

        #region From IEnumerable<CIContainer>
        public new IEnumerator<CIContainer> GetEnumerator()
        {
            foreach ( CIContainer container in Containers )
            {
                yield return container;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIContainer container in Containers )
            {
                yield return container;
            }
        }
        #endregion

        #region Data members
        private TState iState = TState.EStateUninitialized; 
        private CIEngineSource iSource = null;
        private List<OutputEntry> iOutputs = new List<OutputEntry>();
        private CIContainerCollection iContainers = null;
        private TMobileCrashContentType iContentType = TMobileCrashContentType.EContentTypeUnknown;
        private MobileCrashItemUint32 iRomId = new MobileCrashItemUint32();
        #endregion
    }

    public enum TOutputStatus
    {
        ESuccess = 0,
        EFailed
    }
}
