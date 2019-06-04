using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CompBind
{
    public class Path
    {
        public bool IsGlobal = false;
        public LinkedList<PathElement> PathElements = new LinkedList<PathElement>();

        public Path() { }
        public Path(string path)
        {
            ParsePath(path);
        }
        public Path(Path otherPath)
        {
            IsGlobal = otherPath.IsGlobal;
            PathElements = new LinkedList<PathElement>(otherPath.PathElements);
        }

        public int Length
        {
            get
            {
                return PathElements.Count;
            }
        }

        /// <summary>
        /// Clears current elements and parses given string.
        /// </summary>
        /// <param name="path"></param>
        public void ParsePath(string path)
        {
            PathElements.Clear();

            List<string> splits = path.Split(new char[] { '.' }).ToList();

            // Always resolve this path from the tree root
            if (splits[0] == "#")
            {
                splits.RemoveAt(0);
                IsGlobal = true;
            }                                                                           //TODO: SOME FAIL CHECKS!

            foreach (string s in splits)
            {
                if(s == "-" || string.IsNullOrEmpty(s))
                {
                    // Ignore - empty path
                    continue;
                }
                else if (Regex.IsMatch(s, @"^\[\d+\]"))
                {
                    // Its an index
                    Match match = Regex.Match(s, @"^\[(\d+)\]");

                    int index = int.Parse(match.Groups[1].Value);
                    PathElements.AddLast(new PathElement(index));
                }
                else
                {
                    // Its a field
                    PathElements.AddLast(new PathElement(s));
                }
            }
        }

        public override string ToString()
        {
            string s = "";
            if (IsGlobal)
            {
                s += "#.";
            }

            int index = 0;
            foreach(PathElement pe in PathElements)
            {
                if (index != 0) s += ".";

                if(pe.Type == PathElement.PathElementType.Index)
                {
                    s += "[" + pe.Index + "]";
                }
                else
                {
                    s += pe.FieldName;
                }
                index++;
            }
            return s;
        }

        /// <summary>
        /// Checks if the path is a subpath (not equal) of the given path.
        /// </summary>
        /// <param name="otherPath"></param>
        /// <returns></returns>
        public bool IsSubpathOf(Path otherPath)
        {
            // Same length or shorter -> not a subpath!
            if (PathElements.Count <= otherPath.PathElements.Count) return false;

            // Walk both lists of path elements and compare
            var otherEnumerator = otherPath.PathElements.GetEnumerator();
            var thisEnumerator = PathElements.GetEnumerator();
            while (otherEnumerator.MoveNext())
            {
                thisEnumerator.MoveNext();
                if (otherEnumerator.Current != thisEnumerator.Current) return false;
            }
            return true;
        }

        public bool IsSamePath(Path otherPath)
        {
            if (PathElements.Count != otherPath.PathElements.Count) return false;

            // Walk both lists of path elements and compare
            var otherEnumerator = otherPath.PathElements.GetEnumerator();
            var thisEnumerator = PathElements.GetEnumerator();
            while (otherEnumerator.MoveNext())
            {
                thisEnumerator.MoveNext();
                if (otherEnumerator.Current != thisEnumerator.Current) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns pathelements in range [from, to[ .
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public Path GetSlice(int from, int to = -1)
        {
            if (to == -1) to = PathElements.Count;

            Path newPath = new Path();
            var enumerator = PathElements.GetEnumerator();
            enumerator.MoveNext();

            // Go to start position
            for (int i = 0; i < from; i++)
            {
                enumerator.MoveNext();
            }
            int currentIndex = from;
            while(currentIndex < to)
            {
                newPath.PathElements.AddLast(enumerator.Current);
                enumerator.MoveNext();
                currentIndex++;
            }
            return newPath;
        }

        /// <summary>
        /// Retrieves index of last index typed path element.
        /// </summary>
        /// <returns>Index, or -1, if no index typed element was found.</returns>
        public int GetLastIndex()
        {
            if (Length == 0) return -1;

            Path workingCopy = new Path(this);

            PathElement pe = workingCopy.PathElements.Last.Value;

            // Walk backwards until index pathelement is found
            while (pe.Type != PathElement.PathElementType.Index)
            {
                workingCopy.PathElements.RemoveLast();

                // Is list now empty?
                if (workingCopy.PathElements.Last == null) return -1;

                pe = workingCopy.PathElements.Last.Value;
            }

            if (pe.Type == PathElement.PathElementType.Index) return pe.Index;
            else return -1;
        }

        public override bool Equals(object obj)
        {
            Path other = (Path)obj;

            if (IsGlobal != other.IsGlobal) return false;

            return IsSamePath(other);
        }

        public override int GetHashCode()
        {
            string pathString = ToString();
            if (IsGlobal) pathString = "#" + pathString;
            return pathString.GetHashCode();
        }

        public static Path Concatenate(Path pathA, Path pathB)
        {
            Path concatenatedPath = new Path();
            foreach(PathElement pe in pathA.PathElements)
            {
                concatenatedPath.PathElements.AddLast(pe);
            }
            foreach (PathElement pe in pathB.PathElements)
            {
                concatenatedPath.PathElements.AddLast(pe);
            }
            return concatenatedPath;
        }

        public static implicit operator Path(string pathString)
        {
            return new Path(pathString);
        }
    }
}
