using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Avatar
{
    public class UxrAnimatedAvatarEventHandler : MonoBehaviour
    {
        public UxrHandSide HandSide { get; set; }

        private Dictionary<string, Action> _animationCompleteCallbacks = new Dictionary<string, Action>();

        public void RegisterOnComplete(string poseName, Action action)
        {
            if (action != null)
            {
                //Debug.LogError($"XXXXXX Animation event registered {poseName} - {HandSide.ToString()}");
                _animationCompleteCallbacks.Add(poseName, action);
            }
        }
        
        // Triggered by animation event
        public void OnAnimationCompleted(string poseName)
        {
            //Debug.LogError($"XXXXXX Animation completed {poseName} - {HandSide.ToString()}");
            if (_animationCompleteCallbacks.TryGetValue(poseName, out var callback) && callback != null)
            {
                //Debug.LogError($"XXXXXX Animation completed callback {poseName} - {HandSide.ToString()}");
                _animationCompleteCallbacks.Remove(poseName);
                callback.Invoke();
            }
        }
    }
}