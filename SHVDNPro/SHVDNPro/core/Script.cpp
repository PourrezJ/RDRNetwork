#include <cstdio>

#include <Script.h>

#include <ManagedGlobals.h>
#include <Log.h>

#include <Config.h>

#pragma unmanaged
#include <Windows.h>
#undef Yield

#include <UnmanagedLog.h>

static void ScriptSwitchToMainFiber(void* fiber)
{
	SwitchToFiber(fiber);
}
#pragma managed

RDR2::Script::Script()
{
	m_fiberMain = nullptr;
	m_fiberWait = 0;
}

void RDR2::Script::Wait(int ms)
{
#ifdef THROW_ON_WRONG_FIBER_YIELD
	if (GetCurrentFiber() != m_fiberCurrent) {
		throw gcnew System::Exception(System::String::Format("Illegal fiber wait {0} from invalid thread:\n{1}", ms, System::Environment::StackTrace));
	}
#endif

	m_fiberWait = ms;
	ScriptSwitchToMainFiber(m_fiberMain);
}

void RDR2::Script::Yield()
{
	Wait(0);
}

void RDR2::Script::OnInit()
{
}

void RDR2::Script::OnTick()
{
}

void RDR2::Script::OnPresent(System::IntPtr swapchain)
{
}

void RDR2::Script::OnKeyDown(System::Windows::Forms::KeyEventArgs^ args)
{
}

void RDR2::Script::OnKeyUp(System::Windows::Forms::KeyEventArgs^ args)
{
}

RDR2::Script^ RDR2::Script::GetExecuting()
{
	return RDR2::ManagedGlobals::g_scriptDomain->GetExecuting();
}

void RDR2::Script::WaitExecuting(int ms)
{
	auto script = GetExecuting();
	if (script == nullptr) {
		throw gcnew System::Exception("Illegal call to WaitExecuting() from a non-script fiber!");
	}
	script->Wait(ms);
}

void RDR2::Script::YieldExecuting()
{
	WaitExecuting(0);
}

void RDR2::Script::ProcessOneTick()
{
	System::Tuple<bool, System::Windows::Forms::Keys>^ ev = nullptr;

	while (m_keyboardEvents->TryDequeue(ev)) {
		try {
			if (ev->Item1) {
				OnKeyDown(gcnew System::Windows::Forms::KeyEventArgs(ev->Item2));
			} else {
				OnKeyUp(gcnew System::Windows::Forms::KeyEventArgs(ev->Item2));
			}
		} catch (System::Exception^ ex) {
			if (ev->Item1) {
				RDR2::WriteLog("*** Exception during OnKeyDown: {0}", ex->ToString());
			} else {
				RDR2::WriteLog("*** Exception during OnKeyUp: {0}", ex->ToString());
			}
		}
	}

	try {
		OnTick();
	} catch (System::Exception^ ex) {
		RDR2::WriteLog("*** Exception during OnTick: {0}", ex->ToString());
	} catch (...) {
		RDR2::WriteLog("*** Unmanaged exception during OnTick in {0}", GetType()->FullName);
	}
}
