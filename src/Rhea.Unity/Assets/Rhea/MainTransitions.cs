using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Rhea.Unity
{
	public interface IMainTransitions
	{
		UniTask<IMainWorldView> StartedAsync(CancellationToken cancellationToken = default);
	}

	public class MainTransitions : MonoBehaviour, IMainTransitions
	{
		public PageContainer pageContainer;
		public MainWorldView mainWorld;

		public UniTask<IMainWorldView> StartedAsync(CancellationToken cancellationToken = default)
		{
			return UniTask.FromResult((IMainWorldView)mainWorld);
		}
	}
}
