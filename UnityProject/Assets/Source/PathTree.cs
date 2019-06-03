using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind
{
    /// <summary>
    /// Tree datastructure based on paths and their elements.
    /// </summary>
    /// <typeparam name="NodeValueType"></typeparam>
    public class PathTree<NodeValueType>
    {
        private PathTreeNodeInternal root = new PathTreeNodeInternal();

        internal class PathTreeNodeInternal
        {
            public bool Resolvable = false;
            public NodeValueType Value;
            public PathTreeNodeInternal Parent;
            public PathElement LocalPath;
            public Dictionary<PathElement, PathTreeNodeInternal> Children = new Dictionary<PathElement, PathTreeNodeInternal>();

            public PathTreeNodeInternal Step(PathElement pe)
            {
                if (Children.ContainsKey(pe))
                {
                    return Children[pe];
                }
                else return null;
            }

            public bool AddChild(PathElement pe, out PathTreeNodeInternal child)
            {
                child = Step(pe);
                if (child == null)
                {
                    child = new PathTreeNodeInternal();
                    child.Parent = this;
                    child.LocalPath = pe;
                    Children.Add(pe, child);
                    return true;
                }
                return false;
            }

            public PathTreeNodeInternal FindNextValueParent()
            {
                return FindNextValueParent(out PathElement pe);
            }

            public PathTreeNodeInternal FindNextValueParent(out PathElement childPath)
            {
                childPath = LocalPath;
                PathTreeNodeInternal current = Parent;
                while (current.Resolvable == false && current.Parent != null)
                {
                    childPath = current.LocalPath;
                    current = current.Parent;
                }
                if (current.Resolvable) return current;
                else return null;
            }

            public void FindNextValueChildren(ref List<PathTreeNodeInternal> valueChildren)
            {
                foreach(PathTreeNodeInternal child in Children.Values)
                {
                    if (child.Resolvable)
                    {
                        valueChildren.Add(child);
                    }
                    else
                    {
                        child.FindNextValueChildren(ref valueChildren);
                    }
                }
            }

            public void GetAllSubNodes(ref List<PathTreeNodeInternal> subnodes)
            {
                foreach (PathTreeNodeInternal child in Children.Values)
                {
                    subnodes.Add(child);
                    child.GetAllSubNodes(ref subnodes);
                }
            }

            public void GetAbsolutePath(ref Path path)
            {
                path.PathElements.AddFirst(LocalPath);
                if (Parent != null)
                {
                    Parent.GetAbsolutePath(ref path);
                }
            }
        }

        public class PathTreeNode
        {
            private PathTreeNodeInternal capsuledNode;

            internal PathTreeNode(PathTreeNodeInternal capsuledNode)
            {
                this.capsuledNode = capsuledNode;
            }

            public bool Resolvable
            {
                get { return capsuledNode.Resolvable; }
            }
            public NodeValueType Value
            {
                get
                {
                    if (capsuledNode.Resolvable)
                    {
                        return capsuledNode.Value;
                    }
                    else throw new InvalidOperationException("Value could not be retrieved.");
                }
                set
                {
                    capsuledNode.Value = value;
                    capsuledNode.Resolvable = true;
                }
            }

            public PathTreeNode FindNextValueParent()
            {
                return new PathTreeNode(capsuledNode.FindNextValueParent());
            }

            public List<PathTreeNode> FindNextValueChildren()
            {
                List<PathTreeNodeInternal> treeNodes = new List<PathTreeNodeInternal>();
                capsuledNode.FindNextValueChildren(ref treeNodes);
                return treeNodes.Select((node) => new PathTreeNode(node)).ToList();
            }

            public List<PathTreeNode> GetAllSubNodes()
            {
                List<PathTreeNodeInternal> treeNodes = new List<PathTreeNodeInternal>();
                capsuledNode.GetAllSubNodes(ref treeNodes);
                return treeNodes.Select((node) => new PathTreeNode(node)).ToList();
            }

            public Path GetAbsolutePath()
            {
                Path path = new Path();
                capsuledNode.GetAbsolutePath(ref path);
                return path;
            }
        }

        public void Set(Path path, NodeValueType value)
        {
            PathTreeNodeInternal current = root;
            foreach(PathElement pe in path.PathElements)
            {
                current.AddChild(pe, out current);
            }
            current.Value = value;
            current.Resolvable = true;
        }

        public void Remove(Path path)
        {
            PathTreeNodeInternal node = getNode(path);
            if(node != null)
            {
                PathElement childPath;
                PathTreeNodeInternal valueParent = node.FindNextValueParent(out childPath);
                if(valueParent != null)
                {
                    valueParent.Children.Remove(childPath);
                }
            }
        }

        public void Clear()
        {
            root = new PathTreeNodeInternal();
        }

        public PathTreeNode GetNode(Path path)
        {
            return new PathTreeNode(getNode(path));
        }
        private PathTreeNodeInternal getNode(Path path)
        {
            PathTreeNodeInternal current = root;
            foreach (PathElement pe in path.PathElements)
            {
                current = current.Step(pe);
                if (current == null) return null;
            }
            return current;
        }

        public List<PathTreeNode> GetAllNodes()
        {
            return getAllNodes().Select((node) => new PathTreeNode(node)).ToList();
        }
        private List<PathTreeNodeInternal> getAllNodes()
        {
            List<PathTreeNodeInternal> allNodes = new List<PathTreeNodeInternal>();
            root.GetAllSubNodes(ref allNodes);
            return allNodes;
        }

        public PathTreeNode Root
        {
            get { return new PathTreeNode(root); }
        }

        public bool Empty
        {
            get { return root.Children.Values.Count == 0; }
        }
    }
}
