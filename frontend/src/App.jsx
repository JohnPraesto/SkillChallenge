// src/App.jsx
import React, { useState } from "react";
import Login from "./Login";
import Register from "./Register";
import './App.css';

function App() {
  const [showRegister, setShowRegister] = useState(false);

  return showRegister ? (
    <Register />
  ) : (
    <Login onShowRegister={() => setShowRegister(true)} />
  );
}

export default App;