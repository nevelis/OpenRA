using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenRA.FileFormats
{
    public class PathElement
    {
        readonly string s;

        public PathElement(string s) { this.s = s; }
        public override string ToString() { return s; }
        public static PathElement operator /(PathElement a, PathElement b) { return new PathElement(Path.Combine(a.s, b.s)); }
        public static PathElement operator /(PathElement a, string b) { return new PathElement(Path.Combine(a.s, b)); }

        public void CreateDir() { Directory.CreateDirectory(s); }
        public void DeleteDir() { Directory.Delete(s); }
        public void DeleteDirRecursive() { Directory.Delete(s, true); }
        public bool Exists() { return Directory.Exists(s) || File.Exists(s); }

        public FileStream Create() { return File.Create(s); }
        public StreamWriter CreateText() { return File.CreateText(s); }

        public IEnumerable<PathElement> GetFiles(string pattern) { return Directory.GetFiles(s, pattern).Select(f => P.E(f)); }
        public IEnumerable<PathElement> GetFiles() { return Directory.GetFiles(s).Select(f => P.E(f)); }
        public IEnumerable<PathElement> GetDirs() { return Directory.GetDirectories(s).Select(f => P.E(f)); }

        public PathElement BaseName() { return P.E(Path.GetFileName(s)); }
        public PathElement DirName() { return P.E( Path.GetDirectoryName(s)); }

        public PathElement ExpandHomeDir() { if (s.StartsWith("~")) return P.HomeDir / s.Substring(1); else return this; }
    }

    public static class P
    {
        public static PathElement E(string s) { return new PathElement(s); }
        public static PathElement CurrentDir { get { return E(Environment.CurrentDirectory); } }
        public static PathElement HomeDir { get { return E(Environment.GetFolderPath(Environment.SpecialFolder.Personal)); } }
        public static PathElement MakeTempFilename() { return P.E(Path.GetTempFileName()); }
        public static PathElement TempDir { get { return E(Path.GetTempPath()); } }
    }
}
