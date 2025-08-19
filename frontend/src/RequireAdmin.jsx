import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

function RequireAdmin({ children }) {
  const { user, loading } = useAuth();
  
  if (loading) {
    return <div>Loading...</div>;
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (user.roles !== "Admin") {
    console.log("this is never logged")
    return (
      <div style={{ textAlign: "center", marginTop: "3rem" }}>
        <h2>Access Denied</h2>
        <p>You do not have permission to view this page.</p>
      </div>
    );
  }

  return children;
}

export default RequireAdmin;