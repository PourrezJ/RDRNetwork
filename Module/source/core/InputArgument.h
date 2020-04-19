#pragma once

#include "INativeValue.h"

namespace RDR2
{
	#pragma region Forward Declarations
		ref class Blip;
		ref class Camera;
		ref class Entity;
		ref class Ped;
		ref class PedGroup;
		ref class Player;
		ref class Prop;
		ref class Rope;
		ref class Vehicle;
	#pragma endregion

	namespace Native
	{
		public ref class InputArgument
		{
		public:
			InputArgument(System::UInt64 value);
			InputArgument(System::Object^ value);
			inline InputArgument(bool value) : InputArgument(static_cast<bool>(value) ? System::UInt64(1) : System::UInt64(0)) { }
			inline InputArgument(int value) : InputArgument(static_cast<System::UInt32>(value)) { }
			inline InputArgument(unsigned int value) : InputArgument(static_cast<System::UInt64>(value)) { }
			inline InputArgument(float value)
			{
				unsigned int valueUInt32 = reinterpret_cast<System::UInt32&>(value);
				_data = valueUInt32;
			}
			inline InputArgument(double value)
			{
				float valueFloat = static_cast<float>(value);
				unsigned int valueUInt32 = reinterpret_cast<System::UInt32&>(valueFloat);
				_data = valueUInt32;
			}
			inline InputArgument(System::String^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			//inline InputArgument(Model value) : InputArgument(static_cast<System::UInt32>(value.Hash)) { }
			inline InputArgument(Blip^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			inline InputArgument(Camera^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			inline InputArgument(Entity^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			inline InputArgument(Ped^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			inline InputArgument(PedGroup^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			inline InputArgument(Player^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			inline InputArgument(Prop^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			inline InputArgument(Vehicle^ value) : InputArgument(static_cast<System::Object^>(value)) { }
			//inline InputArgument(Rope^ value) : InputArgument(static_cast<System::Object^>(value)) { }

			static inline operator InputArgument ^ (bool value)
			{
				return gcnew InputArgument(value);
			}
			static inline operator InputArgument ^ (bool* value)
			{
				return gcnew InputArgument(reinterpret_cast<unsigned long long>(value));
			}
			static inline operator InputArgument ^ (char value)
			{
				return gcnew InputArgument(static_cast<unsigned char>(value));
			}
			static inline operator InputArgument ^ (unsigned char value)
			{
				return gcnew InputArgument(static_cast<int>(value));
			}
			static inline operator InputArgument ^ (short value)
			{
				return gcnew InputArgument(static_cast<unsigned short>(value));
			}
			static inline operator InputArgument ^ (unsigned short value)
			{
				return gcnew InputArgument(static_cast<int>(value));
			}
			static inline operator InputArgument ^ (int value)
			{
				return gcnew InputArgument(value);
			}
			static inline operator InputArgument ^ (int* value)
			{
				return gcnew InputArgument(reinterpret_cast<unsigned long long>(value));
			}
			static inline operator InputArgument ^ (unsigned int value)
			{
				return gcnew InputArgument(value);
			}
			static inline operator InputArgument ^ (unsigned int* value)
			{
				return gcnew InputArgument(reinterpret_cast<unsigned long long>(value));
			}
			static inline operator InputArgument ^ (float value)
			{
				return gcnew InputArgument(value);
			}
			static inline operator InputArgument ^ (float* value)
			{
				return gcnew InputArgument(reinterpret_cast<unsigned long long>(value));
			}
			static inline operator InputArgument ^ (double value)
			{
				return gcnew InputArgument(value);
			}
			static inline operator InputArgument ^ (System::String^ value)
			{
				return gcnew InputArgument(value);
			}
			static inline operator InputArgument ^ (const char value[])
			{
				return gcnew InputArgument(gcnew System::String(value));
			}
			/*
			static inline operator InputArgument ^ (Model model)
			{
				return gcnew InputArgument(model);
			}*/
			static inline operator InputArgument ^ (Blip^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (Camera^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (Entity^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (Ped^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (PedGroup^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (Player^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (Prop^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (Vehicle^ object)
			{
				return gcnew InputArgument(object);
			}
			static inline operator InputArgument ^ (Rope^ object)
			{
				return gcnew InputArgument(object);
			}

			virtual System::String^ ToString() override
			{
				return _data.ToString();
			}

		internal:
			System::UInt64 _data;
		};
	}
}
