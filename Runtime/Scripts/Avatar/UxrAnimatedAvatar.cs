﻿using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Avatar
{
    public partial class UxrAnimatedAvatar : UxrAvatar
    {
        public delegate AnimationClip LoadAnimationClipDelegate(string key);
        public LoadAnimationClipDelegate LoadAnimationClip { get; set; }

        public delegate string GetDefaultPoseNameDelegate();
        public GetDefaultPoseNameDelegate GetDefaultPoseName { get; set; }

        private HandState _leftHandState;
        private HandState _rightHandState;

        protected override void Awake()
        {
            base.Awake();

            _leftHandState = new HandState(LeftHandBone, UxrHandSide.Left);
            _rightHandState = new HandState(RightHandBone, UxrHandSide.Right);
        }

        public override string DefaultHandPoseName
        {
            get => GetDefaultPoseName.Invoke();
        }

        public override bool SetCurrentHandPose(UxrHandSide handSide, string poseName, float blendValue = 0.0f, bool propagateEvents = true)
        {
            return SetCurrentHandAnimation(handSide, poseName);
        }

        public bool SetCurrentHandAnimation(UxrHandSide handSide, string poseName, Action onComplete = null)
        {
            var handState = handSide == UxrHandSide.Left ? _leftHandState : _rightHandState;

            if (handState.CurrentAnimName.Equals(poseName))
                return true;
            
            if (TryEnsureClipExistsForHand(handState, poseName))
            {
                if (handState.EventHandler && onComplete != null)
                {
                    handState.EventHandler.OnAnimationCompleted(handState.CurrentAnimName);
                    handState.EventHandler.RegisterOnComplete(poseName, onComplete);
                }
                handState.Animation.CrossFade(poseName, 0.1f);
                handState.CurrentAnimName = poseName;
                return true;
            }

            return false;
        }

        private bool TryEnsureClipExistsForHand(HandState hand, string animName)
        {
            if (hand.Animation.GetClip(animName))
            {
                return true;
            }
            
            AnimationClip clip = LoadAnimationClip.Invoke(animName);
            if (clip)
            {
                clip.legacy = true;
                clip.wrapMode = WrapMode.ClampForever;
                if (clip.events.FirstOrDefault(x => x.functionName.Equals("OnAnimationCompleted")) == null)
                {
                    AnimationEvent endEvent = new AnimationEvent();
                    endEvent.time = clip.length;
                    endEvent.functionName = "OnAnimationCompleted";
                    endEvent.stringParameter = animName;
                    endEvent.intParameter = (int)hand.EventHandler.HandSide;
                    clip.AddEvent(endEvent);
                }
                hand.Animation.AddClip(clip, animName);
                return true;
            }
            
            Debug.LogError($"Hand animation clip \"{animName}\" not found");

            return false;
        }
    }
}