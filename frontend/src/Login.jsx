import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { useToast } from "./ToastContext";

function Login() {
  const [form, setForm] = useState({ username: "", password: "" });
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();
  const { showError, showSuccess } = useToast();
  const [showForgotModal, setShowForgotModal] = useState(false);
  const [forgotEmail, setForgotEmail] = useState("");
  const [forgotLoading, setForgotLoading] = useState(false);
  const apiUrl = import.meta.env.VITE_API_URL;

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const response = await fetch(apiUrl + "/api/account/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userName: form.username,
          password: form.password,
        }),
      });

      if (response.ok) {
        const data = await response.json();
        login(data.token);
        showSuccess("Welcome back! 🎉");
        navigate("/");
      } else {
        let errorMsg = "Invalid username or password";
        try{
          const errorData = await response.json();
          if (typeof errorData === "string"){
            errorMsg = errorData;
          } else if (errorData?.message){
            errorMsg = errorData.message;
          }
        } catch {
          showError(errorMsg);
        }
      }
    } catch (err) {
      showError("Connection error. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPassword = async (e) => {
    e.preventDefault();
    setForgotLoading(true);
    try {
      const response = await fetch(apiUrl + "/api/account/forgot-password", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          email: forgotEmail,
          resetLinkBaseUrl: window.location.origin, // base URL for reset link
        }),
      });
      if (response.ok) {
        showSuccess("If the email exists, a reset link has been sent.");
        setShowForgotModal(false);
        setForgotEmail("");
      } else {
        showError("Failed to send reset link.");
      }
    } catch {
      showError("Connection error.");
    } finally {
      setForgotLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <h1 className="auth-title">Welcome Back!</h1>
          <p className="auth-subtitle">Sign in to continue your skill journey</p>
        </div>
        
        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <div className="input-wrapper">
              <span className="input-icon">👤</span>
              <input
                id="username"
                name="username"
                type="text"
                placeholder="Enter your username"
                value={form.username}
                onChange={handleChange}
                className="form-control auth-input"
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <div className="input-wrapper">
              <span className="input-icon">🔒</span>
              <input
                id="password"
                name="password"
                type="password"
                placeholder="Enter your password"
                value={form.password}
                onChange={handleChange}
                className="form-control auth-input"
                required
              />
            </div>
          </div>

          <button 
            type="submit" 
            className={`btn btn-primary auth-submit ${loading ? 'loading' : ''}`}
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="spinner"></span>
                Signing in...
              </>
            ) : (
              'Sign In'
            )}
          </button>
        </form>

        <div className="forgot-password">
          <button
            type="button"
            className="link-button"
            onClick={() => setShowForgotModal(true)}
            tabIndex={-1}
          >
            Forgot password?
          </button>
        </div>

        {showForgotModal && (
          <div className="modal-backdrop">
            <div className="modal">
              <h2>Reset Password</h2>
              <form onSubmit={handleForgotPassword}>
                <input
                  type="email"
                  placeholder="Enter your email"
                  value={forgotEmail}
                  onChange={(e) => setForgotEmail(e.target.value)}
                  required
                  className="form-control"
                  autoFocus
                  style={{marginBottom: "0.5rem", marginTop: "-1rem"}}
                />
                <div className="modal-actions">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowForgotModal(false)}
                    disabled={forgotLoading}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={forgotLoading}
                    style={{marginLeft: "1rem", marginBottom: "1rem"}}
                  >
                    {forgotLoading ? "Sending..." : "Send Reset Link"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        <div className="auth-footer">
          <p>Don't have an account? 
            <button 
              onClick={() => navigate('/register')} 
              className="link-button"
            >
              Sign up here
            </button>
          </p>
        </div>
      </div>
    </div>
  );
}

export default Login;