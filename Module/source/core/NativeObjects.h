#pragma once

System::UInt64 EncodeObject(System::Object^ obj);
System::Object^ DecodeObject(System::Type^ type, System::UInt64* value);

public interface class IHandleable
{
	property int Handle
	{
		int get();
	}

	bool Exists();
};