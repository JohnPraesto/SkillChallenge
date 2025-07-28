import React, { useState } from "react";

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

  const handleJoin = async () => {
    if (!user) {
    navigate("/register");
    return;
    }
    setJoining(true);
    setMessage("");
    try {
      const res = await fetch(`${apiUrl}/challenges/${id}/join`, {
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

  const handleLeave = async () => {
    setJoining(true);
    setMessage("");
    try {
      const res = await fetch(`${apiUrl}/challenges/${id}/leave`, {
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

  const handleUploadResult = async () => {
    const uploadedResultURL = window.prompt("Enter the URL for your result:");
    if (!uploadedResultURL) return;

    try {
      const res = await fetch(
        `${apiUrl}/challenges/${challenge.challengeId}/upload-result`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`,
          },
          body: JSON.stringify(uploadedResultURL),
        }
      );
      if (res.ok) {
        alert("Result uploaded!");
        if (fetchChallenge) fetchChallenge();
      } else {
        const text = await res.text();
        alert("Failed to upload result: " + text);
      }
    } catch (err) {
      alert("Failed to upload result: " + err.message);
    }
  };

  const hasUploadedResult = alreadyJoined && challenge.uploadedResults?.some(r => r.userId === user.id);

  // const test = challenge.uploadedResults?.some(r => r.userId === user.id)

  const handleRemoveResult = async () => {
    try {
      const res = await fetch(
        `${apiUrl}/challenges/${challenge.challengeId}/uploaded-result`,
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
        <strong>Joined users:</strong>
        {challenge.joinedUsers && challenge.joinedUsers.length > 0 ? (
          <ul>
            {challenge.joinedUsers.map((joinedUser, index) => {
              const hasResult = challenge.uploadedResults?.some(r => r.userName === joinedUser);
              return (
              <li key={index} style={{ cursor: "pointer", color: "var(--primary-color)", display: "flex", alignItems: "center" }} onClick={() => navigate(`/users/username/${joinedUser}`)}>
                {joinedUser}
                {hasResult && (
                  <span style={{ color: "green", marginLeft: 8, fontWeight: "bold" }}>
                    ✔ Result uploaded
                  </span>)}
              </li>
              );
            })}
          </ul>
        ) : (
          <span> None</span>
        )}
      </div>
      <button
        className="btn btn-primary"
        style={{ marginTop: 16 }}
        onClick={alreadyJoined ? handleLeave : handleJoin}
        disabled={joining}
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
          style={{ marginLeft: 12, marginTop: 16 }}
          onClick={handleUploadResult}
        >
          Upload Result
        </button>
      )}
      {alreadyJoined && hasUploadedResult && (
      <button
        className="btn btn-danger"
        style={{ marginLeft: 12, marginTop: 16 }}
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