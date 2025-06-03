import './index.css';
import {Header} from "../../widgets/Header";
import {useEffect, useState} from "react";
import {useNavigate} from "react-router";
import {fetchAuthUserData} from "../../processes/fetchAuthUserData.ts";
import type {User} from "../../entities/User.ts";
import {useSignalR} from "../../app/signalRContext.tsx";

const GENRES = [
    { id: 152, name: 'Pop' },
    { id: 85, name: 'Alternative' },
    { id: 132, name: 'Rock' },
    { id: 116, name: 'Rap/Hip-Hop' },
    { id: 113, name: 'Dance' },
    { id: 144, name: 'Jazz' },
    { id: 129, name: 'Metal' },
    { id: 165, name: 'R&B' },
    { id: 153, name: 'Electronic' },
    { id: 169, name: 'Classical' },
    { id: 98, name: 'Reggae' },
];

const QUESTION_COUNTS = [3, 4, 5];

export const CreateRoomPage = () => {
    const navigate = useNavigate();
    const connection = useSignalR();
    const [user, setUser] = useState<User | null>(null);
    const [selectedGenre, setSelectedGenre] = useState<number | null>(null);
    const [questionCount, setQuestionCount] = useState<number>(0);
    const [isCreating, setIsCreating] = useState(false);

    useEffect(() => {
        const loadUser = async () => {
            try {
                const fetchedUser = await fetchAuthUserData();
                const loggedUser: User = fetchedUser as User;
                setUser(loggedUser);
            } catch {
                setUser(null);
            }
        };
        loadUser();
    }, []);

    const handleCreateRoom = async () => {
        if (!selectedGenre) {
            alert('Выберите жанр!');
            return;
        }

        setIsCreating(true);

        try {
            const genreName = GENRES.find(g => g.id === selectedGenre)?.name || '';
            const token = localStorage.getItem('token');
            if (user) {
                const response = await fetch('http://localhost:19288/api/rooms', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        "Authorization": `Bearer ${token}`,
                    },
                    body: JSON.stringify({
                        genre: genreName,
                        questionCount,
                        userHostId: user.userId
                    })
                });

                if (!response.ok) throw new Error('Failed to create room');
                const data = await response.json();

                if (connection) {
                    navigate(`/waiting-room/${data.roomId}`);
                }
            }
        } catch (error) {
            console.error('Ошибка создания комнаты:', error);
            alert('Не удалось создать комнату');
        } finally {
            setIsCreating(false);
        }
    };

    return (
        <>
            <Header />
            <div className="create-room-container">
                <h1>Создать комнату</h1>

                <div className="form-group">
                    <label htmlFor="genre-select">Выберите жанр музыки:</label>
                    <select
                        id="genre-select"
                        value={selectedGenre || ''}
                        onChange={(e) => setSelectedGenre(Number(e.target.value))}
                        className="form-control"
                    >
                        <option value="" disabled>-- Выберите жанр --</option>
                        {GENRES.map(genre => (
                            <option key={genre.id} value={genre.id}>
                                {genre.name}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="form-group">
                    <label>Количество вопросов:</label>
                    <div className="question-count-buttons">
                        {QUESTION_COUNTS.map(count => (
                            <button
                                key={count}
                                className={`count-btn ${questionCount === count ? 'active' : ''}`}
                                onClick={() => setQuestionCount(count)}
                            >
                                {count}
                            </button>
                        ))}
                    </div>
                </div>

                <button
                    onClick={handleCreateRoom}
                    disabled={isCreating || !selectedGenre}
                    className="create-btn"
                >
                    {isCreating ? 'Создание...' : 'Создать комнату'}
                </button>
            </div>
        </>
    );
};