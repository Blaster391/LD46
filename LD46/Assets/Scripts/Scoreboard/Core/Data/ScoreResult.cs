using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardCore.Data
{
    public class ScoreResult
    {
        public int Ranking { get; set; } = -1;
        public Score Score { get; set; } = new Score();
    }
}
