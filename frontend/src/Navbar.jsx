import React from "react";
import { useNavigate } from "react-router-dom";
import "./App.css";

function Navbar() {
  const navigate = useNavigate();

  return (
    <nav className="navbar">
      <button onClick={() => navigate("/users")}>Users</button>
      <button onClick={() => navigate("/")}>Welcome to skillchallenge</button>
      <button onClick={() => navigate("/auth")}>Login/Register</button>
    </nav>
  );
}

export default Navbar;