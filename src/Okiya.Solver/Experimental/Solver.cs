using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static Okiya.TryHelpers;

namespace Okiya.Experimental;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
    private readonly int[] _board;
    private readonly Node _currentNode;

    private Solver(int[] board, Node rootNode)
    {
        _board = board;
        _currentNode = rootNode;
    }

    public static Solver Create(int[] board)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Length < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));

        return new(board, new());
    }

    public static Solver Create(
        int[] board, int firstPlayerTokens, int secondPlayerTokens, int sideToMove, int lastCard = default)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Length < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));
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
        if ((uint)lastCard >= Constants.CardCount)
            throw new ArgumentOutOfRangeException(nameof(lastCard));

        var rootNode = Node.CreateUnchecked(firstPlayerTokensChecked, secondPlayerTokensChecked, sideToMove, lastCard);
        return new(board, rootNode);
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
            _currentNode.AddPlayerToken(move, _board[move]);

        return result;
    }

    public double MakeMoves<TCollection>(TCollection moves)
        where TCollection : ICollection<int>
    {
        if (TryMakeMove(out int firstMove, out double firstScore))
            moves.Add(firstMove);
        else
            return firstScore;

        while (TryMakeMove(out int move, out double score))
        {
            Debug.Assert(firstScore.Equals(score));
            moves.Add(move);
        }

        return firstScore;
    }

    private bool TrySelectMoveCore(out int move, out double score)
    {
        if (IsTerminalRoot(_currentNode, out score))
            return None(-1, out move);

        int[] buffer = ArrayPool<int>.Shared.Rent(_board.Length);
        try
        {
            int possibleMoveCount = PopulatePossibleMoves(_currentNode, buffer);
            if (possibleMoveCount is 0)
            {
                int tokenCount = _currentNode.GetTokenCount();
                score = -sbyte.MaxValue + tokenCount;
                return None(-1, out move);
            }

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            double bestScore = double.NegativeInfinity;
            int bestMove = -1;
            foreach (int moveCandidate in possibleMoves)
            {
                Node child = _currentNode.AddPlayerToken(moveCandidate, _board[moveCandidate]);
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
        if (IsTerminalNode(node, out double score))
            return score;

        int[] buffer = ArrayPool<int>.Shared.Rent(_board.Length);
        try
        {
            int possibleMoveCount = PopulatePossibleMoves(node, buffer);
            if (possibleMoveCount is 0)
            {
                int tokenCount = node.GetTokenCount();
                return -sbyte.MaxValue + tokenCount;
            }

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            double bestScore = double.NegativeInfinity;
            foreach (int moveCandidate in possibleMoves)
            {
                Node child = node.AddPlayerToken(moveCandidate, _board[moveCandidate]);
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

    private static bool IsTerminalRoot(Node node, out double score)
    {
        (int playerTokens, int opponentTokens) = node.GetPlayersTokens();
        if (RuleHelpers.IsWinning(playerTokens))
        {
            double tokenCount = node.GetTokenCount();
            score = sbyte.MaxValue - tokenCount;
            return true;
        }

        if (RuleHelpers.IsWinning(opponentTokens))
        {
            double tokenCount = node.GetTokenCount();
            score = -sbyte.MaxValue + tokenCount;
            return true;
        }

        if (node.IsFull())
            return Some(0.0, out score);

        score = double.NaN;
        return false;
    }

    private static bool IsTerminalNode(Node node, out double score)
    {
        (int playerTokens, int opponentTokens) = node.GetPlayersTokens();
        Debug.Assert(RuleHelpers.IsNotWinning(playerTokens));
        if (RuleHelpers.IsWinning(opponentTokens))
        {
            double tokenCount = node.GetTokenCount();
            score = -sbyte.MaxValue + tokenCount;
            return true;
        }

        if (node.IsFull())
            return Some(0.0, out score);

        score = double.NaN;
        return false;
    }

    private static int Sign(int sideToMove) =>
        sideToMove switch { 0 => 1, 1 => -1, _ => ThrowUnreachableException() };

    private int PopulatePossibleMoves(Node node, Span<int> destination)
    {
        if (!node.TryGetCard(out int lastCard))
            return PopulatePossibleFirstMoves(destination);
        Int32CardConcept c = Int32CardConcept.Instance;
        int tokensPlayed = node.GetPlayerTokens(0) | node.GetPlayerTokens(1);
        int moveCount = 0;
        for (int i = 0; i < _board.Length; ++i)
        {
            int mask = 1 << i;
            if ((tokensPlayed & mask) is not 0)
                continue;
            int candidateCard = _board[i];
            if (c.Rank(candidateCard) != c.Rank(lastCard) && c.Suit(candidateCard) != c.Suit(lastCard))
                continue;
            destination[moveCount++] = i;
        }

        return moveCount;
    }

    private int PopulatePossibleFirstMoves(Span<int> destination)
    {
        int moveCount = 0;
        for (int i = 0; i < _board.Length; ++i)
        {
            if (i is 5 or 6 or 9 or 10)
                continue;
            destination[moveCount++] = i;
        }

        return moveCount;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static int ThrowUnreachableException() => throw new UnreachableException();
}
