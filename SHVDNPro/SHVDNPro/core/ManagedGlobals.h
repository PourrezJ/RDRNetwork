#pragma once

#include <Script.h>
#include <ScriptDomain.h>

namespace RDR2
{
	public ref class ManagedGlobals
	{
	public:
		static System::AppDomain^ g_appDomain;
		static RDR2::ScriptDomain^ g_scriptDomain;
	};
}
