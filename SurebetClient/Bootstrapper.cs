using Prism.Modularity;
using Prism.Unity;
using Project.Interfaces;
using Microsoft.Practices.Unity;
using System.Windows;
using Project.Views;
using Project.ViewModels;

namespace Project
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            
            Container.RegisterType<IBetViewModel, BetViewModel>();
            Container.RegisterType<ISettingViewModel, SettingViewModel>();
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            var moduleCatalog = (ModuleCatalog)ModuleCatalog;
        }
    }
}
