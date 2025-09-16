import React, { useState } from "react";

function ClosedChallengeDetails({ 
  challenge, 
  user, 
  navigate, 
  apiUrl, 
  fetchChallenge }) {
    
  // Find the uploadedResultId the current user voted for (if any)
  // const userVote = challenge.uploadedResults?.flatMap(r => r.votes || [])
  //   .find(vote => vote.userId === user.id);
  const userVote = user
  ? challenge.uploadedResults?.flatMap(r => r.votes || []).find(vote => vote.userId === user.id)
  : null;

  const votedResultId = userVote?.uploadedResultId;

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
        <strong>Voting closes:</strong> {new Date(challenge.votePeriodEnd).toLocaleString('en-US', {year: 'numeric', month: 'long', day: 'numeric'})}
        <br />
        <strong>Uploaded Results:</strong>
        {challenge.uploadedResults && challenge.uploadedResults.length > 0 ? (
          <ul>
            {challenge.uploadedResults.map((result, idx) => {
              // Is this result voted by the current user?
              const isVotedByUser = result.votes?.some(vote => vote.userId === user?.id) ?? false;

              // Has the user voted for any result?
              const userHasVoted = !!votedResultId;

              // Button style logic
              let btnStyle = { marginLeft: 8, cursor: "pointer" };
              if (userHasVoted) {
                btnStyle = isVotedByUser
                  ? { marginLeft: 8, background: "#1aeb7b", fontWeight: "bold", cursor: "pointer" } // Highlight
                  : { marginLeft: 8, background: "#404040", cursor: "not-allowed" }; // Greyed out
              }

              const voteCount = result.votes ? result.votes.length : 0;

              return (
                <React.Fragment key={idx}>
                  <li>
                    <span>{result.userName}</span>
                    <button
                      style={btnStyle}
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
                      👍
                    </button>
                    <span style={{ marginLeft: 8, fontWeight: "bold" }}>
                      Vote count: {voteCount}
                    </span>
                    {result.url && (
                      <div style={{ marginTop: 8 }}>
                        {(() => {
                          // YouTube
                          const ytId = extractYouTubeId(result.url);
                          if (ytId) {
                            return (
                              <iframe
                                width="320"
                                height="180"
                                src={`https://www.youtube.com/embed/${ytId}`}
                                title="YouTube video"
                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                allowFullScreen
                              ></iframe>
                            );
                          }
                          // File extension logic
                          const url = result.url;
                          const ext = url.split('.').pop().toLowerCase();
                          if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) {
                            // Image
                            return (
                              <img
                                src={url.startsWith("http") ? url : `${apiUrl}/${url}`}
                                alt="Uploaded result"
                                style={{ maxWidth: 500, maxHeight: 500 }}
                              />
                            );
                          }
                          if (["mp4", "webm", "mov"].includes(ext)) {
                            // Video
                            return (
                              <video
                                controls
                                width="320"
                                height="180"
                                src={url.startsWith("http") ? url : `${apiUrl}/${url}`}
                              >
                                Your browser does not support the video tag.
                              </video>
                            );
                          }
                          if (ext === "pdf") {
                            // PDF
                            return (
                              <a
                                href={url.startsWith("http") ? url : `${apiUrl}/${url}`}
                                target="_blank"
                                rel="noopener noreferrer"
                                style={{ display: "inline-block", marginTop: 8 }}
                              >
                                📄 View PDF
                              </a>
                            );
                          }
                          // Fallback
                          return (
                            <div style={{ marginTop: 8 }}>
                              Unable to display this result: "{url}"
                            </div>
                          );
                        })()}
                      </div>
                    )}
                  </li>
                  <hr style={{ margin: "16px 0", border: "none", borderTop: "1px solid #ccc" }} />
                </React.Fragment>
              );
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