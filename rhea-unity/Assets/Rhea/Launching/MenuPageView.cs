using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Rhea.Launching
{
	public interface IMenuPageView
	{
		UniTask<TitleMenu> WaitMenuSelectedAsync(CancellationToken cancellationToken = default);
	}

	public class MenuPageView : Page, IMenuPageView
	{
		[SerializeField]
		Button startButton;

		[SerializeField]
		Button settingsButton;

		[SerializeField]
		Button quitButton;

		void Start()
		{
			startButton.Select();
		}

		public async UniTask<TitleMenu> WaitMenuSelectedAsync(CancellationToken cancellationToken = default)
		{
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			var tasks = new[]
			{
				startButton.OnClickAsync(cts.Token).ContinueWith(() => TitleMenu.Start),
				settingsButton.OnClickAsync(cts.Token).ContinueWith(() => TitleMenu.Settings),
				quitButton.OnClickAsync(cts.Token).ContinueWith(() => TitleMenu.Quit),
			};
			var (_, r) = await UniTask.WhenAny(tasks);
			cts.Cancel();
			return r;
		}
	}
}
