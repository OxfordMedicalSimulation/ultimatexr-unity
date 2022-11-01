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
        [SerializeField] private List<AnimationClip> _tempClipStore = new List<AnimationClip>();
        
        private UnityEngine.Animation _leftAnim; 
        private UnityEngine.Animation _rightAnim;
        private UxrAnimatedAvatarEventHandler _leftHandler;
        private UxrAnimatedAvatarEventHandler _rightHandler;

        private string _currentAnimName;

        protected override void Awake()
        {
            base.Awake();

            _leftAnim = LeftHandBone.GetComponentInParent<UnityEngine.Animation>();
            if (_leftAnim)
            {
                _leftHandler = _leftAnim.GetOrAddComponent<UxrAnimatedAvatarEventHandler>();
                _leftHandler.HandSide = UxrHandSide.Left;
            }
            
            _rightAnim = RightHandBone.GetComponentInParent<UnityEngine.Animation>();
            if (_rightAnim)
            {
                _rightHandler = _rightAnim.GetOrAddComponent<UxrAnimatedAvatarEventHandler>();
                _rightHandler.HandSide = UxrHandSide.Right;
            }
        }

        public override string DefaultHandPoseName
        {
            get => _tempClipStore.FirstOrDefault()?.name;
        }

        public override bool SetCurrentHandPose(UxrHandSide handSide, string poseName, float blendValue = 0.0f, bool propagateEvents = true)
        {
            if (SetCurrentHandAnimation(handSide, poseName))
            {
                return true;
            }

            return false;
            //return base.SetCurrentHandPose(handSide, poseName, blendValue, propagateEvents);
        }

        public bool SetCurrentHandAnimation(UxrHandSide handSide, string poseName, Action onComplete = null)
        {
            if (poseName.Equals(_currentAnimName))
                return true;

            var anim = handSide == UxrHandSide.Left ? _leftAnim : _rightAnim;
            var handler = handSide == UxrHandSide.Left ? _leftHandler : _rightHandler;

            if (!_tempClipStore.Exists(x => x.name.Equals(poseName)))
                poseName = "HandTestAnim";

            if (anim)
            {
                AnimationClip clip = anim.GetClip(poseName); 
                if (!clip)
                {
                    clip = _tempClipStore.Find(x => x.name.Equals(poseName));
                    if (clip)
                    {
                        clip.legacy = true;
                        clip.wrapMode = WrapMode.ClampForever;
                        clip.events = Array.Empty<AnimationEvent>();
                        AnimationEvent endEvent = new AnimationEvent();
                        endEvent.time = clip.length;
                        endEvent.functionName = "OnAnimationCompleted";
                        endEvent.stringParameter = poseName;
                        endEvent.intParameter = (int)handSide;
                        clip.AddEvent(endEvent);
                        anim.AddClip(clip, poseName);
                    }
                }

                if (clip)
                {
                    if (handler && onComplete != null)
                    {
                        handler.OnAnimationCompleted(_currentAnimName);
                        handler.RegisterOnComplete(poseName, onComplete);
                    }
                    anim.CrossFade(poseName, 0.1f);
                    _currentAnimName = poseName;
                }
                else
                {
                    Debug.LogError($"Hand animation clip \"{poseName}\" not found");
                }

                return true;
            }

            return false;
        }

        
    }
}