import React, { useState } from "react";
import { Link } from "react-router-dom";

function OpenChallengeDetails({
  challenge,
  user,
  navigate,
  apiUrl,
  fetchChallenge,
}) {

  const [message, setMessage] = useState("");
  const [joining, setJoining] = useState(false);
  const alreadyJoined = user && challenge.joinedUsers.includes(user.userName);
  const isFull = challenge.joinedUsers.length >= challenge.numberOfParticipants;
  const endDateStr = new Date(challenge.endDate).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

  const handleJoin = async () => {
    if (!user) {
    navigate("/register");
    return;
    }
    setJoining(true);
    setMessage("");
    try {
      const res = await fetch(`${apiUrl}/api/challenges/${challenge.challengeId}/join`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${localStorage.getItem("token")}`,
        }
      });
      if (res.ok) {
        setMessage("You joined the challenge!");
        fetchChallenge(); // Refresh challenge data to update joined users
      } else {
        if (res.status === 401 || res.status === 403) {
          setMessage("Failed to join: your session expired or you are not authorized. Please log in again.");
        } else {
          const text = await res.text();
          setMessage("Failed to join: " + (text || `HTTP ${res.status}`));
        }
      }
    } catch (err) {
      setMessage("Failed to join: " + err.message);
    }
    setJoining(false);
  };

  const handleLeave = async () => {
    setJoining(true);
    setMessage("");
    try {
      const res = await fetch(`${apiUrl}/api/challenges/${challenge.challengeId}/leave`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${localStorage.getItem("token")}`,
        }
      });
      if (res.ok) {
        setMessage("You left the challenge.");
        fetchChallenge();
      } else {
        if (res.status === 401 || res.status === 403) {
          setMessage("Failed to leave: your session expired or you are not authorized. Please log in again.");
        } else {
          const text = await res.text();
          setMessage("Failed to leave: " + (text || `HTTP ${res.status}`));
        }
      }
    } catch (err) {
      setMessage("Failed to leave: " + err.message);
    }
    setJoining(false);
  };

  const hasUploadedResult = alreadyJoined && challenge.uploadedResults?.some(r => r.userId === user.id);

  const handleRemoveResult = async () => {
    try {
      const res = await fetch(
        `${apiUrl}/api/challenges/${challenge.challengeId}/uploaded-result`,
        {
          method: "DELETE",
          headers: {
            "Authorization": `Bearer ${localStorage.getItem("token")}`,
          },
        }
      );
      if (res.ok) {
        alert("Result removed!");
        if (fetchChallenge) fetchChallenge();
      } else {
        const text = await res.text();
        alert("Failed to remove result: " + text);
      }
    } catch (err) {
      alert("Failed to remove result: " + err.message);
    }
  };
  
  return (
    <>
      <div>
        <h2 className="section-title">Participants:</h2>
        {challenge.joinedUsers && challenge.joinedUsers.length > 0 ? (
          <ul>
            {challenge.joinedUsers.map((joinedUser, index) => {
              const hasResult = challenge.uploadedResults?.some(r => r.userName === joinedUser);
              return (
                <li key={index} style={{ display: "flex", alignItems: "center" }}>
                  <Link className="user-link" to={`/users/username/${joinedUser}`}>
                    {joinedUser}
                  </Link>
                  {hasResult && (
                    <span className="result-uploaded">
                      ✔ Result uploaded
                    </span>
                  )}
                </li>
              );
            })}
          </ul>
        ) : (
          <span> None</span>
        )}
      </div>

      <p style={{textAlign:"center"}}>Uploads will be revealed when voting starts {endDateStr}</p>

      <button
        className="btn btn-primary"
        style={{
          marginTop: 16,
          cursor: alreadyJoined ? "pointer" : isFull ? "not-allowed" : "pointer",
          opacity: alreadyJoined ? 1 : isFull ? 0.6 : 1
        }}
        onClick={alreadyJoined ? handleLeave : handleJoin}
        disabled={joining || (!alreadyJoined && isFull)}
        title={!alreadyJoined && isFull ? "Maximum number of participants reached" : ""}
      >
        {joining
          ? alreadyJoined
            ? "Leaving..."
            : "Joining..."
          : alreadyJoined
          ? "Leave Challenge"
          : "Join Challenge"}
      </button>
      {/* Upload result button for joined users */}
      {alreadyJoined && !hasUploadedResult && (
        <button
          className="btn btn-secondary"
          style={{ marginBottom: 12, marginTop: 16 }}
          onClick={() => navigate("/upload-result", {state:{apiUrl, challengeId: challenge.challengeId}})}
        >
          Upload Result
        </button>
      )}
      {alreadyJoined && hasUploadedResult && (
      <button
        className="btn btn-danger"
        style={{ marginBottom: 12, marginTop: 16 }}
        onClick={handleRemoveResult}
      >
        Remove Result
      </button>
    )}
      {message && <div style={{ marginTop: 12, color: "var(--primary-color)" }}>{message}</div>}
    </>
  );
}

export default OpenChallengeDetails;