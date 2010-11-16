#region Copyright & License Information
/*
 * Copyright 2007-2010 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see LICENSE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace OpenRA.Network
{
    public enum ConnectionState
	{
		PreConnecting,
		NotConnected,
		Connecting,
		Connected,
	}

    public interface IConnection : IDisposable
	{
		int LocalClientId { get; }
		ConnectionState ConnectionState { get; }
		int OrderLatency { get; }
		void Send( int frame, List<byte[]> orders );
		void SendImmediate( List<byte[]> orders );
		void SendSync( int frame, byte[] syncData );
		void Receive( Action<int, byte[]> packetFn );
	}

	class EchoConnection : IConnection
	{
		protected struct ReceivedPacket
		{
			public int FromClient;
			public byte[] Data;
		}
		protected List<ReceivedPacket> receivedPackets = new List<ReceivedPacket>();

		public virtual int LocalClientId
		{
			get { return 1; }
		}

		public virtual ConnectionState ConnectionState
		{
			get { return ConnectionState.PreConnecting; }
		}

		public virtual int OrderLatency { get { return 0; } }

		int nextSendFrame = 1;

		public virtual void Send( int frame, List<byte[]> orders )
		{
			if( nextSendFrame != frame )
				throw new InvalidOperationException( "nextSentFrame is wrong" );

			var ms = new MemoryStream();
			ms.Write( BitConverter.GetBytes( frame ) );
			foreach( var o in orders )
				ms.Write( o );
			Send( ms.ToArray() );

			nextSendFrame = frame + 1;
		}

		public virtual void SendImmediate( List<byte[]> orders )
		{
			var ms = new MemoryStream();
			ms.Write( BitConverter.GetBytes( (int)0 ) );
			foreach( var o in orders )
				ms.Write( o );
			Send( ms.ToArray() );
		}

		public virtual void SendSync( int frame, byte[] syncData )
		{
			var ms = new MemoryStream();
			ms.Write( BitConverter.GetBytes( frame ) );
			ms.Write( syncData );
			Send( ms.ToArray() );
		}

		protected virtual void Send( byte[] packet )
		{
			if( packet.Length == 0 )
				throw new NotImplementedException();
			lock( this )
				receivedPackets.Add( new ReceivedPacket { FromClient = LocalClientId, Data = packet } );
		}

		public virtual void Receive( Action<int, byte[]> packetFn )
		{
			List<ReceivedPacket> packets;
			lock( this )
			{
				packets = receivedPackets;
				receivedPackets = new List<ReceivedPacket>();
			}

			foreach( var p in packets )
				packetFn( p.FromClient, p.Data );
		}

		public virtual void Dispose() { }
	}
}
