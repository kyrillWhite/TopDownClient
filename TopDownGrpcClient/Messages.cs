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
        static Grpc.Core.AsyncClientStreamingCall<ControlStateRequest, Google.Protobuf.WellKnownTypes.Empty> sendControllCall;
        public delegate void RetrieveEntitiesDelegate(RetrieveEntitiesEventArgs e);
        public static event RetrieveEntitiesDelegate RetrieveEntitiesEvent;

        public static void Initialize()
        {
            var _chanel = GrpcChannel.ForAddress("http://26.104.61.15:5000");
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
                }
        }

        public static async Task GetEntityPositions()
        {
            using var retrieveControlCall = _client.RetrieveEntites(new Google.Protobuf.WellKnownTypes.Empty());
            await foreach (var message in retrieveControlCall.ResponseStream.ReadAllAsync())
            {
                RetrieveEntitiesEvent?.Invoke(new RetrieveEntitiesEventArgs() {
                    EntityPositions =  message.Vectors.Select(p => (p.LastInputId, p.X, p.Y)).ToList() 
                });
            }
        }
    }
}
