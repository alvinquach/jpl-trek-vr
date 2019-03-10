using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public abstract class UnityBrowserFunctionSet {

        protected readonly ICollection<Type> SupportedTypes = new HashSet<Type> {
            typeof(string),
            typeof(JSONNode),
            //typeof(int),
            //typeof(float),
            typeof(double),
            typeof(bool),
            // TODO Add array type
        };

        [AttributeUsage(AttributeTargets.Method, Inherited = true)]
        protected class RegisterToBrowser : Attribute {
            public bool acceptJsonNodeArgs = false;
        }

        protected abstract string FunctionsReadyVariable { get; }

        protected readonly Browser _browser;

        public UnityBrowserFunctionSet(Browser browser) {
            _browser = browser;
        }

        public virtual void RegisterFunctions() {

            foreach (MethodInfo method in GetType().GetMethods()) {

                RegisterToBrowser attribute = method.GetCustomAttribute<RegisterToBrowser>();

                if (attribute == null) {
                    continue;
                }

                string functionName = StringUtils.FirstCharacterToLower(method.Name);

                ParameterInfo[] parameters = method.GetParameters();

                if (attribute.acceptJsonNodeArgs) {

                    // If the method accepts JSONNode argument, then it
                    // should have exactly one parameter of type JSONNode.
                    if (parameters.Length != 1 || parameters[0].ParameterType != typeof(JSONNode)) {
                        Debug.LogError($"{method.Name} should only accept one parameter of type JSONNode.");
                        continue;
                    }

                    ZFBrowserUtils.RegisterFunction(_browser, functionName, args => {
                        method.Invoke(this, new object[] { args });
                    });

                }
                else {

                    int maxParamsCount = parameters.Length;
                    int requiredParamsCount = 0;

                    // Check if all the parameter types are supported.
                    // Also update how many params are required.
                    bool hasInvalidTypes = false;
                    foreach (ParameterInfo param in parameters) {
                        if (!SupportedTypes.Contains(param.ParameterType)) {
                            Debug.LogError($"Parameter {param.Name} is of type {param.ParameterType}, which is not supported.");
                            hasInvalidTypes = true;
                            break;
                        }
                        requiredParamsCount += param.IsOptional ? 0 : 1;
                    }

                    if (hasInvalidTypes) {
                        continue;
                    }

                    ZFBrowserUtils.RegisterFunction(_browser, functionName, args => {

                        // Check if there are enough values in the argument
                        if (args.Count < requiredParamsCount) {
                            throw new Exception();
                        }

                        // Build parameters array.
                        int paramsCount = Math.Min(maxParamsCount, args.Count);
                        object[] paramsArray = new object[paramsCount];
                        for (int i = 0; i < paramsCount; i++) {
                            paramsArray[i] = args[i].Type == JSONNode.NodeType.Object
                                ? args[i]           // Pass objects as JSON nodes
                                : args[i].Value;    // Pass other types as the actual value
                        }

                        method.Invoke(this, paramsArray);
                    });

                }

            }

            if (!string.IsNullOrEmpty(FunctionsReadyVariable)) {
                _browser.EvalJS($"{UnityGlobalObjectPath}.{FunctionsReadyVariable} = true;");
            }

        }

        private T CastToType<T>(object obj, Type type) {
            if (obj is T) {
                return (T)obj;
            }
            return default;
        }

    }

}
