#include "Function.h"

#include "NativeObjects.h"
#include "ManagedGlobals.h"
#include "Log.h"

#include "Config.h"

#include "Script.h"

#pragma unmanaged
#include <main.h>
#pragma managed


generic <typename T> T RDR2::Native::Function::Call(RDR2::Native::Hash hash, ... array<InputArgument^>^ arguments)
{
	//TODO: If calling outside of the executing fiber, queue a NativeTask to an SVHDN control thread

#ifdef THROW_ON_MULTITHREADED_NATIVES
	if (RDR2::Script::GetExecuting() == nullptr) {
		throw gcnew System::Exception("Illegal native call outside of main script thread!");
	}
#endif

	nativeInit((UINT64)hash);
	for each (InputArgument^ arg in arguments) {
		nativePush64(arg->_data);
	}

	auto ret = static_cast<T>(DecodeObject(T::typeid, nativeCall()));

	return ret;
}

void RDR2::Native::Function::Call(RDR2::Native::Hash hash, ... array<InputArgument^>^ arguments)
{
#ifdef THROW_ON_MULTITHREADED_NATIVES
	if (RDR2::Script::GetExecuting() == nullptr) {
		throw gcnew System::Exception("Illegal native call outside of main script thread!");
	}
#endif

	nativeInit((UINT64)hash);
	for each (auto arg in arguments) {
		nativePush64(EncodeObject(arg));
	}
	nativeCall();

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
