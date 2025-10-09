import React, { useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { useToast } from "./ToastContext";

function ResetPassword() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { showError, showSuccess } = useToast();
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [loading, setLoading] = useState(false);

  const email = searchParams.get("email");
  const token = searchParams.get("token");
  const apiUrl = import.meta.env.VITE_API_URL;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (password !== confirm) {
      showError("Passwords do not match.");
      return;
    }
    setLoading(true);
    try {
      const response = await fetch(apiUrl + "/api/account/reset-password", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, token, newPassword: password }),
      });
      if (response.ok) {
        showSuccess("Password reset. Please log in.");
        navigate("/login");
      } else {
        const err = await response.json().catch(() => ({}));
        showError(err?.message || "Failed to reset password.");
      }
    } catch {
      showError("Connection error.");
    } finally {
      setLoading(false);
    }
  };

  if (!email || !token) return <div>Invalid or missing reset link.</div>;

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2 style={{ textAlign: "center"}}>Reset Password</h2>
        <form onSubmit={handleSubmit} style={{ textAlign: "center"}}>
          <input
            type="password"
            placeholder="New password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className="form-control"
            style={{ marginBottom: "0.5rem"}}
          />
          <input
            type="password"
            placeholder="Confirm new password"
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            required
            className="form-control"
            style={{ marginBottom: "0.5rem"}}
          />
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? "Resetting..." : "Reset Password"}
          </button>
        </form>
      </div>
    </div>
  );
}

export default ResetPassword;