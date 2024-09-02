using Godot;
using OpenARPG.Player;
using System;

namespace OpenARPG.System
{
    public static class GameStateManager
    {
        public static Action<float> Tick { get; private set; } = (_) => { Logger.Error("GameStateManager not initialized "); };
        public static Action<float> PhysicsTick { get; private set; } = (_) => { Logger.Error("GameStateManager not initialized "); };
        public static Action FatalError { get; private set; } = () => { Logger.Error("GameStateManager not initialized "); };
        public static Action<GameState> RequestNewState {get; private set; } = (newState) => { Logger.Error("GameStateManager not initialized "); };

        public static Action Initialize { get; private set; } = () => 
        {
            Initialize = () => { Logger.Error("GameStateManager: Initialize: GameStateManager already initialized"); };

            FatalError = () => 
            {
                Logger.Log("GameStateManager: FatalError: GameStateManager set to fatal error state");
                _currentState = new GameState_FatalError();
            };

            RequestNewState = (newState) => 
            {
                currentState = newState;
            };

            Tick = (delta) => { currentState.Tick(delta); };

            PhysicsTick = (delta) => { currentState.PhysicsTick(delta); };

            Logger.Log("GameStateManager: Initialize: GameStateManager initialized");

            currentState = new GameState_Init();
        };

        private static GameState _currentState = new GameState_Null();
        public static GameState currentState
        {
            get { return _currentState; }
            
            private set 
            { 
                if (!_currentState.AllowExit)
                {
                    Logger.Log($"GameStateManager: currentState.Set: Current \"{_currentState}\" does not allow exit.");
                    return;
                }

                _currentState.ExitState();

                _currentState = value;

                Logger.Log($"GameStateManager: Current state set to \"{_currentState}\"");

                _currentState.EnterState();
            }
        }

        public abstract class GameState
        {
            public virtual void Tick(float delta){}
            public virtual void PhysicsTick(float delta){}
            public virtual void EnterState(){}
            public virtual void ExitState(){}

            public virtual bool AllowExit => true;
            public virtual bool AllowPlayerEvents => false;
        }

        public class GameState_Null : GameState { }

        public class GameState_FatalError : GameState 
        {
            public override bool AllowExit => false;
        }

        public class GameState_Play : GameState 
        {
            public override bool AllowPlayerEvents => true;

            float elapsedTime = -600.0f;

            public override void Tick(float delta)
            {
                elapsedTime += delta;

                //to avoid overflowing noise functions on lower precision GPUs
                if (elapsedTime > 600.0f)
                    elapsedTime = -600.0f;

                RenderingServer.GlobalShaderParameterSet("ElapsedTime", elapsedTime);

                MainUI.UpdateMouseState();
                //if (MainUI.currentMouseMode == MainUI.MouseMode.Gui)
                    //MouseHandler3D.UpdateMouseWorldPosition();
                PlayerCharacter.Tick(delta);
            }

            public override void PhysicsTick(float delta)
            {
                PlayerCharacter.PhysicsTick(delta);
            }
        }

        public class GameState_ChangeLevel : GameState     
        {
        }

        public class GameState_Init : GameState 
        { 
            public override void Tick(float delta)
            {
                currentState = new GameState_Play();
            }
        }
    }
}