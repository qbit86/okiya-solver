# Okiya Solver

## Overview

This repository contains a C# implementation of a solver for the two-player abstract strategy game [Okiya](https://boardgamegeek.com/boardgame/125311/okiya).
The solver is based on the [Negamax](https://en.wikipedia.org/wiki/Negamax) algorithm, a variant of the Minimax search commonly used for game playing.

<p align="center" width="100%"><img src="https://cf.geekdo-images.com/N76vlNw73LTaF2Go5tnn_Q__imagepage/img/HfginKXCgY410jpgKytyYhV4ELQ=/fit-in/900x600/filters:no_upscale():strip_icc()/pic3711389.png" width="512" alt="Explanation" /></p>

## Observations

In this project, I have stochastically analyzed numerous play-outs of the game Okiya, starting from random permutations.

<p align="center" width="100%"><img src="assets/position-example.svg" alt="Position example" /></p>

When both players make perfect moves, the outcomes are as follows:

- Approximately 30% of all games end in a draw.
- About 70% of the games result in a victory for the first player.

If the first legal move (outside the central block) for the starting player is chosen randomly, the game dynamics take a different turn:

- Approximately 18% of the games end in a win for the second player.
- About 11% end in a win for the first player.
- The remaining games result in draws.

If the starting player's first move is chosen as the weakest, the results are as follows:

- 72% of the games end in a win for the second player.
- The remaining 28% end in a draw.

When the first move is inside the central block (illegal according to the original rules), then almost all games end in a win for the first player, and very rarely in a draw.

These findings provide insights into the impact of initial moves and the strategic landscape of Okiya gameplay.

## Acknowledgments

Special thanks to the creator of Okiya, Bruno Cathala, for providing an engaging game for strategic minds.

## License

This project is licensed under the MIT License — see the [LICENSE](./LICENSE.txt) file for details.
