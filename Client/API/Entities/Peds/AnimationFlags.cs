using System;

namespace RDRNetwork.API
{
	[Flags]
	public enum AnimationFlags
	{
		None = 0,
		Loop = 1 << 0,
		StayInEndFrame = 1 << 1,
		UpperBodyOnly = 1 << 4,
		AllowRotation = 1 << 5,
		CancelableWithMovement = 1 << 7,
		RagdollOnCollision = 4194304
	}
}
