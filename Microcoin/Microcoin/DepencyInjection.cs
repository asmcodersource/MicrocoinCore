using SimpleInjector;
using Microcoin.Microcoin.ChainStorage;

namespace Microcoin.Microcoin
{
    public static class DepencyInjection
    {
        static public Container Container; // Multiple peers running within the same process will share a shared chain store.

        public static void CreateContainer()
            => Container = new Container();

        public static void AddChainsStorage(string workingDirectory)
        {
            Directory.CreateDirectory(workingDirectory);
            Container.Register<ChainStorage.ChainStorage>(Lifestyle.Singleton);
            var chainStorage = Container.GetInstance<ChainStorage.ChainStorage>();
            chainStorage.WorkingDirectory = workingDirectory;
            chainStorage.FetchChains();
        }
    }
}
