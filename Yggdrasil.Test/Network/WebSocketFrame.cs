// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System.Text;
using Xunit;
using Yggdrasil.Network.WebSocket;

namespace Yggdrasil.Test.Network
{
	public class WebSocketFrameTests
	{
		[Fact]
		public void InitUnmasked()
		{
			var framer = new WebSocketFramer(1024);

			var buffer = framer.Frame(new byte[] { 4, 5, 6 }, false);
			var frame = new WebSocketFrame(buffer);
			Assert.Equal(true, frame.Fin);
			Assert.Equal(FrameOpCode.BinaryData, frame.OpCode);
			Assert.Equal(new byte[] { 4, 5, 6 }, frame.PayLoad);

			buffer = framer.Frame("test", false);
			frame = new WebSocketFrame(buffer);
			Assert.Equal(true, frame.Fin);
			Assert.Equal(FrameOpCode.TextData, frame.OpCode);
			Assert.Equal(Encoding.UTF8.GetBytes("test"), frame.PayLoad);
		}

		[Fact]
		public void InitMasked()
		{
			var framer = new WebSocketFramer(1024);

			var buffer = framer.Frame(new byte[] { 4, 5, 6 }, true);
			var frame = new WebSocketFrame(buffer);
			Assert.Equal(true, frame.Fin);
			Assert.Equal(FrameOpCode.BinaryData, frame.OpCode);
			Assert.Equal(new byte[] { 4, 5, 6 }, frame.PayLoad);

			buffer = framer.Frame("test", true);
			frame = new WebSocketFrame(buffer);
			Assert.Equal(true, frame.Fin);
			Assert.Equal(FrameOpCode.TextData, frame.OpCode);
			Assert.Equal(Encoding.UTF8.GetBytes("test"), frame.PayLoad);
		}
	}
}
