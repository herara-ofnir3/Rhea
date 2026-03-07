using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.InputSystem;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Rhea.Launching
{
	public interface ISplashPageView
	{
		UniTask WaitFinishedAsync(CancellationToken cancellationToken = default);
	}

	public class SplashPageView : Page, ISplashPageView
	{
		public InputAction submitAction;

		public async UniTask WaitFinishedAsync(CancellationToken cancellationToken = default)
		{
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			var tasks = new[]
			{
				WaitSkipAsync(cts.Token),
				WaitLogoAsync(cts.Token),
			};
			await UniTask.WhenAny(tasks);
			cts.Cancel();
		}

		UniTask WaitSkipAsync(CancellationToken cancellationToken = default)
		{
			return UniTask.WaitUntil(() => submitAction.triggered);
		}

		UniTask WaitLogoAsync(CancellationToken cancellationToken = default)
		{
			// 3秒ロゴとか流れる想定
			return UniTask.Delay(3000, cancellationToken: cancellationToken);
		}
	}
}
