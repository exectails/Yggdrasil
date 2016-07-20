// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil.Logging
{
	[Flags]
	public enum LogLevel
	{
		Info = 0x0001,
		Warning = 0x0002,
		Error = 0x0004,
		Debug = 0x0008,
		Status = 0x0010,
		None = 0x7FFF,
	}
}
