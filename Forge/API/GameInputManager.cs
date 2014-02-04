using Forge.Entities;
using Forge.Networking.AutomaticTurnGame;
using System.Collections.Generic;
using UnityEngine;

namespace Forge.Unity {
    public interface IGameInputManager {
        /// <summary>
        /// Pushes the given input command that will be sent across the network and eventually
        /// executed.
        /// </summary>
        void Send(IGameInput input);
    }

    /// <summary>
    /// Manages game input sending across the network.
    /// </summary>
    public class GameInputManager : SingletonBehavior<GameInputManager, IGameInputManager>, IGameInputManager {
        public ForgeDependencyComponent Dependencies;

        private List<IGameInput> _input = new List<IGameInput>();

        public void Send(IGameInput input) {
            _input.Add(input);
        }

        protected void Update() {
            // TODO: implement command filtering, ie, don't send 20 position move commands; perhaps
            //       have AutomaticTurnGame do this
            Dependencies.SendInput(_input);
            _input.Clear();
        }
    }
}