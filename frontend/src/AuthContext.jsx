import React, { createContext, useContext, useState, useEffect } from "react";

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [userName, setUserName] = useState(localStorage.getItem("userName"));

  useEffect(() => {
    const onStorage = () => setUserName(localStorage.getItem("userName"));
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const login = (name) => {
    localStorage.setItem("userName", name);
    setUserName(name);
  };

  const logout = () => {
    localStorage.removeItem("userName");
    setUserName(null);
  };

  return (
    <AuthContext.Provider value={{ userName, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}