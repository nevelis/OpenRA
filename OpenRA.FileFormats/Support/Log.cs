#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using OpenRA.FileFormats;

namespace OpenRA
{
	public struct ChannelInfo
	{
	//	public string Filename;
		public StreamWriter Writer;
	}

	public static class Log
	{
        static PathElement LogPathPrefix = P.HomeDir;
		static Dictionary<string, ChannelInfo> channels = new Dictionary<string,ChannelInfo>();

		public static PathElement LogPath
		{
			get { return LogPathPrefix; }
			set
			{
				LogPathPrefix = value;
                LogPathPrefix.CreateDir();
			}
		}

        static IEnumerable<PathElement> FilenamesForChannel(string channelName, string baseFilename)
        {
            for (var i = 0; ; i++)
                if (i == 0)
                    yield return LogPathPrefix / baseFilename;
                else
                    yield return LogPathPrefix / "{0}.{1}".F(baseFilename, i);
        }

        public static void AddChannel(string channelName, string baseFilename)
        {
            if (channels.ContainsKey(channelName)) return;

            foreach (var filename in FilenamesForChannel(channelName, baseFilename))
                try
                {
                    var writer = filename.CreateText();
                    writer.AutoFlush = true;

                    channels.Add(channelName,
                        new ChannelInfo()
                        {
                       //     Filename = filename,
                            Writer = writer
                        });

                    return;
                }
                catch (IOException) { }
        }

		public static void Write(string channel, string format, params object[] args)
		{
			ChannelInfo info;
			if (!channels.TryGetValue(channel, out info))
				throw new Exception("Tried logging to non-existant channel " + channel);

			info.Writer.WriteLine(format, args);
		}
	}
}
