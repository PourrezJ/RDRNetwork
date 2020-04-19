#pragma once
#include "main.h"
#include <iostream>
#include <fstream>
#include <stdio.h> 
#include <algorithm>
#include <vector>
#include <Psapi.h>
#include <map>
#include <MinHook/MinHook.h>
#include "./Invoker/hash_to_address_table.hpp"
#include "./Invoker/invoker.hpp"
#include "./Memory/memory.hpp"

typedef int(__fastcall* is_dlc_present_fn)(__int64 a1, DWORD dlchash);

namespace hooks
{
	namespace globals
	{
		extern RECT resolution;
		extern uintptr_t  base_address;
	}

	namespace original
	{
		extern is_dlc_present_fn o_is_dlc_present;
	}

	extern int __fastcall is_dlc_present_hook(__int64 a1, DWORD dlcHash);

	extern void Init(MH_STATUS status);

	void OnTick();

	void Cleanup();

}