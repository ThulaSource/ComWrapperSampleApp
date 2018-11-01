using StructureMap;

namespace COMWrapperSampleApp.StructureMap
{
    public static class Bootstrapper
    {
        private static bool initialized;

        public static T GetInstance<T>()
        {
            if (!initialized)
            {
                Init();
            }

            return ObjectFactory.GetInstance<T>();
        }

        private static void Init()
        {
            ObjectFactory.Initialize(x =>
            {
                x.AddRegistry<COMWrapperSampleAppRegistry>();
                x.AddRegistry<FM.Common.CommonRegistry>();
            });
            initialized = true;
        }
    }
}