using Forge.Entities;
using Forge.Utilities;
using FullInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Forge.Editing {
    /// <summary>
    /// Do *NOT* extend this interface. It is used internally to discover types which should be used
    /// as DataInspectors. Assumptions are made about the types which extend this that will not be
    /// guaranteed if custom code implements this type.
    /// </summary>
    /// TODO: Eventually this will be converted to annotation discovery, similar to Full Inspector
    public interface IDataInspector {
        /// <summary>
        /// Display a GUI that can edit the given Data instance. Modifications are made directly to
        /// the instance.
        /// </summary>
        /// <param name="data">The data instance that will be modified</param>
        /// <param name="context">The GameObject that contains this Data instance</param>
        void Edit(Data.IData data, GameObject context);

        /// <summary>
        /// Render something on the scene.
        /// </summary>
        /// <param name="data">The data instance that is being modified</param>
        /// <param name="context">The GameObject that contains the Data instance</param>
        void OnSceneGUI(Data.IData data, GameObject context);
    }

    /// <summary>
    /// A DataInspector is used to provide a completely customized editing experience for a
    /// Data.IData instance. It is analogous to having a custom Editor for a Unity component.
    /// </summary>
    /// <remarks>
    /// Class that all DataInspectors must extend from. The generic argument is the type that the
    /// inspector edits. This class is automatically discovered and injected in the data editing
    /// process. No explicit registration of the type is necessary.
    /// </remarks>
    /// <typeparam name="DataType">The data type that the inspector will be used on</typeparam>
    public abstract class DataInspector<DataType> : IDataInspector where DataType : Data.IData {
        // Use the magic of explicit interfaces to provide an Edit function that expects the
        // required DataType when our local type knows that the IDataInspector is an instance of a
        // particular type

        /// <summary>
        /// Provide a custom inspector interface for the given Data instance.
        /// </summary>
        /// <param name="data">The data instance that is being edited.</param>
        /// <param name="context">The GameObject that contains the Data instance.</param>
        protected abstract void Edit(DataType data, GameObject context);

        /// <summary>
        /// Called when a new Data instance is being edited. This is useful for pulling in initial
        /// values into the inspector from the Data instance.
        /// </summary>
        /// <remarks>
        /// This method is optional to implement. The base method does nothing.
        /// </remarks>
        /// <param name="data">The data that will be edited.</param>
        /// <param name="context">The GameObject that contains the Data instance</param>
        protected virtual void Prepare(DataType data, GameObject context) {
        }

        /// <summary>
        /// The data type that Edit was previously called on. We cached this so that we can call
        /// Prepare intelligently.
        /// </summary>
        private DataType _previouslyEdited;

        /// <summary>
        /// Ensures that the inspector has been prepared for editing the given data instance.
        /// </summary>
        /// <param name="data"></param>
        private void EnsurePrepared(Data.IData data, GameObject context) {
            if (ReferenceEquals(_previouslyEdited, data) == false) {
                Prepare((DataType)data, context);

                _previouslyEdited = (DataType)data;
            }
        }

        void IDataInspector.Edit(Data.IData data, GameObject context) {
            EnsurePrepared(data, context);
            Edit((DataType)data, context);
        }

        /// <summary>
        /// Optional method for scene rendering based of the given data instance.
        /// </summary>
        public virtual void OnSceneGUI(DataType data, GameObject context) {
        }

        void IDataInspector.OnSceneGUI(Data.IData data, GameObject context) {
            EnsurePrepared(data, context);
            OnSceneGUI((DataType)data, context);
        }
    }

    /// <summary>
    /// Manages the discovery of DataInspector class instances.
    /// </summary>
    public sealed class DataInspector {
        private static Lazy<Dictionary<Type, IDataInspector>> _dataInspectors =
            new Lazy<Dictionary<Type, IDataInspector>>(() => {
                var dataInspectors = new Dictionary<Type, IDataInspector>();

                // get types that extend IDataInspector
                var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                            from type in assembly.GetTypes()
                            where typeof(IDataInspector).IsAssignableFrom(type)
                            where type.IsAbstract == false
                            where type.IsInterface == false
                            where type != typeof(GenericDataInspector)
                            select type;

                // populate the inspector dictionary with said types
                foreach (var type in types) {
                    // get the type that this IDataInspector edits
                    Type[] genericArguments = type.BaseType.GetGenericArguments();
                    Contract.Requires(genericArguments.Length == 1);
                    Type editedType = genericArguments[0];

                    // If there is already an inspector, log an error
                    if (dataInspectors.ContainsKey(editedType)) {
                        Debug.LogError("There are conflicting DataInspectors registered for type "
                            + type + ": " + dataInspectors[editedType].GetType().FullName +
                            " and " + type.FullName);
                    }

                    // store the reference to the inspector
                    dataInspectors[editedType] = (IDataInspector)Activator.CreateInstance(type);
                }

                return dataInspectors;
            });

        /// <summary>
        /// The default data inspector that is used when there is not a custom one registered for
        /// the requested data type.
        /// </summary>
        /// <remarks>
        /// The current default uses reflection to determine how to draw the inspector.
        /// </remarks>
        private static Lazy<IDataInspector> _defaultDataInspector = new Lazy<IDataInspector>(() => {
            return new GenericDataInspector();
        });

        /// <summary>
        /// Returns a DataInspector that will allow the user to edit the given data type.
        /// </summary>
        /// <param name="dataType">The type of data that is being edited.</param>
        public static IDataInspector Get(Type dataType) {
            if (_dataInspectors.Value.ContainsKey(dataType) == false) {
                return _defaultDataInspector.Value;
            }

            return _dataInspectors.Value[dataType];
        }

        /// <summary>
        /// Returns a DataInspector that will allow the user to edit the given data type.
        /// </summary>
        /// <param name="dataType">The type of data that is being edited.</param>
        public static DataInspector<TData> Get<TData>() where TData : Data.IData {
            return (DataInspector<TData>)Get(typeof(TData));
        }
    }

    /// <summary>
    /// The generic data inspector that uses reflection to provide an editor interface for the data
    /// element. Fields inside the data instance are rendered using instances of IDataEditors, which
    /// are automatically loaded (which are found by searching all loaded assemblies).
    /// </summary>
    public class GenericDataInspector : IDataInspector {
        void IDataInspector.Edit(Data.IData data, GameObject context) {
            TypeMetadata metadata = TypeCache.FindTypeMetadata(data.GetType());

            foreach (var property in metadata.Properties) {
                EditProperty(data, property);
            }
        }

        /// <summary>
        /// Helper method to edit the given field on the specified data instance.
        /// </summary>
        /// <param name="data">The data instance to edit.</param>
        /// <param name="property">The property on the instance to modify.</param>
        private void EditProperty(Data.IData data, PropertyMetadata property) {
            TooltipAttribute tooltip = (TooltipAttribute)Attribute.GetCustomAttribute(property.MemberInfo, typeof(TooltipAttribute));
            CommentAttribute comment = (CommentAttribute)Attribute.GetCustomAttribute(property.MemberInfo, typeof(CommentAttribute));

            GUIContent label = new GUIContent(property.Name, tooltip != null ? tooltip.Tooltip : "");

            if (comment != null) {
                EditorGUILayout.HelpBox(comment.Comment, MessageType.Info);
            }

            // edit the field
            IPropertyEditor propertyEditor = PropertyEditor.Get(property.StorageType);

            object currentValue = property.Read(data);

            Rect rect = EditorGUILayout.GetControlRect(true, propertyEditor.GetElementHeight(label, currentValue));
            object updatedValue = propertyEditor.Edit(rect, label, currentValue);

            property.Write(data, updatedValue);
        }

        void IDataInspector.OnSceneGUI(Data.IData data, GameObject context) {
        }
    }
}