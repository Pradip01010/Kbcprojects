using System;

namespace Kbcprojects.Models
{
    public class GameResultViewModel
    {
        public int Score { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
    }
}

