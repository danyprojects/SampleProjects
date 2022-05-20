using System;
using UnityEngine;

namespace Bacterio.Input
{
    public interface IInputHandler
    {
        public void RegisterOnMovementEvent(Action<Vector2> onMovementCb, Action onMovementReleaseCb);
        public void RegisterOnShootEvent(Action<Vector2> onShootCb, Action onShootReleaseCb);
        public bool IsPaused { get; set; }
        public void RunOnce();
    }
}
