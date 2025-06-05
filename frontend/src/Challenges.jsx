import React, { useEffect, useState } from "react";

function Challenges() {
  const [challenges, setChallenges] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("https://localhost:7212/challenges")
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch challenges"))
      .then(data => setChallenges(data))
      .catch(err => setError(err.toString()));
  }, []);

  if (error) return <div style={{ color: "red" }}>{error}</div>;

  return (
    <div style={{ maxWidth: 600, margin: "2em auto" }}>
      <h2>All Challenges</h2>
      {challenges.length === 0 ? (
        <div>No challenges found.</div>
      ) : (
        <ul>
          {challenges.map(ch => (
            <li key={ch.challengeId}>
                {ch.underCategory && ch.underCategory.imagePath && (
                <>
                    {console.log("Image URL:", ch.underCategory.imagePath)}
                    <img
                    src={`https://localhost:7212/${ch.underCategory.imagePath}`}
                    alt={ch.underCategory.name || "Category"}
                    style={{ width: 64, height: 64, objectFit: "cover", marginBottom: 8 }}
                    />
                </>
                )}
              <strong>{ch.challengeName}</strong> <br />
              {ch.description} <br />
              <small>Created by: {ch.creatorUserName}</small>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default Challenges;