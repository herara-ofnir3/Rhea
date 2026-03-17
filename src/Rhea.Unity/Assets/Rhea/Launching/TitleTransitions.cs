using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Rhea.Unity.Launching
{
	public interface ITitleTransitions
	{
		UniTask<ISplashPageView> LaunchedAsync(CancellationToken cancellationToken = default);

		UniTask<IMenuPageView> SplashFinishedAsync(CancellationToken cancellationToken = default);

		UniTask StartedAsync(CancellationToken cancellationToken = default);
	}

	public class TitleTransitions : MonoBehaviour, ITitleTransitions
	{
		[SerializeField]
		PageContainer pageContainer;

		public async UniTask<ISplashPageView> LaunchedAsync(CancellationToken cancellationToken = default)
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

		public async UniTask StartedAsync(CancellationToken cancellationToken = default)
		{
			await SceneManager.LoadSceneAsync("Assets/Rhea/MainScene.unity");
		}
	}
}
