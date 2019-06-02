using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompBind.View;
using CompBind.ViewModel;
using UnityEngine;

namespace CompBind
{
    public class Scope : MonoBehaviour, IDataNode
    {
        private Dictionary<string, IDataBinding> bindings = new Dictionary<string, IDataBinding>();

        private Dictionary<Path, HashSet<Path>> linkedPaths = new Dictionary<Path, HashSet<Path>>();

        private List<IValueUpdateReceiver> updateReceivers = new List<IValueUpdateReceiver>();

        private PathTree<Path> updatedPaths = new PathTree<Path>();

        private void Update()
        {
            if (!updatedPaths.Empty)
            {
                var rootPaths = updatedPaths.Root.FindNextValueChildren().Select((node) => node.Value);
                foreach (Path path in rootPaths)
                {
                    HashSet<Path> set;
                    if (linkedPaths.TryGetValue(path, out set))
                    {
                        foreach (Path linkedPath in set)
                        {
                            updatedPaths.Set(linkedPath, linkedPath);
                        }
                    }
                }
                rootPaths = updatedPaths.Root.FindNextValueChildren().Select((node) => node.Value);
                foreach (Path path in rootPaths)
                {
                    // Notify all registered update receivers (UI)
                    foreach (IValueUpdateReceiver vur in updateReceivers)
                    {
                        vur.OnUpdate(path);
                    }
                }
                updatedPaths.Clear();
            }
        }

        #region Binding Helpers

        public IDataBinding GetBinding(Path path)
        {
            Path workingCopy = new Path(path);
            try
            {
                PathElement firstPe = workingCopy.PathElements.ElementAt(0);
                workingCopy.PathElements.RemoveFirst();
                IDataBinding root = bindings[firstPe.FieldName];
                return root.ResolvePath(workingCopy);
            }
            catch (Exception)
            {
                throw new Exception("Could not resolve path: '" + path + "'");
            }
        }
        public IDataBinding GetBinding(string pathString)
        {
            return GetBinding(new Path(pathString));
        }

        public IDataBindingOutput<OutputType> SetValue<OutputType>(Path path, OutputType value)
        {
            IDataBindingOutput<OutputType> dcOutput = (IDataBindingOutput<OutputType>)GetBinding(path);
            dcOutput.SetValue(value);
            return dcOutput;
        }
        public IDataBindingOutput<OutputType> SetValue<OutputType>(string pathString, OutputType value)
        {
            return SetValue(new Path(pathString), value);
        }

        public CaptureBindingSetup<OutputType> Bind<OutputType>(string fieldName)
        {
            CaptureBinding<OutputType> newBinding = new CaptureBinding<OutputType>();
            PathElement pe = new PathElement(fieldName);
            newBinding.LocalPath = pe;
            AddBinding(newBinding);
            return new CaptureBindingSetup<OutputType>(newBinding);
        }
        public FieldBasedBindingSetup<InputType, OutputType> Bind<InputType, OutputType>(InputType inputObject, string fieldName)
        {
            FieldBasedBinding<InputType, OutputType> newBinding = new FieldBasedBinding<InputType, OutputType>(inputObject);
            PathElement pe = new PathElement(fieldName);
            newBinding.LocalPath = pe;
            AddBinding(newBinding);
            return new FieldBasedBindingSetup<InputType, OutputType>(newBinding);
        }
        public FieldBasedBindingSetup<InputOutputType, InputOutputType> Bind<InputOutputType>(InputOutputType inputObject, string fieldName)
        {
            FieldBasedBinding<InputOutputType, InputOutputType> newBinding = new FieldBasedBinding<InputOutputType, InputOutputType>(inputObject);

            // Automatically create setter / getter
            newBinding.SetGetter((val) => val);
            newBinding.SetSetter((ref InputOutputType val, InputOutputType newVal) => val = newVal);

            PathElement pe = new PathElement(fieldName);
            newBinding.LocalPath = pe;
            AddBinding(newBinding);
            return new FieldBasedBindingSetup<InputOutputType, InputOutputType>(newBinding);
        }

        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> BindList<ListEntryType, ListEntryOutputType>(List<ListEntryType> inputObject, string fieldName)
        {
            ListBasedBinding<ListEntryType, ListEntryOutputType> listBinding = new ListBasedBinding<ListEntryType, ListEntryOutputType>(inputObject);
            PathElement pe = new PathElement(fieldName);
            listBinding.LocalPath = pe;
            AddBinding(listBinding);
            return new ListBasedBindingSetup<ListEntryType, ListEntryOutputType>(listBinding);
        }
        public ListBasedBindingSetup<ListEntryType, ListEntryType> BindList<ListEntryType>(List<ListEntryType> list, string fieldName)
        {
            ListBasedBinding<ListEntryType, ListEntryType> listBinding = new ListBasedBinding<ListEntryType, ListEntryType>(list);

            // Automatically create entry getter & setter
            listBinding.SetEntryGetter((entry) => entry);
            listBinding.SetEntrySetter((entry, newValue) => entry = newValue);

            PathElement pe = new PathElement(fieldName);
            listBinding.LocalPath = pe;
            AddBinding(listBinding);
            return new ListBasedBindingSetup<ListEntryType, ListEntryType>(listBinding);
        }
        
        /// <summary>
        /// Directly adds a databinding to the scope.
        /// </summary>
        /// <remarks>
        /// Usually the setup is done using the Bind[...] methods offered.
        /// </remarks>
        /// <param name="dataBinding"></param>
        public void AddBinding(IDataBinding dataBinding)
        {
            bindings[dataBinding.LocalPath.FieldName] = dataBinding;
            dataBinding.ManagingScope = this;
        }

        /// <summary>
        /// Links two paths, so that the dependend one is updated, when the parent one is updated.
        /// </summary>
        /// <param name="parentPath"></param>
        /// <param name="dependendPath"></param>
        public void LinkPaths(Path parentPath, Path dependendPath)
        {
            HashSet<Path> set;
            if(!linkedPaths.TryGetValue(parentPath, out set))
            {
                set = new HashSet<Path>();
                linkedPaths.Add(parentPath, set);
            }
            set.Add(dependendPath);
        }
        public void LinkPaths(string parentPath, string dependendPath)
        {
            LinkPaths(new Path(parentPath), new Path(dependendPath));
        }

        #endregion

        public void InitializeScope(bool initialUpdate = true)
        {
            // TODO: Initial assert run to make sure all bindings have at least
            // reading ability

            InitializeDataNode();

            if (!initialUpdate) return;

            // Initialize all bindings to their initial value
            foreach (IValueUpdateReceiver vur in updateReceivers)
            {
                vur.ForceUpdate();
            }
        }

        public void InitializeDataNode()
        {
            this.DefaultInitialize();
        }

        public void RegisterUpdateReceivers(List<IValueUpdateReceiver> newUpdateReceivers)
        {
            updateReceivers.AddRange(newUpdateReceivers);
        }

        public void OnDataChanged(IDataBinding dataBinding)
        {
            Path absolutePath = dataBinding.GetAbsoluteBindingPath();
            BroadcastDataChange(absolutePath);
        }
        public void BroadcastDataChange(Path dataPath)
        {
            updatedPaths.Set(dataPath, dataPath);
        }
        public void BroadcastDataChange(string pathString)
        {
            BroadcastDataChange(new Path(pathString));
        }
    }
}
