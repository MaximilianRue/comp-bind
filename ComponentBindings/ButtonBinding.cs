using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CompBind
{
    [RequireComponent(typeof(Button))]
    class ButtonBinding : BindableComponent
    {
        Button button;

        public override void InitializeBindableComponent()
        {
            base.InitializeBindableComponent();
            button = GetComponent<Button>();
        }

        // Binding selection below.
        // Function names will be displayed in the dropdown.

        UnityAction onClick;
        [BindingCallback]
        void OnClick(Action clickAction)
        {
            // Remove old listener
            if (onClick != null) button.onClick.RemoveListener(onClick);

            onClick = () =>
            {
                clickAction.Invoke();
            };
            button.onClick.AddListener(onClick);
        }
    }
}
