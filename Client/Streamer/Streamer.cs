using RDR2;
using RDRNetwork.Misc;
using RDRNetwork.Sync;
using RDRNetworkShared;
using System;
using System.Linq;
using EntityType = RDRNetworkShared.EntityType;

namespace RDRNetwork
{
    internal static partial class Streamer
    {
        #region Fields
        internal static BiDictionary<int, int> HandleMap;
        internal static BiDictionary<int, IStreamedItem> ClientMap; // Global, IStreamedItem

        internal static WorldProperties ServerWorld;
        internal static RemotePlayer LocalCharacter;
        #endregion

        #region C4tor
        internal static void Init()
        {
            ClientMap = new BiDictionary<int, IStreamedItem>();
            HandleMap = new BiDictionary<int, int>();
        }
        #endregion

        #region Methods
        internal static void StreamIn(IStreamedItem item)
        {
            if (item.StreamedIn) 
                return;

            if (item.Dimension != Main.LocalDimension && item.Dimension != 0)
                return;

            switch ((EntityType)item.EntityType)
            {
                case EntityType.Player:
                    {
                        
                        var ped = item as SyncPed;
                        if (ped != null)
                        {
                            ped.StreamedIn = true;
                            //JavascriptHook.InvokeStreamInEvent(new LocalHandle(ped.LocalHandle), (int)EntityType.Player);
                        }
                    }
                    break;
            }

            var handleable = item as ILocalHandleable;
            if (handleable != null)
            {
                var han = handleable;
                if (han.LocalHandle == 0) return;

                lock (HandleMap)
                {
                    if (HandleMap.ContainsKey(item.RemoteHandle))
                    {
                        HandleMap[item.RemoteHandle] = han.LocalHandle;
                    }
                    else
                    {
                        HandleMap.Add(item.RemoteHandle, han.LocalHandle);
                    }
                }
            }
        }

        internal static void StreamOut(IStreamedItem item)
        {
            if (item == null) return;
            if (!item.StreamedIn) return;

            switch ((EntityType)item.EntityType)
            {
              
                case EntityType.Player:
                    var ped = item as SyncPed;
                    if (ped != null)
                    {
                        //JavascriptHook.InvokeStreamOutEvent(new LocalHandle(ped.Character?.Handle ?? 0), (int)EntityType.Player);
                        ped.Clear();
                    }
                    break;
            }

            item.StreamedIn = false;

            lock (HandleMap)
            {
                if (HandleMap.ContainsKey(item.RemoteHandle))
                {
                    HandleMap.Remove(item.RemoteHandle);
                }
            }

        }

        internal static IStreamedItem NetToStreamedItem(int netId, bool local = false, bool useGameHandle = false)
        {
            if (!useGameHandle)
            {
                lock (ClientMap)
                {
                    return ClientMap.ContainsKey(netId) ? ClientMap[netId] : null;
                }
            }
            else
            {
                lock (ClientMap)
                {
                    if (HandleMap.Reverse.ContainsKey(netId))
                    {
                        int remId = HandleMap.Reverse[netId];
                        if (ClientMap.ContainsKey(remId))
                            return ClientMap[remId];
                    }

                    if (netId == Game.Player.Character.Handle)
                    {
                        netId = -2;
                        if (HandleMap.Reverse.ContainsKey(-2) && ClientMap.ContainsKey(HandleMap.Reverse[-2]))
                            return ClientMap[HandleMap.Reverse[-2]];
                    }

                    //return ClientMap.OfType<ILocalHandleable>().FirstOrDefault(item => item.LocalHandle == netId) as IStreamedItem;
                    return null;
                }
            }
        }

        #region Handling
        internal static IStreamedItem EntityToStreamedItem(int gameHandle)
        {
            return NetToStreamedItem(gameHandle, useGameHandle: true);
        }

        internal static void AddLocalCharacter(int nethandle)
        {
            LocalCharacter = new RemotePlayer() { LocalHandle = -2, RemoteHandle = nethandle, StreamedIn = true };
            lock (ClientMap)
            {
                ClientMap.Add(nethandle, LocalCharacter);
                HandleMap.Add(nethandle, -2);
            }
        }

        internal static Entity NetToEntity(int netId)
        {
            lock (ClientMap)
            {
                var streamedItem = NetToStreamedItem(netId);
                if (streamedItem == null || !streamedItem.StreamedIn) return null;
                var handleable = streamedItem as ILocalHandleable;
                if (handleable == null) return new Prop(netId);
                if (handleable.LocalHandle == -2) return Game.Player.Character;
                return new Prop(handleable.LocalHandle);
            }
        }

        internal static Entity NetToEntity(IStreamedItem netId)
        {
            lock (ClientMap)
            {
                var handleable = netId as ILocalHandleable;
                if (handleable == null || netId == null) return new Prop(netId?.RemoteHandle ?? 0);
                if (handleable.LocalHandle == -2) return Game.Player.Character;
                return new Prop(handleable.LocalHandle);
            }
        }

        internal static bool ContainsLocalHandle(int localHandle)
        {
            return NetToStreamedItem(localHandle, useGameHandle: true) != null;
        }

        internal static bool ContainsLocalOnlyNetHandle(int localHandle)
        {
            return NetToStreamedItem(localHandle, true) != null;
        }

        internal static int EntityToNet(int entityHandle)
        {
            if (entityHandle == 0) return 0;
            if (entityHandle == Game.Player.Character.Handle)
                return HandleMap.Reverse[-2];
            lock (ClientMap)
            {
                if (HandleMap.Reverse.ContainsKey(entityHandle))
                    return HandleMap.Reverse[entityHandle];

                return entityHandle;
            }
        }

        internal static void Remove(IStreamedItem item)
        {
            lock (ClientMap)
            {
                if (item != null)
                {
                    ClientMap.Remove(item.RemoteHandle);
                    HandleMap.Remove(item.RemoteHandle);
                }
            }
        }

        internal static void RemoveByNetHandle(int netHandle)
        {
            lock (ClientMap)
            {
                Remove(NetToStreamedItem(netHandle));
            }
        }
        #endregion

        internal static int Count(Type type) 
            => ClientMap.Count(item => item.GetType() == type);

        internal static SyncPed GetPlayer(int netHandle)
        {
            SyncPed rem = NetToStreamedItem(netHandle) as SyncPed;
            if (rem != null) return rem;

            rem = new SyncPed()
            {
                RemoteHandle = netHandle,
                EntityType = (byte)EntityType.Player,
                StreamedIn = false, // change me
                LocalOnly = false,

                BlipSprite = -1,
                BlipColor = -1,
                BlipAlpha = 255,
                Alpha = 255,
                Team = -1,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static bool IsLocalPlayer(IStreamedItem item)
        {
            if (item == null) return false;
            return NetToEntity(item.RemoteHandle)?.Handle == Game.Player.Character.Handle;
        }

        #endregion
    }
}
