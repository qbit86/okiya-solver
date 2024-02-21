using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static Okiya.TryHelpers;

namespace Okiya;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
    private readonly Game<int[]> _game;
    private Node _currentNode;

    private Solver(Game<int[]> game, Node rootNode)
    {
        _game = game;
        _currentNode = rootNode;
    }

    public static Solver Create(int[] board) => new(Game.Create(board), new());

    public static Solver Create(Game<int[]> game)
    {
        if (!game.IsValid)
            throw new ArgumentException("Game instance is not initialized.", nameof(game));

        return new(game, new());
    }

    public static Solver Create(
        Game<int[]> game, int firstPlayerTokens, int secondPlayerTokens, int sideToMove, int lastCardIndex = default)
    {
        if (!game.IsValid)
            throw new ArgumentException("Game instance must be valid.", nameof(game));
        uint firstPlayerTokensChecked = (uint)firstPlayerTokens;
        if (firstPlayerTokensChecked > Constants.PlayerTokensMask)
            throw new ArgumentOutOfRangeException(nameof(firstPlayerTokens));
        uint secondPlayerTokensChecked = (uint)secondPlayerTokens;
        if (secondPlayerTokensChecked > Constants.PlayerTokensMask)
            throw new ArgumentOutOfRangeException(nameof(secondPlayerTokens));
        if ((firstPlayerTokensChecked & secondPlayerTokensChecked) is not 0)
            throw new ArgumentException("Players' tokens may not overlap.", nameof(secondPlayerTokens));
        if (sideToMove is not (0 or 1))
            throw new ArgumentOutOfRangeException(nameof(sideToMove));
        if ((uint)lastCardIndex >= Constants.CardCount)
            throw new ArgumentOutOfRangeException(nameof(lastCardIndex));

        var rootNode = Node.CreateUnchecked(
            firstPlayerTokensChecked, secondPlayerTokensChecked, sideToMove, lastCardIndex);
        return new(game, rootNode);
    }

    public bool TrySelectMove(out int move, out double score)
    {
        bool result = TrySelectMoveCore(out move, out double relativeScore);
        int sign = Sign(_currentNode.GetSideToMove());
        score = sign * relativeScore;
        return result;
    }

    private bool TryMakeMove(out int move, out double score)
    {
        bool result = TrySelectMoveCore(out move, out double relativeScore);
        int sign = Sign(_currentNode.GetSideToMove());
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
            int possibleMoveCount = _game.PopulatePossibleMoves(_currentNode, buffer);
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
                double scoreCandidate = -Negamax(child);
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

    private double Negamax(Node node)
    {
        if (Game.IsTerminalNode(node, out double score))
            return score;

        int[] buffer = ArrayPool<int>.Shared.Rent(Constants.CardCount);
        try
        {
            int possibleMoveCount = _game.PopulatePossibleMoves(node, buffer);
            if (possibleMoveCount is 0)
                return -sbyte.MaxValue + node.GetTokenCount();

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            double bestScore = double.NegativeInfinity;
            foreach (int moveCandidate in possibleMoves)
            {
                Node child = node.AddPlayerTokenUnchecked(moveCandidate);
                double scoreCandidate = -Negamax(child);
                if (scoreCandidate > bestScore)
                    bestScore = scoreCandidate;
            }

            return bestScore;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }

    private static int Sign(int sideToMove) =>
        sideToMove switch { 0 => 1, 1 => -1, _ => ThrowUnreachableException() };

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static int ThrowUnreachableException() => throw new UnreachableException();
}
