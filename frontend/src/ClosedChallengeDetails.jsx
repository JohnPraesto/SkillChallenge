import React from "react";

function ClosedChallengeDetails({ challenge, user, navigate, apiUrl }) {

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
            {challenge.uploadedResults.map((result, idx) => (
              <React.Fragment key={idx}>
                <li key={idx}>
                  <span>{result.userName}</span>
                  {/* Example: vote buttons */}
                  <button style={{ marginLeft: 8 }}>👍</button>
                  {/* Display result info, e.g. image or link */}
                  {/* {result.url && (
                    <span>{result.url}</span>
                  )} */}
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
            ))}
          </ul>
        ) : (
          <span> No results uploaded.</span>
        )}
      </div>
    </>
  );
}

export default ClosedChallengeDetails;