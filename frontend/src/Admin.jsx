import React, { useEffect, useState } from "react";

function Admin() {
  const [users, setUsers] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);

  const USERS_PER_PAGE = 20;
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async (query = "") => {
    setLoading(true);
    let url = `${apiUrl}/users`;
    if (query) url += `/username/${query}`;
    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      setUsers(Array.isArray(data) ? data : [data]);
      setPage(1);
    }
    setLoading(false);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    fetchUsers(search);
  };

  const handleRoleChange = async (userId, newRole) => {
    const token = localStorage.getItem("token");
    const res = await fetch(`${apiUrl}/users/${userId}/change-role`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`,
      },
      body: JSON.stringify(newRole),
    });
    if (res.ok) {
      setUsers((prev) =>
        prev.map((u) =>
          u.id === userId ? { ...u, role: newRole } : u
        )
      );
    }
  };

  const sortedUsers = [...users].sort((a, b) => a.userName.localeCompare(b.userName));
  const totalPages = Math.ceil(sortedUsers.length / USERS_PER_PAGE);
  const paginatedUsers = sortedUsers.slice(
    (page - 1) * USERS_PER_PAGE,
    page * USERS_PER_PAGE
  );

  return (
    <div style={{ maxWidth: 700, margin: "3rem auto" }}>
      <h1>Admin Dashboard</h1>
      <form onSubmit={handleSearch} style={{ marginBottom: 20 }}>
        <input
          type="text"
          placeholder="Search by username"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button type="submit">Search</button>
        <button type="button" onClick={() => { setSearch(""); fetchUsers(); }}>
          Reset
        </button>
      </form>
      {loading ? (
        <div>Loading users...</div>
      ) : (
        <>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr>
                <th>Username</th>
                <th>Email</th>
                <th>Role</th>
              </tr>
            </thead>
              <tbody>
                {paginatedUsers.map((u) => (
                  <tr key={u.id}>
                    <td>{u.userName}</td>
                    <td>{u.email}</td>
                    <td>
                      <select
                        value={u.role}
                        onChange={(e) => handleRoleChange(u.id, e.target.value)}
                        style={{
                          border: "none",
                          background: "transparent",
                          color: "white",
                          outline: "none",
                        }}
                      >
                        <option value="User">User</option>
                        <option value="Admin">Admin</option>
                      </select>
                    </td>
                  </tr>
                ))}
              </tbody>
          </table>
          <div style={{ marginTop: 16, textAlign: "center" }}>
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                style={{ marginRight: 8 }}
              >
                Previous
              </button>
              Page {page} of {totalPages}
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                style={{ marginLeft: 8 }}
              >
                Next
              </button>
            </div>
        </>
      )}
    </div>
  );
}

export default Admin;