# Go Backend API – Go Game and Bot

Base URL: `https://{host}` (development defaults to `https://localhost:7240` when running `dotnet run` in `Go.Backend.API`).

Board coordinates are 0-based `(x, y)` with `x` as row and `y` as column on a 19×19 board. Stone colors are the strings `black` or `white`.

## Endpoints

### Create a new game
- **POST** `/api/games`
- **Response 201**
  ```json
  {
    "gameId": "guid",
    "size": 19,
    "nextPlayer": "Black",
    "moveNumber": 0,
    "isFinished": false,
    "winner": null,
    "blackCaptures": 0,
    "whiteCaptures": 0,
    "board": ["...................", "..."]
  }
  ```

### Get game state
- **GET** `/api/games/{id}`
- **Response 200**: same shape as create response.
- **Response 404**: game not found.

### Play a human move
- **POST** `/api/games/{id}/moves`
- **Request body**
  ```json
  { "x": 3, "y": 3, "color": "black", "pass": false }
  ```
- **Responses**
  - **200**
    ```json
    {
      "move": { "x": 3, "y": 3 },
      "captured": [{ "x": 4, "y": 3 }],
      "state": { /* game state object as above */ }
    }
    ```
    - Note: when `pass: true`, `"move"` will be `null` and the server advances the turn. Two consecutive passes end the game.
  - **400** with error string (invalid color, occupied point, suicide, ko, not your turn, or game finished).
  - **404** if the game does not exist.

### Ask the bot to play a move
- **POST** `/api/games/{id}/bot-move`
- **Request body (optional color; defaults to current next player)**
  ```json
  { "color": "white" }
  ```
- **Responses**
  - **200**
    ```json
    {
      "move": { "x": 10, "y": 10 },
      "captured": [],
      "state": { /* game state object as above */ }
    }
    ```
    - `"move"` may be `null` if the bot passes; two consecutive passes end the game.
  - **400** if the suggested move is illegal or game is finished.
  - **404** if the game does not exist.

## Game state object
Fields in responses:
- `gameId` (guid)
- `size` (int, always 19)
- `nextPlayer` (`"Black"` or `"White"`)
- `moveNumber` (int)
- `isFinished` (bool)
- `winner` (`"Black"`, `"White"`, or `null`)
- `blackCaptures` / `whiteCaptures` (int)
- `board`: array of 19 strings, each length 19; characters: `B`, `W`, or `.` for empty.
    - Board state is maintained server-side; clients should rely on these responses.

## Bot model configuration
- Configure path in `Go.Backend.API/appsettings.json` under:
  ```json
  "BotModel": { "ModelPath": "models/go-bot.onnx" }
  ```
- Place your pre-trained model file at that path. If missing, the stub engine falls back to a simple heuristic (center-first legal move).

## Notes
- Coordinates are validated to be on-board; moves that repeat a previous board state are rejected (simple ko).
- HTTPS redirection is enabled; use HTTPS during development unless disabled manually.
