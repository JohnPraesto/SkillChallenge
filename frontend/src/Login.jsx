import React, { useState } from "react";

function Login({ onShowRegister }) {
  const [form, setForm] = useState({ username: "", password: "" });
  const [message, setMessage] = useState("");

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
        // Optionally, save token or redirect user here
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
      <button type="button" onClick={onShowRegister} style={{ marginLeft: "1em" }}>
        Register new user
      </button>
      {message && <div>{message}</div>}
    </form>
  );
}

export default Login;