using Forge.Entities;
using Newtonsoft.Json;

namespace Forge.Unity {
    /// <summary>
    /// Data that specifies that an IEntity should use a prefab as its base GameObject instead of an
    /// empty GameObject.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PrefabData : Data.NonVersioned {
        /// <summary>
        /// The prefab to use for getting the base GameObject that will render the IEntity instance
        /// that this data instance is attached to.
        /// </summary>
        // TODO: this should also contain the prefab GUID
        [JsonProperty("PrefabResourcePath")]
        public string PrefabResourcePath;
    }
}