using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using static Okiya.TryHelpers;

namespace Okiya;

public sealed class RandomizedSolver
{
    private readonly Game<int[]> _game;
    private readonly Random _prng;
    private Node _currentNode;

    private RandomizedSolver(Game<int[]> game, Node node, Random prng)
    {
        Debug.Assert(game.IsValid);
        _game = game;
        _currentNode = node;
        _prng = prng;
    }

    public static RandomizedSolver Create(Game<int[]> game, Node node, Random prng)
    {
        ArgumentNullException.ThrowIfNull(prng);
        if (!game.IsValid)
            throw new ArgumentException("Game instance is not initialized.", nameof(game));

        return new(game, node, prng);
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
            int possibleMoveCount = _game.PopulateLegalMoves(_currentNode, buffer.AsSpan());
            if (possibleMoveCount is 0)
            {
                score = -sbyte.MaxValue + _currentNode.GetTokenCount();
                return None(-1, out move);
            }

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            int moveIndex = _prng.Next(possibleMoveCount);
            move = possibleMoves[moveIndex];
            Node child = _game.MakeMoveUnchecked(_currentNode, move);
            score = -SolverHelpers.Negamax(_game, child);
            return true;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }
}
