using RDR2.Native;
using Native = RDR2.Native;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RDRNetwork.API
{
	public class PedGroup : PoolObject, IEnumerable<Ped>, IDisposable
	{
		public class enumerator : IEnumerator<Ped>
		{
			#region Fields
			int index;
			readonly PedGroup group;
			#endregion

			public enumerator(PedGroup group)
			{
				index = -2;
				this.group = group;
			}

			public virtual Ped Current
			{
				get; private set;
			}

			public object Current2 => Current;

			object IEnumerator.Current => Current;

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			protected virtual void Dispose(bool disposing)
			{
			}

			public virtual void Reset()
			{
			}

			public virtual bool MoveNext()
			{
				if (index++ < (group.MemberCount - 1))
				{
					Current = index < 0 ? group.Leader : group.GetMember(index);

					if (Entity.Exists(Current))
					{
						return true;
					}

					return MoveNext();
				}

				return false;
			}
		}

		public PedGroup() : this(Function.Call<int>(Hash.CREATE_GROUP, 0))
		{
		}
		public PedGroup(int handle) : base(handle)
		{
			Handle = handle;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Function.Call(Native.Hash.REMOVE_GROUP, Handle);
			}
		}


		public int MemberCount
		{
			get
			{
				int count, val1;
				unsafe
				{
					Function.Call(Hash.GET_GROUP_SIZE, Handle, &val1, &count);
				}
				return count;
			}
		}

		public float SeparationRange
		{
			set => Function.Call(Hash.SET_GROUP_SEPARATION_RANGE, Handle, value);
		}

		public FormationType FormationType
		{
			set => Function.Call(Hash.SET_GROUP_FORMATION, Handle, (int)value);
		}

		public void Add(Ped ped, bool leader)
		{
			Function.Call(leader ? Hash.SET_PED_AS_GROUP_LEADER : Hash.SET_PED_AS_GROUP_MEMBER, ped.Handle, Handle);
		}
		public void Remove(Ped ped)
		{
			Function.Call(Hash.REMOVE_PED_FROM_GROUP, ped.Handle);
		}
		public bool Contains(Ped ped)
		{
			return Function.Call<bool>(Hash.IS_PED_GROUP_MEMBER, ped.Handle, Handle);
		}

		public Ped Leader => Function.Call<Ped>(Hash.GET_PED_AS_GROUP_LEADER, Handle);

		public Ped GetMember(int index)
		{
			return Function.Call<Ped>(Hash.GET_PED_AS_GROUP_MEMBER, Handle, index);
		}

		public Ped[] ToArray(bool includingLeader)
		{
			return ToList(includingLeader).ToArray();
		}

		public List<Ped> ToList(bool includingLeader)
		{
			var result = new List<Ped>();

			if (includingLeader)
			{
				Ped leader = Leader;

				if (Entity.Exists(leader))
				{
					result.Add(leader);
				}
			}

			for (int i = 0; i < MemberCount; i++)
			{
				Ped member = GetMember(i);

				if (Entity.Exists(member))
				{
					result.Add(member);
				}
			}

			return result;
		}
		public override void Delete()
		{
			Function.Call(Hash.REMOVE_GROUP, Handle);
		}

		public override bool Exists()
		{
			return Function.Call<bool>(Hash.DOES_GROUP_EXIST, Handle);
		}
		public static bool Exists(PedGroup pedGroup)
		{
			return pedGroup != null && pedGroup.Exists();
		}

		public bool Equals(PedGroup obj)
		{
			return !(obj is null) && Handle == obj.Handle;
		}
		public override bool Equals(object obj)
		{
			return !(obj is null) && obj.GetType() == GetType() && Equals((PedGroup)obj);
		}

		public static bool operator ==(PedGroup left, PedGroup right)
		{
			return left is null ? right is null : left.Equals(right);
		}
		public static bool operator !=(PedGroup left, PedGroup right)
		{
			return !(left == right);
		}

		public override int GetHashCode()
		{
			return Handle.GetHashCode();
		}

		public IEnumerator GetEnumerator2()
		{
			return GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public virtual IEnumerator<Ped> GetEnumerator()
		{
			return new enumerator(this);
		}
	}
}
