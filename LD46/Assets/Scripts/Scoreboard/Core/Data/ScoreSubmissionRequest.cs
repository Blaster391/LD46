using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardCore.Data
{
    public class ScoreSubmissionRequest
    {
        public string Key { get; set; } = "";
        public Score Score { get; set; } = new Score();
    }
}
