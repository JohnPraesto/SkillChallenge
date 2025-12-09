import React, { createContext, useContext, useState, useEffect } from "react";
import { jwtDecode } from "jwt-decode";

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (token) {
      try {
        const decoded = jwtDecode(token);
        const nowSec = Math.floor(Date.now() / 1000);
        const exp = decoded.exp;
        if (exp && exp <= nowSec) {
          // Token expired – clear and treat as logged out
          localStorage.removeItem("token");
          setUser(null);
          setToken(null);
        } else {
          setUser({
            id: decoded.sub,
            userName: decoded.userName || decoded.sub || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
            roles: decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || []
          });
        }
      } catch {
        setUser(null);
      }
    } else {
      setUser(null);
    }
    setLoading(false);
  }, [token]);

  // Optional: auto-logout when token is about to expire
  useEffect(() => {
    if (!token) return;
    let timeoutId;
    try {
      const decoded = jwtDecode(token);
      if (decoded?.exp) {
        const msUntilExpiry = decoded.exp * 1000 - Date.now();
        if (msUntilExpiry <= 0) {
          logout();
        } else {
          timeoutId = setTimeout(() => logout(), msUntilExpiry);
        }
      }
    } catch {
      // ignore
    }
    return () => clearTimeout(timeoutId);
  }, [token]);

  useEffect(() => {
    const onStorage = () => setToken(localStorage.getItem("token"));
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const login = (jwtToken) => {
    localStorage.setItem("token", jwtToken);
    setToken(jwtToken);
  };

  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}