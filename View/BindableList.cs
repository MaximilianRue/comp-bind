using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompBind.ViewModel;
using UnityEngine;

namespace CompBind.View
{
    public class BindableList : MonoBehaviour, IMetaDataNode
    {
        public string ListBindingPath;
        public GameObject ListEntryTemplate;
        public bool HideEntryOnStart = false;

        public Path BindingPath { get; set; }
        public IDataNode Context { get; set; }

        private Dictionary<int, ListBindingEntry> updateReceivers = new Dictionary<int, ListBindingEntry>();

        public void ForceUpdate()
        {
            foreach(ListBindingEntry entry in updateReceivers.Values)
            {
                entry.ForceUpdate();
            }
        }

        public IDataBinding GetBinding(Path path)
        {
            if (path.IsGlobal)
            {
                return Context.GetBinding(path);
            }
            else
            {
                Path absolutePath = Path.Concatenate(BindingPath, path);
                return Context.GetBinding(absolutePath);
            }
        }
        public IDataBinding GetBinding()
        {
            return Context.GetBinding(BindingPath);
        }

        public void InitializeDataNode()
        {
            if (HideEntryOnStart)
            {
                ListEntryTemplate.SetActive(false);
            }

            BindingPath = new Path(ListBindingPath);
            makeAllListEntries();
        }

        public void OnUpdate(Path updatedPath)
        {
            // Check if this is for the list
            if (BindingPath.IsSamePath(updatedPath))
            {
                // We might have to adjust the amount of children (list size expands / shrinks)
                IDataBindingOutputReadonly<IList> db = GetBinding() as IDataBindingOutputReadonly<IList>;
                IList boundList = db.GetValue();
                int oldCount = updateReceivers.Count;
                int newCount = boundList.Count;

                int countDifference = oldCount - newCount;
                if(countDifference > 0)
                {
                    // We have to remove some of the children
                    for(int i = 0; i < countDifference; i++)
                    {
                        int removeIndex = oldCount - 1 - i;
                        removeListEntryAt(removeIndex);
                    }
                }
                if(countDifference < 0)
                {
                    // We have to add some children
                    for(int i = 0; i < -countDifference; i++)
                    {
                        int addIndex = oldCount + i;
                        updateReceivers[addIndex] = makeListEntry(addIndex);
                    }
                }
                ForceUpdate();
            }
            else if (updatedPath.IsSubpathOf(BindingPath))
            {
                Path subPath = updatedPath.GetSlice(BindingPath.PathElements.Count);

                PathElement indexPathElement = subPath.PathElements.First();
                if(indexPathElement.Type != PathElement.PathElementType.Index)
                {
                    throw new ArgumentException("Error at path '" + updatedPath.ToString() + "'. List has to be accessed by index.");
                }

                int index = indexPathElement.Index;
                updateReceivers[index].OnUpdate(subPath);
            }
        }

        // Not needed, as all children are managed by the list itself dynamically
        public void RegisterUpdateReceivers(List<IValueUpdateReceiver> updateReceivers) { }

        private void makeAllListEntries()
        {
            IDataBindingOutputReadonly<IList> db = GetBinding() as IDataBindingOutputReadonly<IList>;
            IList boundList = db.GetValue();

            for (int i = 0; i < boundList.Count; i++)
            {
                updateReceivers[i] = makeListEntry(i);
            }
        }

        private ListBindingEntry makeListEntry(int index)
        {
            GameObject entry = Instantiate(ListEntryTemplate, transform);
            entry.SetActive(true);

            ListBindingEntry listBindingEntry = new ListBindingEntry();
            listBindingEntry.Context = this;
            listBindingEntry.BindingPath = new Path("[" + index + "]");
            listBindingEntry.DefaultInitialize(entry, true);
            listBindingEntry.EntryGameobject = entry;

            return listBindingEntry;
        }

        private void removeListEntryAt(int index)
        {
            GameObject go = updateReceivers[index].EntryGameobject;
            updateReceivers.Remove(index);
            go.transform.parent = null;
            Destroy(go);
        }
    }

    class ListBindingEntry : IMetaDataNode
    {
        public GameObject EntryGameobject { get; set; }

        public Path BindingPath { get; set; }
        public IDataNode Context { get; set; }

        public List<IValueUpdateReceiver> ValueUpdateReceivers = new List<IValueUpdateReceiver>();

        public IDataBinding GetBinding(Path path)
        {
            if (path.IsGlobal)
            {
                return Context.GetBinding(path);
            }
            else
            {
                Path absolutePath = Path.Concatenate(BindingPath, path);
                return Context.GetBinding(absolutePath);
            } 
        }

        public void ForceUpdate()
        {
            foreach(IValueUpdateReceiver vur in ValueUpdateReceivers)
            {
                vur.ForceUpdate();
            }
        }

        public void OnUpdate(Path updatedPath)
        {
            // We dont check if this actually is a valid update path, as
            // the parenting listBinding class does this for us

            // Remove index
            updatedPath.PathElements.RemoveFirst();
            // Pass to children
            foreach (IValueUpdateReceiver vur in ValueUpdateReceivers)
            {
                vur.OnUpdate(updatedPath);
            }
        }

        public void RegisterUpdateReceivers(List<IValueUpdateReceiver> updateReceivers)
        {
            ValueUpdateReceivers.AddRange(updateReceivers);
        }

        // Handled by ListBinding class
        public void InitializeDataNode() { }
    }
}
