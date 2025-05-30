import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

function Login() {
  const [form, setForm] = useState({ username: "", password: "" });
  const [message, setMessage] = useState("");
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage("");
    try {
      const response = await fetch("https://localhost:7212/account/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userName: form.username,
          password: form.password,
        }),
      });

      if (response.ok) {
        const data = await response.json();
        setMessage("Login successful! Token: " + data.token);
        localStorage.setItem("userName", data.userName);
        // Optionally, save token or redirect user here
        login(data.userName);
        navigate(`/users/${data.userName}`);
      } else {
        const error = await response.json();
        setMessage("Login failed: " + JSON.stringify(error));
      }
    } catch (err) {
      setMessage("Error: " + err.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Login</h2>
      <input
        name="username"
        placeholder="Username"
        value={form.username}
        onChange={handleChange}
        required
      />
      <input
        name="password"
        type="password"
        placeholder="Password"
        value={form.password}
        onChange={handleChange}
        required
      />
      <button type="submit">Login</button>
      {message && <div>{message}</div>}
    </form>
  );
}

export default Login;