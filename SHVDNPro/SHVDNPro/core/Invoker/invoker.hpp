#pragma once
typedef DWORD Void;
typedef DWORD Any;
typedef DWORD uint;
typedef DWORD Hash;
typedef int Entity;
typedef int Player;
typedef int FireId;
typedef int Ped;
typedef int Vehicle;
typedef int Cam;
typedef int CarGenerator;
typedef int Group;
typedef int Train;
typedef int Pickup;
typedef int Object;
typedef int Weapon;
typedef int Interior;
typedef int Blip;
typedef int Texture;
typedef int TextureDict;
typedef int CoverPoint;
typedef int Camera;
typedef int TaskSequence;
typedef int ColourIndex;
typedef int Sphere;
typedef int INT, ScrHandle;
struct Vector3
{
	float x;
	float y;
	float z;
};

class Context // credits to rdr2 ScriptHook
{
	// Internal RAGE stuff
	uint64_t* retVal = stack;
	uint64_t argCount = 0;
	uint64_t* stackPtr = stack;
	uint64_t dataCount = 0;
	uint64_t spaceForResults[24];
	// Our stack
	uint64_t stack[24]{ 0 };

public:
	template<class T>
	T& At(uint32_t idx) {
		static_assert(sizeof(T) <= 8, "Argument is too big");

		return *reinterpret_cast<T*>(stack + idx);
	}

	uint32_t GetArgsCount() {
		return argCount;
	}

	void SetArgsCount(uint32_t idx) {
		argCount = idx;
	}

	template<class T, class... Args>
	void Push(T arg, Args... args) {
		static_assert(sizeof(T) <= 8, "Argument is too big");

		*(T*)(stack + argCount++) = arg;

		if constexpr (sizeof...(Args) > 0)
			Push(args...);
	}

	template<class T>
	T Result() {
		return *reinterpret_cast<T*>(retVal);
	}
	template<>
	void Result<void>() { }

	template<>
	Vector3 Result<Vector3>() {
		Vector3 vec;
		vec.x = *(float*)((uintptr_t)retVal + 0);
		vec.y = *(float*)((uintptr_t)retVal + 8);
		vec.z = *(float*)((uintptr_t)retVal + 16);
		return vec;
	}

	void Reset() {
		argCount = 0;
		dataCount = 0;
	}

	void CopyResults() {
		uint64_t a1 = (uint64_t)this;

		uint64_t result;

		for (; *(uint32_t*)(a1 + 24); *(uint32_t*)(*(uint64_t*)(a1 + 8i64 * *(signed int*)(a1 + 24) + 32) + 16i64) = result)
		{
			--* (uint32_t*)(a1 + 24);
			**(uint32_t**)(a1 + 8i64 * *(signed int*)(a1 + 24) + 32) = *(uint32_t*)(a1 + 16 * (*(signed int*)(a1 + 24) + 4i64));
			*(uint32_t*)(*(uint64_t*)(a1 + 8i64 * *(signed int*)(a1 + 24) + 32) + 8i64) = *(uint32_t*)(a1
				+ 16i64
				* *(signed int*)(a1 + 24)
				+ 68);
			result = *(unsigned int*)(a1 + 16i64 * *(signed int*)(a1 + 24) + 72);
		}
		--* (uint32_t*)(a1 + 24);
	}
};

typedef void(__cdecl* Handler)(Context* context);
template<class Retn = uint64_t, class... Args>
static Retn invoke_(Handler fn, Args... args)
{
	static Context ctx;

	if (!fn) return Retn();

	ctx.Reset();

	if constexpr (sizeof...(Args) > 0)
		ctx.Push(args...);

	fn(&ctx);
	ctx.CopyResults();


	return ctx.Result<Retn>();
}

static Handler get_handler(uintptr_t hash_) {
	static auto base_address = (uintptr_t)GetModuleHandleA(0);
	auto it = nativehash_to_address_table.find(hash_);
	if (it != nativehash_to_address_table.end()) {
		if (it->first == hash_)
			return (Handler)(base_address + it->second);
	}
	return 0;
}

