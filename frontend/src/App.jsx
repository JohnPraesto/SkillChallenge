import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route, useNavigate } from "react-router-dom";
import Login from "./Login";
import Register from "./Register";
import Users from "./Users";
import ArchivedChallenges from "./ArchivedChallenges";
import UserDetail from "./UserDetail";
import Navbar from "./Navbar";
import Profile from "./Profile"
import Challenges from "./Challenges"
import ChallengeDetails from "./ChallengeDetails";
import ErrorBoundary from "./ErrorBoundary";
import CreateChallenge from "./CreateChallenge";
import UploadResult from "./UploadResult";
import Admin from "./Admin";
import RequireAdmin from "./RequireAdmin";
import About from "./About";
import './App.css';

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
          <Route path="/archived-challenges" element={<ArchivedChallenges />} />
          <Route path="/users/username/:userName" element={<UserDetail />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/challenges/:id" element={<ChallengeDetails />} />
          <Route path="/create-challenge" element={<CreateChallenge />} />
          <Route path="/create-challenge" element={<CreateChallenge />} />
          <Route path="/about" element={<About />} />
          <Route path="/upload-result" element={<UploadResult />} />
          <Route path="/admin" element={
            <RequireAdmin>
              <Admin />
            </RequireAdmin>
          }/>
        </Routes>
      </Router>
    </ErrorBoundary>
  );
}

export default App;