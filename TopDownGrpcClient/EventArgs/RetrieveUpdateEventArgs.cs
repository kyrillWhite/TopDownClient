using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownLibrary;

namespace TopDownGrpcClient
{
    public class RetrieveUpdateEventArgs : EventArgs
    {
        public List<(string, int, float, float, bool)> EntityPositions { get; set; }
        public List<BulletData> Bullets { get; set; }
        public int FirstTeamScore { get; set; }
        public int SecondTeamScore { get; set; }
        public int CurrentRound { get; set; }
        public bool IsEndGame { get; set; }
        public TimeSpan RoundTimeLeft { get; set; }
    }
}
