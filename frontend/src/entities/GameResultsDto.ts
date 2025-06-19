import type {PlayerScoreDto} from "./PlayerScoreDto.ts";

export type GameResultsDto = {
    gameId: string;
    roomId: string;
    genre: string;
    winners: string[];
    winnerNames: string[];
    scores: Record<string, PlayerScoreDto>;
};