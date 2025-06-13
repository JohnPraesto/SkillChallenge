import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route, useNavigate } from "react-router-dom";
import Login from "./Login";
import Register from "./Register";
import Users from "./Users";
import UserDetail from "./UserDetail";
import Navbar from "./Navbar";
import Profile from "./Profile"
import Challenges from "./Challenges"
import ChallengeDetails from "./ChallengeDetails";
import ErrorBoundary from "./ErrorBoundary";
import CreateChallenge  from "./CreateChallenge";
import './App.css';

function Home() {
  return (
    <div>
      <div style={{ textAlign: "center", marginTop: "30vh", fontSize: "2em" }}>
        One list of Open Challenges. One list of Closed Challenges.
      </div>
    </div>
  );
}

function Auth() {
  const [showRegister, setShowRegister] = useState(false);
  return showRegister ? (
    <Register />
  ) : (
    <Login onShowRegister={() => setShowRegister(true)} />
  );
}

function App() {
  return (
    <ErrorBoundary>
      <Router>
        <Navbar />
        <Routes>
          <Route path="/" element={<Challenges />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/users" element={<Users />} />
          <Route path="/users/username/:userName" element={<UserDetail />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/challenges/:id" element={<ChallengeDetails />} />
          <Route path="/create-challenge" element={<CreateChallenge />} />
        </Routes>
      </Router>
    </ErrorBoundary>
  );
}

export default App;