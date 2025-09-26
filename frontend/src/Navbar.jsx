import React from "react";
import { useNavigate, Link } from "react-router-dom";
import userIcon from "./assets/user_icon_001.jpg";
import { useAuth } from "./AuthContext";
import { ThemeToggle } from "./ThemeToggle";

function Navbar() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  return (
    <nav className="navbar">
      <div className="navbar-flex">
        <Link to="/" className="navbar-brand">
          <span className="brand-icon">⚡</span>
          <span className="brand-text">
            <span className="brand-skill">Skill</span>
            <span className="brand-challenge">Challenge</span>
          </span>
        </Link>

        <Link to="/about" className="navbar-brand">
          <span className="brand-text">
            <span className="brand-skill">About</span>
          </span>
        </Link>
        
        <Link to="/" className="nav-link">
          <span className="nav-icon">🏆</span>
          Challenges
        </Link>

        <Link to="/users" className="nav-link">
          <span className="nav-icon">👥</span>
          Users
        </Link>
        
        <ThemeToggle />
        
        {user?.userName ? (
          <>
            <button 
              className="btn btn-outline logout-btn" 
              onClick={() => { logout(); navigate("/login"); }}
            >
              <span className="nav-icon">🚪</span>
              Log out
            </button>
            <button
              className="profile-button"
              onClick={() => navigate("/profile")}
              title={`Logged in as ${user.userName}`}
            >
              <img src={userIcon} alt="User" className="user-avatar" />
              <span className="user-name">{user.userName}</span>
            </button>
          </>
        ) : (
          <>
            <Link to="/login" className="btn btn-outline">
              <span className="nav-icon">🔑</span>
              Sign In
            </Link>
            <Link to="/register" className="btn btn-primary">
              <span className="nav-icon">✨</span>
              Sign Up
            </Link>
          </>
        )}
      </div>  
    </nav>
  );
}

export default Navbar;