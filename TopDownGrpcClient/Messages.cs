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
        public static string ServerAddress;
        public static int ServerPort;

        public static Exception Exception { get; set; } = null;

        public static bool CanUpdate { get; set; } = false;
        public static CancellationTokenSource CanUpdateToken = new CancellationTokenSource();

        public static void Initialize()
        {
            Close();

            Exception = null;
            if (ServerPort == 0)
            {
                Exception = new Exception("No ServerPort");
                return;
            }
            if (string.IsNullOrEmpty(ServerAddress))
            {
                Exception = new Exception("No ServerAddress");
                return;
            }
            _chanel = GrpcChannel.ForAddress($"http://{ServerAddress}:{ServerPort}");
            _client = new TopDownServer.TopDownServerClient(_chanel);
            sendControllCall = _client.UpdateUserState();
        }

        public static void Close()
        {
            //if (sendControllCall != null)
            //{
            //    sendControllCall.Dispose();
            //}
            if (_chanel != null)
            {
                _chanel.Dispose();
            }
        }

        public static void SendControlState(Dictionary<int, Input> inputs, string playerId)
        {
            if (Exception != null) return;

            try
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
                            HpPercent = playerData.HpPercent,
                            ReloadPercent = playerData.ReloadPercent,
                            BulletsCount = playerData.BulletsCount,
                            Capacity = playerData.Capacity,
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Exception = e;
                Close();
            }
        }

        public static async Task GetUpdate(string playerId)
        {
            try
            {
                using var retrieveControlCall = _client.RetrieveUpdate(new PlayerId() { Id = playerId });
                await foreach (var message in retrieveControlCall.ResponseStream.ReadAllAsync())
                {
                    try
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
                            FirstTeamScore = message.RoundData.FirstTeamScore,
                            SecondTeamScore = message.RoundData.SecondTeamScore,
                            CurrentRound = message.RoundData.CurrentRound,
                            IsEndGame = message.RoundData.IsEndGame,
                            RoundTimeLeft = message.RoundData.RoundTimeLeft.ToTimeSpan(),
                        });
                    }
                    catch (Exception e)
                    {
                        Exception = e;
                        Close();
                    }
                }
            }
            catch (Exception e)
            {
                Exception = e;
                Close();
            }
        }

        public static string GetMap()
        {
            var getMapCall = _client.GetMap(new Google.Protobuf.WellKnownTypes.Empty());
            return getMapCall.MapStr;
        }

        public static List<(string, int, float, float)> GetEntities(string playerId)
        {
            var getMapCall = _client.GetEntities(new PlayerId() { Id = playerId });
            return getMapCall.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y)).ToList();
        }

        public static string GetPlayerId()
        {
            return _client.GetPlayerId(new Google.Protobuf.WellKnownTypes.Empty()).Id;
        }

        public static void SendGun(string playerId, int type)
        {
            _client.SendGunType(new GunType() { PlayerId = playerId, Type = type });
        }
    }
}
