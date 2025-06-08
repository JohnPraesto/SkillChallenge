import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

function Challenges() {
  const [challenges, setChallenges] = useState([]);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    fetch("https://localhost:7212/challenges")
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch challenges"))
      .then(data => setChallenges(data))
      .catch(err => setError(err.toString()));
  }, []);

  if (error) return <div style={{ color: "red" }}>{error}</div>;

  const now = new Date();
  const futureChallenges = challenges.filter(ch => new Date(ch.endDate) > now);
  const pastChallenges = challenges.filter(ch => new Date(ch.endDate) <= now);

  return (
    <div style={{
      display: "flex",
      maxWidth: 900,
      margin: "2em auto",
      border: "1px solid #ccc",
      borderRadius: 8,
      overflow: "hidden"
    }}>
      {/* Future Challenges */}
      <div style={{ flex: 1, padding: 24 }}>
        <h3 style={{ textAlign: "center" }}>Open Challenges</h3>
        {futureChallenges.length === 0 ? (
          <div>No open challenges.</div>
        ) : (
          <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
            {futureChallenges.map(ch => (
              <div key={ch.challengeId} className="card" onClick={() => navigate(`/challenges/${ch.challengeId}`)}>
                {ch.subCategory && ch.subCategory.imagePath && (
                  <img
                    src={`https://localhost:7212/${ch.subCategory.imagePath}`}
                    alt={ch.subCategory.subCategoryName || "Category"}
                    style={{ width: 150, height: 150, objectFit: "cover", borderRadius: 8, marginBottom: 12 }}
                  />
                )}
                <div style={{ fontWeight: "bold", fontSize: 18, textAlign: "center" }}>
                  {ch.challengeName}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
      {/* Vertical Divider */}
      <div style={{
        width: 1,
        background: "#ccc",
        margin: "0 0.5em"
      }} />
      {/* Past Challenges */}
      <div style={{ flex: 1, padding: 24 }}>
        <h3 style={{ textAlign: "center" }}>Closed Challenges</h3>
        {pastChallenges.length === 0 ? (
          <div>No closed challenges.</div>
        ) : (
          <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
            {pastChallenges.map(ch => (
              <div key={ch.challengeId} className="card" onClick={() => navigate(`/challenges/${ch.challengeId}`)}>
                {ch.subCategory && ch.subCategory.imagePath && (
                  <img
                    src={`https://localhost:7212/${ch.subCategory.imagePath}`}
                    alt={ch.subCategory.subCategoryName || "Category"}
                    style={{ width: 150, height: 150, objectFit: "cover", borderRadius: 8, marginBottom: 12 }}
                  />
                )}
                <div style={{ fontWeight: "bold", fontSize: 18, textAlign: "center" }}>
                  {ch.challengeName}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default Challenges;