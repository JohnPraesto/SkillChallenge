import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";

function ChallengeDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [challenge, setChallenge] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    fetch(`https://localhost:7212/challenges/${id}`)
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch challenge"))
      .then(data => setChallenge(data))
      .catch(err => setError(err.toString()));
  }, [id]);

  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!challenge) return <div>Loading...</div>;

  return (
    <div style={{ maxWidth: 500, margin: "2em auto", padding: 24, border: "1px solid #ccc", borderRadius: 8 }}>
      <h2>{challenge.challengeName}</h2>
      <img
        src={`https://localhost:7212/${challenge.subCategory.imagePath}`}
        alt={challenge.subCategory.subCategoryName || "Category"}
        style={{ width: 300, height: 300, objectFit: "cover", borderRadius: 8, marginBottom: 12 }}
        />
      <div><strong>End Date:</strong> {new Date(challenge.endDate).toLocaleString()}</div>
      <div><strong>Description:</strong> {challenge.description}</div>
      <div>
        <strong>Joined users:</strong>
        {challenge.joinedUsers && challenge.joinedUsers.length > 0 ? (
          <ul>
            {challenge.joinedUsers.map((user, index) => (
              <li
                key={index}
                style={{ cursor: "pointer", color: "var(--primary-color)" }}
                onClick={() => navigate(`/users/username/${user}`)}
              >
                {user}
              </li>
            ))}
          </ul>
        ) : (
          <span> None</span>
        )}
      </div>
      <div><strong>Created by:</strong> {challenge.creatorUserName}</div>
    </div>
  );
}

export default ChallengeDetails;