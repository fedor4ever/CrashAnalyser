/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Summarisable;
using XPTable.Models;
using CAPCrashAnalysis.Plugin;

namespace CAPluginCrashAnalysisUi.Tabs
{
    internal partial class CATabCrashContainerSummary : CATabCrashBase
    {
        #region Constructors
        public CATabCrashContainerSummary( CAPluginCrashAnalysis aSubEngine )
            : base( aSubEngine )
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        private void CASubControlCrashSummary_Load( object sender, EventArgs e )
        {
            UpdateTable();
            UpdateButtons();
        }

        private void iBT_Open_Click( object sender, EventArgs e )
        {
            if ( iTable.SelectedIndicies.Length > 0 )
            {
                int index = iTable.SelectedIndicies[ 0 ];
                CIContainer container = (CIContainer) iTableModel.Rows[ index ].Tag;
                //
                CATabCrashContainerExplorer control = new CATabCrashContainerExplorer( SubEngine, container );
                base.UIManager.UIManagerContentAdd( control );
            }
        }

        private void iBT_Open_All_Click( object sender, EventArgs e )
        {
            foreach ( CIContainer container in base.CrashItemEngine )
            {
                CATabCrashContainerExplorer control = new CATabCrashContainerExplorer( SubEngine, container );
                base.UIManager.UIManagerContentAdd( control );
            }
        }

        private void iBT_Close_Click( object sender, EventArgs e )
        {
            base.UIManager.UIManagerContentClose( this );
        }

        private void iTable_SelectionChanged( object sender, XPTable.Events.SelectionEventArgs e )
        {
            UpdateButtons();
        }
        #endregion

        #region Internal methods
        private void UpdateTable()
        {
            iTable.BeginUpdate();
            iTableModel.Rows.Clear();

            foreach ( CIContainer item in base.CrashItemEngine )
            {
                CISourceElement source = item.Source;

                // Main row
                Row row = new Row();
                row.Tag = item;

                // Cell: crash source file name
                Cell cFile = new Cell( source.MasterFileName );
                cFile.ForeColor = Color.Blue;
                cFile.ColSpan = iColModel.Columns.Count;
                cFile.Font = new Font( iTable.Font, FontStyle.Bold );
                row.Cells.Add( cFile );
                iTableModel.Rows.Add( row );

                // Sub-row
                int index = 1;
                int numberOfNonCrashingThreads = 0;
                CISummarisableEntityList list = item.Summaries;
                foreach ( CISummarisableEntity entity in list )
                {
                    if ( entity.IsAbnormalTermination )
                    {
                        Row subRow = new Row();
                        subRow.Tag = item; 

                        // Spacer
                        subRow.Cells.Add( new Cell() );

                        // Sub-cell: line number of crash item within source file
                        Cell scLineNumber = new Cell( index.ToString() );
                        if ( source.IsLineNumberAvailable )
                        {
                            scLineNumber.Text = source.LineNumber.ToString();
                        }
                        subRow.Cells.Add( scLineNumber );
                        ++index;

                        // Sub-cell: summary name (thread/stack name)
                        Cell scThreadName = new Cell( entity.Name );
                        subRow.Cells.Add( scThreadName );

                        // Sub-cell: summary exit info (for threads)
                        if ( entity.IsAvailable( CISummarisableEntity.TElement.EElementThread ) )
                        {
                            Cell scThreadExitInfo = new Cell( entity.Thread.ExitInfo.ToString() );
                            subRow.Cells.Add( scThreadExitInfo );
                        }

                        // Save subrow as a child of main row. *MUST* do this 
                        // after adding row to table model.
                        row.SubRows.Add( subRow );
                    }
                    else
                    {
                        ++numberOfNonCrashingThreads;
                    }
                }

                // If we saw other (non-crashed) threads, also include a count
                if ( numberOfNonCrashingThreads > 0 )
                {
                    Row subRow = new Row();
                    subRow.Tag = item; 

                    // Spacer
                    subRow.Cells.Add( new Cell() );

                    // Our text
                    string numberOfNonCrashingThreadsText = string.Format( "...plus {0} more running thread{1}", numberOfNonCrashingThreads, numberOfNonCrashingThreads != 1 ? "s" : string.Empty );
                    Cell cell = new Cell( numberOfNonCrashingThreadsText );
                    subRow.Cells.Add( cell );

                    // Spacer
                    subRow.Cells.Add( new Cell() );

                    // Spacer
                    subRow.Cells.Add( new Cell() );
                }
            }

            iTable.EndUpdate();
        }

        private void UpdateButtons()
        {
            iBT_Open.Enabled = ( iTable.TableModel.Selections.SelectedIndicies.Length > 0 );
            iBT_Open_All.Enabled = ( iTable.TableModel.Rows.Count > 0 );
        }
        #endregion

        #region Data members
        #endregion
    }
}
