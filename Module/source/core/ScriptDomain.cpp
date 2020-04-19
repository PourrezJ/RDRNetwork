#include "ScriptDomain.h"

#include "Function.h"

#include "ManagedGlobals.h"
#include "Log.h"

#pragma unmanaged
#include <Windows.h>
#pragma managed

RDR2::ScriptDomain::ScriptDomain()
{
	System::AppDomain::CurrentDomain->UnhandledException += gcnew System::UnhandledExceptionEventHandler(this, &RDR2::ScriptDomain::OnUnhandledException);
	System::AppDomain::CurrentDomain->AssemblyResolve += gcnew System::ResolveEventHandler(this, &RDR2::ScriptDomain::OnAssemblyResolve);

	RDR2::ManagedGlobals::g_scriptDomain = this;
}

void RDR2::ScriptDomain::FindAllTypes()
{
	auto ret = gcnew System::Collections::Generic::List<System::Type^>();

	CurrentDir = System::Reflection::Assembly::GetExecutingAssembly()->Location;
	CurrentDir = System::IO::Path::GetDirectoryName(CurrentDir);

	auto file = System::IO::Path::Combine(CurrentDir, "Scripts\\RDRNetwork.dll");
	//auto cefsharp = "C:\\RDRNetwork\\cefsharp\\CefSharp.dll";
	//System::String^ cefcore = System::IO::Path::Combine(CurrentDir, "Scripts\\CefSharp.Core.dll");
	//auto cefcore = System::IO::Path::Combine(CurrentDir, "Scripts\\CefSharp.Core.dll");

	auto fileName = System::IO::Path::GetFileName(file);

	//RDR2::WriteLog("DEBUG CEFSHARP.CORE {0}", cefcore);

	System::Reflection::Assembly^ assembly = nullptr;
	try {
		//System::Reflection::Assembly::LoadFrom(cefcore);
		assembly = System::Reflection::Assembly::LoadFrom(file);
	}
	catch (System::Exception ^ ex) {
		RDR2::WriteLog("*** Assembly load exception for {0}: {1}", fileName, ex->ToString());
	}
	catch (...) {
		RDR2::WriteLog("*** Unmanaged exception while loading assembly!");
	}

	RDR2::WriteLog("Loaded assembly {0}", assembly);

	try {
		for each (auto type in assembly->GetTypes()) {
			if (!type->IsSubclassOf(RDR2::Script::typeid)) {
				continue;
			}

			ret->Add(type);

			RDR2::WriteLog("Registered type {0}", type->FullName);
		}
	}
	catch (System::Reflection::ReflectionTypeLoadException ^ ex) {
		RDR2::WriteLog("*** Exception while iterating types: {0}", ex->ToString());
		for each (auto loaderEx in ex->LoaderExceptions) {
			RDR2::WriteLog("***    {0}", loaderEx->ToString());
		}
		//continue;
	}
	catch (System::Exception ^ ex) {
		RDR2::WriteLog("*** Exception while iterating types: {0}", ex->ToString());
		//continue;
	}
	catch (...) {
		RDR2::WriteLog("*** Unmanaged exception while iterating types");
		//continue;
	}

	m_types = ret->ToArray();
	m_scripts = gcnew array<RDR2::Script^>(m_types->Length);

	RDR2::WriteLog("{0} script types found:", m_types->Length);
	for (int i = 0; i < m_types->Length; i++) {
		RDR2::WriteLog("  {0}: {1}", i, m_types[i]->FullName);
	}
}

RDR2::Script^ RDR2::ScriptDomain::GetExecuting()
{
	void* currentFiber = GetCurrentFiber();

	// I don't know if GetCurrentFiber ever returns null, but whatever
	if (currentFiber == nullptr) {
		return nullptr;
	}

	for each (auto script in m_scripts) {
		if (script != nullptr && script->m_fiberCurrent == currentFiber) {
			return script;
		}
	}

	return nullptr;
}

