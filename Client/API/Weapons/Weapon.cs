using RDR2.Native;
using Native = RDR2.Native;

namespace RDRNetwork.API
{
	public sealed class Weapon
	{
		private Ped Owner { get; }

		public WeaponHash Hash { get; } = WeaponHash.Unarmed;

		public Weapon()
		{
		}

		public Weapon(Ped owner, WeaponHash hash)
		{
			Owner = owner;
			Hash = hash;
		}

		public bool IsPresent => Hash == WeaponHash.Unarmed || Function.Call<bool>(Native.Hash.HAS_PED_GOT_WEAPON, Owner.Handle, (uint)Hash, false, false);

		public Model Model => GetModelByHash(Hash);

		public WeaponGroup Group => Function.Call<WeaponGroup>(Native.Hash.GET_WEAPONTYPE_GROUP, (uint)Hash);

		public AmmoType AmmoType => Function.Call<AmmoType>(Native.Hash.GET_PED_AMMO_TYPE_FROM_WEAPON, Owner.Handle, (uint)Hash);

		public int Ammo
		{
			get => Hash == WeaponHash.Unarmed
				? 1
				: Function.Call<int>(Native.Hash.GET_PED_AMMO_BY_TYPE, Owner.Handle, (uint)AmmoType);
			set => Function.Call(Native.Hash.SET_PED_AMMO_BY_TYPE, Owner.Handle, (uint)AmmoType, value);
		}

		public int AmmoInClip
		{
			get {
				if (Hash == WeaponHash.Unarmed)
				{
					return 1;
				}
				var ammo = new OutputArgument();
				if (Function.Call<bool>(Native.Hash.GET_AMMO_IN_CLIP, Owner.Handle, (uint)Hash, ammo))
				{
					return ammo.GetResult<int>();
				}
				return 0;
			}
			set {
				if (Hash == WeaponHash.Unarmed)
				{
					return;
				}
				if (IsPresent)
				{
					Function.Call(Native.Hash.SET_AMMO_IN_CLIP, Owner.Handle, (uint)Hash, value);
				}
				else
				{
					Owner.GiveWeapon(Hash, value);
				}
			}
		}

		public int MaxAmmo
		{
			get {
				if (Hash == WeaponHash.Unarmed || !IsPresent)
				{
					return 1;
				}

				var ammo = new OutputArgument();
				return Function.Call<bool>(Native.Hash.GET_MAX_AMMO, Owner.Handle, (uint)Hash, ammo) ? ammo.GetResult<int>() : 1;
			}
		}

		public int MaxAmmoInClip => Hash == WeaponHash.Unarmed ? 1 : Function.Call<int>(Native.Hash.GET_MAX_AMMO_IN_CLIP);

		public bool InfiniteAmmo
		{
			set => Function.Call(Native.Hash.SET_PED_INFINITE_AMMO, Owner.Handle, (uint)Hash, value);
		}

		public void Remove()
		{
			Function.Call(Native.Hash.REMOVE_WEAPON_FROM_PED, Owner.Handle, (uint)Hash, false, false);
		}

		public static Model GetModelByHash(WeaponHash hash)
		{
			return new Model(Function.Call<int>((Hash)0x865F36299079FB75, (uint)hash));
		}

		public static implicit operator WeaponHash(Weapon weapon)
		{
			return weapon.Hash;
		}
	}	
}
