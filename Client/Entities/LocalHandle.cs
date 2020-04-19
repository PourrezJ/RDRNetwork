using System;

namespace RDRNetwork.Utils
{
    internal enum HandleType
    {
        GameHandle,
        LocalHandle,
        NetHandle,
    }

    internal struct LocalHandle
    {
        public LocalHandle(int handle)
        {
            _internalId = handle;
            HandleType = HandleType.GameHandle;
        }

        internal LocalHandle(int handle, HandleType localId)
        {
            _internalId = handle;
            HandleType = localId;
        }

        private int _internalId;

        internal int Raw => _internalId;

        internal int Value
        {
            get
            {
                switch (HandleType)
                {
                    case HandleType.LocalHandle:
                        //return Main.NetEntityHandler.NetToEntity(Main.NetEntityHandler.NetToStreamedItem(_internalId, true))?.Handle ?? 0;
                    case HandleType.NetHandle:
                        //return Main.NetEntityHandler.NetToEntity(_internalId)?.Handle ?? 0;
                    case HandleType.GameHandle:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return _internalId;
            }
        }

        internal T Properties<T>()
        {
            //if (HandleType == HandleType.LocalHandle)
            //   // return (T)Main.NetEntityHandler.NetToStreamedItem(_internalId, true);
            //else if (HandleType == HandleType.NetHandle)
            //    //return (T)Main.NetEntityHandler.NetToStreamedItem(_internalId);
            //else
            // return (T)Main.NetEntityHandler.EntityToStreamedItem(_internalId);
            return default(T);
        }

        internal HandleType HandleType;

        //internal override bool Equals(object obj)
        //{
        //    return (obj as LocalHandle?)?.Value == Value;
        //}

        //internal static bool operator ==(LocalHandle left, LocalHandle right)
        //{
        //    return left.Value == right.Value;
        //}

        //internal static bool operator !=(LocalHandle left, LocalHandle right)
        //{
        //    return left.Value != right.Value;
        //}

        //internal override int GetHashCode()
        //{
        //    return Value.GetHashCode();
        //}

        //internal override string ToString()
        //{
        //    return Value.ToString();
        //}

        internal bool IsNull => Value == 0;
    }
}