bool RDR2::ScriptDomain::ScriptInit(int scriptIndex, void* fiberMain, void* fiberScript)
{
	auto scriptType = m_types[scriptIndex];

	RDR2::WriteLog("Instantiating {0}", scriptType->FullName);

	RDR2::Script^ script = nullptr;
	try {
		script = static_cast<RDR2::Script^>(System::Activator::CreateInstance(scriptType));
	} catch (System::Reflection::ReflectionTypeLoadException^ ex) {
		RDR2::WriteLog("*** Exception while instantiating script: {0}", ex->ToString());
		for each (auto loaderEx in ex->LoaderExceptions) {
			RDR2::WriteLog("***    {0}", loaderEx->ToString());
		}
		return false;
	} catch (System::Exception^ ex) {
		RDR2::WriteLog("*** Exception while instantiating script: {0}", ex->ToString());
		return false;
	} catch (...) {
		RDR2::WriteLog("*** Unmanaged exception while instantiating script!");
		return false;
	}

	RDR2::WriteLog("Instantiated {0}", scriptType->FullName);

	m_scripts[scriptIndex] = script;
	script->m_fiberMain = fiberMain;
	script->m_fiberCurrent = fiberScript;

	try {
		script->OnInit();
	} catch (System::Exception^ ex) {
		RDR2::WriteLog("*** Exception in script OnInit: {0}", ex->ToString());
		return false;
	} catch (...) {
		RDR2::WriteLog("*** Unmanaged exception in script OnInit!");
		return false;
	}

	return true;
}

bool RDR2::ScriptDomain::ScriptExists(int scriptIndex)
{
	return scriptIndex < m_types->Length;
}

int RDR2::ScriptDomain::ScriptGetWaitTime(int scriptIndex)
{
	auto script = m_scripts[scriptIndex];
	if (script == nullptr) {
		return 0;
	}
	return script->m_fiberWait;
}

void RDR2::ScriptDomain::ScriptResetWaitTime(int scriptIndex)
{
	auto script = m_scripts[scriptIndex];
	if (script == nullptr) {
		return;
	}
	script->m_fiberWait = 0;
}

void RDR2::ScriptDomain::ScriptTick(int scriptIndex)
{
	try {
		auto script = m_scripts[scriptIndex];
		if (script != nullptr) {
			script->ProcessOneTick();
		}
	} catch (System::Exception^ ex) {
		RDR2::WriteLog("*** Exception in script ProcessOneTick: {0}", ex->ToString());
	} catch (...) {
		RDR2::WriteLog("*** Unmanaged exception in script ProcessOneTick!");
	}

	try {
		RDR2::Native::Function::ClearStringPool();
	} catch (System::Exception^ ex) {
		RDR2::WriteLog("*** Exception while clearing string pool: {0}", ex->ToString());
	} catch (...) {
		RDR2::WriteLog("*** Unmanaged exception while clearning string pool!");
	}
}


void RDR2::ScriptDomain::QueueKeyboardEvent(System::Tuple<bool, System::Windows::Forms::Keys>^ ev)
{
	for each (auto script in m_scripts) {
		if (script == nullptr) {
			continue;
		}
		script->m_keyboardEvents->Enqueue(ev);
	}
}

void RDR2::ScriptDomain::OnUnhandledException(System::Object^ sender, System::UnhandledExceptionEventArgs^ e)
{
	RDR2::WriteLog("*** Unhandled exception: {0}", e->ExceptionObject->ToString());
}

System::Reflection::Assembly^ RDR2::ScriptDomain::OnAssemblyResolve(System::Object^ sender, System::ResolveEventArgs^ args)
{
	if (args->RequestingAssembly != nullptr) {
		RDR2::WriteLog("Resolving assembly: \"{0}\" from: \"{1}\"", args->Name, args->RequestingAssembly->FullName);
	} else {
		RDR2::WriteLog("Resolving assembly: \"{0}\"", args->Name);
	}

	auto exeAssembly = System::Reflection::Assembly::GetExecutingAssembly();
	if (args->Name == exeAssembly->FullName) {
		RDR2::WriteLog("  Returning exeAssembly: \"{0}\"", exeAssembly->FullName);
		return exeAssembly;
	}

	return args->RequestingAssembly;

	//RDR2::WriteLog("  Returning nullptr");
	//return nullptr;
}
