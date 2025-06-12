export type QuizQuestion = {
    id: string;
    questionText: string;
    questionType: "artist" | "track";
    correctAnswer: string;
    correctIndex: number;
    options: string[];
    previewUrl?: string;
    coverUrl?: string;
    trackTitle: string;
    artist: string;
};