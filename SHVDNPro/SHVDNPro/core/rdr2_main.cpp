#pragma once

#include "rdr2_main.hpp"
#include "memory/memory.hpp"
#include "invoker/natives.hpp"

namespace hooks
{
	namespace globals
	{
		RECT resolution;
		uintptr_t base_address;
	}

	namespace original
	{
		is_dlc_present_fn o_is_dlc_present;
	}
	

	int __fastcall is_dlc_present_hook(__int64 a1, DWORD dlcHash) {

		//auto playerid = PLAYER::PLAYER_ID();
		//std::string s;
		//int value = 3;
		//s.push_back((char)('0' + value));
		//printf(s.c_str());
		//PLAYER::SET_PLAYER_INVINCIBLE(playerid, true);
		////features::c_esp().draw_players();
		//rendering::c_renderer::get()->draw_text(800, 600, 14.f, "Aaaaaaaaaaaaaaaaaaaaaaaaaa", 255, 0, 255, 255);

		return original::o_is_dlc_present(a1, dlcHash);
	}

	void Init(MH_STATUS status)
	{

		printf("Cheat loaded!\n\n");
		globals::base_address = uintptr_t(GetModuleHandleA(0));
		auto hwnd_ = FindWindowA(0, "Red Dead Redemption 2");
		GetWindowRect(hwnd_, &globals::resolution);


		auto is_dlc_present = memory::find_signature(0, "\xE8\x45\x9C\x01\x00\x48\x8B", "xxxxxxx") - 0x1002; // DOES_CAM_EXIST
		printf("is_dlc_present: %I64X\n", is_dlc_present);

		status = MH_CreateHook((PVOID)is_dlc_present, is_dlc_present_hook, reinterpret_cast<void**>(&original::o_is_dlc_present));
		printf("create_status : %s\n", std::string(MH_StatusToString(status)).c_str());

		status = MH_EnableHook((PVOID)is_dlc_present);
		printf("enable_status : %s\n\n", std::string(MH_StatusToString(status)).c_str());
		
	}

	void OnTick() {
		Cleanup();
		GameScript::DisableAllScripts();
		//printf("Tick");
	}

	void Cleanup()
	{
		auto playerID = PLAYER::GET_PLAYER_INDEX();

		//char s[1]; // Nombre maximal de chiffres + 1

		//sprintf(s, "%d", playerID);
		//printf(s);
		VEHICLE::SET_DISABLE_RANDOM_TRAINS_THIS_FRAME(false);
		VEHICLE::DELETE_ALL_TRAINS();
		VEHICLE::SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(0);
		VEHICLE::SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(0);
		VEHICLE::SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(0);
		VEHICLE::SET_RANDOM_BOATS(false);

		PATHFIND::SET_AMBIENT_PED_RANGE_MULTIPLIER_THIS_FRAME(0);

		PED::SET_SCENARIO_PED_DENSITY_MULTIPLIER_THIS_FRAME(0);
		

		PLAYER::SET_PLAYER_WANTED_LEVEL(playerID, 0, false);
		PLAYER::SET_MAX_WANTED_LEVEL(0);
		PLAYER::CLEAR_PLAYER_WANTED_LEVEL(playerID);

		for (int a = 0; a < 12; a++)
			MISC::ENABLE_DISPATCH_SERVICE(a, false);

		PED::ADD_SCENARIO_BLOCKING_AREA(-10000.0f, -10000.0f, -1000.0f, 10000.0f, 10000.0f, 1000.0f, 0, 1);
		PED::SET_CREATE_RANDOM_COPS(false);


	}
}
