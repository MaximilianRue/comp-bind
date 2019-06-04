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
    /// <summary>
    /// Manages databindings and updating subscribers.
    /// </summary>
    /// <remarks>
    /// Main component in CombBind, inherit from this class and setup your bindings in
    /// the Start() callback. Note that if you override the Update() callback, you have to
    /// call the baseclass's Update(), too. Otherwise bindings will not be sent out.
    /// </remarks>
    public class Scope : MonoBehaviour, IDataNode
    {
        private Dictionary<string, IDataBinding> bindings = new Dictionary<string, IDataBinding>();

        private Dictionary<Path, HashSet<Path>> linkedPaths = new Dictionary<Path, HashSet<Path>>();

        private List<IValueUpdateReceiver> updateReceivers = new List<IValueUpdateReceiver>();

        private PathTree<Path> updatedPaths = new PathTree<Path>();

        /// <summary>
        /// Triggers notification of all subscribers whose paths were updated
        /// last frame.
        /// </summary>
        /// <remarks>
        /// Updates happen in a way so that only the minimal number of calls necessary are
        /// made. For this they are processed in batches at each frame.
        /// </remarks>
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

        /// <summary>
        /// Returns the databinding behind the given path. ArgumentException
        /// is thrown if the databinding could not be resolved.
        /// </summary>
        /// <param name="path">Path of requested databinding.</param>
        /// <returns>Databinding behind given path.</returns>
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
                throw new ArgumentException("Could not resolve path: '" + path + "'");
            }
        }

        /// <summary>
        /// Sets the value of the databinding specified by the given path.
        /// Needs the databinding to have a specified setter!
        /// </summary>
        /// <typeparam name="OutputType">Type of the value that is set.</typeparam>
        /// <param name="path">Requested databinding's path.</param>
        /// <param name="value">Value that should be set.</param>
        /// <returns>Databinding whose value has been set.</returns>
        public IDataBindingOutput<OutputType> SetValue<OutputType>(Path path, OutputType value)
        {
            IDataBindingOutput<OutputType> dcOutput = (IDataBindingOutput<OutputType>)GetBinding(path);
            dcOutput.SetValue(value);
            return dcOutput;
        }

        /// <summary>
        /// Creates a capture binding for the given path.
        /// </summary>
        /// <typeparam name="OutputType">Type that is returned by the created capture binding.</typeparam>
        /// <param name="fieldName">Name of the databinding that will be used for path creation.</param>
        /// <returns>Created binding's setup class.</returns>
        public CaptureBindingSetup<OutputType> Bind<OutputType>(string fieldName)
        {
            CaptureBinding<OutputType> newBinding = new CaptureBinding<OutputType>();
            PathElement pe = new PathElement(fieldName);
            newBinding.LocalPath = pe;
            AddBinding(newBinding);
            return new CaptureBindingSetup<OutputType>(newBinding);
        }

        /// <summary>
        /// Creates an object binding for the given path.
        /// </summary>
        /// <typeparam name="InputType">Type that is received by the created object binding.</typeparam>
        /// <typeparam name="OutputType">Type that is returned by the created object binding.</typeparam>
        /// <param name="inputObject">Object that is bound.</param>
        /// <param name="fieldName">Name of the databinding that will be used for path creation.</param>
        /// <returns>Created binding's setup class.</returns>
        public FieldBasedBindingSetup<InputType, OutputType> Bind<InputType, OutputType>(InputType inputObject, string fieldName)
        {
            FieldBasedBinding<InputType, OutputType> newBinding = new FieldBasedBinding<InputType, OutputType>(inputObject);
            PathElement pe = new PathElement(fieldName);
            newBinding.LocalPath = pe;
            AddBinding(newBinding);
            return new FieldBasedBindingSetup<InputType, OutputType>(newBinding);
        }

        /// <summary>
        /// Creates an object binding for the given path. Will automatically create default getter & setter.
        /// </summary>
        /// <remarks>
        /// This is shortcut for the fully type specified version.
        /// </remarks>
        /// <typeparam name="InputOutputType">Type of in- and output of the created object binding.</typeparam>
        /// <param name="inputObject">Object that is bound.</param>
        /// <param name="fieldName">Name of the databinding that will be used for path creation.</param>
        /// <returns>Created binding's setup class.</returns>
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

        /// <summary>
        /// Creates a list binding for the given path.
        /// </summary>
        /// <typeparam name="ListEntryType">Type of the entries of the bound list.</typeparam>
        /// <typeparam name="ListEntryOutputType">Type that the list binding should return for each input list entry.</typeparam>
        /// <param name="list">Bound list.</param>
        /// <param name="fieldName">Name of the databinding that will be used for path creation.</param>
        /// <returns>Created binding's setup class.</returns>
        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> BindList<ListEntryType, ListEntryOutputType>(List<ListEntryType> list, string fieldName)
        {
            ListBasedBinding<ListEntryType, ListEntryOutputType> listBinding = new ListBasedBinding<ListEntryType, ListEntryOutputType>(list);
            PathElement pe = new PathElement(fieldName);
            listBinding.LocalPath = pe;
            AddBinding(listBinding);
            return new ListBasedBindingSetup<ListEntryType, ListEntryOutputType>(listBinding);
        }

        /// <summary>
        /// Creates a list binding for the given path. Will automatically create default getter & setter for the entries.
        /// </summary>
        /// /// <remarks>
        /// This is shortcut for the fully type specified version. 
        /// </remarks>
        /// <typeparam name="ListEntryType">Type of the bound list's elements. Also type that will be returned for each list element.</typeparam>
        /// <param name="list">Bound list.</param>
        /// <param name="fieldName">Name of the databinding that will be used for path creation.</param>
        /// <returns>Created binding's setup class.</returns>
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
        /// Usually the setup is done using the Bind[...] methods offered by the scope class.
        /// </remarks>
        /// <param name="dataBinding">Databinding that should be added to the scope's management.</param>
        public void AddBinding(IDataBinding dataBinding)
        {
            bindings[dataBinding.LocalPath.FieldName] = dataBinding;
            dataBinding.ManagingScope = this;
        }

        /// <summary>
        /// Links two paths, so that the dependend one is updated, when the parent one is updated.
        /// </summary>
        /// <param name="parentPath">Parenting path. If it is updated, the other will be updated, too.</param>
        /// <param name="dependendPath">Dependend path. If the parent path is updated, it will receive an update, too.</param>
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

        #endregion

        /// <summary>
        /// Initializes scope. Will traverse the local gameobject tree, register & setup all component bindings and
        /// set the initial values.
        /// </summary>
        /// <param name="initialUpdate">If true (default), all bindings will be set initially.</param>
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

        /// <summary>
        /// Initializes this datanode.
        /// </summary>
        public void InitializeDataNode()
        {
            this.DefaultInitialize();
        }

        /// <summary>
        /// Registers update receivers at this datanode.
        /// </summary>
        /// <param name="newUpdateReceivers">List of update receivers that should be registered.</param>
        public void RegisterUpdateReceivers(List<IValueUpdateReceiver> newUpdateReceivers)
        {
            updateReceivers.AddRange(newUpdateReceivers);
        }

        /// <summary>
        /// Registers the change for a binding.
        /// </summary>
        /// <param name="dataBinding">Databinding whose value has been changed.</param>
        public void OnDataChanged(IDataBinding dataBinding)
        {
            Path absolutePath = dataBinding.GetAbsoluteBindingPath();
            BroadcastDataChange(absolutePath);
        }

        /// <summary>
        /// Schedules a data change broadcast for the next frame.
        /// </summary>
        /// <param name="dataPath">Path that should be updated.</param>
        public void BroadcastDataChange(Path dataPath)
        {
            updatedPaths.Set(dataPath, dataPath);
        }
    }
}
