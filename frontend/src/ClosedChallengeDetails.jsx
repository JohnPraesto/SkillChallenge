import React, { useState } from "react";
import { Link } from "react-router-dom";

function ClosedChallengeDetails({ 
  challenge, 
  user, 
  navigate, 
  apiUrl, 
  fetchChallenge }) {
    
  // Authenticated voting:
  // Find the uploadedResultId the current user voted for (if any)
  // const userVote = user
  // ? challenge.uploadedResults?.flatMap(r => r.votes || []).find(vote => vote.userId === user.id)
  // : null;
  // const votedResultId = userVote?.uploadedResultId;

  // Anonymous voting
  const votedResultId = challenge.votedResultIdForCurrentClient ?? null;

  function FreeText({ text }) {
    const [expanded, setExpanded] = useState(false);
    const PREVIEW_LEN = 400;
    if (!text) return null;
    const needsTruncate = text.length > PREVIEW_LEN;
    const display = !expanded && needsTruncate ? text.slice(0, PREVIEW_LEN) + "…" : text;
    return (
      <div style={{ marginTop: 8 }}>
        <div
          style={{
            border: "1px solid #e6e6e6",
            padding: 12,
            borderRadius: 7,
            color: "#bebebeff",
            whiteSpace: "pre-wrap",
            lineHeight: 1.5,
            fontSize: 14
          }}
        >
          {display}
        </div>
        {needsTruncate && (
          <button
            onClick={() => setExpanded(s => !s)}
            style={{ marginTop: 6, padding: "6px 8px", cursor: "pointer" }}
            aria-expanded={expanded}
          >
            {expanded ? "Show less" : "Show more"}
          </button>
        )}
      </div>
    );
  }

  function extractYouTubeId(url) {
    // Handles regular, short, and shorts URLs
    const patterns = [
      /(?:youtube\.com\/.*v=|youtu\.be\/)([a-zA-Z0-9_-]{11})/, // regular and youtu.be
      /youtube\.com\/shorts\/([a-zA-Z0-9_-]{11})/,             // shorts
      /youtube\.com\/embed\/([a-zA-Z0-9_-]{11})/,              // embed
      /youtube\.com\/watch\?v=([a-zA-Z0-9_-]{11})/,            // watch?v=
      /youtu\.be\/([a-zA-Z0-9_-]{11})/                         // youtu.be
    ];
    for (const pattern of patterns) {
      const match = url.match(pattern);
      if (match) return match[1];
    }
    return null;
  }

  return (
    <>
      {/* Display uploaded results */}
      <div>
        <h2 className="section-title">Uploaded Results</h2>
        {challenge.uploadedResults && challenge.uploadedResults.length > 0 ? (
          <ul className="results-list">
            {challenge.joinedUsers.map((joinedUser, idx) => {
              // Find the uploaded result for this user
              const result = challenge.uploadedResults?.find(r => r.userName === joinedUser);

              if (result) {
                
                // Authenticated voting
                // const isVotedByUser = result.votes?.some(vote => vote.userId === user?.id) ?? false;
                // const userHasVoted = !!votedResultId;
                // const voteCount = result.votes ? result.votes.length : 0;

                // Anonymous voting
                const isVotedByClient = result && votedResultId === result.uploadedResultId;
                const voteCount = result.votes ? result.votes.length : 0;

                return (
                  <React.Fragment key={idx}>
                    <li className="result-item">
                      <div className="result-card">
                        <div className="result-header">
                          <div className="result-user">
                            <Link to={`/users/username/${result.userName}`}>
                              <div className="avatar-circle">
                                {result.userName}
                              </div>
                             </Link>
                          </div>
                          <span className="vote-badge">
                            {voteCount} vote{voteCount === 1 ? "" : "s"}
                          </span>




                          {/* Anonymous voting */}
                          <button
                            className={`btn vote-btn ${isVotedByClient ? "voted" : votedResultId ? "dimmed" : ""}`}
                            aria-pressed={isVotedByClient}
                            // Allow moving or toggling; no disable
                            onClick={async () => {
                              try {
                                const res = await fetch(
                                  `${apiUrl}/api/challenges/${challenge.challengeId}/uploaded-result/vote/${result.uploadedResultId}`,
                                  {
                                    method: "POST",
                                    credentials: "include"
                                  }
                                );
                                if (res.status === 429) {
                                  alert("Too many votes too quickly. Please wait a minute and try again.");
                                  return;
                                }
                                if (!res.ok) {
                                  const t = await res.text();
                                  alert(`Vote failed: ${t}`);
                                } else if (typeof fetchChallenge === "function") {
                                  fetchChallenge();
                                }
                              } catch (err) {
                                alert("Error voting: " + err.message);
                              }
                            }}
                          >
                            {isVotedByClient ? <strong>Voted</strong> : "Vote"}
                          </button>




                          {/* Authenticated voting */}
                          {/* <button
                            className={`btn vote-btn ${isVotedByUser ? "voted" : ""}`}
                            aria-pressed={isVotedByUser}
                            disabled={userHasVoted && !isVotedByUser}

                            onClick={async () => {
                              if (!user) {
                                alert("Log in to place your vote!");
                                return;
                              }
                              await fetch(
                                `${apiUrl}/api/challenges/${challenge.challengeId}/uploaded-result/vote/${result.uploadedResultId}`,
                                {
                                  method: "POST",
                                  headers: {
                                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                                  },
                                }
                              );
                              if (typeof fetchChallenge === "function") fetchChallenge();
                            }}
                          >
                            {isVotedByUser ? <strong>Voted</strong> : "Vote"}
                          </button> */}






                        </div>

                        {result.url && (
                          <div className="result-media">
                            {(() => {
                              const ytId = extractYouTubeId(result.url);
                              if (ytId) {
                                return (
                                  <iframe
                                    width="560"
                                    height="315"
                                    src={`https://www.youtube.com/embed/${ytId}`}
                                    title="YouTube video"
                                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                    allowFullScreen
                                  ></iframe>
                                );
                              }
                              const url = result.url;
                              const ext = url.split(".").pop().toLowerCase();
                              if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) {
                                return (
                                  <img
                                    src={url.startsWith("http") ? url : `${apiUrl}/${url}`}
                                    alt="Uploaded result"
                                  />
                                );
                              }
                              if (["mp4", "webm", "mov"].includes(ext)) {
                                return (
                                  <video controls src={url.startsWith("http") ? url : `${apiUrl}/${url}`}>
                                    Your browser does not support the video tag.
                                  </video>
                                );
                              }
                              if (["mp3", "wav"].includes(ext)) {
                                return (
                                  <audio controls src={url.startsWith("http") ? url : `${apiUrl}/${url}`}>
                                    Your browser does not support the audio tag.
                                  </audio>
                                );
                              }
                              if (ext === "pdf") {
                                return (
                                  <a
                                    href={url.startsWith("http") ? url : `${apiUrl}/${url}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="pdf-link"
                                  >
                                    📄 View PDF
                                  </a>
                                );
                              }
                              return (
                                <div className="media-fallback">
                                  Unable to display this result: "{url}"
                                </div>
                              );
                            })()}
                          </div>
                        )}

                        {result.freeText && (
                          <div className="result-freetext">
                            <FreeText text={result.freeText} />
                          </div>
                        )}
                      </div>
                    </li>
                  </React.Fragment>
                );
              } else {
                // User did not upload any results
                return (
                  <React.Fragment key={idx}>
                    <li className="result-item">
                      <div className="result-card result-card--muted">
                        <div className="result-header">
                          <Link to={`/users/username/${joinedUser}`}>
                            <div className="avatar-circle">
                              {joinedUser}
                            </div>
                          </Link>
                          <span className="no-upload-text">did not upload any results</span>
                        </div>
                      </div>
                    </li>
                  </React.Fragment>
                );
              }
            })}
          </ul>
        ) : (
          <span> No results uploaded.</span>
        )}




        {/* For local dev purposes */}
      {/* <button
        className="btn btn-primary"
        style={{ marginTop: 24 }}
        onClick={async () => {
          try {
            const res = await fetch(
              `${apiUrl}/api/challenges/${challenge.challengeId}/submit-result`,
              {
                method: "POST",
                headers: {
                  "Authorization": `Bearer ${localStorage.getItem("token")}`,
                },
              }
            );
            if (res.ok) {
              alert("Results submitted successfully!");
              if (typeof fetchChallenge === "function") fetchChallenge();
            } else {
              const text = await res.text();
              alert("Failed to submit results: " + text);
            }
          } catch (err) {
            alert("Error submitting results: " + err.message);
          }
        }}
      >
        Submit Results
      </button> */}
      {/* Local dev purposes END */}







      </div>
    </>
  );
}

export default ClosedChallengeDetails;