// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Yggdrasil.Network
{
	public interface IMessageFramer
	{
		byte[] Frame(byte[] message);
		void ReceiveData(byte[] data, int length);
	}
}
