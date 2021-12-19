using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using Grpc.Core;
using TopDownGrpcClient.EventArgs;
using TopDownGrpcGameServer;
using TopDownLibrary;

namespace TopDownGrpcClient
{
    public static class Messages
    {
        static TopDownServer.TopDownServerClient _client;
        static AsyncDuplexStreamingCall<ControlStateRequest, GameStateResponse> sendControllCall;

        public delegate void RetrieveEntitiesDelegate(RetrieveEntitiesEventArgs e);
        public delegate void PlayerDataDelegate(PlayerDataEventArgs e);
        public delegate void GetPlayerInputsDelegate(PlayerInputsEventArgs e);

        public static event RetrieveEntitiesDelegate RetrieveEntitiesEvent;
        public static event PlayerDataDelegate PlayerDataEvent;
        public static event GetPlayerInputsDelegate GetPlayerInputsEvent;

        public static void Initialize()
        {
            var _chanel = GrpcChannel.ForAddress("http://26.104.61.15:5000");
            _client = new TopDownServer.TopDownServerClient(_chanel);
            sendControllCall = _client.UpdateUserState();
        }

        public static void SendControlState(List<Input> inputs, string playerId)
        {
            // if (  sendControllCall.GetStatus().StatusCode == )

            var controlStateRequest = new ControlStateRequest()
            {
                PlayerMove = new PlayerMoveClient()
                {
                    DirX = inputs.Last().DirX,
                    DirY = inputs.Last().DirY,
                    GlobalMousePosX = inputs.Last().GlobalMousePosX,
                    GlobalMousePosY = inputs.Last().GlobalMousePosY,
                    LeftMouse = inputs.Last().LeftMouse,
                    RightMouse = inputs.Last().RightMouse,
                    Id = playerId,
                    MsDuration = inputs.Last().SimulationTime,
                    Time = inputs.Last().Time,
                }
            };

            Task.Run(() =>
            {
                lock (sendControllCall)
                {
                    sendControllCall.RequestStream.WriteAsync(controlStateRequest).Wait();

                    sendControllCall.ResponseStream.MoveNext().Wait();

                    var gameStateResponse = sendControllCall.ResponseStream.Current;

                    PlayerDataEvent?.Invoke(new PlayerDataEventArgs()
                    {
                        Time = gameStateResponse.PlayerServerPosition.Time,
                        X = gameStateResponse.PlayerServerPosition.Position.X,
                        Y = gameStateResponse.PlayerServerPosition.Position.Y,
                    });

                    RetrieveEntitiesEvent?.Invoke(new RetrieveEntitiesEventArgs()
                    {
                        EntityPositions = gameStateResponse.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y)).ToList()
                    });
                }
            });
        }

        //public static async Task GetEntityPositions()
        //{
        //    using var retrieveControlCall = _client.RetrieveEntities(new Google.Protobuf.WellKnownTypes.Empty());
        //    await foreach (var message in retrieveControlCall.ResponseStream.ReadAllAsync())
        //    {
        //        RetrieveEntitiesEvent?.Invoke(new RetrieveEntitiesEventArgs()
        //        {
        //            EntityPositions = message.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y)).ToList()
        //        });
        //    }
        //}

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
