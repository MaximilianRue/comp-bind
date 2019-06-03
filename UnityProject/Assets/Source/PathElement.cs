using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind
{
    public struct PathElement
    {
        public enum PathElementType
        {
            Member,
            Index
        }

        public PathElementType Type;
        public string FieldName;
        public int Index;

        public PathElement(string fieldName)
        {
            Type = PathElementType.Member;
            FieldName = fieldName;
            Index = -1;
        }
        public PathElement(int index)
        {
            Type = PathElementType.Index;
            FieldName = "";
            Index = index;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is PathElement)) return false;

            PathElement other = (PathElement)obj;

            if (Type != other.Type) return false;
            switch (Type)
            {
                case PathElementType.Index:
                    return Index == other.Index;
                case PathElementType.Member:
                    return FieldName == other.FieldName;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            switch (Type)
            {
                case PathElementType.Index:
                    hashCode =  Index;
                    break;
                case PathElementType.Member:
                    hashCode =  FieldName.GetHashCode();
                    break;
            }
            return hashCode;
        }

        public static bool operator ==(PathElement a, PathElement b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (((object)a == null) || ((object)b == null)) return false;
            return a.Equals(b);
        }
        public static bool operator !=(PathElement a, PathElement b)
        {
            return !(a == b);
        }
    }
}
