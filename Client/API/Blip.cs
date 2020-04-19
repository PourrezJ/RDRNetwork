using RDR2.Native;
using System;

namespace RDRNetwork.API
{
	public sealed class Blip : PoolObject, IEquatable<Blip>
	{
		public Blip( int handle ) : base( handle ) {

		}

		public BlipSprite Sprite
		{
			set => Function.Call( Hash.SET_BLIP_SPRITE, Handle, (uint)value );
		}
		
		public void ModifierAdd(BlipModifier modifier)
		{
			 Function.Call((Hash)0x662D364ABF16DE2F, Handle, (uint)modifier);
		}
		
		public void ModifierRemove(BlipModifier modifier)
		{
			Function.Call(Hash._SET_BLIP_FLASH_STYLE, Handle, (uint)modifier);
		}
		
		public Vector2 Scale
		{
			set => Function.Call( Hash.SET_BLIP_SCALE, Handle, value.X, value.Y );
		}

		public string Label
		{
			set => Function.Call(Hash._SET_BLIP_NAME_FROM_PLAYER_STRING, Handle,
				Function.Call<string>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", value));
		}

		public bool IsFlashing
		{
			set => Function.Call( Hash.SET_BLIP_FLASHES, Handle, value, 2 );
		}

		public bool IsOnMinimap => Function.Call<bool>( Hash.IS_BLIP_ON_MINIMAP, Handle );

		public override bool Exists() {
			return Function.Call<bool>( Hash.DOES_BLIP_EXIST, Handle );
		}

		public override void Delete() {
			unsafe
			{
				int ptr = Handle;
				Function.Call(Hash.REMOVE_BLIP, &ptr);
			}
		}

		public bool Equals( Blip other ) {
			return !ReferenceEquals( null, other ) && other.Handle == Handle;
		}

		public override bool Equals( object obj ) {
			return ReferenceEquals( this, obj ) || obj is Blip other && Equals( other );
		}

		public override int GetHashCode() {
			return Handle.GetHashCode();
		}
	}
}

