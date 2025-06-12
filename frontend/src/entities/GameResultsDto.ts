import type {PlayerScoreDto} from "./PlayerScoreDto.ts";

export type GameResultsDto = {
    gameId: string;
    roomId: string;
    genre: string;
    winnerId: string;
    winnerName: string;
    scores: Record<string, PlayerScoreDto>;
};