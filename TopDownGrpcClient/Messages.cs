using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grpc.Net.Client;
using Grpc.Core;
using TopDownGrpcGameServer;
using TopDownLibrary;

namespace TopDownGrpcClient
{
    public static class Messages
    {
        static TopDownServer.TopDownServerClient _client;
        static AsyncDuplexStreamingCall<ControlStateRequest, PlayerDataResponse> sendControllCall;
        public delegate void RetrieveEntitiesDelegate(RetrieveEntitiesEventArgs e);
        public delegate void PlayerDataDelegate(PlayerDataEventArgs e);
        public static event RetrieveEntitiesDelegate RetrieveEntitiesEvent;
        public static event PlayerDataDelegate PlayerDataEvent;

        public static void Initialize()
        {
            var _chanel = GrpcChannel.ForAddress("http://26.202.152.148:5000");
            _client = new TopDownServer.TopDownServerClient(_chanel);
            sendControllCall = _client.UpdateUserState();
        }

        public static void SendControlState(Dictionary<int, Input> inputs)
        {
            foreach (var input in inputs.ToList())
            {
                var controlStateReq = new ControlStateRequest()
                {
                    DirX = input.Value.DirX,
                    DirY = input.Value.DirY,
                    GlobalMousePosX = input.Value.GlobalMousePosX,
                    GlobalMousePosY = input.Value.GlobalMousePosY,
                    LeftMouse = input.Value.LeftMouse,
                    RightMouse = input.Value.RightMouse,
                    InputId = input.Key,
                };
                sendControllCall.RequestStream.WriteAsync(controlStateReq);
                sendControllCall.ResponseStream.MoveNext();
                var playerData = sendControllCall.ResponseStream.Current;
                PlayerDataEvent?.Invoke(new PlayerDataEventArgs() {
                    LastId = playerData.LastInputId,
                    X = playerData.Position.X,
                    Y = playerData.Position.Y,
                });
                
            }
        }

        public static async Task GetEntityPositions()
        {
            using var retrieveControlCall = _client.RetrieveEntities(new Google.Protobuf.WellKnownTypes.Empty());
            await foreach (var message in retrieveControlCall.ResponseStream.ReadAllAsync())
            {
                RetrieveEntitiesEvent?.Invoke(new RetrieveEntitiesEventArgs()
                {
                    EntityPositions = message.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y)).ToList()
                });
            }
        }

        public static string GetMap()
        {
            var getMapCall = _client.GetMap(new Google.Protobuf.WellKnownTypes.Empty());
            return getMapCall.MapStr;
        }
    }
}
