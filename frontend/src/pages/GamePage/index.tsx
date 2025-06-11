import './index.css';
import { useParams } from "react-router-dom";
import { useSignalR } from "../../app/signalRContext.tsx";
import {useEffect, useRef, useState} from "react";
import { useNavigate } from "react-router";
import {fetchAuthUserData} from "../../processes/fetchAuthUserData.ts";
import type {User} from "../../entities/User.ts";

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

export const GamePage = () => {
    const { gameId } = useParams();
    const connection = useSignalR();
    const [currentQuestion, setCurrentQuestion] = useState<QuizQuestion | null>(null);
    const [score, setScore] = useState(0);
    const [timeLeft, setTimeLeft] = useState(5);
    const [totalQuestions, setTotalQuestions] = useState(0);
    const [questionIndex, setQuestionIndex] = useState(0);
    const [isAnswerSubmitted, setIsAnswerSubmitted] = useState(false);
    const navigate = useNavigate();
    const questionTimerRef = useRef(5);
    const countdownTimerRef = useRef(5);
    const [selectedAnswerIndex, setSelectedAnswerIndex] = useState<number | null>(null);
    const [isHost, setIsHost] = useState(false);
    const [isHostChecked, setIsHostChecked] = useState(false);

    useEffect(() => {
        if (!connection || !gameId) return;

        const initializeGame = async () => {
            try {
                const fetchedUser = await fetchAuthUserData();
                const loggedUser: User = fetchedUser as User;

                connection.on("ReceiveHostStatus", (isHost: boolean) => {
                    setIsHost(isHost);
                    setIsHostChecked(true);

                    if (isHost) {
                        connection.invoke("GetNextQuestion", gameId);
                    }
                });

                connection.invoke("IsUserHost", gameId, loggedUser.userId);

            } catch (error) {
                console.error("Error initializing game:", error);
            }
        };

        initializeGame();

        return () => {
            connection.off("ReceiveHostStatus");
        };
    }, [connection, gameId]);

    useEffect(() => {
        if (!connection || !gameId || !isHostChecked) return;

        const startQuestionTimer = () => {
            if (questionTimerRef.current) {
                clearTimeout(questionTimerRef.current);
            }

            questionTimerRef.current = setTimeout(() => {
                if (isHost) {
                    connection.invoke("GetNextQuestion", gameId);
                }
            }, 5000);
        };

        connection.on("NextQuestion", (question: QuizQuestion & {
            questionIndex: number,
            totalQuestions: number
        }) => {
            setCurrentQuestion(question);
            setTimeLeft(5);
            setQuestionIndex(question.questionIndex + 1);
            setTotalQuestions(question.totalQuestions);
            setIsAnswerSubmitted(false);
            startQuestionTimer();
        });

        connection.on("GameEnded", (finalScores: Record<string, number>) => {
            if (questionTimerRef.current) {
                clearTimeout(questionTimerRef.current);
            }
            if (countdownTimerRef.current) {
                clearInterval(countdownTimerRef.current);
            }
            navigate(`/game-results/${gameId}`, { state: { scores: finalScores } });
        });

        return () => {
            connection.off("NextQuestion");
            connection.off("GameEnded");
            if (questionTimerRef.current) {
                clearTimeout(questionTimerRef.current);
            }
            if (countdownTimerRef.current) {
                clearInterval(countdownTimerRef.current);
            }
        };
    }, [gameId, connection, navigate, isHost, isHostChecked]);

    useEffect(() => {
        if (countdownTimerRef.current) {
            clearInterval(countdownTimerRef.current);
        }

        if (currentQuestion) {
            setTimeLeft(5);

            countdownTimerRef.current = setInterval(() => {
                setTimeLeft(prev => {
                    if (prev <= 1) {
                        clearInterval(countdownTimerRef.current);
                        return 0;
                    }
                    return prev - 1;
                });
            }, 500);
        }

        return () => {
            if (countdownTimerRef.current) {
                clearInterval(countdownTimerRef.current);
            }
        };
    }, [currentQuestion]);

    const handleAnswer = async (answerIndex: number) => {
        if (!connection || !gameId || !currentQuestion || isAnswerSubmitted) return;
        setIsAnswerSubmitted(true);
        setSelectedAnswerIndex(answerIndex);
        try {
            const fetchedUser = await fetchAuthUserData();
            const loggedUser: User = fetchedUser as User;
            const points = await connection.invoke<number>("SubmitAnswer", loggedUser.userId, gameId, answerIndex, questionIndex - 1, timeLeft);
            setScore(points);
        } catch (error) {
            console.error("Error submitting answer:", error);
            setIsAnswerSubmitted(false);
            setSelectedAnswerIndex(null);
        }
    };

    const timePercentage = (timeLeft / 10) * 100;

    return (
        <div className="game-page">
            <h1>Вопрос {questionIndex}/{totalQuestions}</h1>

            {currentQuestion?.questionType === "artist" ? (
                <>
                    <h2>Кто исполнитель этой песни?</h2>
                </>
            ) : (
                <>
                    <h2>Какое название этой песни?</h2>
                </>
            )}

            {currentQuestion?.previewUrl && (
                <audio controls autoPlay src={currentQuestion.previewUrl}/>
            )}

            <div className="time-container">
                <div
                    className="time-bar"
                    style={{width: `${timePercentage}%`}}
                ></div>
            </div>

            <div className="question-options">
                {currentQuestion?.options.map((option, index) => (
                    <button
                        key={index}
                        onClick={() => handleAnswer(index)}
                        disabled={timeLeft <= 0 || isAnswerSubmitted}
                        className={
                            isAnswerSubmitted
                                ? index === currentQuestion.correctIndex
                                    ? "correct-answer"
                                    : index === selectedAnswerIndex
                                        ? "incorrect-answer"
                                        : ""
                                : ""
                        }
                    >
                        {option}
                    </button>
                ))}
            </div>

            <h2>Ваш счёт: {score}</h2>
        </div>
    );
};