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

        public static async void SendControlState(string playerId)
        {
            // if (  sendControllCall.GetStatus().StatusCode == )
            while (true)
            {
                if (GetPlayerInputsEvent == null) break;

                PlayerInputsEventArgs inputsEventArgs = new PlayerInputsEventArgs();
                GetPlayerInputsEvent.Invoke(inputsEventArgs);

                var controlStateReq = new ControlStateRequest();
                controlStateReq.PlayerMove.AddRange(inputsEventArgs.Inputs.OrderBy(x=>x.Key).Select(input => new PlayerMoveClient()
                {
                    DirX = input.Value.DirX,
                    DirY = input.Value.DirY,
                    GlobalMousePosX = input.Value.GlobalMousePosX,
                    GlobalMousePosY = input.Value.GlobalMousePosY,
                    LeftMouse = input.Value.LeftMouse,
                    RightMouse = input.Value.RightMouse,
                    InputId = input.Key,
                    Id = playerId,
                }));

                sendControllCall.RequestStream.WriteAsync(controlStateReq);

                if (await sendControllCall.ResponseStream.MoveNext())
                {
                    var gameStateResponse = sendControllCall.ResponseStream.Current;

                    foreach (var playerMoveServer in gameStateResponse.PlayerMoveServer)
                    {
                        PlayerDataEvent?.Invoke(new PlayerDataEventArgs()
                        {
                            LastId = playerMoveServer.LastInputId,
                            X = playerMoveServer.Position.X,
                            Y = playerMoveServer.Position.Y,
                        });
                    }
                    RetrieveEntitiesEvent?.Invoke(new RetrieveEntitiesEventArgs()
                    {
                        EntityPositions = gameStateResponse.Entities.Select(p => (p.Id, p.Team, p.Position.X, p.Position.Y)).ToList()
                    });
                }
            }
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
