// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarComponent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Core.Components.Composite
{
    /// <summary>
    ///     Generic base class for components that are added to an <see cref="UxrAvatar" /> and we want to keep track of.
    ///     It allows to enumerate components using static methods.
    ///     This class could have instead inherited from <see cref="UxrComponent{TP,TC}" /> but we avoided
    ///     this to not have redundancy between Avatar/Parent properties and methods.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    public abstract class UxrAvatarComponent<T> : UxrComponent<T> where T : UxrAvatarComponent<T>
    {
        protected override void AddComponent()
        {
            base.AddComponent();
            AddAvatarComponent(this);
        }

        protected override void RemoveComponent()
        {
            base.RemoveComponent();
            RemoveAvatarComponent(this);
        }

        private static void AddAvatarComponent(UxrComponent component)
        {
            if (component is not UxrAvatarComponent<T> avatarComponent)
            {
                return;
            }

            UxrAvatar avatar = avatarComponent.Avatar;
            if (avatar == null)
            {
                return;
            }
            
            if (!s_avatarComponents.TryGetValue(avatar, out List<T> components))
            {
                components = new List<T>();
                s_avatarComponents[avatar] = components;
            }
                
            T typedComponent = component as T;
            if (typedComponent != null && !components.Contains(typedComponent))
            {
                components.Add(typedComponent);
            }
        }
        
        public static void RemoveAvatarComponent(UxrComponent component)
        {
            if (component is not UxrAvatarComponent<T> avatarComponent)
            {
                return;
            }

            UxrAvatar avatar = avatarComponent.Avatar;
            if (avatar == null)
            {
                return;
            }

            if (s_avatarComponents.TryGetValue(avatar, out var components))
            {
                T typedComponent = component as T;
                if (typedComponent != null)
                {
                    components.Remove(typedComponent);

                    if (components.Count == 0)
                    {
                        s_avatarComponents.Remove(avatar);
                    }
                }
            }
        }
        
        public static IReadOnlyList<T> GetAvatarComponents(UxrAvatar avatar)
        {
            if (avatar != null && s_avatarComponents.TryGetValue(avatar, out var components))
            {
                return components;
            }

            return Array.Empty<T>();
        }
        
        private static readonly Dictionary<UxrAvatar, List<T>> s_avatarComponents = new Dictionary<UxrAvatar, List<T>>();

        
        #region Public Types & Data

        /// <summary>
        ///     Gets all the components, enabled or not, of this specific type that belong to the local avatar.
        /// </summary>
        /// <remarks>
        ///     Components that have never been enabled are not returned. Components are automatically registered through their
        ///     Awake() call, which is never called if the object has never been enabled. In this case it is recommended to resort
        ///     to <see cref="UnityEngine.GameObject.GetComponentsInChildren{T}(bool)">GetComponentsInChildren</see>.
        /// </remarks>
        public static IEnumerable<T> AllComponentsInLocalAvatar 
        {
            get
            {
                foreach (var c in AllComponents)
                {
                    if (c.Avatar != null && c.Avatar.AvatarMode == UxrAvatarMode.Local)
                    {
                        yield return c;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all the enabled components of this specific type that belong to the local avatar.
        /// </summary>
        public static IEnumerable<T> EnabledComponentsInLocalAvatar
        {
            get
            {
                var result = new List<T>();

                foreach (var c in AllComponents)
                {
                    if (c.Avatar != null && c.Avatar.AvatarMode == UxrAvatarMode.Local && c.enabled)
                    {
                        result.Add(c);
                    }
                }

                return result;
            }
        }

        /// <summary>
        ///     Gets the local avatar or null if there is none.
        /// </summary>
        public static UxrAvatar LocalAvatar
        {
            get
            {
                foreach (var c in AllComponents)
                {
                    if (c.Avatar != null && c.Avatar.AvatarMode == UxrAvatarMode.Local)
                    {
                        return c.Avatar;
                    }
                }
                return null;
            }
        }

        /// <summary>
        ///     Gets all the components, enabled of not, of this specific type that belong to this instance of the avatar.
        /// </summary>
        /// <remarks>
        ///     Components that have never been enabled are not returned. Components are automatically registered through their
        ///     Awake() call, which is never called if the object has never been enabled. In this case it is recommended to resort
        ///     to <see cref="GameObject.GetComponentsInChildren{T}(bool)" />.
        /// </remarks>
        public IEnumerable<T> AllComponentsInAvatar => GetComponents(Avatar, true);

        /// <summary>
        ///     Gets only the enabled components of this specific type that belong to this instance of the avatar.
        /// </summary>
        public IEnumerable<T> EnabledComponentsInAvatar => GetComponents(Avatar);

        /// <summary>
        ///     Gets the <see cref="UxrAvatar" /> the component belongs to.
        /// </summary>
        public UxrAvatar Avatar
        {
            get
            {
                if (_avatar == null)
                {
                    _avatar = this.SafeGetComponentInParent<UxrAvatar>();
                }

                return _avatar;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the components of a specific avatar.
        /// </summary>
        /// <param name="avatar">Avatar to get the components from</param>
        /// <param name="includeDisabled">Whether to include disabled components or not</param>
        /// <returns>Components meeting the criteria</returns>
        /// <remarks>
        ///     When using the <paramref name="includeDisabled" /> parameter, components that have never been enabled are not
        ///     returned. Components are automatically registered through their Awake() call, which is never called if the object
        ///     has never been enabled. In this case it is recommended to resort to
        ///     <see cref="GameObject.GetComponentsInChildren{T}(bool)" />.
        /// </remarks>
        public static IEnumerable<T> GetComponents(UxrAvatar avatar, bool includeDisabled = false)
        {
            List<T> result = new List<T>();
            
            foreach (var component in AllComponents)
            {
                if (component.Avatar == avatar && (includeDisabled || component.enabled))
                {
                    result.Add(component);
                }
            }

            return result;
        }

        public static IEnumerable<T> GetAllComponentsForAvatar(UxrAvatar avatar)
        {
            if (avatar == null)
            {
                return Array.Empty<T>();
            }

            if (s_avatarComponents.TryGetValue(avatar, out var components))
            {
                return components;
            }
               
            return Array.Empty<T>();
        }
        
        /// <summary>
        ///     Gets the components of a specific avatar.
        /// </summary>
        /// <param name="avatar">Avatar to get the components from</param>
        /// <param name="includeDisabled">Whether to include disabled components or not</param>
        /// <returns>Components meeting the criteria</returns>
        /// <remarks>
        ///     When using the <paramref name="includeDisabled" /> parameter, components that have never been enabled are not
        ///     returned. Components are automatically registered through their Awake() call, which is never called if the object
        ///     has never been enabled. In this case it is recommended to resort to
        ///     <see cref="GameObject.GetComponentsInChildren{T}(bool)" />.
        /// </remarks>
        public static IEnumerable<TC> GetComponents<TC>(UxrAvatar avatar, bool includeDisabled = false) where TC : T
        {
            List<TC> result = new List<TC>();

            foreach (var c in AllComponents)
            {
                if (c is TC component)
                {
                    if (component.Avatar == avatar)
                    {
                        if (includeDisabled || component.enabled)
                        {
                            result.Add(component);
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Pre-caches the avatar component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (_avatar == null)
            {
                _avatar = GetComponentInParent<UxrAvatar>();
            }
        }

        #endregion

        #region Private Types & Data

        private UxrAvatar _avatar;

        #endregion
    }
}