template<class Retn = uint64_t, class... Args>
static Retn invoke(uint64_t hashName, Args... args) {
	return invoke_<Retn>(get_handler(hashName), args...);
}

template <uint32_t stackSize> class NativeStack
{
public:

	static constexpr uint32_t size = stackSize;

protected:

	uint64_t stack[stackSize];

	template <typename T> T getAt(uint32_t index)const
	{
		return reinterpret_cast<const T&>(stack[index]);
	}

	template <> bool getAt<bool>(uint32_t index)const
	{
		return reinterpret_cast<const int&>(stack[index]) != 0;
	}

	template <typename T> void setAt(uint32_t index, const T& value)
	{
		reinterpret_cast<T&>(stack[index]) = value;
	}

	template <> void setAt<bool>(uint32_t index, const bool& value)
	{
		reinterpret_cast<int&>(stack[index]) = value != 0;
	}

public:

	decltype(stack)& getRawPtr() { return stack; };
	const decltype(stack)& getRawPtr()const { return stack; }
};


struct NativeArgStack : NativeStack<32u>
{
	template <typename T> NativeArgStack* setArg(const T& value, uint32_t index)
	{
		if (index < size) setAt<T>(index, value); return this;
	}

	template <typename T, uint32_t index> NativeArgStack* setArg(const T& value)
	{
		setAt<T>(index, value); return this;
	}

	template <typename T> T getArg(uint32_t index)const
	{
		return (index < size) ? getAt<T>(index) : T();
	}

	template <typename T, uint32_t index> NativeArgStack* getArg()const
	{
		return getAt<T>(index);
	}
};

struct NativeReturnStack : NativeStack<3u>
{
	template <typename T> T Get() const
	{
		return getAt<T>(0);
	}

	template <typename T> NativeReturnStack* Set(const T& value)
	{
		setAt(0, value); return this;
	}
};

class scrNativeCallContext
{
public:
	static constexpr uint32_t maxNativeArgCount = NativeArgStack::size;
	static constexpr uint32_t maxNativeReturnCount = NativeReturnStack::size;
	static void(*SetVectorResults)(scrNativeCallContext*);
private:
	NativeReturnStack* const m_pReturns;
	uint32_t m_nArgCount;
	NativeArgStack* const m_pArgs;
	uint32_t m_nDataCount;
	uint64_t reservedSpace[24] = {};
public:
	scrNativeCallContext(NativeReturnStack* returnStack, NativeArgStack* argStack)
		: m_pReturns(returnStack)
		, m_nArgCount(0)
		, m_pArgs(argStack)
		, m_nDataCount(0) {}

	void Reset()
	{
		SetArgCount<0>();
		SetDataCount<0>();
	}

	void FixVectors() { SetVectorResults(this); }

	template <typename T> scrNativeCallContext* Push(const T& value)
	{
		m_pArgs->setArg(value, m_nArgCount++); return this;
	}

	template <typename T, uint32_t index> scrNativeCallContext* Push(const T& value)
	{
		m_pArgs->setArg<T, index>(value); return *this;
	}

	template <uint32_t ArgCount> void SetArgCount()
	{
		m_nArgCount = ArgCount;
	}

	template <uint32_t DataCount> void SetDataCount()
	{
		m_nDataCount = DataCount;
	}

	template <typename T> T GetResult() const
	{
		return m_pReturns->Get<T>();
	}

	template <typename T> scrNativeCallContext* SetResult(const T& value)
	{
		m_pReturns->Set(value); return this;
	}

	template <typename T> T GetArg(uint32_t index) const
	{
		return m_pArgs->getArg<T>(index);
	}

	template <typename T> scrNativeCallContext* SetArg(uint32_t index, const T& value)
	{
		m_pArgs->setArg(index, value); return this;
	}

	uint32_t getArgCount() const { return m_nArgCount; }

	NativeArgStack& getArgStack() const { return *m_pArgs; }
	NativeReturnStack& getReturnStack() const { return *m_pReturns; }
};