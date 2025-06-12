import {Link} from "react-router";
import {Header} from "../../widgets/Header";
import {Logo} from "../../shared/assets/svg/Logo.tsx";
import './index.css';
import './adaptive.css';

export const LandingPage = () => {
    return(
        <>
            <Header/>
            <main className="landing-main">
                <div className="landing-topic">
                    <p>ОТКРОЙТЕ<br />ДЛЯ СЕБЯ <strong><br />МИР МУЗЫКИ</strong></p>
                    <Logo />
                </div>
                <ul className="landing-list">
                    <li>соревнуйтесь с другими людьми</li>
                    <li>покажите своё мастерство</li>
                    <li>получте уникальные плейлисты</li>
                </ul>
                <Link to="/sign-up" className="signUp-button">начать</Link>
            </main>
        </>
    )
}