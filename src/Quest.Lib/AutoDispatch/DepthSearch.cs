////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Lib.AutoDispatch

{
    public class DepthSearch
    {

        public void Test()
        {
            Random r = new Random(DateTime.Now.Millisecond);

            TestBoard node = new TestBoard() { Name = "0", cx = r.NextDouble() * 100.0, cy = r.NextDouble() * 100.0, tx = r.NextDouble() * 100.0, ty = r.NextDouble() * 100.0 };
            node.GenerationLimit = 2;

            // CREATE A DEPTH-FIRST QUERY
            IEnumerable<TestBoard> result = node.AsDepthFirstEnumerable(n => n.Children);

            // work out which element returns the best position
            var maxHeight = result.AsParallel().Aggregate((agg, next) => next.Value > agg.Value ? next : agg);
     
            Console.WriteLine(maxHeight.Name);
        }
    }

    class TestMove 
    {
        public double dx;
        public double dy;
    }

    class TestBoard
    {
        public double cx, cy;
        public double tx, ty;

        private int _generation;
        private int _generationLimit;
        private string _name;

        private readonly List<TestBoard> _children = new List<TestBoard>();

        public IEnumerable<TestBoard> Children
        {
            get
            {
                AddChildBoards();
                return _children;
            }
        }

        public List<TestMove> CalcPossibleMoves()
        {
            Random r = new Random(DateTime.Now.Millisecond);
            List<TestMove> moves = new List<TestMove>();
            for (int i = 0; i < 50; i++)
                moves.Add(new TestMove() { dx = r.NextDouble() - 0.5, dy = r.NextDouble() - 0.5 });
            return moves;
        }

        public void ApplyMove(TestMove m)
        {
            TestMove mv = (TestMove)m;
            cx += mv.dx;
            cy += mv.dy;
        }

        public double Value
        {
            get
            {
                double dx = cx - tx;
                double dy = cy - ty;
                return Math.Sqrt(dx * dx + dy * dy);
            }
            set
            { }
        }

        public bool LimitReached()
        {
            return Value < 10;
        }

        public object Clone()
        {
            return new TestBoard() { cx = cx, cy = cy, tx = tx, ty = ty, GenerationLimit = _generationLimit };
        }

        public String Name
        {
            get
            {

                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public int Generation
        {
            get
            {
                return _generation;
            }
            set
            {
                _generation = value;
            }
        }

        public int GenerationLimit
        {
            get
            {
                return _generationLimit;
            }
            set
            {
                _generationLimit = value;
            }
        }

        /// <summary>
        /// general strategy is to take a snapshot of the board and then work out which moves are possible.
        /// for each move, build another board, move the peice and then predict what will happen in n minutes.
        /// the process is then repeated
        /// </summary>
        public void AddChildBoards()
        {
            String name = Generation.ToString();

            // Console.WriteLine(">> Generation {0} Value={1}", Name, Value);

            if (!LimitReached() && Generation < GenerationLimit)
            {
                // haven't reached the right depth.. find best child
                // calculate a list of posible moves
                List<TestMove> moves = CalcPossibleMoves();

                int movenum = 0;
                // apply each move in turn and figure out the best one
                foreach (TestMove m in moves)
                {
                    movenum++;
                    TestBoard childBoard = (TestBoard)Clone();
                    childBoard.Generation = Generation + 1;
                    childBoard.Name = Name + "." + movenum.ToString();
                    // apply the move
                    childBoard.ApplyMove(m);

                    _children.Add(childBoard);
                }
            }
        }
    }

    public static class TreeToEnumerableEx
    {
        public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc)
        {
            yield return head;
            foreach (var node in childrenFunc(head))
            {
                foreach (var child in AsDepthFirstEnumerable(node, childrenFunc))
                {
                    yield return child;
                }
            }
        }

        public static IEnumerable<T> AsBreadthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc)
        {
            yield return head;
            var last = head;
            foreach (var node in AsBreadthFirstEnumerable(head, childrenFunc))
            {
                foreach (var child in childrenFunc(node))
                {
                    yield return child;
                    last = child;
                }
                if (last.Equals(node)) yield break;
            }
        }

    }

}
