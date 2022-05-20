using System;
using UnityEngine;

namespace Bacterio.Input
{
    public sealed class PcInputHandler : IInputHandler
    {
        private Action<Vector2> _onMovementCb = null;
        private Action _onMovementReleaseCb = null;
        private Action<Vector2> _onShootCb = null;
        private Action _onShootReleaseCb = null;

        private readonly KeyCode[] _movementKeyCodes = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D }; //Up, left, down, right
        private readonly Vector2Int[] _movementDirectionLookup = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right};
        private readonly KeyCode _shootKeyCode = KeyCode.Mouse0;
        private readonly Vector2Int _halfScreenDims = new Vector2Int(Screen.width / 2, Screen.height / 2);


        public bool IsPaused { get; set; } = false;

        public PcInputHandler()
        {
           
        }

        public void RegisterOnMovementEvent(Action<Vector2> onMovementCb, Action onMovementReleaseCb)
        {
            _onMovementCb = onMovementCb;
            _onMovementReleaseCb = onMovementReleaseCb;
            WDebug.Log(_onMovementCb != null ? "Registered movement event cb" : "De-registered movement event cb");
        }

        public void RegisterOnShootEvent(Action<Vector2> onShootCb, Action onShootReleaseCb)
        {
            _onShootCb = onShootCb;
            _onShootReleaseCb = onShootReleaseCb;
            WDebug.Log(_onShootCb != null ? "Registered shoot event cb" : "De-registered shoot event cb");
        }

        public void RunOnce()
        {
            WDebug.Assert(_onMovementCb != null, "No movement cb during runOnce");
            WDebug.Assert(_onShootCb != null, "No shoot cb during runOnce");

            if (IsPaused)
                return;

            //Check for movement
            Vector2Int direction = Vector2Int.zero;
            bool hadRelease = false;
            for (int i = 0; i < 4; i++)
            {
                if (UnityEngine.Input.GetKey(_movementKeyCodes[i]))
                    direction += _movementDirectionLookup[i];
                else
                    hadRelease |= UnityEngine.Input.GetKeyUp(_movementKeyCodes[i]);
            }

            if (direction != Vector2Int.zero)
                _onMovementCb(direction);
            else if (hadRelease)
                _onMovementReleaseCb();

            //Never check shooting if mouse is in UI
            if (MouseTracker.MouseIsInUI)
                return;

            //Check for shooting key
            if (UnityEngine.Input.GetKey(_shootKeyCode))
            {
                var pos = UnityEngine.Input.mousePosition;
                Vector2 mouseDirection = new Vector2(pos.x - _halfScreenDims.x, pos.y - _halfScreenDims.y);
                _onShootCb(mouseDirection.normalized);
            }
            else if (UnityEngine.Input.GetKeyUp(_shootKeyCode))
                _onShootReleaseCb();
        }
    }
}
