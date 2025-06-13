import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

function ChallengeDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [challenge, setChallenge] = useState(null);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [joining, setJoining] = useState(false);
  const { user } = useAuth();

 const fetchChallenge = () => {
    fetch(`https://localhost:7212/challenges/${id}`)
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch challenge"))
      .then(data => setChallenge(data))
      .catch(err => setError(err.toString()));
  };

  useEffect(() => 
    {
      fetchChallenge();
    }, [id]);

  const handleJoin = async () => {
    if (!user) {
    navigate("/register");
    return;
    }
    setJoining(true);
    setMessage("");
    try {
      const res = await fetch(`https://localhost:7212/challenges/${id}/join`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${localStorage.getItem("token")}`,
        }
      });
      if (res.ok) {
        setMessage("You joined the challenge!");
        fetchChallenge(); // Refresh challenge data to update joined users
      } else {
        const text = await res.text();
        setMessage("Failed to join: " + text);
      }
    } catch (err) {
      setMessage("Failed to join: " + err.message);
    }
    setJoining(false);
  };

  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!challenge) return <div>Loading...</div>;

  const alreadyJoined = user && challenge.joinedUsers.includes(user.userName);

  const handleLeave = async () => {
    setJoining(true);
    setMessage("");
    try {
      const res = await fetch(`https://localhost:7212/challenges/${id}/leave`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${localStorage.getItem("token")}`,
        }
      });
      if (res.ok) {
        setMessage("You left the challenge.");
        fetchChallenge();
      } else {
        const text = await res.text();
        setMessage("Failed to leave: " + text);
      }
    } catch (err) {
      setMessage("Failed to leave: " + err.message);
    }
    setJoining(false);
  };

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
      <button
        className="btn btn-primary"
        style={{ marginTop: 16 }}
        onClick={alreadyJoined ? handleLeave : handleJoin}
        disabled={joining}
      >
        {joining 
          ? (alreadyJoined ? "Leaving..." : "Joining...")
          : (alreadyJoined ? "Leave Challenge" : "Join Challenge")}
      </button>
      {message && <div style={{ marginTop: 12, color: "var(--primary-color)" }}>{message}</div>}
    </div>
  );
}

export default ChallengeDetails;