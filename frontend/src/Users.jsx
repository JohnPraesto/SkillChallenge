import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

function Users() {
  const [users, setUsers] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("https://localhost:7212/users")
      .then(res => {
        if (!res.ok) throw new Error("Failed to fetch users");
        return res.json();
      })
      .then(data => setUsers(data))
      .catch(err => setError(err.message));
  }, []);

  return (
    <div>
      <h2>Users</h2>
      {error && <div style={{ color: "red" }}>{error}</div>}
      <ul>
        {users.map((u, i) => (
          <li key={i}>
            <Link to={`/users/${u.userName}`}>{u.userName}</Link>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default Users;