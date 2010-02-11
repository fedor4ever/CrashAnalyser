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
/*
extern void InflateUnCompress(unsigned char* source, int sourcesize,unsigned char* dest, int destsize);

 void JNICALL Java_rofssym_SymbianInflater_InflateUncompress
  (JNIEnv *env, jobject, jbyteArray aSource, jbyteArray aDest)
	{
	jboolean isCopy; // ignored

	jsize srcsize = env->GetArrayLength(aSource);
	jsize dstsize = env->GetArrayLength(aDest);

	unsigned char * src = (unsigned char *) env->GetByteArrayElements(aSource, &isCopy);
	unsigned char * dst = (unsigned char *) env->GetByteArrayElements(aDest, &isCopy);

	InflateUnCompress(src, srcsize, dst, dstsize);

	env->ReleaseByteArrayElements(aSource, (jbyte *) src, 0);
	env->ReleaseByteArrayElements(aDest, (jbyte *) dst, 0);
	}

    	catch(ErrorHandler& error) 
	{ 
		error.Report();
	} 


*/