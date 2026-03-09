namespace Rhea.Shared
{
	public interface IShardHubReceiver
	{
		void OnJoined();

		void OnLeft();
	}
}
