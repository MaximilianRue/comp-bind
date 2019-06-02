using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CompBind.ComponentBindings
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    class TextMeshProBinding : BindableComponent
    {
        TextMeshProUGUI tmpro;

        public override void InitializeBindableComponent()
        {
            base.InitializeBindableComponent();
            tmpro = GetComponent<TextMeshProUGUI>();
        }

        // Binding selection below.
        // Function names will be displayed in the dropdown.

        [BindingCallback]
        void Text(string value)
        {
            tmpro.text = value;
        }
    }
}
