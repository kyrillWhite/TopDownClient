using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Grpc.Net.Client;
using Grpc.Core;
using TopDownGrpcGameServer;
using TopDownLibrary;

namespace TopDownGrpcClient
{
    public static class Messages
    {
        static GrpcChannel _chanel;
        static TopDownServer.TopDownServerClient _client;
        static AsyncDuplexStreamingCall<ControlStateRequest, PlayerDataResponse> sendControllCall;
        public delegate void RetrieveUpdateDelegate(RetrieveUpdateEventArgs e);
        public delegate void PlayerDataDelegate(PlayerDataEventArgs e);
        public static event RetrieveUpdateDelegate RetrieveUpdateEvent;
        public static event PlayerDataDelegate PlayerDataEvent;

        public static bool CanUpdate { get; set; } = false;
        public static CancellationTokenSource CanUpdateToken = new CancellationTokenSource();

        public static void Initialize()
        {
            if (sendControllCall != null)
            {
                sendControllCall.Dispose();
            }
            if (_chanel != null)
            {
                _chanel.Dispose();
            }

            _chanel = GrpcChannel.ForAddress("http://26.104.61.15:5000");
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
                sendControllCall.RequestStream.WriteAsync(controlStateReq).Wait();
                sendControllCall.ResponseStream.MoveNext().Wait();
                var playerData = sendControllCall.ResponseStream.Current;
                if (playerData != null)
                {
                    PlayerDataEvent?.Invoke(new PlayerDataEventArgs()
                    {
                        LastId = playerData.LastInputId,
                        X = playerData.Position.X,
                        Y = playerData.Position.Y,
                        HpPercent = playerData.HpPercent,
                        ReloadPercent = playerData.ReloadPercent,
                        BulletsCount = playerData.BulletsCount,
                    });
                }
            }
        }

        public static async Task GetUpdate()
        {
            using var retrieveControlCall = _client.RetrieveUpdate(new Google.Protobuf.WellKnownTypes.Empty());
            await foreach (var message in retrieveControlCall.ResponseStream.ReadAllAsync())
            {
                if (!CanUpdate)
                    CanUpdateToken.Token.WaitHandle.WaitOne();

                RetrieveUpdateEvent?.Invoke(new RetrieveUpdateEventArgs()
                {
                    EntityPositions = message.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y, p.IsDead)).ToList(),
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
