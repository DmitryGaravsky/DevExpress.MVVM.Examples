namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    sealed class AssemblyHelper {
        public static IEnumerable<Assembly> GetLoadedAssemblies() {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
        public static Assembly GetLoadedAssembly(string asmName) {
            var assemblies = GetLoadedAssemblies();
            foreach(Assembly asm in assemblies) {
                if(PartialNameEquals(asm.FullName, asmName))
                    return asm;
            }
            return null;
        }
        public static bool PartialNameEquals(string asmName0, string asmName1) {
            return string.Equals(GetPartialName(asmName0), GetPartialName(asmName1), StringComparison.InvariantCultureIgnoreCase);
        }
        public static string GetPartialName(Assembly assembly) {
            return GetPartialName(assembly.FullName);
        }
        public static string GetPartialName(string asmName) {
            int nameEnd = asmName.IndexOf(',');
            return (nameEnd < 0) ? asmName : asmName.Remove(nameEnd);
        }
    }
}