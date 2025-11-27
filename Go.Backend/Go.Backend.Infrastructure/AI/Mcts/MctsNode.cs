using System;
using System.Collections.Generic;
using System.Linq;

namespace Go.Backend.Infrastructure.AI.Mcts
{
    public class MctsNode
    {
        public MctsNode? Parent { get; }
        public List<MctsNode> Children { get; } = new List<MctsNode>();
        public int MoveIndex { get; } // 0-360, 361=Pass
        public int VisitCount { get; set; } = 0;
        public float TotalValue { get; set; } = 0;
        public float PriorProb { get; }

        public float MeanValue => VisitCount == 0 ? 0 : TotalValue / VisitCount;
        public bool IsLeaf => Children.Count == 0;

        public MctsNode(MctsNode? parent, int moveIndex, float priorProb)
        {
            Parent = parent;
            MoveIndex = moveIndex;
            PriorProb = priorProb;
        }

        public float CalculatePuctScore(int parentVisits)
        {
            // C_PUCT khuyến nghị là 1.0 đến 3.0
            const float C_PUCT = 1.5f; 
            return MeanValue + C_PUCT * PriorProb * (float)Math.Sqrt(parentVisits) / (1 + VisitCount);
        }

        public MctsNode SelectBestChild()
        {
            // Chọn con có điểm PUCT cao nhất
            return Children.OrderByDescending(c => c.CalculatePuctScore(this.VisitCount)).First();
        }
        
        public MctsNode GetMostVisitedChild()
        {
            // Chọn nước đi cuối cùng dựa trên số lần thăm
            return Children.OrderByDescending(c => c.VisitCount).First();
        }
    }
}