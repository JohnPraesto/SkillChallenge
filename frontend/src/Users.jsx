import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { LoadingSkeleton } from "./LoadingSkeleton";
import { useToast } from "./ToastContext";

function Users() {
  const [users, setUsers] = useState([]);
  const [leaderboard, setLeaderboard] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const { showError } = useToast();
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetch(apiUrl + "/api/users")
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

    fetch(apiUrl + "/api/leaderboard")
      .then(res => {
        if (!res.ok) throw new Error("Failed to fetch leaderboard");
        return res.json();
      })
      .then(data => {
        setLeaderboard(data);
        setLoading(false);
      })
      .catch(err => {
        showError("Failed to load leaderboard");
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
        <h1 style={{ color: "var(--primary-color)" }}>Leaderboard</h1>
      </div>

      {leaderboard.map(category => (
      <div key={category.categoryId} style={{ marginBottom: "3rem" }}>
        <h2 style={{
          color: "var(--primary-color)",
          textAlign: "center",
          marginBottom: "2rem"
        }}>
          {category.categoryName}
        </h2>
        <div
          className="subcategory-grid"
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))",
            gap: "2rem",
            marginBottom: "2rem"
          }}
        >
          {category.subCategories.map(sub => (
            <div
              key={sub.subCategoryId}
              className="subcategory-card"
              style={{
                background: "var(--surface)",
                borderRadius: "12px",
                boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
                padding: "1.5rem",
                display: "flex",
                flexDirection: "column",
                alignItems: "center"
              }}
            >
              {/* Subcategory Image */}
              <img
                src={sub.imagePath
                  ? sub.imagePath.startsWith("http")
                    ? sub.imagePath
                    : `${apiUrl}/${sub.imagePath}`
                  : `${apiUrl}/category-images/default.png`
                }
                alt={sub.subCategoryName}
                style={{
                  width: "130px",
                  height: "130px",
                  objectFit: "cover",
                  borderRadius: "10px",
                  border: "2px solid var(--secondary-color)"
                }}
              />
              {/* Subcategory Name */}
              <h3 style={{
                color: "var(--secondary-color)",
                marginBottom: "1rem",
                textAlign: "center"
              }}>
                {sub.subCategoryName}
              </h3>
              {/* Top Users */}
              {sub.topUsers.length > 0 ? (
                <ol style={{ width: "100%", paddingLeft: "1.2em" }}>
                  {sub.topUsers.map((user, idx) => (
                    <li
                      key={user.userId}
                      style={{
                        marginBottom: "0.5rem",
                        display: "flex",
                        alignItems: "center",
                        gap: "0.7rem"
                      }}
                    >
                      <img
                        src={user.profilePicture
                          ? user.profilePicture.startsWith("http")
                            ? user.profilePicture
                            : `${apiUrl}/${user.profilePicture}`
                          : `${apiUrl}/profile-pictures/default.png`
                        }
                        alt={user.userName}
                        style={{
                          width: "28px",
                          height: "28px",
                          borderRadius: "50%",
                          objectFit: "cover",
                          border: "1.5px solid var(--primary-color)"
                        }}
                      />
                      <Link
                        className="user-link"
                        to={`/users/username/${user.userName}`}
                        style={{ fontWeight: 600 }}
                        title={`View ${user.userName}'s profile`}
                      >
                        {user.userName}
                      </Link>
                      <span style={{
                        color: "var(--text-secondary)",
                        marginLeft: "auto",
                        fontSize: "0.95em"
                      }}>
                        {user.rating}
                      </span>
                    </li>
                  ))}
                </ol>
              ) : (
                <p style={{ color: "var(--text-secondary)", textAlign: "center" }}>
                  No users yet
                </p>
              )}
            </div>
          ))}
        </div>
      </div>
    ))}

      {leaderboard.length === 0 && (
        <div className="card bounce-in" style={{ textAlign: "center", padding: "3rem" }}>
          <h3>No leaderboard data found</h3>
        </div>
      )}








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
              <img
                src={user.profilePicture ? user.profilePicture.startsWith("http")
                  ? user.profilePicture
                  : `${apiUrl}/${user.profilePicture}`
                  : `${apiUrl}/profile-pictures/default.png`
                  }
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