import React from "react";
import { useNavigate } from "react-router-dom";
import userIcon from "./assets/user_icon_001.jpg";
import "./App.css";
import { useAuth } from "./AuthContext";

function Navbar() {
  const navigate = useNavigate();
  const { userName, logout } = useAuth();

  return (
    <nav className="navbar">
      <button onClick={() => navigate("/users")}>Users</button>
      <button onClick={() => navigate("/")}>Challenges</button>
      {userName ? (
        <>
          <button onClick={() => { logout(); navigate("/login"); }}>Log out</button>
          <button
            className="my-profile"
            onClick={() => navigate("/profile")}
            title="Your profile"
          >
            <img src={userIcon} alt="User" className="user-icon" />
          </button>
        </>
      ) : (
        <>
          <button onClick={() => navigate("/login")}>Log in</button>
          <button onClick={() => navigate("/register")}>Register</button>
        </>
      )}
    </nav>
  );
}

export default Navbar;