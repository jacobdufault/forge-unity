using Forge.Entities;
using System.Collections.Generic;

namespace Forge.Unity {
    /// <summary>
    /// Specifies that the given type acts as a system provider which can then be used to inject
    /// types into IGameSnapshots.
    /// </summary>
    public interface ISystemProvider {
        /// <summary>
        /// Returns the systems that should be injected into a game snapshot to provide a common set
        /// of functionality.
        /// </summary>
        IEnumerable<ISystem> GetSystems();
    }
}