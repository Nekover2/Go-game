export interface GameState {
    gameId: string;
    size: number;
    nextPlayer: "Black" | "White";
    moveNumber: number;
    isFinished: boolean;
    winner: "Black" | "White" | null;
    blackCaptures: number;
    whiteCaptures: number;
    board: string[]; // Mảng 19 chuỗi, mỗi chuỗi 19 ký tự (., B, W)
}

export interface MoveResponse {
    move: { x: number; y: number } | null;
    captured: { x: number; y: number }[];
    state: GameState;
}