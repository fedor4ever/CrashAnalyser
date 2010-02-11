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
using System.Reflection;
using System.ComponentModel;

namespace CrashItemLib.Crash.Base.DataBinding
{
    public class CIDBModel : IEnumerable<CIDBRow>
	{
		#region Constructors
        public CIDBModel( CIElement aElement, bool aAutoPopulate )
        {
            iElement = aElement;
            iAutoPopulate = aAutoPopulate;
        }
		#endregion

        #region API
        public void Add( CIDBColumn aColumn )
        {
            aColumn.Model = this;
            iColumns.Add( aColumn );
        }

        public void Add( CIDBRow aRow )
        {
            aRow.Model = this;
            iRows.Add( aRow );
        }

        public void Add( params CIDBCell[] aCells )
        {
            CIDBRow row = new CIDBRow( aCells );
            Add( row );
        }

        public void ClearRows()
        {
            iRows.Clear();
        }

        public void ClearColumns()
        {
            iColumns.Clear();
        }

        public void TryAutoPopulateColumns( CIElement aElement )
        {
            Type customAttributeType = typeof( CIDBAttributeColumn );
            Type thisObjectType = aElement.GetType();

            object[] attribs = thisObjectType.GetCustomAttributes( customAttributeType, true );
            if ( attribs != null && attribs.Length > 0 )
            {
                ExtractAttributeColumns( attribs, aElement );
            }
        }

        public void TryAutoPopulateCells( CIElement aElement )
        {
            if ( AutoPopulate )
            {
                SortedDictionary<int, CIDBRow> rows = new SortedDictionary<int, CIDBRow>();
                
                Type customAttributeType = typeof( CIDBAttributeCell );
                Type thisObjectType = aElement.GetType();

                // Get properties featuring the CIDBAttribute 
                PropertyInfo[] propertyInfo = thisObjectType.GetProperties();
                foreach ( PropertyInfo p in propertyInfo )
                {
                    object[] attribs = p.GetCustomAttributes( customAttributeType, true );
                    if ( attribs != null && attribs.Length > 0 )
                    {
                        object propertyValue = p.GetValue( aElement, null );
                        ExtractAttributeCells( p.ToString(), aElement, attribs, propertyValue, rows );
                    }
                }

                // Same, but get methods featuring the CIDBAttribute 
                MethodInfo[] methodInfo = thisObjectType.GetMethods();
                foreach ( MethodInfo m in methodInfo )
                {
                    object[] attribs = m.GetCustomAttributes( customAttributeType, true );
                    if ( attribs != null && attribs.Length > 0 )
                    {
                        // We only support this attribute on methods that don't contain
                        // any arguments
                        int paramCount = m.GetParameters().Length;
                        if ( paramCount != 0 )
                        {
                            throw new NotSupportedException( "Method: " + m.ToString() + " has CIDBAttribute but non-empty parameter list -> Not supported" );
                        }

                        // Get property value 
                        object propertyValue = m.Invoke( aElement, null );
                        ExtractAttributeCells( m.ToString(), aElement, attribs, propertyValue, rows );
                    }
                }

                // Since the list is already sorted for us, just add the items in order
                foreach ( KeyValuePair<int, CIDBRow> kvp in rows )
                {
                    this.Add( kvp.Value );
                }
            }
        }
        #endregion

        #region Properties
        public bool AutoPopulate
        {
            get { return iAutoPopulate; }
        }

        public CIElement Element
        {
            get { return iElement; }
        }

        public List<CIDBColumn> Columns
        {
            get { return iColumns; }
        }

        public List<CIDBRow> Rows
        {
            get { return iRows; }
        }
        #endregion

        #region Internal methods
        private void ExtractAttributeColumns( object[] aColumnAttributes, CIElement aElement )
        {
            bool foundColumnAttributes = false;
            SortedDictionary<int, CIDBColumn> columns = new SortedDictionary<int,CIDBColumn>();

            foreach ( object obj in aColumnAttributes )
            {
                if ( obj is CIDBAttributeColumn )
                {
                    CIDBAttributeColumn attribute = (CIDBAttributeColumn) obj;

                    // Make a column
                    CIDBColumn col = new CIDBColumn( attribute.Caption );
                    col.Width = attribute.Width;
                    col.WidthSet = attribute.WidthSet;
                    col.TakesUpSlack = attribute.TakesUpSlack;

                    if ( columns.ContainsKey( attribute.Order ) )
                    {
                        throw new Exception( string.Format( "Column: [{0}] in element: [{1}] specifies order: {2} which is already in use",
                                                            attribute.Caption, aElement, attribute.Order ) );
                    }

                    columns.Add( attribute.Order, col );
                    foundColumnAttributes = true;
                }
            }

            // Since the list is already sorted for us, just add the items in order
            foreach ( KeyValuePair<int, CIDBColumn> kvp in columns )
            {
                this.Add( kvp.Value );
            }

            // We'll override the auto populate feature if we find a valid
            // column attribute
            iAutoPopulate = foundColumnAttributes;
        }

