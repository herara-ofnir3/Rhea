using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Rhea.Launching
{
	public interface ITitleTransitions
	{
		UniTask<ISplashPageView> StartedAsync(CancellationToken cancellationToken = default);

		UniTask<IMenuPageView> SplashFinishedAsync(CancellationToken cancellationToken = default);
	}

	public class TitleTransitions : MonoBehaviour, ITitleTransitions
	{
		[SerializeField]
		PageContainer pageContainer;

		public async UniTask<ISplashPageView> StartedAsync(CancellationToken cancellationToken = default)
		{
			var pushed = await pageContainer.PushAsync<SplashPageView>(
				"Assets/Rhea/Launching/SplashPage.prefab",
				playAnimation: true,
				cancellation: cancellationToken
				);
			return pushed.Page;
		}

		public async UniTask<IMenuPageView> SplashFinishedAsync(CancellationToken cancellationToken = default)
		{
			var pushed = await pageContainer.PushAsync<MenuPageView>(
				"Assets/Rhea/Launching/MenuPage.prefab",
				playAnimation: true,
				cancellation: cancellationToken
				);
			return pushed.Page;
		}
	}
}
