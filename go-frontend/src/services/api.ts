import axios from 'axios';
import type { GameState, MoveResponse } from '../types/game';

// Đổi port này cho đúng với Backend của bạn
const API_URL = 'http://localhost:5217/api/games'; 

export const gameService = {
    // 1. Tạo game mới
    createGame: async (): Promise<GameState> => {
        const response = await axios.post<GameState>(API_URL);
        return response.data;
    },

    // 2. Lấy trạng thái game
    getGame: async (id: string): Promise<GameState> => {
        const response = await axios.get<GameState>(`${API_URL}/${id}`);
        return response.data;
    },

    // 3. Người chơi đi quân
    playMove: async (id: string, x: number, y: number, color: string): Promise<MoveResponse> => {
        const response = await axios.post<MoveResponse>(`${API_URL}/${id}/moves`, {
            x, y, color, pass: false
        });
        return response.data;
    },

    // 4. Yêu cầu Bot đi
    getBotMove: async (id: string, color: string): Promise<MoveResponse> => {
        const response = await axios.post<MoveResponse>(`${API_URL}/${id}/bot-move`, {
            color
        });
        console.log(response);
        
        return response.data;
    }
};