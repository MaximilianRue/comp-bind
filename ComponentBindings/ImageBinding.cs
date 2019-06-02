//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UI;

//namespace CompBind.ComponentBindings
//{
//    [RequireComponent(typeof(Image))]
//    class ImageBinding : ComponentBinding
//    {
//        Image image;

//        public override void InitializeBindingNode()
//        {
//            base.InitializeBindingNode();
//            image = GetComponent<Image>();
//        }

//        // Binding selection below.
//        // Function names will be displayed in the dropdown.

//        [BindingCallback]
//        void SourceImage(object value)
//        {
//            image.sprite = (Sprite)value;
//        }
//        [BindingCallback]
//        void Color(object value)
//        {
//            image.color = (Color)value;
//        }
//    }
//}
