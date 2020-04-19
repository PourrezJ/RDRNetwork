using RDR2.Native;
using Native = RDR2.Native;
using System;

namespace RDRNetwork.API
{
	public abstract class Entity : PoolObject, ISpatial
	{
		internal Entity(int handle) : base(handle)
		{
		}

		/// <summary>
		/// Creates a new instance of an <see cref="Entity"/> from the given handle.
		/// </summary>
		/// <param name="handle">The entity handle.</param>
		/// <returns>Returns a <see cref="Ped"/> if this handle corresponds to a Ped.
		/// Returns a <see cref="Vehicle"/> if this handle corresponds to a Vehicle.
		/// Returns a <see cref="Prop"/> if this handle corresponds to a Prop.
		/// Returns <c>null</c> if no <see cref="Entity"/> exists this the specified <paramref name="handle"/></returns>
		public static Entity FromHandle(int handle)
		{
			switch ((EntityType)Function.Call<int>(Hash.GET_ENTITY_TYPE, handle))
			{
				case EntityType.Ped:
					return new Ped(handle);
				case EntityType.Vehicle:
					return new Vehicle(handle);
				case EntityType.Prop:
					return new Prop(handle);
			}
			return null;
		}



		public bool IsDead => Function.Call<bool>(Hash.IS_ENTITY_DEAD, Handle);
		public bool IsAlive => !IsDead;

		#region Styling

		public Model Model => new Model(Function.Call<int>(Hash.GET_ENTITY_MODEL, Handle));

		public int Alpha
		{
			get => Function.Call<int>(Hash.GET_ENTITY_ALPHA, Handle);
			set => Function.Call(Hash.SET_ENTITY_ALPHA, Handle, value, false);
		}

		public void ResetAlpha()
		{
			Function.Call(Hash.RESET_ENTITY_ALPHA, Handle);
		}

		#endregion

		#region Configuration
		public EntityType EntityType => Function.Call<EntityType>(Hash.GET_ENTITY_TYPE, Handle);

		public int LodDistance
		{
			get => Function.Call<int>(Hash.GET_ENTITY_LOD_DIST, Handle);
			set => Function.Call(Hash.SET_ENTITY_LOD_DIST, Handle, value);
		}

