import { useParams, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { useSignalR } from "../../app/signalRContext.tsx";
import { fetchAuthUserData } from "../../processes/fetchAuthUserData.ts";
import type { User } from "../../entities/User.ts";
import { Header } from "../../widgets/Header";
import './index.css';
import photoPlaceholder from '../../shared/assets/photo-placeholder.png';
import {Link} from "react-router";

export type PlayerScoreDto = {
    username: string;
    userPhoto?: string;
    score: number;
};

export type GameResultsDto = {
    gameId: string;
    roomId: string;
    genre: string;
    winnerId: string;
    winnerName: string;
    scores: Record<string, PlayerScoreDto>;
};

export const GameResultsPage = () => {
    const { gameId } = useParams();
    const navigate = useNavigate();
    const connection = useSignalR();
    const [results, setResults] = useState<GameResultsDto | null>(null);
    const [currentUser, setCurrentUser] = useState<User | null>(null);
    const [isWinner, setIsWinner] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!connection || !gameId) return;

        const initializeGame = async () => {
            try {
                const fetchedUser = await fetchAuthUserData();
                const loggedUser: User = fetchedUser as User;
                setCurrentUser(loggedUser);

                connection.on("ReceiveGameResults", (serverResults: GameResultsDto) => {
                    setResults(serverResults);
                    if (loggedUser) {
                        setIsWinner(serverResults.winnerId === loggedUser.userId);
                    }
                    setLoading(false);
                });

                connection.on("Error", (errorMessage: string) => {
                    setError(errorMessage);
                    setLoading(false);
                });

                connection.on("ReceiveHostStatus", (isHost: boolean) => {
                    if (isHost) {
                        connection.invoke("GetGameResults", gameId)
                            .catch(err => {
                                console.error("Error fetching game results:", err);
                                setError("Failed to get game results");
                                setLoading(false);
                            });
                    } else {
                        // Non-host players wait for the host to send results
                        const timeout = setTimeout(() => {
                            if (!results) {
                                setError("Waiting for game results...");
                                setLoading(false);
                            }
                        }, 15000); // 15 seconds timeout

                        return () => clearTimeout(timeout);
                    }
                });

                connection.invoke("IsUserHost", gameId, loggedUser.userId);

            } catch (error) {
                console.error("Error initializing game:", error);
                setError("Failed to initialize game");
                setLoading(false);
            }
        };

        initializeGame();

        return () => {
            connection.off("ReceiveHostStatus");
            connection.off("ReceiveGameResults");
            connection.off("Error");
        };
    }, [connection, gameId]);
    
    const handleBackToHome = () => {
        navigate(`/home`);
    };

    if (loading) {
        return (
            <div className="game-results-page">
                <Header />
                <div className="loading-container">
                    <p>Loading results...</p>
                </div>
            </div>
        );
    }

    if (!results) {
        return (
            <div className="game-results-page">
                <Header />
                <div className="error-container">
                    <p>No results found for this game.</p>
                    <button onClick={handleBackToHome}>Back to Home</button>
                </div>
            </div>
        );
    }

    const sortedPlayers = Object.entries(results.scores)
        .map(([userId, player]) => ({ userId, ...player }))
        .sort((a, b) => b.score - a.score);

    return (
        <div className="game-results-page">
            <Header />

            <div className="results-container">
                <h1>Game Results</h1>
                <div className="winner-section">
                    <h2>🏆 Winner: {results.winnerName}</h2>
                    {isWinner && (
                        <div className="playlist-reward">
                            <p>Congratulations! You've won a playlist with {results.genre} songs!</p>
                            <button
                                onClick={() => navigate(`/user/${currentUser?.userId}`)}
                                className="view-playlist-btn"
                            >
                                View Your Playlists
                            </button>
                        </div>
                    )}
                </div>

                <div className="scores-table">
                    <table>
                        <thead>
                        <tr>
                            <th>Rank</th>
                            <th>Player</th>
                            <th>Score</th>
                        </tr>
                        </thead>
                        <tbody>
                        {sortedPlayers.map((player, index) => (
                            <tr
                                key={player.userId}
                                className={player.userId === results.winnerId ? "winner-row" : ""}
                            >
                                <td>{index + 1}</td>
                                <td>
                                    <Link to={`/user/${player.userId}`} className="player-info">
                                        <img
                                            src={player.userPhoto || photoPlaceholder}
                                            alt={player.username}
                                            className="player-avatar"
                                        />
                                        <span>{player.username}</span>
                                        {player.userId === currentUser?.userId && (
                                            <span className="you-badge">(You)</span>
                                        )}
                                    </Link>
                                </td>
                                <td>{player.score}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>

                <div className="actions">
                    <button onClick={handleBackToHome} className="back-button">
                        Back to Home
                    </button>
                </div>
            </div>
        </div>
    );
};