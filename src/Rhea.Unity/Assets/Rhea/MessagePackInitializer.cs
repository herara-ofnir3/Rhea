using MessagePack;
using MessagePack.Unity;
using UnityEngine;

namespace Rhea.Unity
{
	public class MessagePackInitializer
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			MessagePackSerializer.DefaultOptions = 
				MessagePackSerializerOptions.Standard.WithResolver(UnityResolver.InstanceWithStandardResolver);
		}
	}
}
