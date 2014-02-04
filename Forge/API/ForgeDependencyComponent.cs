using Forge.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// The ForgeDependencyComponent contains all of the dependencies that the the runtime system
    /// requires that can be configured by the end-user.
    /// </summary>
    public abstract class ForgeDependencyComponent : MonoBehaviour {
        /// <summary>
        /// Try to get any pending input that the IGameEngine needs to execute.
        /// </summary>
        /// <param name="input">The input, if there is any.</param>
        /// <returns>True if there was any input, false otherwise.</returns>
        public abstract bool TryGetInput(out List<IGameInput> input);

        /// <summary>
        /// Send the given input across the network or similar.
        /// </summary>
        /// <param name="input">The input to send.</param>
        public abstract void SendInput(List<IGameInput> input);

        /// <summary>
        /// Get the current interpolation percentage between updates.
        /// </summary>
        public abstract float InterpolationPercentage {
            get;
        }
    }
}