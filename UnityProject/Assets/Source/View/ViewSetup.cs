using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CompBind.View
{
    /// <summary>
    /// Helper (extension) methods that are used when component bindings are initialized.
    /// </summary>
    public static class ViewSetup
    {
        /// <summary>
        /// Default initialization of datanode. Will trigger initialization routines of
        /// all encountered DataNodes in children. Will register all direct ValueUpdateReceivers.
        /// </summary>
        /// <param name="dataNode"></param>
        /// <param name="rootObject">Where the gameobject tree search should originate from.</param>
        /// <param name="searchRootItself">If true, also considers the root gameobject for the search.</param>
        public static void DefaultInitialize(this IDataNode dataNode, GameObject rootObject, bool searchRootItself = false)
        {
            List<IBindableComponent> bindableComponents = new List<IBindableComponent>();
            List<IDataNode> dataNodes = new List<IDataNode>();

            SearchViewInterfaces(rootObject, ref bindableComponents, ref dataNodes, searchRootItself);

            List<IValueUpdateReceiver> valueUpdateReceivers = new List<IValueUpdateReceiver>();
            foreach (IBindableComponent bc in bindableComponents)
            {
                bc.InitializeBindableComponent();
                valueUpdateReceivers.AddRange(bc.GetValueUpdateReceivers());
            }
            foreach(IDataNode dn in dataNodes)
            {
                valueUpdateReceivers.Add(dn as IValueUpdateReceiver);
            }
            foreach (IValueUpdateReceiver vur in valueUpdateReceivers)
            {
                vur.Context = dataNode;
            }
            foreach (IDataNode dn in dataNodes)
            {
                dn.InitializeDataNode();
            }
            dataNode.RegisterUpdateReceivers(valueUpdateReceivers);
        }

        /// <summary>
        /// Default initialization of datanode. Will trigger initialization routines of
        /// all encountered DataNodes in children. Will register all direct ValueUpdateReceivers.
        /// </summary>
        /// <param name="dataNode"></param>
        public static void DefaultInitialize(this IDataNode dataNode)
        {
            dataNode.DefaultInitialize((dataNode as MonoBehaviour).gameObject);
        }

        /// <summary>
        /// Starting from a given gameobject, will traverse the hierachy and search for direct
        /// BindableComponents and DataNodes in children.
        /// </summary>
        /// <param name="root">Where the gameobject tree search should originate from.</param>
        /// <param name="foundBindableComponents">Found bindable components will be added here.</param>
        /// <param name="foundDataNodes">Found DataNodes will be added here.</param>
        /// <param name="searchRoot">If true, also considers the root gameobject for the search.</param>
        public static void SearchViewInterfaces(
            GameObject root,
            ref List<IBindableComponent> foundBindableComponents,
            ref List<IDataNode> foundDataNodes,
            bool searchRoot = false
        )
        {
            // Note: As there is only one dataNode per gameobject,
            // we only need to search for IBindableComponent
            if (searchRoot)
            {
                IBindableComponent[] ibcs = root.GetComponents<IBindableComponent>();
                foundBindableComponents.AddRange(ibcs);
            }

            foreach (Transform child in root.transform)
            {
                IBindableComponent[] bindableComponents = child.GetComponents<IBindableComponent>()
                    .Where((bindableComp) => (bindableComp as Component).gameObject.activeInHierarchy)
                    .ToArray();

                foundBindableComponents.AddRange(bindableComponents);

                // If child itself is a datanode, dont traverse further into it.
                // Instead, run default initialization for the childDataNode itself.
                IDataNode childDataNode = child.GetComponent<IDataNode>();
                if (childDataNode == null)
                {
                    SearchViewInterfaces(child.gameObject, ref foundBindableComponents, ref foundDataNodes);
                }
                else if((childDataNode as Component).gameObject.activeInHierarchy)
                {
                    foundDataNodes.Add(childDataNode);
                }
            }
        }
    }
}
