export const Logo = () => {
    return (
        <svg width="200" height="200" viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg">
            <circle cx="100" cy="100" r="90" fill="#FFF5E6"/>

            <defs>
                <linearGradient id="vinyl" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stopColor="#2a52be"/>
                    <stop offset="100%" stopColor="#1e3c8b"/>
                </linearGradient>
            </defs>
            <circle cx="100" cy="100" r="80" fill="url(#vinyl)" stroke="#FFF5E6" strokeWidth="3"/>
            <circle cx="100" cy="100" r="60" fill="#1e3c8b" stroke="#FFF5E6" strokeWidth="2"/>
            <circle cx="100" cy="100" r="15" fill="#FFF5E6"/>
            <circle cx="100" cy="100" r="5" fill="#1e3c8b"/>

            <path d="M40 100 Q20 80 40 60 L45 65 Q30 75 45 85 Z" fill="white" stroke="#1e3c8b"
                  strokeWidth="1.5"/>
            <path d="M160 100 Q180 80 160 60 L155 65 Q170 75 155 85 Z" fill="white" stroke="#1e3c8b"
                  strokeWidth="1.5"/>

            <path d="M100 30 Q80 20 60 30 L65 35 Q80 25 95 35 Z" fill="white" stroke="#1e3c8b"
                  strokeWidth="1.5"/>
            <path d="M100 170 Q80 180 60 170 L65 165 Q80 175 95 165 Z" fill="white" stroke="#1e3c8b"
                  strokeWidth="1.5"/>

            <text x="100" y="125" fontFamily="Arial" fontSize="16" fill="#FFF5E6" textAnchor="middle"
                  dominantBaseline="middle" fontWeight="bold">MUSIC QUIZ
            </text>
        </svg>
    )
}