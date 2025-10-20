import React, { useState } from "react";
import { Link } from "react-router-dom";

function FinishedChallengeDetails({ 
  challenge, 
  user, 
  navigate, 
  apiUrl, 
  fetchChallenge }) {

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

  const sortedResults = [...(challenge.uploadedResults || [])].sort(
  (a, b) => (b.votes?.length || 0) - (a.votes?.length || 0)
  );

  const topVoteCount = sortedResults[0]?.votes?.length || 0;

  // Get all usernames with the top vote count (handle ties)
  const winners = sortedResults
    .filter(r => (r.votes?.length || 0) === topVoteCount && topVoteCount > 0)
    .map(r => r.userName);

  // Users that have NOT uploaded any result
  const uploadedUserNames = new Set(
    (challenge.uploadedResults || [])
      .map(r => r.userName ?? r.user?.userName)
      .filter(Boolean)
  );

  let usersWithoutResult = [];

  if (Array.isArray(challenge.joinedUsers) && challenge.joinedUsers.length > 0) {
    usersWithoutResult = challenge.joinedUsers
      .filter(name => !uploadedUserNames.has(name))
      .map(name => ({ userName: name }));
  } else {
    const uploadedUserIds = new Set(
      (challenge.uploadedResults || [])
        .map(r => r.userId ?? r.user?.id)
        .filter(Boolean)
    );
    const participants = challenge.participants || [];
    usersWithoutResult = participants.filter(
      p => !uploadedUserIds.has(p.id ?? p.userId)
    );
  }

return (
    <>
      <div>
        <div className="winners-banner">
          <span className="trophy">🏆</span>
          <span className="winners-text">
            {winners.length > 0 ? winners.join(", ") : "No winner"}{" "}is the winner
          </span>
        </div>

        {sortedResults.length > 0 ? (
          <ul className="results-list">
            {sortedResults.map((result, idx) => {
              const voteCount = result.votes ? result.votes.length : 0;
              const isWinner = winners.includes(result.userName);

              return (
                <li className="result-item" key={idx}>
                  <div className={`result-card ${isWinner ? "result-card--winner" : ""}`}>
                    <div className="result-header">
                      <div className="result-user">
                        {/* <div className="avatar-circle">
                          {result.userName ?? result.user?.userName ?? "Unknown"}
                        </div> */}
                        <Link to={`/users/username/${result.userName}`}>
                          <div className="avatar-circle">
                            {result.userName}
                          </div>
                        </Link>
                      </div>
                      <span className="vote-badge">
                        {voteCount} vote{voteCount === 1 ? "" : "s"}
                      </span>
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
                              <video
                                controls
                                src={url.startsWith("http") ? url : `${apiUrl}/${url}`}
                              >
                                Your browser does not support the video tag.
                              </video>
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
              );
            })}

            {/* Users without uploaded results */}
            {usersWithoutResult.length > 0 && (
              <>
                {usersWithoutResult.map((u, idx2) => (
                  <li className="result-item" key={`nores-${u.id ?? u.userId ?? idx2}`}>
                    <div className="result-card result-card--muted">
                      <div className="result-header">
                        <div className="result-user">
                          {/* <div className="avatar-circle">
                            {u.userName ?? u.name ?? u.email ?? u.id ?? u.userId}
                          </div> */}
                          <Link to={`/users/username/${u.userName}`}>
                            <div className="avatar-circle">
                              {u.userName}
                            </div>
                          </Link>
                        </div>
                        <span className="no-upload-text">did not upload any results</span>
                      </div>
                    </div>
                  </li>
                ))}
              </>
            )}
          </ul>
        ) : (
          <span> No results uploaded.</span>
        )}
      </div>
    </>
  );
}

export default FinishedChallengeDetails;