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
        public delegate void RetrieveUpdateDelegate(RetrieveUpdateEventArgs e);
        public delegate void PlayerDataDelegate(PlayerDataEventArgs e);
        public static event RetrieveUpdateDelegate RetrieveUpdateEvent;
        public static event PlayerDataDelegate PlayerDataEvent;

        public static void Initialize()
        {
            var _chanel = GrpcChannel.ForAddress("http://26.202.152.148:5000");
            _client = new TopDownServer.TopDownServerClient(_chanel);
            sendControllCall = _client.UpdateUserState();
        }

        public static void SendControlState(Dictionary<int, Input> inputs, string playerId)
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
                    Id = playerId,
                };
                sendControllCall.RequestStream.WriteAsync(controlStateReq);
                sendControllCall.ResponseStream.MoveNext();
                var playerData = sendControllCall.ResponseStream.Current;
                if (playerData != null)
                {
                    PlayerDataEvent?.Invoke(new PlayerDataEventArgs()
                    {
                        LastId = playerData.LastInputId,
                        X = playerData.Position.X,
                        Y = playerData.Position.Y,
                    });
                }
            }
        }

        public static async Task GetUpdate()
        {
            using var retrieveControlCall = _client.RetrieveUpdate(new Google.Protobuf.WellKnownTypes.Empty());
            await foreach (var message in retrieveControlCall.ResponseStream.ReadAllAsync())
            {
                RetrieveUpdateEvent?.Invoke(new RetrieveUpdateEventArgs()
                {
                    EntityPositions = message.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y)).ToList(),
                    Bullets = message.Bullets.Select(b => new BulletData()
                    {
                        CreationTime = b.CreationTime.ToDateTime().ToLocalTime(),
                        StartPosX = b.StartPos.X,
                        StartPosY = b.StartPos.Y,
                        EndPosX = b.EndPos.X,
                        EndPosY = b.EndPos.Y,
                        Team = b.Team,
                        Speed = b.Speed,
                        Id = b.Id,
                    }).ToList(),
                });
            }
        }

        public static string GetMap()
        {
            var getMapCall = _client.GetMap(new Google.Protobuf.WellKnownTypes.Empty());
            return getMapCall.MapStr;
        }

        public static List<(string, int, float, float)> GetEntities()
        {
            var getMapCall = _client.GetEntities(new Google.Protobuf.WellKnownTypes.Empty());
            return getMapCall.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y)).ToList();
        }

        public static string GetPlayerId()
        {
            return _client.GetPlayerId(new Google.Protobuf.WellKnownTypes.Empty()).Id;
        }
    }
}
