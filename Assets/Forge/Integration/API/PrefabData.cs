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
        [JsonProperty("PrefabResourcePath")]
        public string PrefabResourcePath;

        /// <summary>
        /// The GUID that identifies that asset. If AssetGuid and PrefabResourcePath point to
        /// different resources, then the element at PrefabResourcePath is prioritized. The
        /// AssetGuid is a (hopefully) more stable identifier than the PrefabResourcePath.
        /// </summary>
        [JsonProperty("PrefabGuid")]
        public string PrefabGuid;
    }
}