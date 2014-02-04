using Forge.Entities;
using FullInspector;
using System;
using UnityEngine;

namespace Forge.Editing {
    internal abstract class BaseDataReferencePropertyEditor<TDataReference> : PropertyEditor<IDataReference>
        where TDataReference : IDataReference, new() {

        /// <summary>
        /// The editor we use for selecting a provider.
        /// </summary>
        private static QueryableEntityPropertyEditor _editor = new QueryableEntityPropertyEditor();

        /// <summary>
        /// Returns the data provider for the given reference or null if the reference is null.
        /// </summary>
        private IQueryableEntity GetDataProvider(IDataReference reference) {
            if (reference != null) {
                return reference.Provider;
            }

            return null;
        }

        /// <summary>
        /// Verifies that the given selected entity contains a data instance of the given type. If
        /// the given entity does not contain it, then a warning is issued.
        /// </summary>
        private void Verify(Type type, IQueryableEntity selected) {
            if (selected.ContainsData(new DataAccessor(type)) == false) {
                Debug.LogWarning("Data provider " + selected + " has a reference that requires " +
                    "an instance of " + type + ", but it lacks said reference");
            }
        }

        public override IDataReference Edit(Rect region, GUIContent label, IDataReference element) {
            IQueryableEntity currentProvider = GetDataProvider(element);

            // update the selected provider
            IQueryableEntity provider = _editor.Edit(region, label, currentProvider);

            // no provider selected means there is no data reference
            if (provider == null) {
                return null;
            }

            // a new provider was selected, but it lacks the required data type; warn the user, but
            // still allow the user to select it
            foreach (Type genericType in typeof(TDataReference).GetGenericArguments()) {
                Verify(genericType, provider);
            }

            // do we need to allocate a DataReference to contain the provider?
            if (element == null) {
                element = new TDataReference();
            }
            element.Provider = provider;
            return element;
        }

        public override float GetElementHeight(GUIContent label, IDataReference element) {
            return _editor.GetElementHeight(label, GetDataProvider(element));
        }
    }

    [CustomPropertyEditor(typeof(DataReference<>))]
    internal class DataReferencePropertyEditor<TData0> :
        BaseDataReferencePropertyEditor<DataReference<TData0>>
        where TData0 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,>))]
    internal class DataReferencePropertyEditor<TData0, TData1> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1>>
        where TData0 : Data.IData
        where TData1 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2, TData3> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2, TData3>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData
        where TData3 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2, TData3, TData4> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2, TData3, TData4>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData
        where TData3 : Data.IData
        where TData4 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,,,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2, TData3, TData4, TData5> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2, TData3, TData4, TData5>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData
        where TData3 : Data.IData
        where TData4 : Data.IData
        where TData5 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,,,,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2, TData3, TData4, TData5, TData6> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData
        where TData3 : Data.IData
        where TData4 : Data.IData
        where TData5 : Data.IData
        where TData6 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,,,,,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData
        where TData3 : Data.IData
        where TData4 : Data.IData
        where TData5 : Data.IData
        where TData6 : Data.IData
        where TData7 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,,,,,,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData
        where TData3 : Data.IData
        where TData4 : Data.IData
        where TData5 : Data.IData
        where TData6 : Data.IData
        where TData7 : Data.IData
        where TData8 : Data.IData { }
    [CustomPropertyEditor(typeof(DataReference<,,,,,,,,,>))]
    internal class DataReferencePropertyEditor<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8, TData9> :
        BaseDataReferencePropertyEditor<DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8, TData9>>
        where TData0 : Data.IData
        where TData1 : Data.IData
        where TData2 : Data.IData
        where TData3 : Data.IData
        where TData4 : Data.IData
        where TData5 : Data.IData
        where TData6 : Data.IData
        where TData7 : Data.IData
        where TData8 : Data.IData
        where TData9 : Data.IData { }
}