import React from "react";

function OpenChallengeDetails({
  challenge,
  user,
  alreadyJoined,
  handleJoin,
  handleLeave,
  joining,
  message,
  navigate,
  apiUrl,
}) {
  return (
    <>
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
      {alreadyJoined && (
        <button
          className="btn btn-secondary"
          style={{ marginLeft: 12, marginTop: 16 }}
          onClick={() => navigate(`/challenges/${challenge._id}/upload-result`)}
        >
          Upload Result
        </button>
      )}
      {/* Example: indicator if user has uploaded a result */}
      {alreadyJoined && challenge.uploadedResults?.some(r => r.userName === user.userName) && (
        <span style={{ color: "green", marginLeft: 8 }}>✔ Result uploaded</span>
      )}
      {message && <div style={{ marginTop: 12, color: "var(--primary-color)" }}>{message}</div>}
    </>
  );
}

export default OpenChallengeDetails;