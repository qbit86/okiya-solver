using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using static Okiya.TryHelpers;

namespace Okiya;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
    private readonly Game<int[]> _game;
    private Node _currentNode;

    private Solver(Game<int[]> game, Node node)
    {
        Debug.Assert(game.IsValid);
        _game = game;
        _currentNode = node;
    }

    public static Solver Create(Game<int[]> game)
    {
        if (!game.IsValid)
            throw new ArgumentException("Game instance is not initialized.", nameof(game));

        return new(game, new());
    }

    public static Solver Create(Game<int[]> game, Node node)
    {
        if (!game.IsValid)
            throw new ArgumentException("Game instance is not initialized.", nameof(game));

        return new(game, node);
    }

    public bool TrySelectMove(out int move, out double score)
    {
        bool result = TrySelectMoveCore(out move, out double relativeScore);
        int sign = SolverHelpers.Sign(_currentNode.SideToMove);
        score = sign * relativeScore;
        return result;
    }

    private bool TryMakeMove(out int move, out double score)
    {
        bool result = TrySelectMoveCore(out move, out double relativeScore);
        int sign = SolverHelpers.Sign(_currentNode.SideToMove);
        score = sign * relativeScore;
        if (result)
            _currentNode = _game.MakeMoveUnchecked(_currentNode, move);

        return result;
    }

    public double MakeMoves(Span<int> moves, out int movesWritten)
    {
        movesWritten = 0;
        if (!TryMakeMove(out int firstMove, out double firstScore) || moves.IsEmpty)
            return firstScore;

        moves[movesWritten++] = firstMove;
        while (movesWritten < moves.Length && TryMakeMove(out int move, out double score))
        {
            Debug.Assert(firstScore.Equals(score));
            moves[movesWritten++] = move;
        }

        return firstScore;
    }

    public double MakeMoves<TCollection>(TCollection moves)
        where TCollection : ICollection<int>
    {
        if (moves is null)
            throw new ArgumentNullException(nameof(moves));

        if (!TryMakeMove(out int firstMove, out double firstScore))
            return firstScore;

        moves.Add(firstMove);
        while (TryMakeMove(out int move, out double score))
        {
            Debug.Assert(firstScore.Equals(score));
            moves.Add(move);
        }

        return firstScore;
    }

    private bool TrySelectMoveCore(out int move, out double score)
    {
        if (Game.IsTerminalRoot(_currentNode, out score))
            return None(-1, out move);

        int[] buffer = ArrayPool<int>.Shared.Rent(Constants.CardCount);
        try
        {
            int possibleMoveCount = _game.PopulatePossibleMoves(_currentNode, buffer.AsSpan());
            if (possibleMoveCount is 0)
            {
                score = -sbyte.MaxValue + _currentNode.GetTokenCount();
                return None(-1, out move);
            }

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            double bestScore = double.NegativeInfinity;
            int bestMove = -1;
            foreach (int moveCandidate in possibleMoves)
            {
                Node child = _game.MakeMoveUnchecked(_currentNode, moveCandidate);
                double scoreCandidate = -SolverHelpers.Negamax(_game, child);
                if (scoreCandidate > bestScore)
                {
                    bestScore = scoreCandidate;
                    bestMove = moveCandidate;
                }
            }

            score = bestScore;
            move = bestMove;
            return true;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }
}
