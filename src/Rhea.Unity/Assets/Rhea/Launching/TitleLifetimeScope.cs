using VContainer;
using VContainer.Unity;

namespace Rhea.Unity.Launching
{
	public class TitleLifetimeScope : LifetimeScope
	{
		public TitleTransitions titleTransitions;

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance<ITitleTransitions>(titleTransitions);
			builder.RegisterEntryPoint<TitlePresenter>();
		}
	}
}