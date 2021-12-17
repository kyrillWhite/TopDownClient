using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using TopDownGrpcGameServer;

namespace TopDownGrpcClient
{
    public static class Messages
    {
        static TopDownServer.TopDownServerClient _client;
        static Grpc.Core.AsyncClientStreamingCall<ControlStateRequest, Google.Protobuf.WellKnownTypes.Empty> sendControllCall;
        public static void Initialize()
        {
            var _chanel = GrpcChannel.ForAddress("http://localhost:5000");
            _client = new TopDownServer.TopDownServerClient(_chanel);
            sendControllCall = _client.UpdateUserState();
        }

        public static void SendControlState(
            bool left,
            bool right,
            bool up,
            bool down,
            float globalMousePosX,
            float globalMousePosY,
            bool leftMouse,
            bool rightMouse)
        {
            var controlStateReq = new ControlStateRequest()
            {
                Left = left,
                Right = right,
                Up = up,
                Down = down,
                GlobalMousePosX = globalMousePosX,
                GlobalMousePosY = globalMousePosY,
                LeftMouse = leftMouse,
                RightMouse = rightMouse,
            };
            sendControllCall.RequestStream.WriteAsync(controlStateReq);
        }

        public static List<(float, float)> GetEntityPositions()
        {
            var entitiyPositions = _client.RetrieveEntites(new Google.Protobuf.WellKnownTypes.Empty());
            return entitiyPositions.Vectors.Select(p => (p.X, p.Y)).ToList();
        }
    }
}
