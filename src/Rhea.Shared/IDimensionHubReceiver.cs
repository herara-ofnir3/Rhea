namespace Rhea.Shared
{
	public interface IDimensionHubReceiver
	{
		void OnJoined();

		void OnLeft();
	}
}
