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
    const match = url.match(
      /(?:youtube\.com\/.*v=|youtu\.be\/)([a-zA-Z0-9_-]{11})/
    );
    return match ? match[1] : null;
  }

  return (
    <>
      {/* Display uploaded results */}
      <div style={{ marginTop: 16 }}>
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
                          `${apiUrl}/challenges/${challenge.challengeId}/uploaded-result/vote/${result.uploadedResultId}`,
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
                        <iframe
                          width="320"
                          height="180"
                          src={`https://www.youtube.com/embed/${extractYouTubeId(result.url)}`}
                          title="YouTube video"
                          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                          allowFullScreen
                        ></iframe>
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
        <button
        className="btn btn-primary"
        style={{ marginTop: 24 }}
        onClick={async () => {
          try {
            const res = await fetch(
              `${apiUrl}/challenges/${challenge.challengeId}/submit-result`,
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
      </button>
      {/* This button is to be removed and replaced by an
      automatic function that does the same stuff when the EndDate
      of the challenge has passed */}
      </div>
    </>
  );
}

export default ClosedChallengeDetails;