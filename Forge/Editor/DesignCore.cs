using Forge.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forge.Unity {
    internal static class DesignCore {
        public static Lazy<List<ISystemProvider>> SystemProviders = new Lazy<List<ISystemProvider>>(
            () => {
                return (
                    from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()

                    // the type has to implement ISystemProvider
                    where type.IsImplementationOf(typeof(ISystemProvider))

                    // the type has to have an empty constructor
                    where type.GetConstructor(Type.EmptyTypes) != null

                    select (ISystemProvider)Activator.CreateInstance(type)
                ).ToList();
            });

    }
}