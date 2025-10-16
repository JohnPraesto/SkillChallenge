import React from "react";
import { useNavigate, Link } from "react-router-dom";
import userIcon from "./assets/user_icon_001.jpg";
import { useAuth } from "./AuthContext";
import { ThemeToggle } from "./ThemeToggle";

function Navbar() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  return (
    <>
      <nav className="navbar">
        <div className="navbar-flex">
          <Link to="/" className="nav-link">
            <span className="brand-icon">⚡</span>
            <span className="brand-text">
              <span className="brand-skill">Skill</span>
              <span className="brand-challenge">Challenge</span>
            </span>
          </Link>

          <Link to="/about" className="nav-link">
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
            Leaderboard
          </Link>
          
          <div className="nav-link">
            <ThemeToggle />
          </div>
          
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

      {/* Mobile footer (shown on <=1024px) */}
      <footer className="mobile-footer" role="navigation" aria-label="Mobile navigation">
        <Link to="/" className="mobile-button" title="Challenges">
          <span className="mobile-icon">🏆</span>
          <span className="mobile-label">Challenges</span>
        </Link>

        <Link to="/users" className="mobile-button" title="Leaderboard">
          <span className="mobile-icon">👥</span>
          <span className="mobile-label">Leaderboard</span>
        </Link>

        {user?.userName ? (
          <button
            type="button"
            className="mobile-button"
            onClick={() => { logout(); navigate("/login"); }}
            title="Log out"
          >
            <span className="mobile-icon">🚪</span>
            <span className="mobile-label">Log out</span>
          </button>
        ) : (
          <Link to="/login" className="mobile-button" title="Sign in">
            <span className="mobile-icon">🔑</span>
            <span className="mobile-label">Sign In</span>
          </Link>
        )}

        {user?.userName ? (
          <button
            type="button"
            className="mobile-button"
            onClick={() => navigate("/profile")}
            title="Profile"
          >
            <img src={userIcon} alt="Profile" className="mobile-avatar" />
            <span className="mobile-label">Profile</span>
          </button>
        ) : (
          <Link to="/register" className="mobile-button" title="Sign up">
            <span className="mobile-icon">✨</span>
            <span className="mobile-label">Sign Up</span>
          </Link>
        )}
      </footer>
    </>
  );
}

export default Navbar;