        private void ExtractAttributeCells( string aEntityName, CIElement aElement, object[] aCIDBAttributeEntries, object aValue, SortedDictionary<int, CIDBRow> aRows )
        {
            foreach ( object obj in aCIDBAttributeEntries )
            {
                if ( obj is CIDBAttributeCell )
                {
                    CIDBAttributeCell attribute = (CIDBAttributeCell) obj;

                    // If the property is an 'auto expand' entry, then don't look at this
                    // property, but instead look at the object itself
                    if ( attribute.Options == CIDBAttributeCell.TOptions.EAutoExpand )
                    {
                        // The object must be an element
                        CIElement element = aValue as CIElement;
                        if ( element == null )
                        {
                            throw new ArgumentException( "CIDBAttributeCell(TOptions.EAutoExpand) may only be applied to CIElement objects" );
                        }

                        TryAutoPopulateCells( element );
                    }
                    else
                    {
                        // Whether or not to create a row
                        bool makeRow = true;

                        // Convert attribute value to string. If the object is an enum, we'll check if it has
                        // a System.ComponentModel.Description attached to it, and use that in preference to a raw
                        // value.
                        string defaultValueString = ( attribute.DefaultValue != null ) ? attribute.DefaultValue.ToString() : null;
                        string propertyValueString = aValue.ToString();
                        if ( aValue.GetType().BaseType == typeof( Enum ) )
                        {
                            // Check if it supports System.ComponentModel.Description
                            FieldInfo fi = aValue.GetType().GetField( propertyValueString );
                            if ( fi != null )
                            {
                                DescriptionAttribute[] attributes = (DescriptionAttribute[]) fi.GetCustomAttributes( typeof( DescriptionAttribute ), false );
                                if ( attributes != null && attributes.Length > 0 )
                                {
                                    propertyValueString = attributes[ 0 ].Description;
                                }
                            }
                        }
                        else if ( attribute.Format.Length > 0 && aValue is IFormattable )
                        {
                            string formatSpec = attribute.Format;
                            IFormattable formattable = (IFormattable) aValue;
                            propertyValueString = formattable.ToString( formatSpec, null );

                            // Also get the default value if available
                            bool defaultIsFormattable = attribute.DefaultValue is IFormattable;
                            if ( attribute.DefaultValue != null && defaultIsFormattable )
                            {
                                formattable = (IFormattable) attribute.DefaultValue;
                                defaultValueString = formattable.ToString( formatSpec, null );
                            }
                        }

                        // If the value of the property is the same as the default, then don't
                        // show it.
                        if ( defaultValueString != null )
                        {
                            makeRow = ( defaultValueString.CompareTo( propertyValueString ) != 0 );
                        }

                        // Make a row
                        if ( makeRow )
                        {
                            CIDBRow row = new CIDBRow();
                            row.Add( new CIDBCell( attribute.Caption ) );
                            row.Add( new CIDBCell( propertyValueString, attribute.ForeColor, attribute.BackColor, attribute.Format ) );

                            // Add the row if it doesn't exist. If it does, then that implies
                            // duplicate ordering, which we treat as a programming error.
                            if ( aRows.ContainsKey( attribute.Order ) )
                            {
                                throw new Exception( string.Format( "Entity: [{0}] in element: [{1}] specifies order: {2} which is already in use",
                                                                    aEntityName, aElement, attribute.Order ) );
                            }

                            aRows.Add( attribute.Order, row );
                        }
                    }
                }
            }
        }
        #endregion

        #region From IEnumerable<CIDBRow>
        public IEnumerator<CIDBRow> GetEnumerator()
        {
            foreach ( CIDBRow row in iRows )
            {
                yield return row;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIDBRow row in iRows )
            {
                yield return row;
            }
        }
        #endregion

        #region Data members
        private readonly CIElement iElement;
        private bool iAutoPopulate = false;
        private List<CIDBColumn> iColumns = new List<CIDBColumn>();
        private List<CIDBRow> iRows = new List<CIDBRow>();
		#endregion
    }
}
