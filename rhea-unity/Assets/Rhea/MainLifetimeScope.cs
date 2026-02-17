using VContainer;
using VContainer.Unity;

namespace Rhea
{
	public class MainLifetimeScope : LifetimeScope
	{
		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<MainPresenter>();
		}
	}
}
