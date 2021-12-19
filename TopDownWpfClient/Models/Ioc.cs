using Microsoft.AnyContainer;
using Microsoft.AnyContainer.DryIoc;
using TopDownWpfClient.Services.Pages;
using TopDownWpfClient.Services.Windows;

namespace TopDownWpfClient.Models {
    public static class Ioc {
        public static void SetUp() {
            var container = new DryIocAnyContainer();
            container.RegisterSingleton<IWindowManager, WindowManager>();
            container.RegisterSingleton<IPageManager, PageManager>();
            // container.RegisterSingleton<IAccountsLoader, ExcelAccountsLoader>();
            // container.RegisterSingleton<ICompLoader, JsonCompLoader>();
            // container.RegisterSingleton<IHitAllocationWriter, ExcelHItAllocationWriter>();
            // container.RegisterTransient<IHitAllocator, SimpleHitAllocator>();

            StaticResolver.SetResolver(container);
            Container = container;
        }

        /// <summary>
        /// Use it to register something in runtime (It's not recommended to abuse it)
        /// </summary>
        public static AnyContainerBase Container { get; set; }
    }
}