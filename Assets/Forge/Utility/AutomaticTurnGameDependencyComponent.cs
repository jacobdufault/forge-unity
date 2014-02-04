using Forge.Entities;
using Forge.Networking.AutomaticTurnGame;
using Forge.Networking.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// The default dependency component which just creates a server and uses an AutomaticTurnGame
    /// for command dispatch.
    /// </summary>
    public class AutomaticTurnGameDependencyComponent : ForgeDependencyComponent {
        private NetworkContext _networkContext;
        private AutomaticTurnGame _game;

        public int TargetUpdatesPerSecond;
        public Player HostPlayer;

        protected void Reset() {
            TargetUpdatesPerSecond = 10;
            HostPlayer = new Player("Default Player");
        }

        protected void OnEnable() {
            _networkContext = NetworkContext.CreateServer(HostPlayer, "");
            _game = new AutomaticTurnGame(_networkContext, TargetUpdatesPerSecond);
        }

        protected void Update() {
            _networkContext.Update();

            // deltaTime is in milliseconds, but game.Update expects seconds
            _game.Update(Time.deltaTime * 1000);
        }

        public override bool TryGetInput(out List<IGameInput> input) {
            List<IGameCommand> commands;
            if (_game.TryUpdate(out commands)) {
                input = commands.Cast<IGameInput>().ToList();
                return true;
            }

            input = null;
            return false;
        }

        public override void SendInput(List<IGameInput> input) {
            // TODO: rename SendCommand to SendCommands and take an IEnumerable
            _game.SendCommand(input.Cast<IGameCommand>().ToList());
        }

        public override float InterpolationPercentage {
            get {
                return _game.InterpolationPercentage;
            }
        }
    }
}