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
using System.Linq;
using System.Net.Sockets;

namespace OpenRA.Network
{
	static class OrderIO
	{
		public static void Write(this Stream s, byte[] buf)
		{
			s.Write(buf, 0, buf.Length);
		}

		public static List<Order> ToOrderList(this byte[] bytes, World world)
		{
			var ms = new MemoryStream(bytes, 4, bytes.Length - 4);
			var reader = new BinaryReader(ms);
			var ret = new List<Order>();
			while( ms.Position < ms.Length )
			{
				var o = Order.Deserialize( world, reader );
				if( o != null )
					ret.Add( o );
			}
			return ret;
		}

		public static byte[] SerializeSync( this List<int> sync )
		{
			var ms = new MemoryStream();
			using( var writer = new BinaryWriter( ms ) )
			{
				writer.Write( (byte)0x65 );
				foreach( var s in sync )
					writer.Write( s );
			}
			return ms.ToArray();
		}

		public static void ReadLengthPrefixedBytesAsync( this Socket socket, Action<byte[]> fn, Action<Exception> exceptionCallback )
		{
			SocketAsyncReader.Read( socket, 4, b =>
				{
					SocketAsyncReader.Read( socket, BitConverter.ToInt32( b, 0 ), fn, exceptionCallback );
				}, exceptionCallback );
		}

		class SocketAsyncReader
		{
			Socket socket;
			byte[] bytes;
			int remaining;
			Action<byte[]> callback;
			Action<Exception> exceptionCallback;

			public static void Read( Socket socket, int length, Action<byte[]> fn, Action<Exception> exceptionCallback )
			{
				new SocketAsyncReader
				{
					socket = socket,
					remaining = length,
					bytes = new byte[ length ],
					callback = fn,
					exceptionCallback = exceptionCallback,
				}.Read();
			}

			void Read()
			{
				if( remaining == 0 )
					callback( bytes );
				else
					socket.BeginReceive( bytes, bytes.Length - remaining, remaining, SocketFlags.None, ReadCallback, null );
			}

			void ReadCallback( IAsyncResult ar )
			{
				try
				{
					remaining -= socket.EndReceive( ar );
					Read();
				}
				catch( Exception e )
				{
					if( exceptionCallback != null )
						exceptionCallback( e );
				}
			}
		}
	}
}
