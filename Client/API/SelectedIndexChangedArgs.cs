//
// Copyright (C) 2015 crosire & contributors
// License: https://github.com/crosire/scripthookvdotnet#license
//

using System;

namespace RDRNetwork.API
{
	public class SelectedIndexChangedArgs : EventArgs
	{
		public SelectedIndexChangedArgs(int selectedIndex)
		{
			SelectedIndex = selectedIndex;
		}

		public int SelectedIndex
		{
			get;
		}
	}
}
