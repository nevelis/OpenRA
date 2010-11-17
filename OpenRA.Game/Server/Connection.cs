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
using System.Linq;
using System.Net.Sockets;
using OpenRA.Network;
using System.Net;

namespace OpenRA.Server
{
	public class Connection
	{
		public int MostRecentFrame { get; private set; }

		readonly Socket socket;
		public EndPoint RemoteEndPoint { get { return socket.RemoteEndPoint; } }
		public readonly int PlayerIndex;

		public Connection( Socket socket, int playerIndex )
		{
			this.socket = socket;
			this.PlayerIndex = playerIndex;

			socket.Send(BitConverter.GetBytes(ProtocolVersion.Version));
			socket.Send(BitConverter.GetBytes(PlayerIndex));
		}

		public void StartReader( Server server )
		{
			socket.ReadLengthPrefixedBytesAsync( b =>
				{
					lock( server )
					{
						var frame = BitConverter.ToInt32( b, 0 );
						server.DispatchOrders( this, frame, b.Skip( 4 ).ToArray() );
						MostRecentFrame = frame;
						server.UpdateInFlightFrames( frame, this );
					}
					StartReader( server );
				}, error => server.DropClient( this, error ) );
		}

		public void Send( byte[] bytes )
		{
			socket.Send( bytes );
		}
	}
}
