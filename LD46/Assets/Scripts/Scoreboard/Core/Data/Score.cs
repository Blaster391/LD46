using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardCore.Data
{
    public class Score
    {
        public string Level { get; set; } = "";
        public string User { get; set; } = "";
        public int ScoreValue { get; set; } = 0;
        public DateTime SubmittedDateTime { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> ExtraData { get; set; } = new Dictionary<string, string>();
    }
}
