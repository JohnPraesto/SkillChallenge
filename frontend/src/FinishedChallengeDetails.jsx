import React, { useState } from "react";

function FinishedChallengeDetails({ 
  challenge, 
  user, 
  navigate, 
  apiUrl, 
  fetchChallenge }) {

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

  return (
    <>
      {/* Display uploaded results */}
      <div>
        <strong>Challenge is archived:</strong> {new Date(challenge.isTakenDown).toLocaleString('en-US', {year: 'numeric', month: 'long', day: 'numeric'})}
        <br />
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
      </div>
    </>
  );
}

export default FinishedChallengeDetails;