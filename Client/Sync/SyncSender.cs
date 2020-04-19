using GTA;
using Lidgren.Network;
using RDR2;
using RDRNetwork.Utils;
using RDRNetworkShared;
using System;
using System.Linq;
using System.Threading;

namespace RDRNetwork.Sync
{
    internal static class SyncSender
    {
        private const int LIGHT_SYNC_RATE = 1500;
        private const int PURE_SYNC_RATE = 100;

        internal static void MainLoop()
        {
            bool lastPedData = false;
            int lastLightSyncSent = 0;

            while (true)
            {
                if (!Main.IsConnected())
                {
                    Thread.Sleep(100);
                    continue;
                }

                object lastPacket;
                lock (SyncCollector.Lock)
                {
                    lastPacket = SyncCollector.LastSyncPacket;
                    SyncCollector.LastSyncPacket = null;
                }

                if (lastPacket == null) continue;
                try
                {
                    var data = lastPacket as PedData;
                    if (data != null)
                    {
                        var bin = PacketOptimization.WritePureSync(data);

                        var msg = Main.Client.CreateMessage();
                        msg.Write((byte)PacketType.PedPureSync);
                        msg.Write(bin.Length);
                        msg.Write(bin);


                        Main.Client.SendMessage(msg, NetDeliveryMethod.UnreliableSequenced, (int)ConnectionChannel.PureSync);


                        if (!lastPedData || Environment.TickCount - lastLightSyncSent > LIGHT_SYNC_RATE)
                        {
                            lastLightSyncSent = Environment.TickCount;

                            var lightBin = PacketOptimization.WriteLightSync(data);

                            var lightMsg = Main.Client.CreateMessage();
                            lightMsg.Write((byte)PacketType.PedLightSync);
                            lightMsg.Write(lightBin.Length);
                            lightMsg.Write(lightBin);

                            Main.Client.SendMessage(lightMsg, NetDeliveryMethod.ReliableSequenced, (int)ConnectionChannel.LightSync);

                        }

                        lastPedData = true;
                    }
                    else
                    {
                        var bin = PacketOptimization.WritePureSync((VehicleData)lastPacket);

                        var msg = Main.Client.CreateMessage();
                        msg.Write((byte)PacketType.VehiclePureSync);
                        msg.Write(bin.Length);
                        msg.Write(bin);

                        Main.Client.SendMessage(msg, NetDeliveryMethod.UnreliableSequenced, (int)ConnectionChannel.PureSync);

                        if (lastPedData || Environment.TickCount - lastLightSyncSent > LIGHT_SYNC_RATE)
                        {
                            lastLightSyncSent = Environment.TickCount;

                            var lightBin = PacketOptimization.WriteLightSync((VehicleData)lastPacket);

                            var lightMsg = Main.Client.CreateMessage();
                            lightMsg.Write((byte)PacketType.VehicleLightSync);
                            lightMsg.Write(lightBin.Length);
                            lightMsg.Write(lightBin);

                            Main.Client.SendMessage(lightMsg, NetDeliveryMethod.ReliableSequenced, (int)ConnectionChannel.LightSync);
                        }

                        lastPedData = false;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, "SyncSender - MainLoop");
                }

                Thread.Sleep(PURE_SYNC_RATE);
            }
        }
    }

    public partial class SyncCollector : Script
    {
        internal static bool ForceAimData;
        internal static object LastSyncPacket;
        internal static object Lock = new object();

        public SyncCollector()
        {
            var t = new Thread(SyncSender.MainLoop) { IsBackground = true };
            t.Start();
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!Main.IsOnServer()) return;
            var player = Game.Player.Character;


            if (player.IsInVehicle())
            {
               // VehicleData(player);
            }
            else
            {
                PedData(player);
            }
        }
    }
}
