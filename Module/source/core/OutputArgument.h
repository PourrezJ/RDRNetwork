#pragma once

#include "InputArgument.h"

namespace RDR2
{
	namespace Native
	{
		public ref class OutputArgument : public InputArgument
		{
		public:
			OutputArgument();
			OutputArgument(System::Object^ initvalue);
			inline OutputArgument(bool initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(char initvalue) : OutputArgument(static_cast<System::Object^>(static_cast<int>(initvalue))) { }
			inline OutputArgument(unsigned char initvalue) : OutputArgument(static_cast<System::Object^>(static_cast<int>(initvalue))) { }
			inline OutputArgument(short initvalue) : OutputArgument(static_cast<System::Object^>(static_cast<int>(initvalue))) { }
			inline OutputArgument(unsigned short initvalue) : OutputArgument(static_cast<System::Object^>(static_cast<int>(initvalue))) { }
			inline OutputArgument(int initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(unsigned int initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(float initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(double initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(System::String^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			//inline OutputArgument(Model initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			/*
			inline OutputArgument(Blip^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(Camera^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(Entity^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(Ped^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(PedGroup^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(Player^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(Prop^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			inline OutputArgument(Vehicle^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }*/
			//inline OutputArgument(Rope^ initvalue) : OutputArgument(static_cast<System::Object^>(initvalue)) { }
			~OutputArgument();
			void* GetPointer();
			generic <typename T>
				T GetResult();

		protected:
			
			!OutputArgument();

			unsigned char* _storage;
		};
	}
}
