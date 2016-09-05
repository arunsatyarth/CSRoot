// Sniper.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"


#include<Windows.h>

int _tmain(int argc, _TCHAR* argv[])
{
	DWORD procId;
	HWND handle = FindWindow(NULL, argv[1]);
	if (!handle)
		return 0;
	DWORD threadId = GetWindowThreadProcessId(handle, &procId);

	HMODULE hMod2 = LoadLibrary(L"Spy.dll");
	if (hMod2)
		OutputDebugString(_T("HookProc loaded"));


	//get method address to be given t setwindowhookex
	HOOKPROC method = (HOOKPROC)GetProcAddress(hMod2, "LocalHook");

	HHOOK hk = SetWindowsHookEx(WH_GETMESSAGE, method, hMod2, threadId);
	Sleep(1000);

	//the dll will get loaded only when it recieves this message
	BOOL bFlag = PostThreadMessage(threadId, WM_PAINT, NULL, NULL);
	DWORD err = GetLastError();
	Sleep(1000);

	BOOL d = UnhookWindowsHookEx(hk);

	return 0;
}
