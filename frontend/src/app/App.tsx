import {Route, Routes} from "react-router";
import {SignUpPage} from "../pages/SignUpPage";
import {RegisterPage} from "../pages/RegisterPage";
import {LoginPage} from "../pages/LoginPage";
import {HomePage} from "../pages/HomePage";
import {UserPage} from "../pages/UserPage";
import {WaitingRoomPage} from "../pages/WaitingRoomPage";
import {CreateRoomPage} from "../pages/CreateRoomPage";
import {SignalRProvider} from "./signalRContext.tsx";
import {GamePage} from "../pages/GamePage";
import {GameResultsPage} from "../pages/GameResultsPage";
import {LandingPage} from "../pages/LandingPage";

export default function App() {
  return (
      <SignalRProvider>
          <Routes>
              <Route path="/" element={<LandingPage />}></Route>

              <Route path="/sign-up" element={<SignUpPage />}></Route>
              <Route path="/sign-up/register" element={<RegisterPage />}></Route>
              <Route path="/sign-up/login" element={<LoginPage />}></Route>

              <Route path="/home" element={<HomePage />}></Route>
              <Route path="/user/:userId" element={<UserPage />}></Route>
              <Route path="/create-room" element={<CreateRoomPage />}></Route>
              <Route path="/waiting-room/:roomId" element={<WaitingRoomPage />}></Route>
              <Route path="/game/:gameId" element={<GamePage />}></Route>
              <Route path="/game-results/:gameId" element={<GameResultsPage />}></Route>
          </Routes>
      </SignalRProvider>
  )
}

