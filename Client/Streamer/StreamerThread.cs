using RDR2;
using RDRNetwork.Sync;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RDRNetwork
{
    // Besoin d'être complété
    internal class StreamerThread : Script
    {
        public static SyncPed[] SyncPeds;
        public static SyncPed[] StreamedInPlayers;
        public static RemoteVehicle[] StreamedInVehicles;

        private static List<IStreamedItem> _itemsToStreamIn;
        private static List<IStreamedItem> _itemsToStreamOut;

        public static float GeneralStreamingRange = 1000f;
        public static float VehicleStreamingRange = 350f;
        public static float PlayerStreamingRange = 200f;
        public static float LabelsStreamingRange = 25f;

        public static Stopwatch sw;
        public StreamerThread()
        {
            _itemsToStreamIn = new List<IStreamedItem>();
            _itemsToStreamOut = new List<IStreamedItem>();
            StreamedInPlayers = new SyncPed[MAX_PLAYERS];

            Tick += StreamerTick;

            var calcucationThread = new System.Threading.Thread(StreamerCalculationsThread) { IsBackground = true };
            calcucationThread.Start();
        }

        private void StreamerTick(object sender, EventArgs e)
        {
            
        }

        public const int MAX_PLAYERS = 250; //Max engine ped value: 256, on 236 it starts to cause issues
        public const int MAX_OBJECTS = 500; //Max engine value: 2500
        public const int MAX_VEHICLES = 60; //Max engine value: 64 +/ 1
        public const int MAX_PICKUPS = 50; //NEEDS A TEST
        public const int MAX_BLIPS = 50; //Max engine value: 1298
        public static int MAX_PEDS; //Share the Ped limit, prioritize the players
        public const int MAX_LABELS = MAX_PLAYERS; //NEEDS A TEST
        public const int MAX_MARKERS = 120; //Max engine value: 128
        public const int MAX_PARTICLES = 50;


        private static void StreamerCalculationsThread()
        {

        }
    }
}