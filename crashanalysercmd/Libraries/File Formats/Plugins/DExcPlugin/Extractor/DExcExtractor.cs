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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SymbianUtils.Range;
using EM=DExcPlugin.ExpressionManager.DExcExpressionManager;

namespace DExcPlugin.Extractor
{
	internal class DExcExtractor
	{
        #region Events
        public enum TEvent
        {
            EEventExtractedAllData
        }

        public delegate void EventHandler( TEvent aEvent, DExcExtractor aExtractor );
        public event EventHandler StateChanged;
        #endregion

        #region Constructors
        public DExcExtractor()
		{
            Init();
        }
		#endregion

		#region API
        public void Init()
        {
            iState = TState.EStateIdle;
            iData = new DExcExtractedData();
            iLists = new Dictionary<TState, DExcExtractorList>();
            iCurrentLineNumber = 0;
		    
            // Null (really just exists to catch a state transition)
            // =======================================================
            CreateList( TState.EStateIdle, DExcExtractorListType.EListNull ).AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.LogStart, TState.EStateHeader ) );
		    
            // Header
            // =======================================================
            CreateList( TState.EStateHeader, DExcExtractorListType.EListHeader ).AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.ThreadName, TState.EStateThreadInfo ) );

            // Thread info
            // ===========
            DExcExtractorListThreadInfo listThreadInfo = new DExcExtractorListThreadInfo( TState.EStateThreadInfo, DExcExtractorListType.EListThread );
            PrepareList( listThreadInfo, EM.ThreadName, EM.ThreadId, EM.ThreadStackRange, EM.ThreadPanicDetails );
            listThreadInfo.AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.RegistersExceptionStart, TState.EStateRegisterInfoException ) );
            listThreadInfo.AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.RegistersUserStart, TState.EStateRegisterInfoUser ) );

            // Registers (exception)
            // =====================
            DExcExtractorList listRegisterInfoException = CreateList( TState.EStateRegisterInfoException, DExcExtractorListType.EListRegistersException,
                                                                        EM.RegistersExceptionSet1,
                                                                        EM.RegistersExceptionSet2 );
            listRegisterInfoException.AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.RegistersUserStart, TState.EStateRegisterInfoUser ) );

            // Registers (user)
            // ================
            DExcExtractorList listRegisterInfoUser = CreateList( TState.EStateRegisterInfoUser, DExcExtractorListType.EListRegistersUser,
                                                                   EM.RegistersUserCPSR,
                                                                   EM.RegistersUserSet );

            // Since code segs are optional, we want to record that we at least saw the header text (which is mandatory). This
            // tracking allows us to validate that we have received/observed data for all states.
            listRegisterInfoUser.AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.CodeSegmentsStart, TState.EStateCodeSegments ) );

            // Code segments
            // =============
            DExcExtractorList listCodeSegments = CreateList( TState.EStateCodeSegments, DExcExtractorListType.EListCodeSegments, EM.CodeSegmentsEntry );

            // We need to transition state to "stack data", but we must be sure not to throw away the state line we just encountered.
            listCodeSegments.AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.StackDataEntry, TState.EStateStackData ) );

            // Stack data
            // ==========
            DExcExtractorListStackData listStackData = new DExcExtractorListStackData( TState.EStateStackData, DExcExtractorListType.EListStackData );
            PrepareList( listStackData, EM.StackDataEntry );
            listStackData.AddExpression( DExcExtractorEntry.NewMatchSaveAndTransition( EM.LogStart, TState.EStateHeader ) );

            // We want to observe the stack data as it arrives so that we can identify when all stack data has been supplied.
            listStackData.StackChanged += new DExcExtractorListStackData.StackDataChangeHandler( StackDataChanged );
        }

        public void Init( string aStackFileName )
        {
            Init();
            //
            DExcExtractorListStackData stackData = (DExcExtractorListStackData) iLists[ TState.EStateStackData ];
            stackData.Prime( aStackFileName );
        }

        public bool Offer( string aLine, long aLineNumber )
		{
            bool consumed = false;

            // Cache line number - we use this to update the data starting position
            // when the state is changed.
            iCurrentLineNumber = aLineNumber;

            // Get list for current state
            DExcExtractorList list = CurrentList;
            if ( list != null )
            {
                consumed = list.Offer( aLine, aLineNumber, this );
            }

            return consumed;
		}

        public void Finialize()
		{
			// If we were parsing code segs but we didn't get any stack
			// data then we must ensure we still notify when we've reached
			// the end of the code seg data (or else no crash item will be
			// created).
            switch ( State )
            {
            case TState.EStateIdle:
                // Already finished or not started
                return;
            default:
                break;
            }

            // Did we create entries in all non-idle lists?
            bool haveEntriesForAllLists = ListsAreValid;
            if ( haveEntriesForAllLists )
            {
                NotifyEvent( TEvent.EEventExtractedAllData );
                State = TState.EStateIdle;
            }
		}
		#endregion

        #region Properties
        public TState State
		{
            get { return iState; }
            set
            {
                if ( value != iState )
                {
                    TState oldState = value;
                    iState = value;
                    //
                    if ( oldState == TState.EStateIdle )
                    {
                        // Was idle, now not - set starting line number
                        iData.LineNumber = iCurrentLineNumber;
                    }
                    else if ( iState == TState.EStateIdle )
                    {
                        Init();
                    }
                }
            }
		}

        public DExcExtractedData CurrentData
        {
            get { return iData; }
        }

        public DExcExtractorList CurrentList
        {
            get
            {
                DExcExtractorList ret = null;
                //
                if ( iLists.ContainsKey( State ) )
                {
                    ret = iLists[ State ];
                }
                //
                return ret;
            }
        }
		#endregion

        #region Event handlers
        private void StackDataChanged( DExcExtractorListStackData aSelf )
        {
            DExcExtractorListThreadInfo threadInfo = (DExcExtractorListThreadInfo) iLists[ TState.EStateThreadInfo ];
            AddressRange range = threadInfo.StackRange;
            if ( range.IsValid )
            {
                uint lastAddress = aSelf.StackData.Last.Address;
                if ( lastAddress == range.Max - 1 )
                {
                    NotifyEvent( TEvent.EEventExtractedAllData );
                    State = TState.EStateIdle;
                }
            }
        }
        #endregion
        
        #region Internal enumerations
        internal enum TState
        {
            EStateIdle = 0,
            EStateHeader,
            EStateThreadInfo,
            EStateRegisterInfoException,
            EStateRegisterInfoUser,
            EStateCodeSegments,
            EStateStackData,
        }
        #endregion

        #region Internal methods
        private void NotifyEvent( TEvent aEvent )
        {
            if ( StateChanged != null )
            {
                StateChanged( aEvent, this );
            }
        }

        private void PrepareList( DExcExtractorList aList, params Regex[] aExpressions )
        {
            foreach ( Regex exp in aExpressions )
            {
                DExcExtractorEntry entry = DExcExtractorEntry.NewMatchAndSave( exp );
                aList.AddExpression( entry );
            }
            //
            iLists.Add( aList.State, aList );
            iData.Add( aList );
        }

        private DExcExtractorList CreateList( TState aAssociatedState, DExcExtractorListType aType, params Regex[] aExpressions )
        {
            DExcExtractorList ret = new DExcExtractorList( aAssociatedState, aType );
            PrepareList( ret, aExpressions );
            return ret;
        }

        private bool ListsAreValid
        {
            get
            {
                int countThreadInfo = iLists[ TState.EStateThreadInfo ].Count;
                int countRegUser = iLists[ TState.EStateRegisterInfoUser ].Count;
                int countCodeSegs = iLists[ TState.EStateCodeSegments ].Count;
                int countStackData = ((DExcExtractorListStackData) iLists[ TState.EStateStackData ]).StackData.Count;
                //
                bool valid = ( countThreadInfo >= 3 ) && ( countRegUser > 0 ) && ( countCodeSegs > 0 ) && ( countStackData > 0 );
                return valid;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = iData.ToString();
            return ret;
        }
        #endregion

        #region Data members
        private TState iState;
        private long iCurrentLineNumber = 0;
        private DExcExtractedData iData = new DExcExtractedData();
        private Dictionary<TState, DExcExtractorList> iLists = new Dictionary<TState, DExcExtractorList>();
		#endregion
	}
}
