using Lidgren.Network;
using ResuMPServer.Constant;
using RDRNetworkShared;

namespace ResuMPServer
{
    public partial class API
    {
        public void SendChatMessageToAll(string message)
        {
            foreach (var msg in message.Split('\n'))
                SendChatMessageToAll("", msg);
        }

        public void SendChatMessageToAll(string sender, string message)
        {
            var chatObj = new ChatData()
            {
                Sender = sender,
                Message = message,
            };

            Program.ServerInstance.SendToAll(chatObj, PacketType.ChatData, true, ConnectionChannel.Chat);
        }

        public void SendChatMessageToPlayer(Client player, string message)
        {
            foreach (var msg in message.Split('\n'))
                SendChatMessageToPlayer(player, "", msg);
        }

        public void SendChatMessageToPlayer(Client player, string sender, string message)
        {
            var chatObj = new ChatData()
            {
                Sender = sender,
                Message = message,
            };

            var data = GameServer.SerializeBinary(chatObj);

            NetOutgoingMessage msg = Program.ServerInstance.Server.CreateMessage();
            msg.Write((byte)PacketType.ChatData);
            msg.Write(data.Length);
            msg.Write(data);
            player.NetConnection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, (int)ConnectionChannel.Chat);
        }
    }
}
