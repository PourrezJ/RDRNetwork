#include <Tasks/NativeTask.h>

#include <NativeObjects.h>

#pragma unmanaged
#include <main.h>
#pragma managed

RDR2::Tasks::NativeTask::NativeTask()
{
	m_hash = 0;
	m_result = nullptr;
}

void RDR2::Tasks::NativeTask::Run()
{
	nativeInit(m_hash);
	for each (auto arg in m_arguments) {
		nativePush64(EncodeObject(arg));
	}
	m_result = nativeCall();
}