		public bool IsPersistent
		{
			get => Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, Handle);
			set => Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, Handle, value, !value);
		}

		public bool FreezePosition
		{
			set => Function.Call(Hash.FREEZE_ENTITY_POSITION, Handle, value);
		}

		public int GetBoneIndex(string boneName)
		{
			return Function.Call<int>(Hash.GET_ENTITY_BONE_INDEX_BY_NAME, Handle, boneName);
		}

		public bool HasBone(string boneName)
		{
			return GetBoneIndex(boneName) != -1;
		}


		#endregion

		#region Health

		public int Health
		{
			get => Function.Call<int>(Hash.GET_ENTITY_HEALTH, Handle);
            set => Function.Call(Hash._SET_ENTITY_HEALTH, Handle, value, 0);
		}

		public virtual int MaxHealth
		{
			get => Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Handle);
			set => Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Handle, value);
		}

		#endregion

		#region Positioning

		public virtual Vector3 Position
		{
			get => Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, Handle, true, 0);
			set => Function.Call(Hash.SET_ENTITY_COORDS, Handle, value.X, value.Y, value.Z, 0, 0, 0, 1);
		}

		public Vector3 PositionNoOffset
		{
			set => Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, Handle, value.X, value.Y, value.Z, 1, 1, 1);
		}

		public virtual Vector3 Rotation
		{
			get => Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION, Handle, 2);
			set => Function.Call(Hash.SET_ENTITY_ROTATION, Handle, value.X, value.Y, value.Z, 2, 1);
		}

		public float Heading
		{
			get => Function.Call<float>(Hash.GET_ENTITY_HEADING, Handle);
			set => Function.Call<float>(Hash.SET_ENTITY_HEADING, Handle, value);
		}

		public float HeightAboveGround => Function.Call<float>(Hash.GET_ENTITY_HEIGHT_ABOVE_GROUND, Handle);

		public Quaternion Quaternion
		{
			/*get
			{
				float x;
				float y;
				float z;
				float w;
				unsafe
				{
					Function.Call(Hash.GET_ENTITY_QUATERNION, Handle, &x, &y, &z, &w);
				}

				return new Quaternion(x, y, z, w);
			}*/ // waiting for GET_ENTITY_QUATERNOIN
			set => Function.Call(Hash.SET_ENTITY_QUATERNION, Handle, value.X, value.Y, value.Z, value.W);
		}

		public Vector3 UpVector
		{
			get => Vector3.Cross(RightVector, ForwardVector);
		}

		public Vector3 RightVector
		{
			get
			{
				const double D2R = 0.01745329251994329576923690768489;
				double num1 = System.Math.Cos(Rotation.Y * D2R);
				double x = num1 * System.Math.Cos(-Rotation.Z * D2R);
				double y = num1 * System.Math.Sin(Rotation.Z * D2R);
				double z = System.Math.Sin(-Rotation.Y * D2R);
				return new Vector3((float)x, (float)y, (float)z);
			}
		}

		public Vector3 ForwardVector
		{
			get => Function.Call<Vector3>(Hash.GET_ENTITY_FORWARD_VECTOR, Handle);
		}

		public Vector3 GetOffsetInWorldCoords(Vector3 offset)
		{
			return Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, Handle, offset.X, offset.Y, offset.Z);
		}

		public Vector3 GetOffsetFromWorldCoords(Vector3 worldCoords)
		{
			return Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, Handle, worldCoords.X, worldCoords.Y, worldCoords.Z);
		}


		public Vector3 Velocity
		{
			get => Function.Call<Vector3>(Hash.GET_ENTITY_VELOCITY, Handle);
			set => Function.Call(Hash.SET_ENTITY_VELOCITY, Handle, value.X, value.Y, value.Z);
		}

		#endregion

		#region Damaging

		public bool HasBeenDamagedBy(Entity entity)
		{
			return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, Handle, entity.Handle, 1, 1);
		}

		#endregion

		#region Invincibility

		// is***proof address's arent found yet
		/*public bool IsMeleeProof
		{
			get
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return false;
				}

				return RDR2DN.NativeMemory.IsBitSet(address + 392, 7);
			}
			set
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return;
				}

				if (value)
				{
					RDR2DN.NativeMemory.SetBit(address + 392, 7);
				}
				else
				{
					RDR2DN.NativeMemory.ClearBit(address + 392, 7);
				}
			}
		}

		public bool IsBulletProof
		{
			get
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return false;
				}

				return RDR2DN.NativeMemory.IsBitSet(address + 392, 4);
			}
			set
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return;
				}

				if (value)
				{
					RDR2DN.NativeMemory.SetBit(address + 392, 4);
				}
				else
				{
					RDR2DN.NativeMemory.ClearBit(address + 392, 4);
				}
			}
		}

		public bool IsExplosionProof
		{
			get
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return false;
				}

				return RDR2DN.NativeMemory.IsBitSet(address + 392, 11);
			}
			set
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return;
				}

				if (value)
				{
					RDR2DN.NativeMemory.SetBit(address + 392, 11);
				}
				else
				{
					RDR2DN.NativeMemory.ClearBit(address + 392, 11);
				}
			}
		}

		public bool IsCollisionProof
		{
			get
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return false;
				}

				return RDR2DN.NativeMemory.IsBitSet(address + 392, 6);
			}
			set
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return;
				}

				if (value)
				{
					RDR2DN.NativeMemory.SetBit(address + 392, 6);
				}
				else
				{
					RDR2DN.NativeMemory.ClearBit(address + 392, 6);
				}
			}
		}*/

		public bool IsInvincible
		{
			set => Function.Call(Hash.SET_ENTITY_INVINCIBLE, Handle, value);
		}

		public bool IsOnlyDamagedByPlayer
		{
			set => Function.Call(Hash.SET_ENTITY_ONLY_DAMAGED_BY_PLAYER, Handle, value);
		}

		#endregion

		#region Status Effects

		public bool IsVisible
		{
			get => Function.Call<bool>(Hash.IS_ENTITY_VISIBLE, Handle);
			set => Function.Call(Hash.SET_ENTITY_VISIBLE, Handle, value);
		}

		public bool IsOccluded
		{
			get => Function.Call<bool>(Hash.IS_ENTITY_OCCLUDED, Handle);
		}

		public bool IsOnFire => Function.Call<bool>(Hash.IS_ENTITY_ON_FIRE, Handle);

		public bool IsOnScreen
		{
			get => Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, Handle);
		}

		public bool IsUpright => Function.Call<bool>(Hash.IS_ENTITY_UPRIGHT, Handle, 30.0f);

		public bool IsUpsideDown => Function.Call<bool>(Hash.IS_ENTITY_UPSIDEDOWN, Handle);

		public bool IsInAir => Function.Call<bool>(Hash.IS_ENTITY_IN_AIR, Handle, 0);

		public bool IsInWater => Function.Call<bool>(Hash.IS_ENTITY_IN_WATER, Handle);

		public bool HasGravity
		{
			set => Function.Call(Hash.SET_ENTITY_HAS_GRAVITY, Handle, value);
		}

		#endregion

		#region Collision

		/*public bool HasCollision
		{
			get
			{
				var address = RDR2DN.NativeMemory.GetEntityAddress(Handle);
				if (address == IntPtr.Zero)
				{
					return false;
				}

				return RDR2DN.NativeMemory.IsBitSet(address + 0x29, 1);
			}
			set => Function.Call(Hash.SET_ENTITY_COLLISION, Handle, value, false);
		}*/ // collision address unknown

		public bool HasCollidedWithAnything => Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, Handle);

		public void SetNoCollision(Entity entity, bool toggle)
		{
			Function.Call(Hash.SET_ENTITY_NO_COLLISION_ENTITY, Handle, entity.Handle, !toggle);
		}

		public bool IsInArea(Vector3 minBounds, Vector3 maxBounds)
		{
			return Function.Call<bool>(Hash.IS_ENTITY_IN_AREA, Handle, minBounds.X, minBounds.Y, minBounds.Z, maxBounds.X, maxBounds.Y, maxBounds.Z, true, true, 0);
		}
		public bool IsInArea(Vector3 pos1, Vector3 pos2, float angle)
		{
			return IsInAngledArea(pos1, pos2, angle);
		}
		public bool IsInAngledArea(Vector3 origin, Vector3 edge, float angle)
		{
			return Function.Call<bool>(Hash.IS_ENTITY_IN_ANGLED_AREA, Handle, origin.X, origin.Y, origin.Z, edge.X, edge.Y, edge.Z, angle, false, true, false);
		}

		public bool IsInRangeOf(Vector3 position, float distance)
		{
			return Vector3.Subtract(Position, position).Length() < distance;
		}

		public bool IsNearEntity(Entity entity, Vector3 distance)
		{
			return Function.Call<bool>(Hash.IS_ENTITY_AT_ENTITY, Handle, entity.Handle, distance.X, distance.Y, distance.Z, 0, 1, 0);
		}

		public bool IsTouching(Model model)
		{
			return Function.Call<bool>(Hash.IS_ENTITY_TOUCHING_MODEL, Handle, model.Hash);
		}
		public bool IsTouching(Entity entity)
		{
			return Function.Call<bool>(Hash.IS_ENTITY_TOUCHING_ENTITY, Handle, entity.Handle);
		}

		#endregion

		#region Blips
		
		public Blip AddBlip(BlipType blipType)
		{
			return Function.Call<Blip>(Hash._BLIP_ADD_FOR_ENTITY, (uint)blipType, Handle);
		}
		
		public Blip CurrentBlip => Function.Call<Blip>(Hash.GET_BLIP_FROM_ENTITY, Handle);

		#endregion

		#region Attaching

		public void Detach()
		{
			Function.Call(Hash.DETACH_ENTITY, Handle, 1, 1);
		}
		public void AttachTo(Entity entity, int boneIndex)
		{
			AttachTo(entity, boneIndex, Vector3.Zero, Vector3.Zero);
		}
		public void AttachTo(Entity entity, int boneIndex, Vector3 position, Vector3 rotation)
		{
			Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, Handle, entity.Handle, boneIndex, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z, 0, 0, 0, 0, 2, 1);
		}

		public bool IsAttached()
		{
			return Function.Call<bool>(Hash.IS_ENTITY_ATTACHED, Handle);
		}
		public bool IsAttachedTo(Entity entity)
		{
			return Function.Call<bool>(Hash.IS_ENTITY_ATTACHED_TO_ENTITY, Handle, entity.Handle);
		}

		public Entity GetEntityAttachedTo()
		{
			return Function.Call<Entity>(Hash.GET_ENTITY_ATTACHED_TO, Handle);
		}

		#endregion

		#region Forces

		public void ApplyForce(Vector3 direction)
		{
			ApplyForce(direction, Vector3.Zero, 1);
		}
		public void ApplyForce(Vector3 direction, Vector3 rotation)
		{
			ApplyForce(direction, rotation, 1);
		}
		public void ApplyForce(Vector3 direction, Vector3 rotation, int forceType)
		{
			Function.Call(Hash.APPLY_FORCE_TO_ENTITY, Handle, (int)forceType, direction.X, direction.Y, direction.Z, rotation.X, rotation.Y, rotation.Z, false, false, true, true, false, true);
		}
		public void ApplyForceRelative(Vector3 direction)
		{
			ApplyForceRelative(direction, Vector3.Zero, 1);
		}
		public void ApplyForceRelative(Vector3 direction, Vector3 rotation)
		{
			ApplyForceRelative(direction, Rotation, 1);
		}
		public void ApplyForceRelative(Vector3 direction, Vector3 rotation, int forceType)
		{
			Function.Call(Hash.APPLY_FORCE_TO_ENTITY, Handle, (int)forceType, direction.X, direction.Y, direction.Z, rotation.X, rotation.Y, rotation.Z, false, true, true, true, false, true);
		}

		#endregion

		public void MarkAsNoLongerNeeded()
		{
			int handle = Handle;
			Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, handle, false, true);
			unsafe
			{
				Function.Call(Hash.SET_ENTITY_AS_NO_LONGER_NEEDED, &handle);
			}
		}

		public override void Delete()
		{
			int handle = Handle;
			Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, handle, false, true);
			unsafe
			{
				Function.Call(Hash.DELETE_ENTITY, &handle);
			}
		}

		public override bool Exists()
		{
			return Function.Call<bool>(Hash.DOES_ENTITY_EXIST, Handle);
		}
		public static bool Exists(Entity entity)
		{
			return entity != null && entity.Exists();
		}

		public bool Equals(Entity obj)
		{
			return !(obj is null) && Handle == obj.Handle;
		}
		public override bool Equals(object obj)
		{
			return !(obj is null) && obj.GetType() == GetType() && Equals((Entity)obj);
		}

		public static bool operator ==(Entity left, Entity right)
		{
			return left is null ? right is null : left.Equals(right);
		}
		public static bool operator !=(Entity left, Entity right)
		{
			return !(left == right);
		}

		public override int GetHashCode()
		{
			return Handle.GetHashCode();
		}
	}
}
