using System;

namespace DevExpress {
    public static class MemoryLeaksHelper {
        public static void EnsureCollected(params WeakReference[] references) {
#if !SILVERLIGHT && !NETFX_CORE && !MONO
            DispatcherHelper.DoEvents();
#endif
            GCTestHelper.EnsureCollected(references);
        }
        public static void CollectOptional(params WeakReference[] references) {
#if !SILVERLIGHT && !NETFX_CORE && !MONO
            DispatcherHelper.DoEvents();
#endif
            GCTestHelper.CollectOptional(references);
        }
    }
}