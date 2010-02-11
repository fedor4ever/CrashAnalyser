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
using Microsoft.JScript;
using Microsoft.JScript.Vsa;
using Microsoft.Vsa;


namespace SymBuildParsingLib.Utils
{
	public class SymExpressionEvaluator
	{
		#region API
		public static object Evaluate( string aExpression )
		{
			object ret = null;
			//
			try
			{
				ret = Microsoft.JScript.Eval.JScriptEvaluate( aExpression, iScriptEngine );
			}
			catch (Exception)
			{
				ret = false;
			}
			//
			return ret;
		}

		public static bool EvaluateAsBoolean( string aExpression )
		{
			bool ret = false;
			//
			object result = Evaluate( aExpression );
			//
			if	( result != null )
			{
				if	( result is bool )
				{
					ret = (bool) result;
				}
				else
				{
					// Anything non-NULL that is not a bool will be treated
					// as 'true'
					ret = true;
				}
			}
			else
			{
				// Null evaluation so we'll treat that as 'false'
				ret = false;
			}
			//
			return ret;
		}
		#endregion

		#region Data members
		private static VsaEngine iScriptEngine = VsaEngine.CreateEngine();
		#endregion
	}
}
