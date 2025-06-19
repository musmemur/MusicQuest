import { useParams, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { useSignalR } from "../../app/signalRContext.tsx";
import { fetchAuthUserData } from "../../processes/fetchAuthUserData.ts";
import type { User } from "../../entities/User.ts";
import { Header } from "../../widgets/Header";
import './index.css';
import photoPlaceholder from '../../shared/assets/photo-placeholder.png';
import { Link } from "react-router-dom";
import type { GameResultsDto } from "../../entities/GameResultsDto.ts";

export const GameResultsPage = () => {
    const { gameId } = useParams();
    const navigate = useNavigate();
    const connection = useSignalR();
    const [results, setResults] = useState<GameResultsDto | null>(null);
    const [currentUser, setCurrentUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!connection || !gameId) return;

        const initializeGame = async () => {
            try {
                const fetchedUser = await fetchAuthUserData();
                const loggedUser = fetchedUser as User;
                setCurrentUser(loggedUser);

                connection.on("ReceiveGameResults", (serverResults: GameResultsDto) => {
                    setResults(serverResults);
                    setLoading(false);
                });

                await connection.invoke("GetGameResults", gameId);

            } catch (error) {
                console.error("Error initializing game:", error);
                setLoading(false);
            }
        };

        initializeGame();

        return () => {
            connection.off("ReceiveGameResults");
        };
    }, [connection, gameId]);

    const isWinner = currentUser && results?.winners.includes(currentUser.userId);

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
                    <button onClick={() => navigate('/home')} className="back-button">
                        Back to Home
                    </button>
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
                    {results.winnerNames.length > 0 ? (
                        <>
                            <h2>🏆 {results.winnerNames.length > 1 ? "Winners" : "Winner"}: {results.winnerNames.join(", ")}</h2>
                            {isWinner && (
                                <div className="playlist-reward">
                                    <p>Congratulations! You've won a playlist with {results.genre} songs!</p>
                                    <button
                                        onClick={() => navigate(`/user/${currentUser.userId}`)}
                                        className="view-playlist-btn"
                                    >
                                        View Your Playlists
                                    </button>
                                </div>
                            )}
                        </>
                    ) : (
                        <h2>No winners - all players scored 0 points</h2>
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
                        {sortedPlayers.map((player, index) => {
                            const isPlayerWinner = results.winners.includes(player.userId);
                            return (
                                <tr key={player.userId} className={isPlayerWinner ? "winner-row" : ""}>
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
                                            {isPlayerWinner && <span className="winner-badge">🏆</span>}
                                        </Link>
                                    </td>
                                    <td>{player.score}</td>
                                </tr>
                            );
                        })}
                        </tbody>
                    </table>
                </div>

                <div className="actions">
                    <button onClick={() => navigate('/home')} className="back-button">
                        Back to Home
                    </button>
                </div>
            </div>
        </div>
    );
};