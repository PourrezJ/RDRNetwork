using RDRNetwork.Utils;

namespace RDRNetwork.Models
{
    public struct LocalHandle
    {
        public LocalHandle(int handle)
        {
            _internalId = handle;
            HandleType = HandleType.GameHandle;
        }

        public LocalHandle(int handle, HandleType localId)
        {
            _internalId = handle;
            HandleType = localId;
        }

        private int _internalId;

        public int Raw
        {
            get
            {
                return _internalId;
            }
        }

        public int Value
        {
            get
            {
                if (HandleType == HandleType.LocalHandle)
                {
                    return Streamer.NetToEntity(Streamer.NetToStreamedItem(_internalId, true))?.Handle ?? 0;
                }
                else if (HandleType == HandleType.NetHandle)
                {
                    return Streamer.NetToEntity(_internalId)?.Handle ?? 0;
                }

                return _internalId;
            }
        }

        public T Properties<T>()
        {
            if (HandleType == HandleType.LocalHandle)
                return (T)Streamer.NetToStreamedItem(_internalId, true);
            else if (HandleType == HandleType.NetHandle)
                return (T)Streamer.NetToStreamedItem(_internalId);
            else
                return (T)Streamer.EntityToStreamedItem(_internalId);
        }

        public HandleType HandleType;

        public override bool Equals(object obj)
        {
            return (obj as LocalHandle?)?.Value == Value;
        }

        public static bool operator ==(LocalHandle left, LocalHandle right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(LocalHandle left, LocalHandle right)
        {
            return left.Value != right.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool IsNull
        {
            get
            {
                return Value == 0;
            }
        }
    }
}
