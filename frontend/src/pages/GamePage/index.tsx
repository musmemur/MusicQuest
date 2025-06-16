import './index.css';
import './adaptive.css';
import { useParams } from "react-router-dom";
import { useSignalR } from "../../app/signalRContext.tsx";
import {useEffect, useRef, useState} from "react";
import { useNavigate } from "react-router";
import {fetchAuthUserData} from "../../processes/fetchAuthUserData.ts";
import type {User} from "../../entities/User.ts";
import type {QuizQuestion} from "../../entities/QuizQuestion.ts";

const questionTimer = 10;

export const GamePage = () => {
    const { gameId } = useParams();
    const connection = useSignalR();
    const [currentQuestion, setCurrentQuestion] = useState<QuizQuestion | null>(null);
    const [score, setScore] = useState(0);
    const [timeLeft, setTimeLeft] = useState(questionTimer);
    const [totalQuestions, setTotalQuestions] = useState(0);
    const [questionIndex, setQuestionIndex] = useState(0);
    const [isAnswerSubmitted, setIsAnswerSubmitted] = useState(false);
    const navigate = useNavigate();
    const questionTimerRef = useRef(questionTimer);
    const countdownTimerRef = useRef(questionTimer);
    const [selectedAnswerIndex, setSelectedAnswerIndex] = useState<number | null>(null);

    useEffect(() => {
        if (!connection || !gameId) return;

        const initializeGame = async () => {
            try {
                connection.invoke("GetNextQuestion", gameId);
            } catch (error) {
                console.error("Error initializing game:", error);
            }
        };

        (async () => {
            await initializeGame();
        })();

        return () => {
            connection.off("ReceiveHostStatus");
        };
    }, [connection, gameId]);

    useEffect(() => {
        if (!connection || !gameId) return;

        const startQuestionTimer = () => {
            if (questionTimerRef.current) {
                clearTimeout(questionTimerRef.current);
            }

            questionTimerRef.current = setTimeout(() => {
                connection.invoke("GetNextQuestion", gameId).catch(err => {
                    console.error("Error getting next question:", err);
                });
            }, questionTimer * 1000);
        };

        connection.on("NextQuestion", (question: QuizQuestion & {
            questionIndex: number,
            totalQuestions: number
        }) => {
            setCurrentQuestion(question);
            setTimeLeft(questionTimer);
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
    }, [gameId, connection, navigate]);

    useEffect(() => {
        if (countdownTimerRef.current) {
            clearInterval(countdownTimerRef.current);
        }

        if (currentQuestion) {
            setTimeLeft(questionTimer);

            countdownTimerRef.current = setInterval(() => {
                setTimeLeft(prev => {
                    if (prev <= 1) {
                        clearInterval(countdownTimerRef.current);
                        return 0;
                    }
                    return prev - 1;
                });
            }, questionTimer * 100);
        }

        return () => {
            if (countdownTimerRef.current) {
                clearInterval(countdownTimerRef.current);
            }
        };
    }, [currentQuestion]);

    useEffect(() => {
        if (!connection) return;

        connection.on("GameDisconnected", (error: string) => {
            console.log(`Disconnection: ${error}.`);
            navigate("/home");
        });

        connection.on("ConnectionLost", () => {
            alert("Connection to the game server was lost. You will be redirected to the main page.");
            navigate("/");
        });

        return () => {
            connection.off("PlayerLeft");
            connection.off("NewHostAssigned");
            connection.off("ConnectionLost");
        };
    }, [connection, navigate]);

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