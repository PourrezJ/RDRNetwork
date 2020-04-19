#include <OutputArgument.h>

#include <NativeObjects.h>

#include <cstring>
#include <cstdlib>

RDR2::Native::OutputArgument::OutputArgument()
{
	_storage = malloc(24);
	memset(_storage, 0, 24);
}

RDR2::Native::OutputArgument::OutputArgument(Object ^value) : OutputArgument()
{
	*reinterpret_cast<System::UInt64*>(_storage) = EncodeObject(value);
}

RDR2::Native::OutputArgument::~OutputArgument()
{
	this->!OutputArgument();
}

generic <typename T> T RDR2::Native::OutputArgument::GetResult()
{
	return static_cast<T>(DecodeObject(T::typeid, reinterpret_cast<System::UInt64*>(_storage)));
}

void* RDR2::Native::OutputArgument::GetPointer()
{
	return _storage;
}

RDR2::Native::OutputArgument::!OutputArgument()
{
	free(_storage);
}
