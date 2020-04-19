//
// Copyright (C) 2015 crosire & contributors
// License: https://github.com/crosire/scripthookvdotnet#license
//

namespace RDRNetwork.API
{
	public interface IHandleable
	{
		int Handle { get; }

		bool Exists();
	}
}
