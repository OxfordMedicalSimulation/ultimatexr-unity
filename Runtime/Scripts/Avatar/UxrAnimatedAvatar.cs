using System;
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
            return SetCurrentHandAnimation(handSide, poseName, propagateEvents);
        }

        public bool SetCurrentHandAnimation(UxrHandSide handSide, string poseName, bool propagateEvents = true, int priority = 0, Action onComplete = null)
        {
            var handState = handSide == UxrHandSide.Left ? _leftHandState : _rightHandState;

            if (LoadAnimationClip == null || handState.CurrentAnimName.Equals(poseName) || handState.CurrentAnimPriority > priority) 
                return true;

            if (TryEnsureClipExistsForHand(handState, poseName))
            {
                UxrAvatarHandPoseChangeEventArgs avatarHandPoseChangeArgs = new UxrAvatarHandPoseChangeEventArgs(this, handSide, poseName);
                
                if (propagateEvents)
                {
                    OnHandPoseChanging(avatarHandPoseChangeArgs);
                }
                
                if (handState.EventHandler)
                {
                    handState.EventHandler.OnAnimationCompleted(handState.CurrentAnimName);
                    handState.EventHandler.RegisterOnComplete(poseName, onComplete);
                }
                handState.Animation.CrossFade(poseName, 0.1f);
                handState.CurrentAnimName = poseName;
                handState.CurrentAnimPriority = priority;
                //Debug.LogError($"XXXXXX Animation started {handState.CurrentAnimName} - {handSide.ToString()}");

                if (propagateEvents)
                {
                    OnHandPoseChanged(avatarHandPoseChangeArgs);
                }
                
                return true;
            }

            return false;
        }

        public void StopHandAnimation(UxrHandSide handSide, string key)
        {
            var handState = handSide == UxrHandSide.Left ? _leftHandState : _rightHandState;
            if (string.IsNullOrEmpty(key) || handState.CurrentAnimName.Equals(key))
            {
                handState.CurrentAnimPriority = 0;
                //Debug.LogError($"XXXXXX Animation stopped {handState.CurrentAnimName} - {handSide.ToString()}");
                SetCurrentHandAnimation(handSide, DefaultHandPoseName);
            }
        }

        private bool TryEnsureClipExistsForHand(HandState hand, string animName)
        {
            if (hand.Animation.GetClip(animName))
            {
                return true;
            }

            string[] animParams = animName.Split(':');

            if (animParams.Length == 0 || string.IsNullOrEmpty(animParams[0]))
            {
                return false;
            }
            
            AnimationClip clip = LoadAnimationClip.Invoke(animParams[0]);
            if (clip)
            {
                clip.legacy = true;
                clip.wrapMode = WrapMode.ClampForever;
                float eventEnd = clip.length;

                if (animParams.Length == 3)
                {
                    int firstFrame = int.Parse(animParams[1]);
                    int lastFrame = int.Parse(animParams[2]);
                    
                    if (firstFrame >= lastFrame) 
                        lastFrame = firstFrame + 1;
                    
                    eventEnd = (lastFrame - firstFrame) / clip.frameRate;
                    hand.Animation.AddClip(clip, animName, firstFrame, lastFrame);
                }
                else
                {
                    hand.Animation.AddClip(clip, animName);
                }
                
                AnimationClip newClip = hand.Animation.GetClip(animName);
                if (newClip.events.FirstOrDefault(x => x.functionName.Equals("OnAnimationCompleted")) == null)
                {
                    AnimationEvent endEvent = new AnimationEvent();
                    endEvent.time = eventEnd;
                    endEvent.functionName = "OnAnimationCompleted";
                    endEvent.stringParameter = animName;
                    endEvent.intParameter = (int)hand.EventHandler.HandSide;
                    endEvent.floatParameter = eventEnd;
                    newClip.AddEvent(endEvent);
                }

                return true;
            }
            
            Debug.LogError($"Hand animation clip \"{animParams[0]}\" not found");

            return false;
        }
    }
}