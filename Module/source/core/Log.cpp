#include "Log.h"

void RDR2::WriteLog(System::String^ format, ... array<System::Object^>^ args)
{
	// Please note that this is required since we're working in a separate AppDomain 50% of the time
	try {
		auto writerPath = System::IO::Path::ChangeExtension(System::Reflection::Assembly::GetExecutingAssembly()->Location, ".log");
		auto writer = gcnew System::IO::StreamWriter(writerPath, true);
		auto text = ("[{0}] {1}", System::DateTime::Now.ToString("HH:mm:ss.fff"), System::String::Format(format, args));
		writer->WriteLine(text);
		System::Console::WriteLine(text);
		delete writer;
	} catch (...) { /* um so shvdn ignores this too.. it's dumb but whatever */ }
}
