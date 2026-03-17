using VContainer;
using VContainer.Unity;

namespace Rhea.Unity
{
	public class MainLifetimeScope : LifetimeScope
	{
		public MainTransitions mainTransitions;

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance<IMainTransitions>(mainTransitions);
			builder.RegisterEntryPoint<MainPresenter>();
		}
	}
}
