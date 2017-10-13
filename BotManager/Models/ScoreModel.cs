using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotManager.Models
{
    public class ScoreModel
    {
        public int OverallScore { set; get; }
        public int Credit { set; get; }
        public int CreditUsed { set; get; }
        public string Rank { set; get; }
        public int Invited { set; get; }
    }
}
