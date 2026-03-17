using Cysharp.Threading.Tasks;
using System.Threading;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Rhea.Unity
{
	public static class PageContainerExtensions
	{
		public async static UniTask<PagePushed<TPage>> PushAsync<TPage>(
			this PageContainer container,
			string resourceKey,
			bool playAnimation,
			bool stack = true,
			string pageId = null,
			bool loadAsync = true,
			CancellationToken cancellation = default)
			where TPage : Page
		{
			var tcs = new UniTaskCompletionSource<PagePushed<TPage>>();
			using var _ = cancellation.Register(() => tcs.TrySetCanceled(cancellation));
			var handle = container.Push<TPage>(
				resourceKey,
				playAnimation,
				stack,
				pageId,
				loadAsync,
				onLoad: loaded =>
				{
					tcs.TrySetResult(new PagePushed<TPage>(loaded.pageId, loaded.page));
				});
			return await tcs.Task;
		}

		public async static UniTask PopAsync(
			this PageContainer container,
			bool playAnimation,
			int popCount = 1,
			CancellationToken cancellation = default)
		{
			var handle = container.Pop(playAnimation, popCount);
			await handle.Task;
		}
	}

	public record PagePushed<TPage>(string Id, TPage Page);
}
