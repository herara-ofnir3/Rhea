using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace Rhea
{
	public class MainPresenter : IAsyncStartable
	{
		public UniTask StartAsync(CancellationToken cancellation = default)
		{
			Debug.Log($"[{nameof(MainPresenter)}] Started");
			return UniTask.CompletedTask;
		}
	}
}
