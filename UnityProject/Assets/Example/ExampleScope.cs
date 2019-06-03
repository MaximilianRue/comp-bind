using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CompBind;

public class ExampleScope : Scope
{
    // Simple class to demonstrate the binding
    public class ExampleStructuredClass
    {
        public string Message;
        public int Number;
    }

    void Start()
    {
        // Create some instances whose values we want to bind to components
        ExampleStructuredClass example = new ExampleStructuredClass
        {
            Message = "Hello World!",
            Number = 0
        };

        // Also create a list of values to demostrate the list binding
        List<string> exampleList = new List<string> { "First", "Second", "Third" };



        // --- Object Binding --------------------------------------------------------------------------
        // The binding acts upon an object instance - and its members.

        // Bind the example instance to the path 'ExampleInstance'
        var objectBinding = Bind(example, "ExampleInstance");

        // Also bind the member "Message", and define a getter, so it can be accessed
        // To allow setting the value (two way binding), a setter can be specified, too
        objectBinding.BindMember<string>("Message")
            .SetGetter((instance) => instance.Message)
            .SetSetter((ref ExampleStructuredClass instance, string value) => instance.Message = value);

        // Proceed equally for the "Number" member
        objectBinding.BindMember<string>("CoolNumber")  // Note that the path can be chosen freely
            .SetGetter((instance) => instance.Number.ToString());  // No setter -> readonly

        // Members are now available at the combined paths...
        // * ExampleInstance.Message
        // * ExampleInstance.CoolNumber



        // --- List Binding ----------------------------------------------------------------------------
        // The binding acts upon a list instance.

        var listBinding = BindList(exampleList, "ExampleList");

        // List is now available at path "ExampleList"
        // The entries are available at paths...
        // * ExampleList.[0]
        // * ExampleList.[1]
        // * ExampleList.[2]
        // * ...



        // --- Capture Binding -------------------------------------------------------------------------
        // Sometimes one just needs to expose a value to the UI, that is not directly reflected by
        // a field / variable. In this case a capture binding offers greatest flexibility.

        var captureBinding = Bind<string>("ExampleCapture")
            .SetGetter(() => "Captured: '" + exampleList[0] + "' and '" + exampleList[1] + "' in one " +
                "simple string!");

        // Note that the returned value does not directly reflect the value of one variable in the
        // scope.
        // Defining an explicit getter is mandatory for caputure bindings.

        // The concatenated string will be available at "ExampleCapture"



        // --- Initialization --------------------------------------------------------------------------
        // After all bindings were set up, you have to tell the scope that it can initialize the
        // bound components.

        InitializeScope();
    }
}
