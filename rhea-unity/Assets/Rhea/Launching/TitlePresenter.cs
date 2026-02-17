using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace Rhea.Launching
{
	public class TitlePresenter : IAsyncStartable
	{
		readonly ITitleTransitions titleTransitions;

		public TitlePresenter(ITitleTransitions titleTransitions)
		{
			this.titleTransitions = titleTransitions;
		}

		public async UniTask StartAsync(CancellationToken cancellation = default)
		{
			DebugLog($"[{nameof(TitlePresenter)}] Started");
			var splashPageView = await titleTransitions.StartedAsync(cancellation);
			await splashPageView.WaitFinishedAsync(cancellation);
			var menuPageView = await titleTransitions.SplashFinishedAsync(cancellation);

			try
			{
				while (!cancellation.IsCancellationRequested)
				{
					var selected = await menuPageView.WaitMenuSelectedAsync(cancellation);
					DebugLog($"{selected} selected");
					switch (selected)
					{
						case TitleMenu.Start:
							break;
						case TitleMenu.Settings:
							break;
						case TitleMenu.Quit:
							break;
					}
					await UniTask.Yield(cancellation);
				}
			}
			catch (OperationCanceledException)
			{
				DebugLog("Canceled");
			}
			catch
			{
				throw;
			}
		}

		static void DebugLog(string message) => Debug.Log($"[{nameof(TitlePresenter)}] {message}");
	}
}
