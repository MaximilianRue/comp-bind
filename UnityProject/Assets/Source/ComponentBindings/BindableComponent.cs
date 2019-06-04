using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CompBind.View;
using UnityEngine;

namespace CompBind
{
    /// <summary>
    /// Abstract base class for fast setting up bindings for single components.
    /// </summary>
    /// <remarks>
    /// Reflects defined functions in the derived classes and checks
    /// if the <see cref="BindingCallbackAttribute"/> for this method is set. 
    /// If this is the case, the method is added to the list of selectable
    /// callbacks for binding. The name is given by the <see cref="BindingCallbackAttribute"/>,
    /// or, if not set, reflected from the method name. See <see cref="ComponentBindingDrawer"/>
    /// for the editor script, which is used for the derived component bindings, too.
    /// </remarks>
    /// <example>
    /// A new, concrete binding for Unity's Image component can be defined in the following way:
    /// <code>
    /// 
    /// // Require the component this binding is used for. Will make sure it is 
    /// // attached to the gameobject and produce an error if deleted by accident.
    /// [RequireComponent(typeof(Image))]
    /// class ImageBinding : ComponentBinding
    /// {
    ///     Image image;
    ///     
    ///     // The Initialize phase of the Binding baseclass
    ///     // should be used to get the reference to the bound component.
    ///     public override void Initialize()
    ///     {
    ///         image = GetComponent<Image>();
    ///     }
    ///
    ///     // Binding selection below. Function names will be displayed 
    ///     // in the dropdown. Functions have to be of delegate type 
    ///     // ValueChangedDelegate.
    ///     [BindingCallback]
    ///     void SourceImage(object value)
    ///     {
    ///         image.sprite = (Sprite)value;
    ///     }
    ///     // Will be displayed in the dropdown as "ColorSetter"
    ///     [BindingCallback("ColorSetter")]
    ///     void Color(object value)
    ///     {
    ///         image.color = (Color)value;
    ///     }
    /// }
    /// 
    /// </code>
    /// This will generate two options in the binding editor menu: SourceImage, Color.
    /// </example>
    /// <example>
    /// This framework can also be used to bind to UnityEvents, like in the case
    /// of the Button component:
    /// <code>
    /// [RequireComponent(typeof(Button))]
    /// class ButtonBinding : ComponentBinding
    /// {
    ///     Button button;
    /// 
    ///     public override void Initialize()
    ///     {
    ///         button = GetComponent<Button>();
    ///     }
    /// 
    ///     // Locally store the current callback, so it can be deregistered, if the
    ///     // bound value is changed.
    ///     UnityAction onClick;
    ///     [BindingCallback]
    ///     void OnClick(object value)
    ///     {
    ///         // Remove old listener, using the locally saved one.
    ///         if (onClick != null) button.onClick.RemoveListener(onClick);
    ///         
    ///         // Create a new callback, that invokes the property wrapped
    ///         // BoundAction delegate of the scope.
    ///         onClick = () =>
    ///         {
    ///             (value as BoundAction).Invoke();
    ///         };
    ///         // Add the new listener back to be button.
    ///         button.onClick.AddListener(onClick);
    ///     }
    /// }
    /// </code>
    /// This will generate one option in the binding editor menu: OnClick.
    /// </example>
    
    public abstract class BindableComponent : MonoBehaviour, IBindableComponent
    {
        [SerializeField]
        List<ActiveBinding> selectedBindings = new List<ActiveBinding>();

        [Serializable]
        class ActiveBinding
        {
            public string BindingPath = "-";
            public string CallbackName = "";
        }

        Dictionary<string, IValueUpdateReceiver> updateReceivers;
        IDataNode dataNode;

        /// <summary>
        /// Reflects methods on the gameobject and adds them to the
        /// list of options to select from in the editor.
        /// </summary>
        /// <returns>
        /// Dictionary of reflected names and the <see cref="ValueChangedDelegate"/>
        /// callback that belongs to it.
        /// </returns>
        public Dictionary<string, IValueUpdateReceiver> GetCallbacks()
        {
            Dictionary<string, IValueUpdateReceiver> availableBindings = new Dictionary<string, IValueUpdateReceiver>();

            MethodInfo[] methods = this.GetType().GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly
            );

            foreach(MethodInfo method in methods)
            {
                // Only use those marked as a callback attribute
                BindingCallbackAttribute bca = method.GetCustomAttribute<BindingCallbackAttribute>();
                if (bca == null) continue;

                string entryName = bca.CallbackName;
                if (string.IsNullOrEmpty(entryName))
                {
                    entryName = method.Name;
                }

                // Retrieve type of first parameter
                ParameterInfo[] parameterInfos = method.GetParameters();
                if(parameterInfos.Length != 1)
                {
                    throw new Exception("Specified BindingCallback '" + method.Name + "' has wrong count of parameters.");
                }
                Type parameterType = parameterInfos[0].ParameterType;
                Type actiontype = typeof(Action<>);
                Type bindingType = typeof(Bindable<>).MakeGenericType(parameterType);
                actiontype = actiontype.MakeGenericType(parameterType);

                var bindable = Activator.CreateInstance(bindingType);
                var cb = Delegate.CreateDelegate(
                    actiontype,
                    this,
                    method.Name,
                    false,
                    false
                );
                PropertyInfo updateAction = bindingType.GetProperty(
                    "UpdateAction",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly
                );
                updateAction.SetValue(bindable, cb);

                if(cb != null)
                {
                    availableBindings.Add(entryName, (IValueUpdateReceiver)bindable);
                }
            }
            return availableBindings;
        }

        public virtual void InitializeBindableComponent()
        {
            updateReceivers = new Dictionary<string, IValueUpdateReceiver>();

            var callbacks = GetCallbacks();
            foreach (ActiveBinding binding in selectedBindings)
            {
                IValueUpdateReceiver vur = callbacks[binding.CallbackName];
                vur.BindingPath = new Path(binding.BindingPath);
                updateReceivers[binding.CallbackName] = vur;
            }
        }

        public List<IValueUpdateReceiver> GetValueUpdateReceivers()
        {
            return updateReceivers.Values.ToList();
        }

        /// <summary>
        /// Returns update receiver that will result from this
        /// callback at runtime.
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public IValueUpdateReceiver GetUpdateReceiver([CallerMemberName] string callbackName = "")
        {
            return updateReceivers[callbackName];
        }
    }

    /// <summary>
    /// Marks a function in a ComponentBinding class as
    /// a binding callback.
    /// </summary>
    /// <remarks>
    /// Only binding callbacks are displayed in the editor list
    /// to select from.
    /// </remarks>
    public class BindingCallbackAttribute : Attribute
    {
        // If not set, the reflected fieldname will be used.
        public string CallbackName;

        /// <summary>
        /// Creates a BindingCallback instance.
        /// </summary>
        /// <param name="callbackName">
        /// Name that should be displayed for the callback. If not
        /// set, the reflected name of the associated field will
        /// be used.
        /// </param>
        public BindingCallbackAttribute(string callbackName = "")
        {
            CallbackName = callbackName;
        }
    }
}
