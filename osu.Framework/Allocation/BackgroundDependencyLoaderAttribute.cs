﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace osu.Framework.Allocation
{
    /// <summary>
    /// Marks a method as the loader-Method of a <see cref="osu.Framework.Graphics.Drawable"/>, allowing for automatic injection of dependencies via the parameters of the method.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class BackgroundDependencyLoaderAttribute : Attribute
    {
        private const BindingFlags activator_flags = BindingFlags.NonPublic | BindingFlags.Instance;

        private bool permitNulls { get; }

        /// <summary>
        /// Marks this method as the initializer for a class in the context of dependency injection.
        /// </summary>
        /// <param name="permitNulls">If true, the initializer may be passed null for the dependencies we can't fulfill.</param>
        public BackgroundDependencyLoaderAttribute(bool permitNulls = false)
        {
            this.permitNulls = permitNulls;
        }

        internal static InjectDependencyDelegate CreateActivator(Type type)
        {
            var loaderMethods = type.GetMethods(activator_flags).Where(m => m.GetCustomAttribute<BackgroundDependencyLoaderAttribute>() != null).ToArray();

            switch (loaderMethods.Length)
            {
                case 0:
                    return (_,__) => { };
                case 1:
                    var method = loaderMethods[0];
                    var permitNulls = method.GetCustomAttribute<BackgroundDependencyLoaderAttribute>().permitNulls;
                    var parameterGetters = method.GetParameters().Select(p => p.ParameterType).Select(t => getDependency(t, type, permitNulls));

                    return (target, dc) =>
                    {
                        var parameters = parameterGetters.Select(p => p(dc)).ToArray();
                        method.Invoke(target, parameters);
                    };
                default:
                    throw new MultipleDependencyLoaderMethodsException(type);
            }
        }

        private static Func<DependencyContainer, object> getDependency(Type type, Type requestingType, bool permitNulls) => dc =>
        {
            var val = dc.Get(type);
            if (val == null && !permitNulls)
                throw new DependencyNotRegisteredException(requestingType, type);
            return val;
        };
    }
}
