import React, { useEffect, useState } from "react";

function AdminChallenges() {
  const [challenges, setChallenges] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);

  const CHALLENGES_PER_PAGE = 20;
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetchChallenges();
  }, []);

  const fetchChallenges = async (query = "") => {
    setLoading(true);
    let url = `${apiUrl}/api/challenges`;
    if (query) url += `/${query}`;
    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      setChallenges(Array.isArray(data) ? data : [data]);
      setPage(1);
    }
    setLoading(false);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    fetchChallenges(search);
  };

  const handleDeleteChallenge = async (challengeId) => {
    const token = localStorage.getItem("token");
    const res = await fetch(`${apiUrl}/api/challenges/${challengeId}`, {
      method: "DELETE",
      headers: {
        "Authorization": `Bearer ${token}`,
      },
    });
    if (res.ok) {
      setChallenges((prev) => prev.filter((u) => u.challengeId !== challengeId));
    }
  };

  const sortedChallenges = [...challenges].sort((a, b) => a.challengeName.localeCompare(b.challengeName));
  const totalPages = Math.ceil(sortedChallenges.length / CHALLENGES_PER_PAGE);
  const paginatedChallenges = sortedChallenges.slice(
    (page - 1) * CHALLENGES_PER_PAGE,
    page * CHALLENGES_PER_PAGE
  );

  return (
    <div style={{ maxWidth: 700, margin: "3rem auto" }}>
      <h1>Admin Dashboard</h1>
      <form onSubmit={handleSearch} style={{ marginBottom: 20 }}>
        <input
          type="text"
          placeholder="Search by challenge name"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button type="submit">Search</button>
        <button type="button" onClick={() => { setSearch(""); fetchChallenges(); }}>
          Reset
        </button>
      </form>
      {loading ? (
        <div>Loading challenges...</div>
      ) : (
        <>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr>
                <th>Challenge name</th>
                <th>Creator</th>
                <th>Delete</th>
              </tr>
            </thead>
              <tbody>
                {paginatedChallenges.map((u) => (
                  <tr key={u.challengeId}>
                    <td>{u.challengeName}</td>
                    <td>{u.creatorUserName}</td>
                    <td>
                      <button
                        onClick={() => handleDeleteChallenge(u.challengeId)}
                        style={{
                          marginLeft: 8,
                          background: "red",
                          color: "white",
                          border: "none",
                          padding: "2px 8px",
                          borderRadius: "4px",
                          cursor: "pointer",
                        }}
                      >
                        Delete
                      </button>
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

export default AdminChallenges;