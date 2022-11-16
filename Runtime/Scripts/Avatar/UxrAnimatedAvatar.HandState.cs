using System;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Avatar
{
    public partial class UxrAnimatedAvatar
    {
        private class HandState
        {
            public UnityEngine.Animation Animation => _animation;
            public UxrAnimatedAvatarEventHandler EventHandler => _eventHandler;
            public string CurrentAnimName { get; set; } = String.Empty;

            private UnityEngine.Animation _animation; 
            private UxrAnimatedAvatarEventHandler _eventHandler;

            public HandState(Transform hand, UxrHandSide handSide)
            {
                _animation = hand.GetComponentInChildren<UnityEngine.Animation>();
                if (_animation)
                {
                    _eventHandler = _animation.GetOrAddComponent<UxrAnimatedAvatarEventHandler>();
                    _eventHandler.HandSide = UxrHandSide.Left;
                }
            }
        }
    }
}