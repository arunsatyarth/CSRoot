// This is the main DLL file.

#include "stdafx.h"

#include "Spy.h"

#include<Windows.h>
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
using namespace System::Diagnostics;
using namespace System::Threading;
using namespace System;
using namespace System::IO;
static bool dll_injected = false;

//using namespace IDNInterfaces;
extern "C" DWORD CALLBACK LocalHook(int code, DWORD wParam, LONG lParam)
{
	if (dll_injected == true)
	{
		DWORD dwRet = ::CallNextHookEx(NULL, code, wParam, lParam);
		if (dwRet)
			return true;
		else
			return false;
	}
	else
	{
		dll_injected = true;


		String ^str = Path::GetDirectoryName(Assembly::GetExecutingAssembly()->Location);
		String ^hookproclib = str + "\\MissionControl.dll";
		//lets go with the first match
		Assembly^ assem = Assembly::LoadFrom(hookproclib);
		if (!assem)
			return 0;
		Type^ type = assem->GetType("MissionControl.Listener");
		if (!type)
			return 0;

		Object ^ obj = (Activator::CreateInstance(type));
		MethodInfo ^ fun = type->GetMethod("Listen");
		fun->Invoke(obj, nullptr);


		DWORD dwRet = ::CallNextHookEx(NULL, code, wParam, lParam);
		if (dwRet)
			return true;
		else
			return false;
	}

}