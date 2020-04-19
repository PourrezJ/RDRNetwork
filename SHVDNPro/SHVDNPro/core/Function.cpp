#include <Function.h>

#include <NativeObjects.h>
#include <ManagedGlobals.h>
#include <Log.h>

#include <Config.h>

#ifdef THROW_ON_MULTITHREADED_NATIVES
#include <Script.h>
#endif

#pragma unmanaged
#include <main.h>
#pragma managed

static void LogNative(System::String^ type, RDR2::Native::Hash hash, array<RDR2::Native::InputArgument^>^ args)
{
	/*
	RDR2::ManagedGlobals::g_logWriter->Write("{0:HH\\:mm\\:ss\\.fff} Native {1} {2:X16} {3}", System::DateTime::Now, type, (System::UInt64)hash, hash);
	for each (auto arg in args) {
		RDR2::ManagedGlobals::g_logWriter->Write(", \"{0}\" ({1})", arg, arg->GetType()->FullName);
	}
	*/
	//TODO
	RDR2::WriteLog("LogNative not implemented atm.. pls fix");
}

static void LogNativeOK()
{
	//RDR2::WriteLog(" OK!");
}

generic <typename T> T RDR2::Native::Function::Call(RDR2::Native::Hash hash, ... array<InputArgument^>^ arguments)
{
	//TODO: If calling outside of the executing fiber, queue a NativeTask to an SVHDN control thread

#ifdef THROW_ON_MULTITHREADED_NATIVES
	if (RDR2::Script::GetExecuting() == nullptr) {
		throw gcnew System::Exception("Illegal native call outside of main script thread!");
	}
#endif

#ifdef LOG_ALL_NATIVES
	LogNative("return invoke", hash, arguments);
#endif

	nativeInit((UINT64)hash);
	for each (InputArgument^ arg in arguments) {
		nativePush64(arg->_data);
	}
	auto ret = static_cast<T>(DecodeObject(T::typeid, nativeCall()));

#ifdef LOG_ALL_NATIVES
	LogNativeOK();
#endif

	return ret;
}

void RDR2::Native::Function::Call(RDR2::Native::Hash hash, ... array<InputArgument^>^ arguments)
{
#ifdef THROW_ON_MULTITHREADED_NATIVES
	if (RDR2::Script::GetExecuting() == nullptr) {
		throw gcnew System::Exception("Illegal native call outside of main script thread!");
	}
#endif

#ifdef LOG_ALL_NATIVES
	LogNative("invoke", hash, arguments);
#endif

	nativeInit((UINT64)hash);
	for each (auto arg in arguments) {
		nativePush64(EncodeObject(arg));
	}
	nativeCall();

#ifdef LOG_ALL_NATIVES
	LogNativeOK();
#endif
}

System::IntPtr RDR2::Native::Function::AddStringPool(System::String^ string)
{
	auto managedBuffer = System::Text::Encoding::UTF8->GetBytes(string);
	unsigned char* buffer = new unsigned char[managedBuffer->Length + 1];
	buffer[managedBuffer->Length] = '\0';
	System::IntPtr ret(buffer);
	System::Runtime::InteropServices::Marshal::Copy(managedBuffer, 0, ret, managedBuffer->Length);
	RDR2::Native::Function::UnmanagedStrings->Add(ret);
	return ret;
}

void RDR2::Native::Function::ClearStringPool()
{
	for each (auto ptr in RDR2::Native::Function::UnmanagedStrings) {
		delete[] ptr.ToPointer();
	}
	RDR2::Native::Function::UnmanagedStrings->Clear();
}
