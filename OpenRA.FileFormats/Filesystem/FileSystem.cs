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
using System.Reflection;
using System.Linq;

namespace OpenRA.FileFormats
{
	public static class FileSystem
	{
		static List<Pair<string, IFolder>> mountedFolders = new List<Pair<string, IFolder>>();

		static Cache<uint, List<IFolder>> allFiles = new Cache<uint, List<IFolder>>( _ => new List<IFolder>() );

		static void MountInner(IFolder folder, string basePath)
		{
			mountedFolders.Add( Pair.New( basePath, folder ) );

			foreach( var hash in folder.AllFileHashes() )
			{
				var l = allFiles[hash];
				if( !l.Contains( folder ) )
					l.Add( folder );
			}
		}

		static int order = 0;

		static IFolder OpenPackage(string filename)
		{
			if (filename.EndsWith(".mix"))
				return new MixFile(filename, order++);
			else if (filename.EndsWith(".zip"))
				return new CompressedPackage(filename, order++);
			else if (filename.EndsWith(".Z"))
				return new InstallShieldPackage(filename, order++);
			else
				return new Folder(filename, order++);
		}

		public static void Mount(string name, string basePath)
		{
			name = name.ToLowerInvariant();
			var optional = name.StartsWith("~");
			if (optional) name = name.Substring(1);

			var a = (Action)(() => FileSystem.MountInner(OpenPackage(name), basePath ?? ""));

			if (optional)
				try { a(); }
				catch { }
			else
				a();
		}

		public static void UnmountAll()
		{
			mountedFolders.Clear();
			allFiles = new Cache<uint, List<IFolder>>( _ => new List<IFolder>() );
		}

		public static void LoadFromManifest( Manifest manifest )
		{
			UnmountAll();
			foreach (var dir in manifest.Folders) Mount(dir.Key, dir.Value);
			foreach (var pkg in manifest.Packages) Mount(pkg.Key, pkg.Value);
		}

		static Stream GetFromCache( Cache<uint, List<IFolder>> index, string filename )
		{
			var folder = index[PackageEntry.HashFilename(filename)]
				.Where(x => x.Exists(filename))
				.OrderBy(x => x.Priority)
				.FirstOrDefault();

			if (folder != null)
				return folder.GetContent(filename);

			return null;
		}

		public static Stream Open(string filename)
		{
			if( filename.IndexOfAny( new char[] { '/', '\\' } ) == -1 )
			{
				var ret = GetFromCache( allFiles, filename );
				if( ret != null )
					return ret;
			}

			foreach( var f in mountedFolders )
			{
				if( !filename.StartsWith( f.First ) ) continue;
				var name = filename.Substring( f.First.Length );
				if( f.Second.Exists( name ) )
					return f.Second.GetContent( name );
			}

			throw new FileNotFoundException( string.Format( "File not found: {0}", filename ), filename );
		}

		public static Stream OpenWithExts( string filename, params string[] exts )
		{
			if( filename.IndexOfAny( new char[] { '/', '\\' } ) == -1 )
			{
				foreach( var ext in exts )
				{
					var s = GetFromCache( allFiles, filename + ext );
					if( s != null )
						return s;
				}
			}

			foreach( var ext in exts )
			{
				foreach( var folder in mountedFolders )
					if (folder.Second.Exists(filename + ext))
						return folder.Second.GetContent( filename + ext );
			}

			throw new FileNotFoundException( string.Format( "File not found: {0}", filename ), filename );
		}

		public static bool Exists(string filename)
		{
			foreach (var folder in mountedFolders)
				if (folder.Second.Exists(filename))
				    return true;
			return false;
		}

		static Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();

		public static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.FullName == e.Name)
					return assembly;
			}

			string[] frags = e.Name.Split(',');
			var filename = frags[0] + ".dll";
			Assembly a;
			if (assemblyCache.TryGetValue(filename, out a))
				return a;

			if (FileSystem.Exists(filename))
				using (Stream s = FileSystem.Open(filename))
				{
					byte[] buf = new byte[s.Length];
					s.Read(buf, 0, buf.Length);
					a = Assembly.Load(buf);
					assemblyCache.Add(filename, a);
					return a;
				}
			
			return null;
		}
	}
}
