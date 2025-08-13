import React, { useState } from "react";

function FinishedChallengeDetails({ 
  challenge, 
  user, 
  navigate, 
  apiUrl, 
  fetchChallenge }) {

  function extractYouTubeId(url) {
    const match = url.match(
      /(?:youtube\.com\/.*v=|youtu\.be\/)([a-zA-Z0-9_-]{11})/
    );
    return match ? match[1] : null;
  }

  const sortedResults = [...(challenge.uploadedResults || [])].sort(
  (a, b) => (b.votes?.length || 0) - (a.votes?.length || 0)
  );

  const topVoteCount = sortedResults[0]?.votes?.length || 0;

  // Get all usernames with the top vote count (handle ties)
  const winners = sortedResults
    .filter(r => (r.votes?.length || 0) === topVoteCount && topVoteCount > 0)
    .map(r => r.userName);

  return (
    <>
      {/* Display uploaded results */}
      <div style={{ marginTop: 16 }}>
        <strong>
          Winner{winners.length > 1 ? "s" : ""}:{" "}
          {winners.length > 0 ? winners.join(", ") : "No winner"}
        </strong>
        {sortedResults.length > 0 ? (
          <ul>
            {sortedResults.map((result, idx) => {
              const voteCount = result.votes ? result.votes.length : 0;
              return (
                <React.Fragment key={idx}>
                  <li>
                    <span>{result.userName}</span>
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
      </div>
    </>
  );
}

export default FinishedChallengeDetails;