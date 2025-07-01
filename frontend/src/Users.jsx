import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { LoadingSkeleton } from "./LoadingSkeleton";
import { useToast } from "./ToastContext";

function Users() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const { showError } = useToast();
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetch(apiUrl + "/users")
      .then(res => {
        if (!res.ok) throw new Error("Failed to fetch users");
        return res.json();
      })
      .then(data => {
        setUsers(data);
        setLoading(false);
      })
      .catch(err => {
        showError("Failed to load users");
        setLoading(false);
      });
  }, [showError]);

  const filteredUsers = users.filter(user =>
    user.userName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) return (
    <div className="container">
      <LoadingSkeleton type="card" count={8} />
    </div>
  );

  return (
    <div className="container fade-in">
      <div className="page-header" style={{ textAlign: "center", marginBottom: "2rem" }}>
        <h1 style={{ color: "var(--primary-color)" }}>Community Members</h1>
        <p style={{ color: "var(--text-secondary)" }}>
          Connect with {users.length} skilled challengers
        </p>
      </div>

      <div className="search-bar" style={{ marginBottom: "2rem" }}>
        <input
          type="text"
          placeholder="Search users..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="form-control search-input"
        />
        <span className="search-icon">👥</span>
      </div>

      <div className="users-grid" style={{ 
        display: "grid", 
        gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", 
        gap: "1.5rem" 
      }}>
        {filteredUsers.map((user, index) => (
          <Link 
            key={user.id || index} 
            to={`/users/username/${user.userName}`}
            className="card user-card stagger-item"
            style={{ 
              textDecoration: "none", 
              color: "inherit",
              animationDelay: `${index * 0.1}s`
            }}
          >
            <div style={{ display: "flex", alignItems: "center", gap: "1rem" }}>
              {/* Debugging to see filepath */}
              {user.profilePicture} 
              <img
                src={user.profilePicture ? `${apiUrl}${user.profilePicture}`: `${apiUrl}/profile-pictures/default.png`}
                alt={user.userName}
                style={{ 
                  width: "60px", 
                  height: "60px", 
                  borderRadius: "50%", 
                  objectFit: "cover",
                  border: "3px solid var(--primary-color)"
                }}
              />
              <div>
                <h3 style={{ margin: "0 0 0.5rem 0", color: "var(--primary-color)" }}>
                  {user.userName}
                </h3>
                <p style={{ margin: 0, color: "var(--text-secondary)", fontSize: "0.9rem" }}>
                  View Profile →
                </p>
              </div>
            </div>
          </Link>
        ))}
      </div>

      {filteredUsers.length === 0 && (
        <div className="card bounce-in" style={{ textAlign: "center", padding: "3rem" }}>
          <h3>No users found</h3>
          <p style={{ color: "var(--text-secondary)" }}>
            {searchTerm ? "Try a different search term" : "No users available"}
          </p>
        </div>
      )}
    </div>
  );
}

export default Users;